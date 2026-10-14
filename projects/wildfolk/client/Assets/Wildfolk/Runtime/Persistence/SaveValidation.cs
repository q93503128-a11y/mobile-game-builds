using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Wildfolk
{
    public static class SaveValidation
    {
        private static void Require(bool valid,string reason){if(!valid)throw new InvalidDataException(reason);}
        private static bool Finite(double value)=>!double.IsNaN(value) && !double.IsInfinity(value);
        public static void Validate(SavedGame game,ContentCatalog catalog,WorldGenerationDefinition definition,AnimalPopulationDefinition animals,int playerCapacity,int maximumHP,SurvivalDefinition survival,CharacterGrowthDefinition growth,LocomotionDefinition locomotion)
        {
            Require(game!=null && (game.Version==2 || game.Version==3),"Unsupported save version");Require(Finite(game.Elapsed) && game.Elapsed>=0,"Invalid world time");
            var map=new WorldMap(definition,game.Seed);var player=game.Player;
            Require(player!=null,"Missing player");GrowthValidation.Validate(player.Growth,growth,player.XP);
            var hpBonus=Mathf.RoundToInt(GrowthValidation.Bonus(player.Growth,growth,SkillEffect.MaximumHealth));
            Require(player.HP>0 && player.HP<=maximumHP+hpBonus && player.Selected>=0 && player.Selected<playerCapacity,"Invalid player");
            var staminaMax=locomotion.MaximumStamina+GrowthValidation.Bonus(player.Growth,growth,SkillEffect.MaximumStamina);
            Require(Finite(player.Stamina) && player.Stamina>=0 && player.Stamina<=staminaMax && Finite(player.StaminaWait) && player.StaminaWait>=0 && player.StaminaWait<=locomotion.RecoveryDelay,"Invalid stamina");
            Armor(player.HeadArmor,ArmorSlot.Head,catalog);Armor(player.BodyArmor,ArmorSlot.Body,catalog);
            Require(player.SeenRecipes!=null,"Missing recipe state");var seen=new HashSet<string>();foreach(var id in player.SeenRecipes){Require(seen.Add(id),"Duplicate recipe state");catalog.Recipe(id);}
            Position(map,player.Position);Require(map.IsWalkable(player.Position,0),"Player is outside traversable terrain");
            Require(player.XP>=0 && player.Gold>=0 && Finite(player.CombatRecovery) && player.CombatRecovery>=0 && player.CombatRecovery<=survival.RegenDelay,"Invalid progression");
            new InventoryModel(playerCapacity).Restore(player.Inventory,catalog);
            var needs=player.Survival;Require(needs!=null,"Missing survival state");
            foreach(var value in new[]{needs.Hunger,needs.Thirst,needs.HungerClock,needs.ThirstClock,needs.RecoveryBlock,needs.DrainPenalty,needs.DrainMultiplier})Require(Finite(value) && value>=0,"Invalid survival value");
            Require(needs.Hunger<=survival.MaximumHunger && needs.Thirst<=survival.MaximumThirst && needs.DrainMultiplier>=1 && needs.HungerClock<survival.DamageInterval && needs.ThirstClock<survival.DamageInterval,"Invalid survival state");
            Require(game.Resources!=null && game.Animals!=null && game.Placeables!=null && game.Pickups!=null,"Missing world state");
            Require(game.Version<3 || game.Recovery!=null,"Missing recovery state");
            var ids=new HashSet<string>();
            for(var i=0;i<map.Resources.Count;i++)ids.Add("r"+i);
            var resourceIds=new HashSet<string>();
            foreach(var state in game.Resources)
            {
                Require(state!=null && resourceIds.Add(state.Id) && !string.IsNullOrEmpty(state.Id) && (state.Id.StartsWith("r") || game.Version>=3 && state.Id.StartsWith("p")),"Resource ID collision");
                Require(int.TryParse(state.Id.Substring(1),out var index) && index>=0 && state.Rule>=0 && state.Rule<definition.Resources.Length,"Unknown resource ID");
                if(state.Id.StartsWith("r"))Require(index<map.Resources.Count && state.Rule==map.Resources[index].Rule,"Resource rule mismatch");else{Require(!state.Destroyed && game.Recovery!=null && index>0 && index<=game.Recovery.Serial,"Invalid recovered resource");Require(ids.Add(state.Id),"Recovered resource ID collision");}Position(map,state.Position);
                Require(Finite(state.Scale) && state.Scale>=definition.Resources[state.Rule].ScaleRange.x && state.Scale<=definition.Resources[state.Rule].ScaleRange.y && Finite(state.RespawnAt) && state.RespawnAt>=0,"Invalid resource state");
                Require(state.Destroyed ? state.HP==0 : state.HP>0 && state.HP<=definition.Resources[state.Rule].Prefab.Health.Maximum,"Invalid resource HP");
            }
            if(game.Recovery!=null)ValidateRecovery(game.Recovery,map,definition);
            var expectedAnimals=0;foreach(var rule in animals.Rules)expectedAnimals+=rule.Population;
            Require(game.Animals.Length==expectedAnimals,"Animal population mismatch");
            foreach(var state in game.Animals)
            {
                Require(state!=null && !string.IsNullOrEmpty(state.Id) && ids.Add(state.Id) && state.Rule>=0 && state.Rule<animals.Rules.Length,"Invalid animal ID");
                Require(state.Id.StartsWith("a"+state.Rule+"_") && int.TryParse(state.Id.Substring(state.Id.IndexOf('_')+1),out var slot) && slot>=0 && slot<animals.Rules[state.Rule].Population,"Unknown animal slot");
                Position(map,state.Position);Position(map,state.Home);Require(Finite(state.RespawnAt) && state.RespawnAt>=0,"Invalid animal timer");
                Require(state.Missing ? state.HP==0 : state.HP>0 && state.HP<=animals.Rules[state.Rule].Prefab.Health.Maximum,"Invalid animal HP");
            }
            foreach(var state in game.Placeables)
            {
                Require(state!=null && !string.IsNullOrEmpty(state.Id) && ids.Add(state.Id),"Placeable ID collision");Position(map,state.Position);Require(Finite(state.Rotation),"Invalid rotation");
                var item=catalog.Item(state.Item);var placed=item.Placement;Require(placed!=null && state.HP>0 && state.HP<=placed.Vitality.MaximumHealth,"Invalid facility");
                Require(!state.DoorOpen || placed.IsDoor,"Invalid door state");
                if(placed.StorageSlots>0)new InventoryModel(placed.StorageSlots).Restore(state.Inventory,catalog);else Require(state.Inventory==null || state.Inventory.Length==0,"Unexpected chest contents");
                if(state.Processing==null)continue;
                if(state.Processing.Sequential){ValidateQueue(state.Processing,placed,catalog);continue;}
                if(string.IsNullOrEmpty(state.Processing.Recipe)){Require(state.Processing.Batch==0 && state.Processing.Remaining==0,"Invalid empty processing state");continue;}
                var recipe=catalog.Recipe(state.Processing.Recipe);
                Require(recipe.RequiredStation==placed.Station && recipe.ProcessingSeconds>0 && state.Processing.Batch>0 && state.Processing.Batch<=placed.ParallelCapacity,"Invalid processing batch");
                Require(Finite(state.Processing.Remaining) && state.Processing.Remaining>=0 && state.Processing.Remaining<=(recipe.LegacyProcessingSeconds>0 ? recipe.LegacyProcessingSeconds : recipe.ProcessingSeconds)/placed.ProcessingSpeed,"Invalid processing timer");
            }
            foreach(var pickup in game.Pickups){Require(pickup!=null,"Missing pickup");Position(map,pickup.Position);Require(pickup.Count>0 && pickup.Count<=catalog.Item(pickup.Item).MaxStack,"Invalid pickup");}
        }
        private static void ValidateQueue(ProcessingSnapshot state,PlaceableDefinition placed,ContentCatalog catalog)
        {
            Require(placed.ProcessingMode==ProcessingMode.Sequential && state.Waiting!=null && state.Output!=null,"Invalid queue");var input=0;var output=0;var outputIds=new HashSet<string>();
            foreach(var entry in state.Waiting){Require(entry!=null && entry.Count>0 && entry.Count<=placed.InputCapacity,"Invalid waiting input");var r=catalog.Recipe(entry.Recipe);Require(r.RequiredStation==placed.Station && r.ProcessingSeconds>0,"Invalid queued recipe");input=checked(input+entry.Count);}
            foreach(var entry in state.Output){Require(entry!=null && entry.Count>0 && entry.Count<=placed.OutputCapacity && outputIds.Add(entry.Recipe),"Invalid output");var r=catalog.Recipe(entry.Recipe);Require(r.RequiredStation==placed.Station && r.ProcessingSeconds>0,"Invalid result recipe");output=checked(output+entry.Count*r.ResultCount);}
            if(!string.IsNullOrEmpty(state.Recipe)){var r=catalog.Recipe(state.Recipe);Require(r.RequiredStation==placed.Station && state.Batch==1 && Finite(state.Remaining) && state.Remaining>=0 && state.Remaining<=r.ProcessingSeconds/placed.ProcessingSpeed,"Invalid active unit");input++;}
            else Require(state.Batch==0 && state.Remaining==0,"Invalid idle queue");Require(input<=placed.InputCapacity && output<=placed.OutputCapacity,"Queue capacity exceeded");
        }
        private static void ValidateRecovery(ResourceRecoverySnapshot state,WorldMap map,WorldGenerationDefinition definition)
        {
            Require(state.Serial>=0 && state.CandidateCursor>=0 && state.Pending!=null && state.Harvests!=null,"Invalid population recovery");var keys=new HashSet<string>();
            foreach(var t in state.Pending)Require(t!=null && t.Rule>=0 && t.Rule<definition.Resources.Length && t.Region>=0 && t.Region<ResourcePopulation.Columns*ResourcePopulation.Rows && Finite(t.Due) && t.Due>=0 && keys.Add(t.Rule+":"+t.Region),"Invalid recovery ticket");
            foreach(var h in state.Harvests){Require(h!=null && h.Rule>=0 && h.Rule<definition.Resources.Length && Finite(h.Until) && h.Until>=0,"Invalid harvest exclusion");Position(map,h.Position);}
        }
        private static void Armor(string id,ArmorSlot slot,ContentCatalog catalog){if(string.IsNullOrEmpty(id))return;var item=catalog.Item(id);Require(item.Armor!=null && item.Armor.Slot==slot,"Invalid armor slot");}
        private static void Position(WorldMap map,Vector2 p)=>Require(Finite(p.x) && Finite(p.y) && map.Bounds.Contains(p),"Position outside world");
    }
}
