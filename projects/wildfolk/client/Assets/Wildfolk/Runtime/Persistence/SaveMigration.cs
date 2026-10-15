using System;
namespace Wildfolk
{
    public static class SaveMigration
    {
        public static void EcologyFromV3(SavedGame game,WorldGenerationDefinition definition,AnimalPopulationDefinition animals)
        {
            var living=new System.Collections.Generic.List<AnimalSnapshot>();var tickets=new System.Collections.Generic.List<RecoveryTicket>();var map=new WorldMap(definition,game.Seed);var targets=AnimalPopulation.BuildTargets(map,animals);
            foreach(var state in game.Animals)
            {
                var region=AnimalPopulation.Region(map,state.Missing ? state.Home : state.Position);
                if(state.Missing){if(targets[state.Rule,region]==0){for(var r=0;r<12;r++)if(targets[state.Rule,r]>0){region=r;break;}}tickets.Add(new RecoveryTicket{Rule=state.Rule,Region=region,Due=game.Elapsed+Math.Min(animals.Rules[state.Rule].RespawnSeconds,Math.Max(0,state.RespawnAt-game.Elapsed))});continue;}
                state.Species=animals.Rules[state.Rule].Prefab.Member.Definition.Id;state.GroupId="legacy_"+state.Id;state.Region=region;state.RespawnAt=0;living.Add(state);
            }
            game.Animals=living.ToArray();game.AnimalEcology=new AnimalEcologySnapshot{Pending=tickets.ToArray()};game.GoblinEcology=null;game.NightCreatures=Array.Empty<NightCreatureSnapshot>();game.Version=4;
        }
        public static void PopulationFromV2(SavedGame game,WorldGenerationDefinition definition)
        {
            var map=new WorldMap(definition,game.Seed);var population=new ResourcePopulation(map);
            foreach(var state in game.Resources)
            {
                var index=int.Parse(state.Id.Substring(1));var initial=map.Resources[index];population.Remove(state.Rule,initial.Position,game.Elapsed,false);
                if(!state.Destroyed)population.Add(state.Rule,state.Position);
                else
                {
                    var rule=definition.Resources[state.Rule];population.Harvests.Add(new HarvestExclusion{Rule=state.Rule,Position=state.Position,Until=game.Elapsed+rule.HarvestExclusionSeconds});
                    var region=population.Region(state.Rule,initial.Position);var ticket=population.Pending.Find(t=>t.Rule==state.Rule && t.Region==region);
                    if(ticket!=null)ticket.Due=Math.Min(ticket.Due,game.Elapsed+Math.Min(rule.RespawnSeconds,Math.Max(0,state.RespawnAt-game.Elapsed)));
                }
                state.RespawnAt=0;
            }
            population.Pending.RemoveAll(t=>population.Count(t.Rule,t.Region)>=population.Target(t.Rule,t.Region));game.Recovery=population.Capture();game.Version=3;
        }
    }
}
