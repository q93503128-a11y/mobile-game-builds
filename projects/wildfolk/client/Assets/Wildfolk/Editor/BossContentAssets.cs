using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using static Wildfolk.Editor.WorldArt;
using static Wildfolk.Editor.GameplayUiFactory;
using Object=UnityEngine.Object;
namespace Wildfolk.Editor
{
    public static class BossContentAssets
    {
        private const string Base="Assets/Wildfolk/";
        private static T Data<T>(string name)where T:Object=>AssetDatabase.LoadAssetAtPath<T>(Base+"Data/"+name+".asset");
        private static T Asset<T>(string name)where T:ScriptableObject
        {var p=Base+"Data/"+name+".asset";var d=AssetDatabase.LoadAssetAtPath<T>(p);if(d!=null)return d;d=ScriptableObject.CreateInstance<T>();AssetDatabase.CreateAsset(d,p);return d;}
        [MenuItem("Wildfolk/Content/Author Boss Foundation")]
        public static void Author()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play first");
            VisualArtImporter.Import();SpriteMaterial=AssetDatabase.LoadAssetAtPath<Material>(Base+"Art/WorldSprite.mat");
            var d=Asset<BossDefinition>("UndeadKing");d.Id="undead_king";d.NameKey="boss.undead_king";
            d.ClearingRadius=7;
            d.Vitality=Asset<VitalityDefinition>("UndeadKingVitality");d.Vitality.MaximumHealth=9000;
            d.Minion=AssetDatabase.LoadAssetAtPath<GameObject>(Base+"Prefabs/Skeleton.prefab").GetComponent<EnemyController>();d.Offering=Data<ItemDefinition>("Bone");
            d.MinionRewards=Asset<LootTable>("BossSummonLoot");d.MinionRewards.Items=d.MinionRewards.RareItems=Array.Empty<LootEntry>();d.MinionRewards.Experience=d.MinionRewards.Gold=0;
            var core=Asset<ItemDefinition>("BossCoreT1");core.Id="boss_core_t1";core.DisplayName="Tier 1 Boss Core";core.Category=ItemCategory.Material;core.MaxStack=20;core.Icon=VisualArtImporter.Sprite(core.Id);
            var crown=Asset<ItemDefinition>("UndeadKingsCrown");crown.Id="undead_kings_crown";crown.DisplayName="Undead King's Crown";crown.Category=ItemCategory.Accessory;crown.MaxStack=1;crown.Icon=VisualArtImporter.Sprite(crown.Id);
            var accessory=Asset<AccessoryDefinition>("LastCrown");accessory.Visual=crown.Icon;accessory.Anchor=AccessoryAnchor.AboveHead;accessory.Threshold=.35f;accessory.MovementBonus=.08f;accessory.DamageReduction=.10f;crown.Accessory=accessory;
            var catalog=Data<ContentCatalog>("ContentCatalog");var items=new List<ItemDefinition>(catalog.Items);if(!items.Contains(core))items.Add(core);if(!items.Contains(crown))items.Add(crown);catalog.Items=items.ToArray();
            d.Rewards=Asset<LootTable>("UndeadKingLoot");d.Rewards.Experience=120;d.Rewards.Gold=35;
            d.Rewards.Items=new[]{Loot(core,1,1),Loot(Data<ItemDefinition>("Bone"),14,20),Loot(Data<ItemDefinition>("IronIngot"),2,4),Loot(Data<ItemDefinition>("Steel"),1,2)};
            d.Rewards.RareItems=new[]{new LootEntry{Item=crown,Minimum=1,Maximum=1,Chance=.2f}};
            d.Attacks=new[]{
                new BossAttack{Id="royal_cleave",Damage=26,Windup=.8f,Recovery=.7f,Range=2.8f,Angle=110,FollowUp=3},
                new BossAttack{Id="execution",Shape=BossAttackShape.Line,Damage=41,Windup=1.15f,Recovery=1.2f,Range=3.5f,Width=1.25f},
                new BossAttack{Id="kings_march",Damage=21,Windup=.75f,Recovery=1.3f,Range=2.7f,Angle=100,Repeats=3,Advance=.5f,FollowUp=1},
                new BossAttack{Id="grave_cleave",Shape=BossAttackShape.Line,Damage=26,Windup=.95f,Recovery=.9f,Range=7,Width=1.35f,FirstPhase=2},
                new BossAttack{Id="crownfall",Shape=BossAttackShape.Circle,Damage=36,Windup=1.3f,Recovery=1.5f,Range=3.5f,FirstPhase=3}};
            d.Schedule=new[]{0,1,0,2,3,0,4};
            Tomb(d);Actor(d);NetworkActor(d);
            foreach(var obj in new Object[]{d,d.Vitality,d.Rewards,d.MinionRewards,core,crown,accessory,catalog})EditorUtility.SetDirty(obj);
            var rules=AssetDatabase.LoadAssetAtPath<SaveGameRules>(Base+"Resources/SaveGameRules.asset");rules.Bosses=new[]{d};EditorUtility.SetDirty(rules);AssetDatabase.SaveAssets();
            foreach(var name in new[]{"Player","NetworkPlayer"})PlayerArt(name);
            var ecology=PrefabUtility.LoadPrefabContents(NetworkEcologyBuilder.PrefabPath);World(ecology.GetComponent<WorldGenerator>(),d);
            if(ecology.GetComponent<NetworkBossWorld>()==null)ecology.AddComponent<NetworkBossWorld>();
            var network=ecology.GetComponent<NetworkCreatureWorld>();var entries=network.Prefabs.ToList();var source=d.Prefab.gameObject;
            if(!entries.Any(e=>e.Source==source))entries.Add(new NetworkSpawnEntry{Source=source,NetworkPrefab=AssetDatabase.LoadAssetAtPath<GameObject>(Base+"Prefabs/NetworkUndeadKing.prefab")});network.Prefabs=entries.ToArray();
            PrefabUtility.SaveAsPrefabAsset(ecology,NetworkEcologyBuilder.PrefabPath);PrefabUtility.UnloadPrefabContents(ecology);
            var session=PrefabUtility.LoadPrefabContents(MultiplayerSceneBuilder.SessionPrefabPath);session.GetComponent<NetworkManager>().NetworkConfig.ProtocolVersion=NetworkProtocol.Current;PrefabUtility.SaveAsPrefabAsset(session,MultiplayerSceneBuilder.SessionPrefabPath);PrefabUtility.UnloadPrefabContents(session);
            foreach(var name in new[]{"Wildfolk","Multiplayer"})
            {
                var scene=EditorSceneManager.OpenScene(Base+"Scenes/"+name+".unity");
                var world=Object.FindFirstObjectByType<WorldGenerator>(FindObjectsInactive.Include);World(world,d);
                if(name=="Multiplayer" && world.GetComponent<NetworkBossWorld>()==null)world.gameObject.AddComponent<NetworkBossWorld>();
                foreach(var ui in Object.FindObjectsByType<InventoryEquipment>(FindObjectsInactive.Include,FindObjectsSortMode.None))EquipmentUi(ui);
                var boss=Object.FindFirstObjectByType<BossHud>(FindObjectsInactive.Include);
                var canvas=Object.FindFirstObjectByType<Canvas>();var safe=canvas.transform.Find("Safe Area");
                if(boss==null){GameplayHudBuilder.BuildBoss(safe);}
                if(Object.FindFirstObjectByType<TimeHud>(FindObjectsInactive.Include)==null)
                {
                    var panel=Image("World Time",safe,"panel",new Vector2(0,1),new Vector2(148,-229),new Vector2(232,44));var hud=panel.gameObject.AddComponent<TimeHud>();hud.Clock=world.GetComponent<WorldClock>();
                    hud.Icon=Image("Time Icon",panel.transform,"sun",Vector2.one*.5f,new Vector2(-92,0),new Vector2(30,30));hud.Label=Text("Time",panel.transform,"",new Vector2(19,0),new Vector2(172,36),20);
                }
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            }
            EditorSceneManager.OpenScene(Base+"Scenes/MainMenu.unity");AssetDatabase.SaveAssets();
        }
        private static LootEntry Loot(ItemDefinition item,int min,int max)=>new LootEntry{Item=item,Minimum=min,Maximum=max,Chance=1};
        private static void World(WorldGenerator world,BossDefinition definition)
        {var b=world.GetComponent<BossWorld>()??world.gameObject.AddComponent<BossWorld>();b.World=world;b.Clock=world.GetComponent<WorldClock>();b.Navigation=world.GetComponent<WorldNavigation>();b.Definitions=new[]{definition};}
        private static void Tomb(BossDefinition d)
        {
            var go=new GameObject("Royal Tomb");go.SetActive(false);var tomb=go.AddComponent<RoyalTomb>();tomb.Definition=d;
            Sprite("Royal stonework","royal_tomb",go.transform,Vector2.zero,1,0);
            tomb.Ember=Sprite("Tomb glow","boss_core_t1",go.transform,new Vector2(0,.1f),.5f,1);tomb.Ember.color=new Color(.4f,.6f,.5f,0);
            tomb.Lid=Sprite("Royal slab","royal_tomb_lid",go.transform,Vector2.zero,1,2);
            var solid=new GameObject("Stone footprint");solid.transform.SetParent(go.transform,false);var box=solid.AddComponent<BoxCollider2D>();box.size=new Vector2(2.8f,.85f);box.offset=new Vector2(0,-.4f);
            var trigger=go.AddComponent<CircleCollider2D>();trigger.isTrigger=true;trigger.radius=1.5f;
            var group=go.AddComponent<SortingGroup>();var depth=go.AddComponent<DepthSort>();depth.Visual=group;go.SetActive(true);
            d.Tomb=PrefabUtility.SaveAsPrefabAsset(go,Base+"Prefabs/RoyalTomb.prefab").GetComponent<RoyalTomb>();Object.DestroyImmediate(go);
        }
        private static void Actor(BossDefinition d)
        {
            var go=new GameObject("Undead King");go.SetActive(false);
            var health=go.AddComponent<Health>();health.Definition=d.Vitality;var body=go.AddComponent<Rigidbody2D>();body.gravityScale=0;body.constraints=RigidbodyConstraints2D.FreezeRotation;
            var collider=go.AddComponent<CircleCollider2D>();collider.radius=.65f;go.AddComponent<DamageProtection>();go.AddComponent<CombatAffiliation>().Relation=EntityRelation.Hostile;
            var e=go.AddComponent<BossEncounter>();e.Definition=d;e.Health=health;go.AddComponent<BossIdentity>().DisplayName="Undead King";
            var group=Group("Body",go.transform);var sorting=group.gameObject.AddComponent<SortingGroup>();go.AddComponent<DepthSort>().Visual=sorting;
            Shadow(group,1.6f,.65f);var pose=Group("Segmented body",group);var visual=go.AddComponent<UndeadKingVisual>();visual.Encounter=e;visual.Pose=pose;
            var parts=new List<SpriteRenderer>();parts.Add(Sprite("Ruined cloak","king_cloak",pose,new Vector2(-.04f,.15f),.7f,0));
            var legs=Sprite("Legs","king_legs",pose,new Vector2(0,-.4f),.35f,1);visual.Legs=legs.transform;parts.Add(legs);
            parts.Add(Sprite("Heavy chest","king_torso",pose,new Vector2(0,.4f),.54f,3));
            var off=Group("Off arm",pose,-.5f,.4f);parts.Add(Sprite("Off gauntlet","king_arm",off,new Vector2(0,-.3f),.3f,2));visual.OffArm=off;
            var swordArm=Group("Sword arm",pose,.55f,.4f);parts.Add(Sprite("Sword gauntlet","king_arm",swordArm,new Vector2(0,-.15f),.3f,5));visual.SwordArm=swordArm;
            var sword=Sprite("Oversized sword","king_sword",swordArm,new Vector2(0,.75f),.8f,6);visual.Sword=sword.transform;parts.Add(sword);
            parts.Add(Sprite("Skull","king_head",pose,new Vector2(0,1.05f),.33f,4));
            visual.Crown=Sprite("Broken royal crown","king_crown",pose,new Vector2(0,1.5f),.43f,7);parts.Add(visual.Crown);visual.WholeCrown=VisualArtImporter.Sprite("king_crown");visual.BrokenCrown=VisualArtImporter.Sprite("king_crown_broken");visual.Parts=parts.ToArray();
            visual.Dust=Sprite("Fading ash","king_ash",group,new Vector2(0,1.6f),.5f,8);visual.Dust.enabled=false;
            visual.Skull=Sprite("Skull mark","skull_mark",go.transform,Vector2.zero,.27f,WorldSorting.Overlay+1);visual.Skull.enabled=false;
            var tell=go.AddComponent<BossTelegraph>();tell.Encounter=e;var edge=Group("Strike boundary",go.transform).gameObject.AddComponent<LineRenderer>();edge.sharedMaterial=SpriteMaterial;edge.useWorldSpace=false;edge.loop=true;edge.widthMultiplier=.065f;edge.sortingOrder=WorldSorting.GroundDecoration+10;tell.Edge=edge;
            tell.Crack=Sprite("Ground impact","grave_crack",go.transform,Vector2.zero,1,WorldSorting.GroundDecoration+11);tell.Crack.enabled=false;
            go.SetActive(true);d.Prefab=PrefabUtility.SaveAsPrefabAsset(go,Base+"Prefabs/UndeadKing.prefab").GetComponent<BossEncounter>();Object.DestroyImmediate(go);
        }
        private static void NetworkActor(BossDefinition d)
        {
            var go=(GameObject)PrefabUtility.InstantiatePrefab(d.Prefab.gameObject);go.name="Network Undead King";go.AddComponent<NetworkObject>();var t=go.AddComponent<NetworkTransform>();t.SyncPositionZ=false;t.SyncRotAngleX=t.SyncRotAngleY=t.SyncRotAngleZ=false;t.SyncScaleX=t.SyncScaleY=t.SyncScaleZ=false;t.PositionThreshold=.04f;t.UseUnreliableDeltas=true;
            go.AddComponent<NetworkRigidbody2D>();go.AddComponent<NetworkBoss>();PrefabUtility.SaveAsPrefabAsset(go,Base+"Prefabs/NetworkUndeadKing.prefab");Object.DestroyImmediate(go);
        }
        private static void PlayerArt(string name)
        {
            var path=Base+"Prefabs/"+name+".prefab";var root=PrefabUtility.LoadPrefabContents(path);var e=root.GetComponent<PlayerEquipment>();
            var visual=root.GetComponent<AccessoryVisual>();if(visual==null){visual=root.AddComponent<AccessoryVisual>();visual.Equipment=e;visual.Renderer=Sprite("Accessory","undead_kings_crown",e.HeadVisual.transform.parent,Vector2.zero,.4f,e.HeadVisual.sortingOrder+2);visual.Renderer.enabled=false;}
            PrefabUtility.SaveAsPrefabAsset(root,path);PrefabUtility.UnloadPrefabContents(root);
        }
        private static void EquipmentUi(InventoryEquipment ui)
        {
            if(ui.AccessoryButton==null)
            {
                var clone=Object.Instantiate(ui.HeadButton.gameObject,ui.HeadButton.transform.parent);clone.name="Accessory";ui.AccessoryButton=clone.GetComponent<Button>();ui.AccessoryButton.onClick=new Button.ButtonClickedEvent();UnityEventTools.AddIntPersistentListener(ui.AccessoryButton.onClick,ui.SelectSlot,3);
                ui.AccessoryIcon=clone.transform.Find(AnimationUtility.CalculateTransformPath(ui.HeadIcon.transform,ui.HeadButton.transform)).GetComponent<Image>();ui.AccessoryIcon.sprite=null;clone.GetComponent<EquipmentSlotDrag>().Slot=ArmorSlot.Accessory;
            }
            var buttons=new[]{ui.HeadButton,ui.BodyButton,ui.UtilityButton,ui.AccessoryButton};
            var labels=ui.HeadButton.transform.parent.GetComponentsInChildren<LocalizedLabel>(true);
            for(var i=0;i<4;i++)
            {
                var x=-640+(i%2)*155;var y=-70-(i/2)*150;var rect=(RectTransform)buttons[i].transform;rect.anchoredPosition=new Vector2(x,y);rect.sizeDelta=new Vector2(88,88);
                var key="ui.slot."+new[]{"Head","Body","Utility","Accessory"}[i];var old=Array.Find(labels,l=>l.Key==key);
                var label=old==null?Text("Slot Name",ui.HeadButton.transform.parent,key,new Vector2(x,y-63),new Vector2(104,32),18):old.GetComponent<Text>();label.rectTransform.anchoredPosition=new Vector2(x,y-63);label.rectTransform.sizeDelta=new Vector2(122,32);
            }
            var hint=Array.Find(labels,l=>l.Key=="ui.inventory.slot_hint");if(hint!=null){var text=hint.GetComponent<Text>();text.rectTransform.anchoredPosition=new Vector2(-554,-330);text.rectTransform.sizeDelta=new Vector2(290,48);text.fontSize=18;}
        }
    }
}
