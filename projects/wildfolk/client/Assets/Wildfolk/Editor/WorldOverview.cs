using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
namespace Wildfolk.Editor
{
    // Observes the canonical layout. No materialization, generator mutation, or runtime Update.
    public sealed class WorldOverview:IDisposable
    {
        public readonly WorldMap Map;
        public readonly Texture2D Texture;
        public readonly Dictionary<string,Vector2> Targets=new Dictionary<string,Vector2>();
        public readonly int[] BiomeSamples=new int[4];
        public readonly List<string> Issues=new List<string>();
        public int LandSamples {get;private set;}
        public static readonly Color32[] Colors={new Color32(139,153,89,255),new Color32(52,91,59,255),new Color32(91,132,119,255),new Color32(143,139,127,255)};
        private readonly Color32[] pixels;private readonly int width;
        public WorldOverview(WorldMap map,int resolution=768)
        {
            if(map==null || !map.Streamed)throw new ArgumentException("A generated continental world is required.");
            Map=map;width=Mathf.Clamp(resolution,128,1536);pixels=new Color32[width*width];var half=map.Size*.5f;var step=map.Size.x/width;
            for(var y=0;y<width;y++)for(var x=0;x<width;x++)
            {
                var p=new Vector2((x+.5f)*step-half.x,(y+.5f)*step-half.y);var c=new Color32(32,62,82,255);
                if(map.Continental.CoastDistance(p)>=0)
                {
                    if(map.IsWater(p))c=new Color32(78,133,155,255);
                    else {var biome=(int)map.Continental.Environment(p).Biome;BiomeSamples[biome]++;LandSamples++;c=Colors[biome];}
                }
                pixels[y*width+x]=c;
            }
            // Rivers can be narrower than a review pixel. Mark their exact analytic centerline
            // at >=1px for legibility; water extent still comes from WaterDistance above.
            for(var y=0;y<width;y++){var wy=(y+.5f)*step-half.y;var p=new Vector2(map.RiverAt(wy),wy);if(map.Continental.CoastDistance(p)>0)Dot(p,new Color32(78,133,155,255),1);}
            foreach(var road in map.Roads)for(var i=1;i<road.Length;i++)Line(road[i-1],road[i],new Color32(220,191,129,255));
            Targets.Add("Spawn",map.Spawn);Targets.Add("Forest anchor",map.Continental.ForestAnchor);Targets.Add("Rocky anchor",map.Continental.RockyAnchor);
            Targets.Add("River bank",map.Continental.SedimentAnchor);
            // Representative wetland is sampled from this very layout, never assumed to exist.
            var wet=FindBiome(SurfaceBiome.Wetlands);if(wet.HasValue)Targets.Add("Wetlands representative",wet.Value);
            foreach(var a in map.Continental.Anchors){Targets.Add(a.Kind+" / "+a.Id,a.Position);Dot(a.Position,MarkerColor(a.Kind),a.Important?2:1);if(!map.Contains(a.Position)||!map.IsWalkable(a.Position,.4f))Issues.Add("Invalid POI "+a.Id);}
            foreach(var cave in map.Continental.Caves)foreach(var room in cave.Rooms)if(room.Depth>=.8f){Targets.Add("Deep Cavern "+cave.Id+" / "+cave.Rooms.IndexOf(room),room.Center);break;}
            Dot(map.Spawn,new Color32(255,244,200,255),4);
            if(!map.IsWalkable(map.Spawn,.6f))Issues.Add("Spawn is not walkable");
            if(map.Roads.Count==0)Issues.Add("No roads");for(var i=0;i<4;i++)if(BiomeSamples[i]==0)Issues.Add("Missing sampled biome "+(SurfaceBiome)i);
            Texture=new Texture2D(width,width,TextureFormat.RGB24,false){name="Seed "+map.Seed,hideFlags=HideFlags.HideAndDontSave,filterMode=FilterMode.Point};Texture.SetPixels32(pixels);Texture.Apply();
        }
        private Vector2? FindBiome(SurfaceBiome biome)
        {var half=Map.Size.x*.5f;for(var r=128f;r<half;r+=128)for(var a=0;a<32;a++){var p=new Vector2(Mathf.Cos(a*Mathf.PI/16),Mathf.Sin(a*Mathf.PI/16))*r;if(Map.Contains(p)&&Map.IsWalkable(p,.7f)&&Map.Continental.Environment(p).Biome==biome)return p;}return null;}
        private Vector2Int Pixel(Vector2 p)=>new Vector2Int(Mathf.Clamp(Mathf.FloorToInt((p.x/Map.Size.x+.5f)*width),0,width-1),Mathf.Clamp(Mathf.FloorToInt((p.y/Map.Size.y+.5f)*width),0,width-1));
        private void Dot(Vector2 p,Color32 c,int radius){var q=Pixel(p);for(var y=Mathf.Max(0,q.y-radius);y<=Mathf.Min(width-1,q.y+radius);y++)for(var x=Mathf.Max(0,q.x-radius);x<=Mathf.Min(width-1,q.x+radius);x++)pixels[y*width+x]=c;}
        private void Line(Vector2 a,Vector2 b,Color32 c){var steps=Mathf.CeilToInt(Vector2.Distance(a,b)/Map.Size.x*width*2);for(var i=0;i<=steps;i++)Dot(Vector2.Lerp(a,b,(float)i/Mathf.Max(1,steps)),c,0);}
        public static Color32 MarkerColor(WorldAnchorKind kind)
        {
            switch(kind){case WorldAnchorKind.CaveEntrance:return new Color32(179,152,210,255);case WorldAnchorKind.Graveyard:return new Color32(225,225,220,255);case WorldAnchorKind.GoblinCamp:return new Color32(202,89,63,255);case WorldAnchorKind.Quarry:return new Color32(170,165,139,255);case WorldAnchorKind.Mine:return new Color32(225,140,72,255);case WorldAnchorKind.Well:return new Color32(120,210,218,255);case WorldAnchorKind.Ruin:return new Color32(174,116,154,255);default:return new Color32(232,180,85,255);}
        }

