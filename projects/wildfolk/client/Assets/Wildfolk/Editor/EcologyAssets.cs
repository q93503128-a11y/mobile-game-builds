using UnityEditor;
using UnityEngine;
namespace Wildfolk.Editor
{
    internal static class EcologyAssets
    {
        internal static T Data<T>(string name) where T:Object=>AssetDatabase.LoadAssetAtPath<T>("Assets/Wildfolk/Data/"+name+".asset");
        internal static CreatureDefinition Species(string name)=>Data<CreatureDefinition>(name+"Creature");
        private static LootEntry Entry(ItemDefinition item,int min,int max,float chance=1)=>new LootEntry{Item=item,Minimum=min,Maximum=max,Chance=chance};
        private static LootTable Loot(string name,LootEntry[] items,int xp=0,int gold=0)=>GameplayAssets.Asset<LootTable>(name,l=>{l.Items=items;l.Experience=xp;l.Gold=gold;});
        private static CreatureDefinition Creature(string name,string id,CreatureFamily family,CreatureFaction faction,CreatureTrait traits,CreatureHabitat habitat,float radius,int min,int max,float roam)
        =>GameplayAssets.Asset<CreatureDefinition>(name+"Creature",d=>{d.Id=id;d.Family=family;d.Faction=faction;d.Traits=traits;d.Habitat=habitat;d.SeparationRadius=radius;d.MinimumGroup=min;d.MaximumGroup=max;d.RoamRadius=roam;d.PreferredSpacing=radius*2+1;});
        internal static void Load(GameplayAssets a)
        {
            var herbivore=CreatureTrait.Herding|CreatureTrait.FleeTogether;
            Creature("Chicken","chicken",CreatureFamily.Animal,CreatureFaction.Wildlife,herbivore,CreatureHabitat.Grassland|CreatureHabitat.ForestEdge,.34f,2,4,7);
            Creature("Boar","boar",CreatureFamily.Animal,CreatureFaction.Wildlife,CreatureTrait.Territorial,CreatureHabitat.Forest|CreatureHabitat.ForestEdge,.62f,1,2,8);
            Creature("Rabbit","rabbit",CreatureFamily.Animal,CreatureFaction.Wildlife,herbivore,CreatureHabitat.Grassland|CreatureHabitat.ForestEdge,.3f,1,3,9);
            Creature("Deer","deer",CreatureFamily.Animal,CreatureFaction.Wildlife,herbivore,CreatureHabitat.Forest|CreatureHabitat.ForestEdge,.7f,2,4,14);
            Creature("Sheep","sheep",CreatureFamily.Animal,CreatureFaction.Wildlife,herbivore,CreatureHabitat.Grassland,.6f,2,5,9);
            Creature("Zombie","zombie",CreatureFamily.Undead,CreatureFaction.Undead,CreatureTrait.LightSensitive|CreatureTrait.Nocturnal,CreatureHabitat.NightSurface,.53f,1,1,8);
            var skeleton=Creature("Skeleton","skeleton",CreatureFamily.Undead,CreatureFaction.Undead,CreatureTrait.LightSensitive|CreatureTrait.Nocturnal,CreatureHabitat.NightSurface,.48f,1,1,8);
            skeleton.Light.DimDetection=.7f;skeleton.Light.SafeMovement=.65f;EditorUtility.SetDirty(skeleton);
            var goblinTraits=CreatureTrait.AllyAssist|CreatureTrait.PackAggro|CreatureTrait.Territorial;
            Creature("Goblin","goblin",CreatureFamily.Humanoid,CreatureFaction.Goblin,goblinTraits,CreatureHabitat.Roadside|CreatureHabitat.Camp,.85f,3,5,4);
            Creature("SlingshotGoblin","slingshot_goblin",CreatureFamily.Humanoid,CreatureFaction.Goblin,goblinTraits|CreatureTrait.Ranged,CreatureHabitat.Roadside|CreatureHabitat.Camp,.85f,3,5,4);
            GameplayAssets.Asset<SlingshotDefinition>("SlingshotAttack",d=>{});GameplayAssets.Asset<GoblinCampDefinition>("GoblinCamp",d=>{});
            var combat=GameplayAssets.Asset<CreatureCombatDefinition>("SkeletonCombat",d=>{});
            var vitality=GameplayAssets.Asset<VitalityDefinition>("SkeletonVitality",d=>d.MaximumHealth=32);
            if(!combat.Configured){combat.Speed=2.65f;vitality.MaximumHealth=32;combat.Configured=true;EditorUtility.SetDirty(combat);EditorUtility.SetDirty(vitality);}
            var wood=Data<ItemDefinition>("Wood");var stone=Data<ItemDefinition>("Stone");var leather=Data<ItemDefinition>("Leather");var ore=Data<ItemDefinition>("IronOre");
            var tool=GameplayAssets.Asset<ToolDefinition>("RustedSwordTool",d=>{d.HeldSprite=VisualArtImporter.Sprite("rusted_sword");d.Tier=a.Progression.WoodTier;d.Kind=ToolKind.Sword;d.Damage=11;d.Duration=.3f;d.Cooldown=.48f;d.HandExtension=.25f;d.SwingArc=55;d.Efficiencies=new ResourceEfficiency[0];d.Attack=new AttackProfile{Range=1.85f,ArcAngle=110,MaxTargets=1};});
            if(tool.Efficiencies.Length==0){tool.Efficiencies=(ResourceEfficiency[])Data<ToolDefinition>("WoodSwordTool").Efficiencies.Clone();EditorUtility.SetDirty(tool);}
            var sword=GameplayAssets.Asset<ItemDefinition>("RustedSword",d=>{d.Id="rusted_sword";d.DisplayName="Rusted Sword";d.MaxStack=1;d.Category=ItemCategory.Weapon;d.Tool=tool;d.Icon=tool.HeldSprite;});
            Loot("RabbitLoot",new[]{Entry(a.Survival.Raw,1,1)});Loot("DeerLoot",new[]{Entry(a.Survival.Raw,2,3),Entry(leather,1,1,.75f)});Loot("SheepLoot",new[]{Entry(a.Survival.Raw,2,2)});
            var baseLoot=Loot("SkeletonLoot",new[]{Entry(stone,1,2,.75f),Entry(ore,1,1,.2f)},7,2);
            var rare=Loot("SkeletonSwordLoot",new[]{Entry(stone,1,2,.75f),Entry(ore,1,1,.2f)},8,2);
            if(rare.RareItems.Length==0){rare.RareItems=new[]{Entry(sword,1,1,.12f)};EditorUtility.SetDirty(rare);}
            GameplayAssets.Asset<CreatureVariantDefinition>("RustedSwordVariant",d=>{d.Id="rusted_sword";d.Weapon=tool.HeldSprite;d.Loot=rare;});
            if(a.World.ZombieLoot.Items.Length==0){a.World.ZombieLoot.Items=new[]{Entry(wood,1,2,.6f),Entry(leather,1,1,.2f)};EditorUtility.SetDirty(a.World.ZombieLoot);}
            if(a.World.GoblinLoot.Items.Length==0){a.World.GoblinLoot.Items=new[]{Entry(stone,1,3,.8f),Entry(wood,1,2,.6f)};EditorUtility.SetDirty(a.World.GoblinLoot);}
            foreach(var name in new[]{"Rabbit","Deer","Sheep"})GameplayAssets.Asset<AnimalMovementDefinition>(name+"Movement",d=>{d.WanderSpeed=name=="Deer" ? .75f : name=="Sheep" ? .45f : .6f;d.FleeSpeed=name=="Rabbit" ? 4.2f : name=="Deer" ? 3.6f : 2;d.DangerRange=name=="Deer" ? 5.5f : name=="Rabbit" ? 3.5f : 3;d.HomeRadius=Species(name).RoamRadius;d.FleeSeconds=3.5f;});
        }
        internal static void ConfigurePopulation()
        {
            var population=StabilityAssets.Animals();if(population.RegionalConfigured)return;
            var names=new[]{"Chicken","Boar","Rabbit","Deer","Sheep"};var counts=new[]{10,6,12,8,10};var timers=new[]{180,300,180,420,300};
            population.Rules=new AnimalPopulationRule[names.Length];for(var i=0;i<names.Length;i++)population.Rules[i]=new AnimalPopulationRule{Prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/"+names[i]+".prefab").GetComponent<NeutralAnimal>(),Population=counts[i],RespawnSeconds=timers[i]};population.RegionalConfigured=true;EditorUtility.SetDirty(population);
        }
    }
}
