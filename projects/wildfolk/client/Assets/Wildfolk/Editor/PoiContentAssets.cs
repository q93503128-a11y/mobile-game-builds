using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
namespace Wildfolk.Editor
{
    public static class PoiContentAssets
    {
        private static T D<T>(string n)where T:UnityEngine.Object=>AssetDatabase.LoadAssetAtPath<T>("Assets/Wildfolk/Data/"+n+".asset");
        private static ItemAmount Item(ContentCatalog c,string id,int count)=>new ItemAmount{Item=c.Item(id),Count=count};
        public static void Build()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play first");
            var c=D<ContentCatalog>("ContentCatalog");var world=D<WorldGenerationDefinition>("WorldGeneration");
            var sourcePath="Assets/Wildfolk/Prefabs/PoiSite.prefab";var netPath="Assets/Wildfolk/Prefabs/NetworkPoiSite.prefab";
            var camp=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/GoblinCamp.prefab").GetComponent<GoblinCamp>();
            var root=new GameObject("Expedition site");var site=root.AddComponent<PoiSite>();
            var visual=new GameObject("Supplies");visual.transform.SetParent(root.transform,false);visual.transform.localPosition=Vector3.right*2;
            site.Visual=visual.AddComponent<SpriteRenderer>();site.Visual.sprite=site.ClosedSprite=camp.SupplyIcon;site.Visual.sharedMaterial=WorldArt.SpriteMaterial??AssetDatabase.LoadAssetAtPath<Material>("Assets/Wildfolk/Art/WorldSprite.mat");site.Visual.sortingOrder=WorldSorting.GroundDecoration+2;
            site.OpenedSprite=camp.OpenedSprite;site.WaterIcon=c.Item("clean_water_cup").Icon;
            var trigger=root.AddComponent<CircleCollider2D>();trigger.isTrigger=true;trigger.radius=.75f;trigger.offset=Vector2.right*2;
            var source=PrefabUtility.SaveAsPrefabAsset(root,sourcePath);root.AddComponent<NetworkObject>();root.AddComponent<NetworkPoiSite>();var net=PrefabUtility.SaveAsPrefabAsset(root,netPath);UnityEngine.Object.DestroyImmediate(root);
            var config=D<PoiContentDefinition>("PoiContent");if(config==null){config=ScriptableObject.CreateInstance<PoiContentDefinition>();AssetDatabase.CreateAsset(config,"Assets/Wildfolk/Data/PoiContent.asset");}
            config.Prefab=source.GetComponent<PoiSite>();config.WellCapacity=3;config.WellRefillSeconds=480;
            config.Campsite=new[]{new PoiSupplyVariant{Items=new[]{Item(c,"torch",2),Item(c,"cooked_meat",1),Item(c,"arrow",4)}},new PoiSupplyVariant{Items=new[]{Item(c,"torch",3),Item(c,"cooked_fish",1),Item(c,"field_dressing",1)}},new PoiSupplyVariant{Items=new[]{Item(c,"torch",1),Item(c,"trail_ration",1),Item(c,"clean_water_cup",1)}}};
            config.Graveyard=new[]{Item(c,"bone",3),Item(c,"medicinal_herb",1),Item(c,"field_dressing",1)};
            var stone=Array.FindIndex(world.Resources,r=>r.Prefab.Definition.DropItem.Id=="stone");var copper=Array.FindIndex(world.Resources,r=>r.Prefab.Definition.DropItem.Id=="copper_ore");var coal=Array.FindIndex(world.Resources,r=>r.Prefab.Definition.DropItem.Id=="coal");config.QuarryRules=new[]{stone,stone,copper,coal};world.PoiContent=config;EditorUtility.SetDirty(world);EditorUtility.SetDirty(config);
            var ecology=PrefabUtility.LoadPrefabContents(NetworkEcologyBuilder.PrefabPath);var host=ecology.GetComponent<NetworkCreatureWorld>();var entries=new List<NetworkSpawnEntry>(host.Prefabs);entries.RemoveAll(e=>e.Source==source);entries.Add(new NetworkSpawnEntry{Source=source,NetworkPrefab=net});host.Prefabs=entries.ToArray();PrefabUtility.SaveAsPrefabAsset(ecology,NetworkEcologyBuilder.PrefabPath);PrefabUtility.UnloadPrefabContents(ecology);
            var session=PrefabUtility.LoadPrefabContents(MultiplayerSceneBuilder.SessionPrefabPath);session.GetComponent<NetworkManager>().NetworkConfig.ProtocolVersion=NetworkProtocol.Current;PrefabUtility.SaveAsPrefabAsset(session,MultiplayerSceneBuilder.SessionPrefabPath);PrefabUtility.UnloadPrefabContents(session);
            foreach(var name in new[]{"BoneTrap","NetworkBoneTrap"}){var path="Assets/Wildfolk/Prefabs/"+name+".prefab";var trap=PrefabUtility.LoadPrefabContents(path);trap.GetComponent<SpikeTrap>().SingleUse=true;trap.GetComponent<SpikeTrap>().Damage=32;PrefabUtility.SaveAsPrefabAsset(trap,path);PrefabUtility.UnloadPrefabContents(trap);}
            AssetDatabase.SaveAssets();
        }
    }
}
