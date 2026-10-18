using System;
using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    public sealed class WorldAnchor
    {public string Id;public Vector2 Position;public WorldAnchorKind Kind;public bool Important;}
    public enum WorldAnchorKind { Campsite,Ruin,Well,Quarry,Graveyard,Mine,GoblinCamp,CaveEntrance }
    public sealed class ContinentalWorldLayout
    {
        private readonly WorldGenerationDefinition data;private readonly int seed;
        public Vector2 Size {get;}
        public float Phase {get;}
        private readonly float coastPhase;
        public SurfaceBiomeRegions Biomes {get;}
        public Vector2 LakeCenter {get;}
        public readonly List<Vector2[]> Roads=new List<Vector2[]>();
        public readonly List<WorldAnchor> Anchors=new List<WorldAnchor>();
        public readonly List<CavernNetwork> Caves=new List<CavernNetwork>();
        public readonly List<ResourcePlacement> GuaranteedResources=new List<ResourcePlacement>();
        private readonly Dictionary<WorldChunk,List<Vector2[]>> roadIndex=new Dictionary<WorldChunk,List<Vector2[]>>();
        public Vector2 ForestAnchor {get;}public Vector2 RockyAnchor {get;}public Vector2 SedimentAnchor {get;private set;}
        public ContinentalWorldLayout(WorldGenerationDefinition definition,int value,bool validateGeneration=true)
        {
            data=definition;seed=value;var width=Mathf.Ceil(definition.WalkingSpeed*60*definition.TravelMinutesAcross/definition.ChunkSize)*definition.ChunkSize;Size=Vector2.one*width;
            Phase=WorldSeeds.Unit(seed,SeedDomain.Hydrology)*6.28f;LakeCenter=new Vector2(-1100+WorldSeeds.Unit(seed,SeedDomain.Hydrology,1)*180,-850);
            coastPhase=WorldSeeds.Unit(seed,SeedDomain.Continental)*6.28f;
            var angle=(WorldSeeds.Unit(seed,SeedDomain.Biome)-.5f)*1.1f;ForestAnchor=new Vector2(Mathf.Cos(angle)*210,Mathf.Sin(angle)*210);RockyAnchor=new Vector2(-650,-80+WorldSeeds.Unit(seed,SeedDomain.Terrain)*160);SedimentAnchor=new Vector2(RiverAt(100)+7,100);
            Biomes=new SurfaceBiomeRegions(this,data);GenerateAnchors();GenerateRoads();WorldProgressionSupply.Build(this,data);if(validateGeneration)WorldGenerationValidator.Validate(this,data);
        }
        public bool Underground(Vector2 p)=>p.x>Size.x;
        public CavernNetwork CaveAt(Vector2 p){CavernNetwork best=null;float d=float.MaxValue;foreach(var c in Caves){var n=(c.Origin-p).sqrMagnitude;if(n<d){d=n;best=c;}}return best;}
        public bool Contains(Vector2 p)=>Underground(p) ? CaveAt(p)!=null && (p-CaveAt(p).Origin).sqrMagnitude<220*220 : Mathf.Abs(p.x)<=Size.x*.5f && Mathf.Abs(p.y)<=Size.y*.5f;
        public bool ContainsChunk(WorldChunk k)=>Contains(k.Center(data.ChunkSize));
        public float CoastDistance(Vector2 p)
        {var angle=Mathf.Atan2(p.y,p.x);var radius=Size.x*(.458f+.014f*Mathf.Sin(angle*3+coastPhase)+.008f*Mathf.Sin(angle*7-coastPhase));return radius-p.magnitude;}
        public float RiverAt(float y)=>600+Mathf.Sin(y/810+Phase)*160+Mathf.Sin(y/260+Phase)*30;
        private float TributaryAt(float x)=>-630+Mathf.Sin(x/580-Phase)*135;
        public bool Ford(Vector2 p)=>Mathf.Abs(p.y/240-Mathf.Round(p.y/240))*240<5 || Mathf.Abs(p.x/240-Mathf.Round(p.x/240))*240<5;
        public float WaterDistance(Vector2 p)
        {
            if(Underground(p))return CaveAt(p).Water(p) ? -2 : 15;
            var river=Mathf.Abs(p.x-RiverAt(p.y))-5;var tributary=Mathf.Abs(p.y-TributaryAt(p.x))-3.5f;
            var lake=(new Vector2((p.x-LakeCenter.x)/105,(p.y-LakeCenter.y)/70).magnitude-1)*70;
            var pond=(new Vector2((p.x+18)/7,(p.y-12)/5).magnitude-1)*5;
            return Mathf.Min(Mathf.Min(river,tributary),Mathf.Min(Mathf.Min(lake,pond),CoastDistance(p)));
        }
        public bool DeepWater(Vector2 p)=>Underground(p) ? CaveAt(p).Water(p) : WaterDistance(p)<-1.2f && !(Ford(p) && Mathf.Min(Mathf.Abs(p.x-RiverAt(p.y))-5,Mathf.Abs(p.y-TributaryAt(p.x))-3.5f)<0);
        public bool Walkable(Vector2 p,float margin=.7f)
        {
            if(!Contains(p))return false;if(Underground(p))return CaveAt(p).FloorDistance(p)<-margin && !DeepWater(p);
            if(CoastDistance(p)<margin || DeepWater(p))return false;
            var rock=RawEnvironment(p);return !(rock.Rockiness>.87f && rock.Roughness>.72f && Mathf.Abs(Mathf.Sin(p.x*.08f+p.y*.035f))<.45f);
        }
        internal EnvironmentSample RawEnvironment(Vector2 p)
        {
            var e=new EnvironmentSample{Moisture=WorldSeeds.Field(seed,SeedDomain.Terrain,p,700),Forestation=WorldSeeds.Field(seed,SeedDomain.Biome,p,520),Rockiness=WorldSeeds.Field(seed,SeedDomain.Terrain,p+Vector2.one*713,880),Height=WorldSeeds.Field(seed,SeedDomain.Continental,p,1300),Roughness=WorldSeeds.Field(seed,SeedDomain.Terrain,p+Vector2.one*251,180),WaterDistance=WaterDistance(p)};
            var safe=1-Mathf.SmoothStep(0,1,Mathf.Clamp01((p.magnitude-60)/100));e.Forestation=Mathf.Lerp(e.Forestation,.24f,safe);e.Rockiness=Mathf.Lerp(e.Rockiness,.24f,safe);e.Height=Mathf.Lerp(e.Height,.5f,safe);
            var forest=Mathf.Exp(-(p-ForestAnchor).sqrMagnitude/(170*170));e.Forestation=Mathf.Lerp(e.Forestation,.91f,forest);
            var rocky=Mathf.Exp(-(p-RockyAnchor).sqrMagnitude/(210*210));e.Rockiness=Mathf.Lerp(e.Rockiness,.94f,rocky);e.Forestation*=1-rocky*.68f;
            var river=Mathf.Clamp01(1-Mathf.Abs(p.x-RiverAt(p.y))/90);e.Moisture=Mathf.Lerp(e.Moisture,.94f,river*.85f);e.Height=Mathf.Lerp(e.Height,.18f,river*.7f);
            return e;
        }
        public EnvironmentSample Environment(Vector2 p)
        {
            if(Underground(p)){var cave=CaveAt(p);var room=cave.Nearest(p);return new EnvironmentSample{Rockiness=.9f,Height=.5f,Moisture=cave.Water(p) ? 1 : .35f,Tags=EnvironmentTag.CaveFloor|EnvironmentTag.CaveWall|(room.Depth>=.6f ? EnvironmentTag.DeepCave : 0)|(room.Depth>=.8f ? EnvironmentTag.CrystalPocket : 0),Biome=SurfaceBiome.Rocky};}
            var e=RawEnvironment(p);var w=Biomes.Sample(p);var selected=0;for(var i=1;i<4;i++)if(w[i]>w[selected])selected=i;e.Weights=w;e.Biome=(SurfaceBiome)selected;
            e.Tags=EnvironmentTag.GrassSurface;if(e.Forestation>.5f)e.Tags|=EnvironmentTag.ForestFloor;if(e.Rockiness>.58f)e.Tags|=EnvironmentTag.ExposedRock|EnvironmentTag.RockFace|EnvironmentTag.CoalSeam;if(e.Rockiness>.78f)e.Tags|=EnvironmentTag.RockRidge;
            if(e.WaterDistance>=0 && e.WaterDistance<18)e.Tags|=EnvironmentTag.RiverBank|EnvironmentTag.ShallowWaterEdge;
            // Alluvial gravel is tied to an actual flowing channel, not every wet biome or lake edge.
            if(e.WaterDistance>=1.1f && Mathf.Abs(p.x-RiverAt(p.y))<18 && e.Rockiness<.78f)e.Tags|=EnvironmentTag.RiverGravel;
            if(e.Moisture>.63f && e.Height<.52f)e.Tags|=EnvironmentTag.WetGround;return e;
        }
        public bool SourceAllowed(int rule,Vector2 p)
        {if(!Walkable(p,.6f) || p.magnitude<7 || RoadDistance(p)<2.5f)return false;var underground=Underground(p);var depth=underground ? CaveAt(p).Nearest(p).Depth : 0;var e=Environment(p);foreach(var s in data.NaturalSources)if(s.ResourceRule==rule && s.Matches(e,underground,depth))return true;return false;}
        public bool Renewable(int rule){foreach(var s in data.NaturalSources)if(s.Enabled && s.ResourceRule==rule && s.NaturalRecovery)return true;return false;}
        public List<ResourcePlacement> GenerateChunk(WorldChunk chunk)
        {
            var result=new List<ResourcePlacement>();var origin=chunk.Origin(data.ChunkSize);
            foreach(var p in GuaranteedResources)if(WorldChunk.At(p.Position,data.ChunkSize).Equals(chunk))result.Add(p);
            for(var phase=0;phase<2;phase++)for(var rule=0;rule<data.NaturalSources.Length;rule++)
            {
                var source=data.NaturalSources[rule];if(!source.Enabled || source.ResourceRule<0 || source.NaturalRecovery!=(phase==1))continue;var rng=new System.Random(WorldSeeds.Derive(seed,source.NaturalRecovery ? SeedDomain.Vegetation : SeedDomain.Resource,chunk.X,chunk.Y,WorldSeeds.StableText(source.SourceId)));
                for(var i=0;i<source.CandidatesPerChunk;i++)
                {var p=origin+new Vector2((float)rng.NextDouble(),(float)rng.NextDouble())*data.ChunkSize;var under=Underground(p);if(!Contains(p))continue;var e=Environment(p);var depth=under ? CaveAt(p).Nearest(p).Depth : 0;if(!source.Matches(e,under,depth) || !SourceAllowed(source.ResourceRule,p) || rng.NextDouble()>source.Probability)continue;
                    if(rng.NextDouble()>source.Density(e))continue;
                    var clear=true;foreach(var r in result)if((r.Position-p).sqrMagnitude<source.MinimumSpacing*source.MinimumSpacing){clear=false;break;}if(!clear)continue;
                    result.Add(new ResourcePlacement(source.ResourceRule,p,Mathf.Lerp(.9f,1.1f,(float)rng.NextDouble()),"c"+chunk.X+":"+chunk.Y+":"+source.SourceId+":"+i));}
            }
            return result;
        }
        private void GenerateAnchors()
        {
            var spacing=data.WalkingSpeed*60*data.PoiTravelMinutes;
            var n=Mathf.FloorToInt(Size.x*.42f/spacing);
            for(var y=-n;y<=n;y++)for(var x=-n;x<=n;x++)
            {
                var p=new Vector2((x+WorldSeeds.Unit(seed,SeedDomain.POI,x,y)*.5f)*spacing,(y+WorldSeeds.Unit(seed,SeedDomain.POI,x,y,1)*.5f)*spacing);
                if(p.magnitude<150 || CoastDistance(p)<120 || !Walkable(p,8) || WaterDistance(p)<15)continue;var e=Environment(p);
                var choice=WorldSeeds.Unit(seed,SeedDomain.POI,x,y,2);var kind=(e.Tags&EnvironmentTag.RockFace)!=0 ? choice<.6f ? WorldAnchorKind.Quarry : WorldAnchorKind.Mine : e.Forestation>.65f ? choice<.55f ? WorldAnchorKind.GoblinCamp : WorldAnchorKind.Graveyard : e.Moisture>.6f ? WorldAnchorKind.Well : choice<.5f ? WorldAnchorKind.Campsite : WorldAnchorKind.Ruin;
                Anchors.Add(new WorldAnchor{Id="poi"+x+":"+y,Kind=kind,Position=p,Important=(x+y)%4==0});
            }
            var count=Mathf.Max(2,Mathf.RoundToInt(Size.x*Size.y/Mathf.Pow(data.WalkingSpeed*60*18,2)));
            for(var i=0;i<count;i++)
            {
                var angle=i*Mathf.PI*2/count+WorldSeeds.Unit(seed,SeedDomain.Cave,0,0,1)*6.28f;var distance=i==0 ? 850 : 1500+WorldSeeds.Unit(seed,SeedDomain.Cave,i)*Size.x*.22f;var center=i==0 ? RockyAnchor+Vector2.up*60 : new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*distance;
                var p=EntranceCandidate(center,i%3!=2);
                var cave=new CavernNetwork(seed,i,new Vector2(Size.x*2+i*512,0),p,i%2==0);Caves.Add(cave);
                for(var j=0;j<cave.SurfaceEntrances.Count;j++){var e=cave.SurfaceEntrances[j];if(!Walkable(e,3) || !Walkable(e+Vector2.down*2,.4f)){cave.SurfaceEntrances[j]=EntranceCandidate(e,false);e=cave.SurfaceEntrances[j];}Anchors.Add(new WorldAnchor{Id="cave"+i+":"+j,Kind=WorldAnchorKind.CaveEntrance,Position=e,Important=true});}
            }
        }
        private Vector2 EntranceCandidate(Vector2 center,bool preferRock)
        {
            var found=false;var best=float.NegativeInfinity;var selected=center;
            for(var attempt=0;attempt<96;attempt++){var angle=attempt*2.399963f;var radius=Mathf.Sqrt(attempt)*22;var p=center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius;if(!Walkable(p,4) || !Walkable(p+Vector2.down*2,.4f) || WaterDistance(p)<10)continue;var score=(preferRock ? RawEnvironment(p).Rockiness*300 : 0)-radius*.15f;if(score<=best)continue;found=true;best=score;selected=p;}
            if(!found)throw new InvalidOperationException("No traversable cave entrance after bounded repair");return selected;
        }
        private void GenerateRoads()
        {
            AddRoad(new[]{Vector2.zero,ForestAnchor});AddRoad(new[]{Vector2.zero,new Vector2(RockyAnchor.x,0),RockyAnchor});
            AddRoad(new[]{Vector2.zero,new Vector2(RiverAt(0),0),new Vector2(RiverAt(0)+45,45)});
            foreach(var a in Anchors)if(a.Important && a.Kind!=WorldAnchorKind.GoblinCamp){var target=a.Position+new Vector2(100,35);AddRoad(new[]{a.Position+Vector2.down*12,a.Position+new Vector2(40,-30),target});}
        }
        private void AddRoad(Vector2[] points)
        {
            Roads.Add(points);for(var i=1;i<points.Length;i++){var a=WorldChunk.At(Vector2.Min(points[i-1],points[i])-Vector2.one*8,256);var b=WorldChunk.At(Vector2.Max(points[i-1],points[i])+Vector2.one*8,256);for(var y=a.Y;y<=b.Y;y++)for(var x=a.X;x<=b.X;x++){var key=new WorldChunk(x,y);if(!roadIndex.TryGetValue(key,out var list))roadIndex[key]=list=new List<Vector2[]>();list.Add(new[]{points[i-1],points[i]});}}
        }
        public bool TryRoadPoint(Vector2 p,float choice,out Vector2 position){if(roadIndex.TryGetValue(WorldChunk.At(p,256),out var list) && list.Count>0){var value=choice*list.Count;var segment=list[Mathf.Min(list.Count-1,(int)value)];position=Vector2.Lerp(segment[0],segment[1],value-Mathf.Floor(value));return true;}position=default;return false;}
        public float RoadDistance(Vector2 p){var best=1000f;if(roadIndex.TryGetValue(WorldChunk.At(p,256),out var list))foreach(var segment in list)best=Mathf.Min(best,WorldGeometry.SegmentDistance(p,segment[0],segment[1]));return best;}
    }
}
