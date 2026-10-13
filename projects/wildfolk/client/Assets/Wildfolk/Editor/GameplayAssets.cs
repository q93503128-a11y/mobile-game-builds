using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Wildfolk.Editor
{
    internal sealed class GameplayAssets
    {
        internal GameplaySettings Settings;
        internal VitalityDefinition PlayerVitality;
        internal VitalityDefinition EnemyVitality;
        internal ResourceDefinition Tree;
        internal ResourceDefinition Stone;
        internal InputActionAsset UIActions;
        internal InputActionReference Point;
        internal InputActionReference Click;
        private const string DataPath="Assets/Wildfolk/Data/";

        internal static GameplayAssets Load()
        {
            Directory.CreateDirectory(DataPath);
            AssetDatabase.Refresh();
            var assets=new GameplayAssets();
            assets.Settings=Asset<GameplaySettings>("GameplaySettings",s=>
            {
                s.MoveSpeed=4.2f; s.CameraSmoothTime=.18f;
                s.PunchDamage=10; s.PunchReach=1.6f; s.PunchDuration=.28f; s.PunchCooldown=.44f; s.PunchExtension=1.05f;
                s.EnemySpeed=2.2f; s.EnemyDetectionRange=4; s.EnemyLoseRange=8;
                s.EnemyStopDistance=1.35f; s.EnemySeparation=1.12f; s.EnemyAttackReach=1.55f;
                s.EnemyDamage=8; s.EnemyWindup=.38f; s.EnemyCooldown=1.15f;
                s.InventorySlots=16; s.PickupRadius=.95f; s.DropScatter=.85f; s.DropTravelTime=.3f; s.DropPickupDelay=.7f;
            });
            assets.PlayerVitality=Asset<VitalityDefinition>("PlayerVitality",v=>v.MaximumHealth=100);
            assets.EnemyVitality=Asset<VitalityDefinition>("EnemyVitality",v=>v.MaximumHealth=40);
            var wood=Asset<ItemDefinition>("Wood",i=>
            { i.Id="wood";i.DisplayName="Wood";i.MaxStack=50;i.Category=ItemCategory.Material;i.Icon=VisualArtImporter.Sprite("icon_wood"); });
            var stone=Asset<ItemDefinition>("Stone",i=>
            { i.Id="stone";i.DisplayName="Stone";i.MaxStack=50;i.Category=ItemCategory.Material;i.Icon=VisualArtImporter.Sprite("icon_stone"); });
            var treeHealth=Asset<VitalityDefinition>("TreeVitality",v=>v.MaximumHealth=30);
            var stoneHealth=Asset<VitalityDefinition>("StoneVitality",v=>v.MaximumHealth=50);
            assets.Tree=Asset<ResourceDefinition>("TreeResource",r=>{r.Vitality=treeHealth;r.DropItem=wood;r.DropCount=3;});
            assets.Stone=Asset<ResourceDefinition>("StoneResource",r=>{r.Vitality=stoneHealth;r.DropItem=stone;r.DropCount=3;});
            assets.LoadInput();
            AssetDatabase.SaveAssets();
            return assets;
        }

        private static T Asset<T>(string name,System.Action<T> initialize) where T:ScriptableObject
        {
            var path=DataPath+name+".asset";
            var asset=AssetDatabase.LoadAssetAtPath<T>(path);
            if(asset!=null) return asset;
            asset=ScriptableObject.CreateInstance<T>();
            initialize(asset);
            AssetDatabase.CreateAsset(asset,path);
            return asset;
        }

        private void LoadInput()
        {
            var path=DataPath+"UI.inputactions";
            if(!File.Exists(path))
            {
                var asset=ScriptableObject.CreateInstance<InputActionAsset>();
                var map=new InputActionMap("UI");
                var point=map.AddAction("Point",InputActionType.PassThrough,expectedControlLayout:"Vector2");
                point.AddBinding("<Mouse>/position"); point.AddBinding("<Pen>/position"); point.AddBinding("<Touchscreen>/touch*/position");
                var click=map.AddAction("Click",InputActionType.PassThrough,expectedControlLayout:"Button");
                click.AddBinding("<Mouse>/leftButton"); click.AddBinding("<Pen>/tip"); click.AddBinding("<Touchscreen>/touch*/press");
                asset.AddActionMap(map);
                File.WriteAllText(path,asset.ToJson());
                Object.DestroyImmediate(asset);
                AssetDatabase.ImportAsset(path);
            }
            UIActions=AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
            Point=Reference("UIPoint",UIActions.FindAction("UI/Point",true));
            Click=Reference("UIClick",UIActions.FindAction("UI/Click",true));
        }

        private static InputActionReference Reference(string name,InputAction action)
        {
            var path=DataPath+name+".asset";
            var reference=AssetDatabase.LoadAssetAtPath<InputActionReference>(path);
            if(reference!=null) return reference;
            reference=InputActionReference.Create(action);
            AssetDatabase.CreateAsset(reference,path);
            return reference;
        }
    }
}
