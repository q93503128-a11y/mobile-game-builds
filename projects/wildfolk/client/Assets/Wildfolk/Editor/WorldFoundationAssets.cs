using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Wildfolk.Editor
{
    public static class WorldFoundationAssets
    {
        public static Sprite[] PoiArt()=>Array.ConvertAll(new[]{"world_campsite","world_ruin","world_well","world_quarry","world_graveyard","world_mine","world_campsite","world_cave"},VisualArtImporter.Sprite);
        private static T Data<T>(string name)where T:UnityEngine.Object=>AssetDatabase.LoadAssetAtPath<T>("Assets/Wildfolk/Data/"+name+".asset");
        private static T Ref<T>(string text,string field)where T:UnityEngine.Object
        {var m=Regex.Match(text,@"\b"+field+@": \{fileID: [^,]+, guid: ([0-9a-f]{32})");return m.Success ? AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(m.Groups[1].Value)) : null;}
        public static void BuildBatch()
        {
            VisualArtImporter.Import();var world=Data<WorldGenerationDefinition>("WorldGeneration");world.ProgressionCatalog=Data<ContentCatalog>("ContentCatalog");world.WalkingSpeed=Data<GameplaySettings>("GameplaySettings").MoveSpeed;world.TravelMinutesAcross=52;world.ChunkSize=64;world.ChunkTexturePixels=128;world.PoiTravelMinutes=2.5f;world.ProgressionSafetyFactor=1.5f;
            var profiles=new List<SurfaceEnvironmentDefinition>();
            foreach(var biome in new[]{SurfaceBiome.Grassland,SurfaceBiome.Forest,SurfaceBiome.Wetlands,SurfaceBiome.Rocky})
            {
                var b=GameplayAssets.Asset<SurfaceEnvironmentDefinition>(biome+"Environment",v=>{});b.Biome=biome;b.MoistureWeight=b.ForestWeight=b.RockWeight=b.HeightWeight=1;
                b.PreferredMoisture=new Vector2(.25f,.65f);b.PreferredForestation=new Vector2(.15f,.48f);b.PreferredRockiness=new Vector2(.1f,.5f);b.PreferredHeight=new Vector2(.3f,.7f);
                if(biome==SurfaceBiome.Grassland){b.Ground=new Color32(116,130,72,255);b.DenseGround=new Color32(91,112,65,255);}
                if(biome==SurfaceBiome.Forest){b.PreferredForestation=new Vector2(.66f,1);b.ForestWeight=2.3f;b.Ground=new Color32(73,91,53,255);b.DenseGround=new Color32(51,73,47,255);}
                if(biome==SurfaceBiome.Wetlands){b.PreferredMoisture=new Vector2(.7f,1);b.PreferredHeight=new Vector2(0,.4f);b.MoistureWeight=1.7f;b.HeightWeight=1.8f;b.Ground=new Color32(81,102,78,255);b.DenseGround=new Color32(58,83,69,255);}
                if(biome==SurfaceBiome.Rocky){b.PreferredRockiness=new Vector2(.7f,1);b.RockWeight=2.4f;b.Ground=new Color32(118,118,100,255);b.DenseGround=new Color32(94,103,83,255);}
                profiles.Add(b);EditorUtility.SetDirty(b);
            }
            world.SurfaceEnvironments=profiles.ToArray();var sources=new List<NaturalSourceDefinition>();
            var tags=new[]{EnvironmentTag.GrassSurface|EnvironmentTag.ForestFloor,EnvironmentTag.GrassSurface|EnvironmentTag.ExposedRock,EnvironmentTag.GrassSurface|EnvironmentTag.RockFace,EnvironmentTag.ForestFloor|EnvironmentTag.GrassSurface,EnvironmentTag.GrassSurface|EnvironmentTag.WetGround,EnvironmentTag.RockFace,EnvironmentTag.RiverGravel,EnvironmentTag.CoalSeam};
            var candidates=new[]{42,48,4,3,8,18,8,18};var probabilities=new[]{1f,.8f,.22f,.7f,.8f,.55f,.8f,.6f};
            for(var i=0;i<world.Resources.Length;i++)
            {
                var node=world.Resources[i].Prefab.Definition;var path=AssetDatabase.GetAssetPath(node);var text=File.ReadAllText(path);
                if(!text.Contains("\n  Harvest:"))node.Harvest=new HarvestDefinition{MinimumTier=Ref<EquipmentTier>(text,"MinimumTier"),RequiredToolItem=Ref<ItemDefinition>(text,"RequiredToolItem"),RequiredTool=Regex.IsMatch(text,@"RequiredTool: (\d+)") ? (ToolKind)int.Parse(Regex.Match(text,@"RequiredTool: (\d+)").Groups[1].Value) : ToolKind.Pickaxe,Method=text.Contains("HarvestMethod: 1") ? HarvestMethod.Panning : HarvestMethod.Strike};
                node.Harvest.Mode=node.HarvestMethod==HarvestMethod.Panning ? HarvestMode.Pan : world.Resources[i].Family==ResourceFamily.Plant ? HarvestMode.Gather : node.RequiredTool==ToolKind.Axe ? HarvestMode.Chop : HarvestMode.Mine;EditorUtility.SetDirty(node);
                var source=GameplayAssets.Asset<NaturalSourceDefinition>("Source"+i,v=>{});source.SourceId="surface_"+node.DropItem.Id;source.ResourceRule=i;source.AnyTags=tags[i];source.Context=ResourceSourceContext.Surface;source.CandidatesPerChunk=candidates[i];source.Probability=probabilities[i];source.MinimumSpacing=world.Resources[i].Spacing;source.VegetationStream=world.Resources[i].Family!=ResourceFamily.Mineral;source.NaturalRecovery=true;source.DepthRange=new Vector2(0,1);source.Enabled=true;EditorUtility.SetDirty(source);sources.Add(source);
                source.StarterSupply=i==2 ? 3 : 0;
                source.DensityField=i==0 ? SourceDensityField.Forestation : i==1 ? SourceDensityField.Rockiness : SourceDensityField.None;source.DensityRange=i==0 ? new Vector2(.12f,.95f) : new Vector2(.1f,1);
                if(world.Resources[i].Family==ResourceFamily.Mineral && node.HarvestMethod!=HarvestMethod.Panning){var cave=GameplayAssets.Asset<NaturalSourceDefinition>("CaveSource"+i,v=>{});cave.SourceId="cave_"+node.DropItem.Id;cave.ResourceRule=i;cave.AnyTags=EnvironmentTag.CaveWall|EnvironmentTag.ExposedRock;cave.Context=ResourceSourceContext.Cave;cave.CandidatesPerChunk=36;cave.Probability=.85f;cave.MinimumSpacing=4;cave.DepthRange=new Vector2(i==5 ? .25f : 0,1);cave.VegetationStream=false;cave.NaturalRecovery=true;EditorUtility.SetDirty(cave);sources.Add(cave);}
            }
            var future=GameplayAssets.Asset<NaturalSourceDefinition>("DiamondPocketSource",v=>{});future.SourceId="diamond_pocket";future.ResourceRule=-1;future.Context=ResourceSourceContext.Cave;future.AnyTags=EnvironmentTag.CrystalPocket|EnvironmentTag.DeepCave;future.DepthRange=new Vector2(.8f,1);future.Enabled=false;future.NaturalRecovery=false;EditorUtility.SetDirty(future);sources.Add(future);world.NaturalSources=sources.ToArray();EditorUtility.SetDirty(world);
            var player=PrefabUtility.LoadPrefabContents(MultiplayerSceneBuilder.PlayerPrefabPath);if(player.GetComponent<NetworkRegionTravel>()==null)player.AddComponent<NetworkRegionTravel>();PrefabUtility.SaveAsPrefabAsset(player,MultiplayerSceneBuilder.PlayerPrefabPath);PrefabUtility.UnloadPrefabContents(player);
            var session=PrefabUtility.LoadPrefabContents(MultiplayerSceneBuilder.SessionPrefabPath);session.GetComponent<Unity.Netcode.NetworkManager>().NetworkConfig.ProtocolVersion=NetworkProtocol.Current;PrefabUtility.SaveAsPrefabAsset(session,MultiplayerSceneBuilder.SessionPrefabPath);PrefabUtility.UnloadPrefabContents(session);
            foreach(var scenePath in new[]{GameplaySceneBuilder.ScenePath,MultiplayerSceneBuilder.ScenePath}){var scene=EditorSceneManager.OpenScene(scenePath);var generator=UnityEngine.Object.FindFirstObjectByType<WorldGenerator>(FindObjectsInactive.Include);generator.PoiSprites=PoiArt();EditorUtility.SetDirty(generator);EditorSceneManager.SaveScene(scene);}
            AssetDatabase.SaveAssets();ProjectDataValidator.Run();
        }
    }
}
