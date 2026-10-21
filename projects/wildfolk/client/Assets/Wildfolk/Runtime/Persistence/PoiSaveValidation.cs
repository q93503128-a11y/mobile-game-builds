using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Wildfolk
{
    public static class PoiSaveValidation
    {
        private static void Need(bool ok){if(!ok)throw new InvalidDataException("Invalid POI state");}
        public static void Validate(SavedGame game,WorldMap map,WorldGenerationDefinition world)
        {
            var config=world.PoiContent;if(config==null)return;
            var anchors=new Dictionary<string,WorldAnchor>();if(map.Streamed)foreach(var a in map.Continental.Anchors)anchors.Add(a.Id,a);
            var features=new Dictionary<string,WorldFeatureSnapshot>();foreach(var f in game.Features)features.Add(f.Id,f);
            foreach(var f in game.Features)
            {
                var s=f.Site;if(s==null || s.Schema==0)continue;
                Need(s.Schema==1 && anchors.TryGetValue(f.Id,out _) && PoiContentDefinition.Supports(anchors[f.Id].Kind));var anchor=anchors[f.Id];
                Need(s.Loot!=null && s.Loot.Length==6 && s.Nodes!=null && s.Water>=0 && s.Water<=config.WellCapacity && !double.IsNaN(s.RefillAt) && !double.IsInfinity(s.RefillAt) && s.RefillAt>=0);
                var expected=s.Legacy ? new InventoryModel(6).Capture() : config.Supplies(anchor.Kind,game.Seed,f.Id);
                for(var i=0;i<6;i++){var slot=s.Loot[i];var limit=expected[i];Need(slot!=null && slot.Count>=0 && slot.Charges==0 && slot.Quality==LiquidState.Empty);Need(slot.Count==0 ? string.IsNullOrEmpty(slot.Item) : slot.Item==limit.Item && slot.Count<=limit.Count);}
                if(anchor.Kind!=WorldAnchorKind.Well)Need(s.Water==0 && s.RefillAt==0);else Need(s.Nodes.Length==0 && (s.Water==config.WellCapacity ? s.RefillAt==0 : s.RefillAt>0));
                if(anchor.Kind!=WorldAnchorKind.Quarry || s.Legacy)Need(s.Nodes.Length==0);
                var ids=new HashSet<int>();foreach(var node in s.Nodes)
                {
                    Need(node!=null && node.Id!=null && node.Id.StartsWith("poi:"+f.Id+":",StringComparison.Ordinal));
                    Need(int.TryParse(node.Id.Substring(node.Id.LastIndexOf(':')+1),out var n) && n>=0 && n<config.QuarryRules.Length && ids.Add(n) && node.Rule==config.QuarryRules[n]);
                    Need(node.Scale==1 && node.RespawnAt==0 && !float.IsNaN(node.Position.x) && !float.IsInfinity(node.Position.x) && !float.IsNaN(node.Position.y) && !float.IsInfinity(node.Position.y));
                    Need(map.Contains(node.Position) && Vector2.Distance(node.Position,anchor.Position)<=8 && WorldChunk.At(node.Position,world.ChunkSize).Equals(WorldChunk.At(anchor.Position,world.ChunkSize)));
                    Need(node.Destroyed ? node.HP==0 : node.HP>0 && node.HP<=world.Resources[node.Rule].Prefab.Health.Maximum);
                }
            }
            var actors=new HashSet<string>();foreach(var actor in game.NightCreatures??Array.Empty<NightCreatureSnapshot>())
            {
                if(actor?.Feature==null || !actor.Feature.EndsWith(":guard",StringComparison.Ordinal))continue;
                var id=actor.Feature.Substring(0,actor.Feature.Length-6);
                Need(actors.Add(actor.Feature) && actor.Species=="skeleton" && string.IsNullOrEmpty(actor.Variant) && anchors.TryGetValue(id,out _) && anchors[id].Kind==WorldAnchorKind.Graveyard && features.TryGetValue(id,out _) && features[id].Site!=null && features[id].Site.Schema==1 && !features[id].Site.Legacy && features.TryGetValue(actor.Feature,out _) && !features[actor.Feature].Cleared);
            }
            foreach(var f in game.Features)if(f.Id.EndsWith(":guard",StringComparison.Ordinal))Need(f.Cleared ? !actors.Contains(f.Id) : actors.Contains(f.Id));
        }
    }
}
