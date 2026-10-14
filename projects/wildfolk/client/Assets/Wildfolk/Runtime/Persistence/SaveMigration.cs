using System;
namespace Wildfolk
{
    public static class SaveMigration
    {
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