        public string Export(string directory)
        {
            Directory.CreateDirectory(directory);var stem="world-overview-seed-"+Map.Seed;var png=Texture.EncodeToPNG();File.WriteAllBytes(Path.Combine(directory,stem+".png"),png);
            var svg=new StringBuilder("<svg xmlns='http://www.w3.org/2000/svg' width='1068' height='840' viewBox='0 0 1068 840'><rect width='1068' height='840' fill='#182820'/><g fill='#f7ebcb' font-family='Noto Sans KR, sans-serif' font-size='17'>");
            svg.Append("<text x='20' y='28'>Lowfire · Actual generation ").Append(Map.GenerationVersion).Append(" · Seed ").Append(Map.Seed).Append("</text><image x='20' y='50' width='768' height='768' href='data:image/png;base64,").Append(Convert.ToBase64String(png)).Append("'/>");
            var labels=new[]{"Grassland","Forest","Wetlands","Rocky","Water / coast","Road / trail","Spawn","Cave entrance","Graveyard","Goblin Camp","Quarry","Mine","Well","Ruin","Campsite"};
            var swatches=Colors.Concat(new[]{new Color32(78,133,155,255),new Color32(220,191,129,255),new Color32(255,244,200,255),MarkerColor(WorldAnchorKind.CaveEntrance),MarkerColor(WorldAnchorKind.Graveyard),MarkerColor(WorldAnchorKind.GoblinCamp),MarkerColor(WorldAnchorKind.Quarry),MarkerColor(WorldAnchorKind.Mine),MarkerColor(WorldAnchorKind.Well),MarkerColor(WorldAnchorKind.Ruin),MarkerColor(WorldAnchorKind.Campsite)}).ToArray();
            for(var i=0;i<labels.Length;i++)svg.Append("<rect x='810' y='").Append(60+i*34).Append("' width='18' height='18' fill='#").Append(ColorUtility.ToHtmlStringRGB(swatches[i])).Append("'/><text x='838' y='").Append(75+i*34).Append("'>").Append(labels[i]).Append("</text>");
            svg.Append("<text x='810' y='615'>North ↑ · Surface only</text><text x='810' y='649'>Rivers/markers widened</text><text x='810' y='677'>for overview readability.</text><text x='810' y='719'>Roads: ").Append(Map.Roads.Count).Append("</text><text x='810' y='753'>POIs: ").Append(Map.Continental.Anchors.Count).Append("</text><text x='810' y='787'>Caves: ").Append(Map.Continental.Caves.Count).Append("</text></g></svg>");
            File.WriteAllText(Path.Combine(directory,stem+".svg"),svg.ToString());
            File.WriteAllText(Path.Combine(directory,stem+".json"),JsonUtility.ToJson(new OverviewRecord{seed=Map.Seed,generation=Map.GenerationVersion,resolution=width,roads=Map.Roads.Count,pois=Map.Continental.Anchors.Count,caves=Map.Continental.Caves.Count,biomeSamples=BiomeSamples,landSamples=LandSamples,issues=Issues.ToArray(),targets=Targets.Select(p=>new TargetRecord{name=p.Key,position=p.Value}).ToArray()},true));return Path.Combine(directory,stem+".svg");
        }
        [Serializable]public sealed class TargetRecord{public string name;public Vector2 position;}
        [Serializable]private sealed class OverviewRecord{public int seed,generation,resolution,roads,pois,caves,landSamples;public int[] biomeSamples;public string[] issues;public TargetRecord[] targets;}
        public void Dispose(){if(Texture!=null)UnityEngine.Object.DestroyImmediate(Texture);}
    }
}
