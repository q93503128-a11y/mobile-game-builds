using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Profiling;
using UnityEngine;
namespace Wildfolk.Editor
{
    // Explicit Editor-only probe, never authored into a scene, prefab, or player HUD.
    public sealed class ExplorationProfiler
    {
        private readonly List<float> frames=new List<float>();
        private readonly List<long> gc=new List<long>();
        private ProfilerRecorder allocations,main;
        private readonly List<double> cpu=new List<double>();
        private string stage;private int remaining,warmup=15,maxChunks;
        private WorldGenerator world;private PlayerInputCommands commands;private Vector2 movement;
        private Vector2 start;
        private static ExplorationProfiler current;
        private int lastFrame=-1;
        public static void Begin(string name,int frameCount=180,float moveX=0,float moveY=0)
        {
            if(current!=null)throw new InvalidOperationException("A profiling stage is still running.");
            var probe=new ExplorationProfiler();current=probe;probe.stage=name;probe.remaining=frameCount;probe.movement=new Vector2(moveX,moveY);probe.world=UnityEngine.Object.FindFirstObjectByType<WorldGenerator>();
            var player=NetworkCreatureWorld.Active==null ? probe.world.Player : UnityEngine.Object.FindFirstObjectByType<MultiplayerArena>().LocalPlayer.GetComponent<PlayerInventory>();
            player.GetComponent<PlayerInputAdapter>().enabled=false;probe.commands=player.GetComponent<PlayerInputCommands>();probe.start=player.transform.position;
            probe.allocations=ProfilerRecorder.StartNew(ProfilerCategory.Memory,"GC Allocated In Frame",1);
            probe.main=ProfilerRecorder.StartNew(ProfilerCategory.Internal,"Main Thread",1);
            UnityEditor.EditorApplication.update+=probe.Update;
        }
        private void Update()
        {
            if(!UnityEditor.EditorApplication.isPlaying){Stop();return;}
            if(lastFrame==Time.frameCount)return;lastFrame=Time.frameCount;
            commands.SetMove(movement);
            if(warmup-->0)return;
            frames.Add(Time.unscaledDeltaTime*1000);if(allocations.Valid)gc.Add(allocations.LastValue);if(main.Valid)cpu.Add(main.LastValue/1000000.0);
            maxChunks=Math.Max(maxChunks,world.Chunks.ActiveCount);
            if(--remaining>0)return;
            commands.SetMove(Vector2.zero);
            var resources=UnityEngine.Object.FindObjectsByType<ResourceNode>(FindObjectsSortMode.None).Length;var creatures=UnityEngine.Object.FindObjectsByType<CreatureMember>(FindObjectsSortMode.None).Length;
            var sorted=frames.OrderBy(x=>x).ToArray();
            var report=new{stage,utc=DateTime.UtcNow.ToString("o"),samples=frames.Count,frameMeanMs=frames.Average(),frameP95Ms=sorted[Mathf.FloorToInt((sorted.Length-1)*.95f)],frameMaxMs=frames.Max(),mainRecorderValid=main.Valid,mainMeanMs=cpu.Count>0 ? (double?)cpu.Average() : null,gcRecorderValid=allocations.Valid,gcMeanBytes=gc.Count>0 ? (double?)gc.Average() : null,gcMaxBytes=gc.Count>0 ? (long?)gc.Max() : null,activeGameObjects=UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None).Length,activeChunks=world.Chunks.ActiveCount,maxChunks,resources,creatures,terrainBytes=world.Terrain.TerrainTextureBytes,cachedTiles=world.Terrain.CachedTileCount,generatedTiles=world.Terrain.GeneratedTiles,cacheHits=world.Terrain.CacheHits,terrainPending=world.Terrain.TerrainWorkCount,terrainSliceMaxMs=world.Terrain.MaximumTerrainSliceMs,start=new{x=start.x,y=start.y},end=new{x=commands.transform.position.x,y=commands.transform.position.y},focused=UnityEditorInternal.InternalEditorUtility.isApplicationActive,graphics=SystemInfo.graphicsDeviceName};
            Directory.CreateDirectory("Captures/exploration");File.WriteAllText("Captures/exploration/profile-"+stage+".json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));Stop();
        }
        private void Stop(){UnityEditor.EditorApplication.update-=Update;if(commands!=null)commands.SetMove(Vector2.zero);allocations.Dispose();main.Dispose();current=null;}
    }
}
