using System;
using UnityEngine;
namespace Wildfolk
{
    [Flags]public enum EnvironmentTag
    {None=0,GrassSurface=1,ForestFloor=2,RockFace=4,ExposedRock=8,RockRidge=16,RiverBank=32,RiverGravel=64,WetGround=128,ShallowWaterEdge=256,CaveFloor=512,CaveWall=1024,DeepCave=2048,CrystalPocket=4096,UndergroundWater=8192,CoalSeam=16384}
    public enum HarvestMode { Gather,Chop,Mine,Pan,Interact }
    public enum SourceDensityField { None,Forestation,Rockiness }
    [Serializable]public sealed class HarvestDefinition
    {
        public HarvestMode Mode;
        public HarvestMethod Method;
        public EquipmentTier MinimumTier;
        public ToolKind RequiredTool;
        public ItemDefinition RequiredToolItem;
        public bool Allows(ToolDefinition tool)=>(tool==null ? Method==HarvestMethod.Strike : tool.HarvestMethod==Method) && (MinimumTier==null || tool!=null && tool.Tier!=null && tool.Tier.Rank>=MinimumTier.Rank && tool.Kind==RequiredTool);
    }
    [CreateAssetMenu(menuName="Wildfolk/Natural Source")]
    public sealed class NaturalSourceDefinition : ScriptableObject
    {
        public string SourceId;
        public int ResourceRule=-1;
        public EnvironmentTag AnyTags;
        public ResourceSourceContext Context;
        public Vector2 DepthRange=new Vector2(0,1),MoistureRange=new Vector2(0,1),RockRange=new Vector2(0,1);
        [Min(0)]public int CandidatesPerChunk=4;
        [Range(0,1)]public float Probability=1;
        public SourceDensityField DensityField;
        public Vector2 DensityRange=new Vector2(1,1);
        [Min(1)]public float MinimumSpacing=4;
        [Min(0)]public int StarterSupply;
        // Generation stream is independent of renewal policy so old c IDs keep their positions.
        public bool VegetationStream;
        public bool NaturalRecovery;
        public bool Enabled=true;
        public SurfaceBiome[] SurfaceBiomes=Array.Empty<SurfaceBiome>();
        public bool Matches(EnvironmentSample e,bool underground,float depth)
            =>Enabled && (SurfaceBiomes==null || SurfaceBiomes.Length==0 || Array.IndexOf(SurfaceBiomes,e.Biome)>=0) && ResourceRule>=0 && (Context==ResourceSourceContext.Cave)==underground && (AnyTags==EnvironmentTag.None || (e.Tags&AnyTags)!=0) && depth>=DepthRange.x && depth<=DepthRange.y && e.Moisture>=MoistureRange.x && e.Moisture<=MoistureRange.y && e.Rockiness>=RockRange.x && e.Rockiness<=RockRange.y;
        public float Density(EnvironmentSample e)=>DensityField==SourceDensityField.None ? 1 : Mathf.Lerp(DensityRange.x,DensityRange.y,DensityField==SourceDensityField.Forestation ? e.Forestation : e.Rockiness);
    }
    public enum SurfaceBiome { Grassland,Forest,Wetlands,Rocky }
    public struct EnvironmentSample
    {
        public float Moisture,Forestation,Rockiness,Height,Roughness,WaterDistance;
        public EnvironmentTag Tags;
        public SurfaceBiome Biome;
        public Vector4 Weights;
    }
}
