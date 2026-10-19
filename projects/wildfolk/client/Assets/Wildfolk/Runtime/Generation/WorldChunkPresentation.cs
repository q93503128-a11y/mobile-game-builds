using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    public sealed partial class WorldChunkRuntime
    {
        private readonly Dictionary<WorldChunk,GameObject> presentation=new Dictionary<WorldChunk,GameObject>();
        private readonly HashSet<WorldChunk> presentationWanted=new HashSet<WorldChunk>();
        private Vector2 scoutPoint;private float scoutUntil;
        public int PresentationOnlyCount=>presentation.Count;
        public void RequestScoutPresentation(Vector2 point){scoutPoint=point;scoutUntil=Time.unscaledTime+3;}
        private void ReconcilePresentation()
        {
            presentationWanted.Clear();
            if(Time.unscaledTime<scoutUntil)
            {
                var size=World.Definition.ChunkSize;var camera=Camera.main;var half=camera==null ? new Vector2(18,11) : new Vector2(camera.orthographicSize*camera.aspect+1,camera.orthographicSize+1);
                var low=WorldChunk.At(scoutPoint-half,size);var high=WorldChunk.At(scoutPoint+half,size);
                for(var y=low.Y;y<=high.Y;y++)for(var x=low.X;x<=high.X;x++){var key=new WorldChunk(x,y);if(presentationWanted.Count<9 && World.Map.Continental.ContainsChunk(key) && !active.ContainsKey(key))presentationWanted.Add(key);}
            }
            retiring.Clear();foreach(var p in presentation)if(!presentationWanted.Contains(p.Key))retiring.Add(p.Key);foreach(var key in retiring)RemovePresentation(key,!active.ContainsKey(key));
            foreach(var key in presentationWanted)if(!presentation.ContainsKey(key))
            {
                var root=new GameObject("Distant landscape");root.transform.SetParent(World.Environment,false);presentation.Add(key,root);World.Terrain.BuildChunk(World.Map,key,root.transform);Decorations(key,root.transform);
                foreach(var a in World.Map.Continental.Anchors)if(a.Kind!=WorldAnchorKind.GoblinCamp && WorldChunk.At(a.Position,World.Definition.ChunkSize).Equals(key))Landmark(root.transform,a.Kind,a.Position);
                foreach(var s in DeepCavernContent.Sites(World.Map.Continental))if(WorldChunk.At(s.Position,World.Definition.ChunkSize).Equals(key))DeepLandmark(root.transform,s);
            }
        }
        private void RemovePresentation(WorldChunk key,bool releaseTerrain)
        {if(!presentation.TryGetValue(key,out var root))return;if(releaseTerrain)World.Terrain.ReleaseChunk(key);presentation.Remove(key);if(root!=null){root.SetActive(false);Destroy(root);}}
        private void ClearPresentation(){foreach(var p in presentation)if(p.Value!=null)Destroy(p.Value);presentation.Clear();scoutUntil=0;}
    }
}
