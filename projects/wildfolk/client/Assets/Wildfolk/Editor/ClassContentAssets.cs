using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using static Wildfolk.Editor.GameplayUiFactory;
namespace Wildfolk.Editor
{
    public static class ClassContentAssets
    {
        public static void Install()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play first");
            VisualArtImporter.Import();var catalog=Resources.Load<SaveGameRules>("SaveGameRules").Catalog;
            foreach(var item in catalog.Items)
            {
                item.ExplorationCategory=item.Telescope?ExplorationToolCategory.Optic:item.ExplorationUtility==null?ExplorationToolCategory.None:item.ExplorationUtility.Kind==ExplorationUtilityKind.Survey?ExplorationToolCategory.Survey:ExplorationToolCategory.Signal;
                if(item.ExplorationCategory!=ExplorationToolCategory.None)EditorUtility.SetDirty(item);
                if(item.Placement==null)continue;var d=item.Placement;
                var recipe=Array.Find(catalog.Recipes,r=>r.Result==item);d.RecoverPortable=recipe!=null && recipe.ResultCount>1;
                if(recipe!=null)d.ConstructionMaterials=recipe.Ingredients.Select(c=>new ItemAmount{Item=c.Item,Count=Mathf.Max(1,c.Count/recipe.ResultCount)}).ToArray();
            }
            // An upgraded bench returns base construction + each upgrade exactly once.
            foreach(var item in catalog.Items)if(item.Placement?.UpgradeTo!=null)
            {
                var root=item.Placement;if(root.WorkbenchTier!=1)continue;
                for(var d=root;d.UpgradeTo!=null;d=d.UpgradeTo)
                {var totals=d.ConstructionMaterials.Concat(d.UpgradeCosts).GroupBy(c=>c.Item).Select(g=>new ItemAmount{Item=g.Key,Count=g.Sum(c=>c.Count)}).ToArray();d.UpgradeTo.ConstructionMaterials=totals;}
            }
            var audit=new System.Text.StringBuilder("item\tfull repair cost\tconstruction refund\tportable\n");
            foreach(var item in catalog.Items)if(item.Placement!=null)
            {
                var d=item.Placement;d.RepairMaterials=d.ConstructionMaterials.Where(c=>c.Item.Id=="wood" || c.Item.Id=="stone" || c.Item.Id=="iron_ingot" || c.Item.Id=="bronze_ingot" || c.Item.Id=="steel").Select(c=>new ItemAmount{Item=c.Item,Count=Mathf.Max(1,Mathf.CeilToInt(c.Count*.25f))}).ToArray();
                if(d.RepairMaterials.Length==0)d.RepairMaterials=new[]{new ItemAmount{Item=catalog.Item("wood"),Count=1}};
                EditorUtility.SetDirty(d);audit.Append(item.Id).Append('\t').Append(string.Join(" + ",d.RepairMaterials.Select(c=>c.Item.Id+":"+c.Count))).Append('\t').Append(string.Join(" + ",d.ConstructionMaterials.Select(c=>c.Item.Id+":"+c.Count))).Append('\t').Append(d.RecoverPortable).Append('\n');
            }
            System.IO.Directory.CreateDirectory("Captures/class-progression");System.IO.File.WriteAllText("Captures/class-progression/building-economy.tsv",audit.ToString());
            foreach(var guid in AssetDatabase.FindAssets("t:Prefab",new[]{"Assets/Wildfolk/Prefabs"}).OrderBy(g=>PrefabUtility.GetPrefabAssetType(AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(g)))==PrefabAssetType.Variant?1:0))
            {
                var path=AssetDatabase.GUIDToAssetPath(guid);var original=AssetDatabase.LoadAssetAtPath<GameObject>(path);var isPlayer=original.GetComponent<PlayerProgression>()!=null;var isStructure=original.GetComponent<WorldPlaceable>()!=null;if(!isPlayer && !isStructure)continue;
                var root=PrefabUtility.LoadPrefabContents(path);
                if(isPlayer)
                {
                    if(root.GetComponent<PlayerClass>()==null)root.AddComponent<PlayerClass>();
                    if(root.GetComponent<ClassWorldFeedback>()==null){var fx=root.AddComponent<ClassWorldFeedback>();fx.Icon=VisualArtImporter.Sprite("class_hunter");}
                    if(root.GetComponent<PlayerMedicine>()==null)root.AddComponent<PlayerMedicine>();
                    if(root.GetComponent<PlayerBuildingService>()==null)root.AddComponent<PlayerBuildingService>();
                    if(root.GetComponent<PlayerTeamMarker>()==null)root.AddComponent<PlayerTeamMarker>();
                    if(root.GetComponent<ClassPurchaseReceipts>()==null)root.AddComponent<ClassPurchaseReceipts>();
                    if(root.GetComponent<NetworkPlayer>()!=null && root.GetComponent<NetworkClassActions>()==null)root.AddComponent<NetworkClassActions>();
                }
                if(isStructure && root.GetComponent<StructureMaintenance>()==null)root.AddComponent<StructureMaintenance>();
                PrefabUtility.SaveAsPrefabAsset(root,path);PrefabUtility.UnloadPrefabContents(root);
            }
            var session=PrefabUtility.LoadPrefabContents(MultiplayerSceneBuilder.SessionPrefabPath);session.GetComponent<Unity.Netcode.NetworkManager>().NetworkConfig.ProtocolVersion=NetworkProtocol.Current;PrefabUtility.SaveAsPrefabAsset(session,MultiplayerSceneBuilder.SessionPrefabPath);PrefabUtility.UnloadPrefabContents(session);
            var homeScene=EditorSceneManager.OpenScene("Assets/Wildfolk/Scenes/MainMenu.unity");var home=UnityEngine.Object.FindFirstObjectByType<HomeScreen>();BuildLobby(home);EditorSceneManager.SaveScene(homeScene);
            foreach(var path in new[]{"Assets/Wildfolk/Scenes/Wildfolk.unity","Assets/Wildfolk/Scenes/Multiplayer.unity"})
            {
                var scene=EditorSceneManager.OpenScene(path);var canvas=UnityEngine.Object.FindFirstObjectByType<Canvas>();var safe=canvas.transform.Find("Safe Area");
                var existing=UnityEngine.Object.FindFirstObjectByType<ClassHud>();if(existing!=null)throw new InvalidOperationException("Class HUD already installed");
                var player=UnityEngine.Object.FindFirstObjectByType<NetworkPlayer>()==null?UnityEngine.Object.FindFirstObjectByType<GameplayHud>()?.Session.PlayerHealth.GetComponent<PlayerInventory>():null;
                BuildHud(canvas.gameObject,safe,player);
                var tree=UnityEngine.Object.FindFirstObjectByType<SkillTreeScreen>();if(tree!=null)
                {var root=tree.gameObject;var old=tree.View;UnityEngine.Object.DestroyImmediate(old);UnityEngine.Object.DestroyImmediate(tree);var method=typeof(CharacterUiBuilder).GetMethod("BuildTree",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static);method.Invoke(null,new object[]{root,safe.Find("Modal Layer")??safe,player.GetComponent<PlayerInputCommands>().Session,player.GetComponent<PlayerProgression>()});}
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();MultiplayerGrowth();EditorSceneManager.OpenScene("Assets/Wildfolk/Scenes/MainMenu.unity");
        }
        public static void MultiplayerGrowth()
        {
            var path=MultiplayerSceneBuilder.PlayerPrefabPath;var root=PrefabUtility.LoadPrefabContents(path);if(root.GetComponent<NetworkCommonGrowth>()==null)root.AddComponent<NetworkCommonGrowth>();PrefabUtility.SaveAsPrefabAsset(root,path);PrefabUtility.UnloadPrefabContents(root);
            var scene=EditorSceneManager.OpenScene("Assets/Wildfolk/Scenes/Multiplayer.unity");var ui=UnityEngine.Object.FindFirstObjectByType<NetworkInventoryUi>(FindObjectsInactive.Include);
            if(ui.GetComponent<CharacterScreen>()==null){var owner=AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponent<PlayerInventory>();CharacterUiBuilder.Build(ui.gameObject,ui.transform,owner.GetComponent<GameplaySession>(),owner);}
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);EditorSceneManager.OpenScene("Assets/Wildfolk/Scenes/MainMenu.unity");
        }

        internal static void BuildLobby(HomeScreen home)
        {
            var screen=home.gameObject.AddComponent<ClassLobbyScreen>();screen.Home=home;screen.Rules=home.Rules;
            var journal=home.Menu.transform.Find("Field Journal");
            var entry=JournalUiBuilder.Action(journal,"Class Journal Entry","class.title",new Vector2(23,-149),new Vector2(434,72),screen.Open,out var entryLabel,false);entryLabel.alignment=TextAnchor.MiddleLeft;entryLabel.fontSize=29;var entryColors=entry.colors;entryColors.normalColor=Color.clear;entry.colors=entryColors;
            var mp=home.Multiplayer.transform.Find("Camp Journal");
            JournalUiBuilder.Action(mp,"Class Bookmark","class.title",new Vector2(235,350),new Vector2(190,72),screen.Open,out _,false);
            var panel=JournalUiBuilder.Book(home.Menu.transform.parent,"Class Field Journal",out screen.View);
            JournalUiBuilder.Label(panel,"Heading","journal.classes",new Vector2(-600,391),new Vector2(330,52),32);
            JournalUiBuilder.Label(panel,"Field Notes","journal.field_notes",new Vector2(-280,391),new Vector2(260,45),22);
            JournalUiBuilder.Action(panel,"Back","ui.action.back",new Vector2(690,394),new Vector2(160,72),screen.Back,out _);
            Image("Gold Icon",panel,"journal_gold",Vector2.one*.5f,new Vector2(108,397),new Vector2(34,34));
            screen.Currency=JournalUiBuilder.Label(panel,"Gold","",new Vector2(228,397),new Vector2(180,40),26);
            Image("Gem Icon",panel,"journal_gem",Vector2.one*.5f,new Vector2(368,397),new Vector2(34,34));
            screen.PremiumCurrency=JournalUiBuilder.Label(panel,"Gems","—",new Vector2(490,397),new Vector2(195,40),26);
            JournalUiBuilder.Rule(panel,new Vector2(-765,351),new Vector2(-82,351));JournalUiBuilder.Rule(panel,new Vector2(88,351),new Vector2(766,351));
            screen.Cards=new Button[6];screen.CardNames=new Text[6];screen.CardRoles=new Text[6];screen.CardStates=new Text[6];screen.CardMarks=new Image[6];
            for(var i=0;i<6;i++)
            {
                var y=275-i*111;var card=Image("Class Entry "+i,panel,"white",Vector2.one*.5f,new Vector2(-615,y),new Vector2(308,104),true);screen.Cards[i]=Button(card);screen.Cards[i].transition=Selectable.Transition.None;
                UnityEventTools.AddIntPersistentListener(screen.Cards[i].onClick,screen.SelectCard,i);
                var icon=Image("Class Icon",card.transform,"white",Vector2.one*.5f,new Vector2(-115,18),new Vector2(58,58));icon.sprite=JournalSymbolAssets.Sprite("class_"+ClassCatalog.Current.Classes[i].Id);icon.preserveAspect=true;
                screen.CardNames[i]=JournalUiBuilder.Label(card.transform,"Name","",new Vector2(27,29),new Vector2(210,50),27);
                screen.CardRoles[i]=JournalUiBuilder.Label(card.transform,"Role","",new Vector2(24,-10),new Vector2(210,44),22);
                screen.CardStates[i]=JournalUiBuilder.Label(card.transform,"Ownership","",new Vector2(-108,-31),new Vector2(90,38),20,TextAnchor.MiddleCenter);
                screen.CardMarks[i]=Image("Selected Stamp",card.transform,"journal_seal",Vector2.one*.5f,new Vector2(135,30),new Vector2(30,30));
                JournalUiBuilder.Rule(card.transform,new Vector2(-145,-52),new Vector2(145,-52));
            }
            screen.ClassIcon=Image("Selected Class Symbol",panel,"white",Vector2.one*.5f,new Vector2(-277,261),new Vector2(140,140));
            screen.Title=JournalUiBuilder.Label(panel,"Class Title","",new Vector2(-272,148),new Vector2(290,76),42,TextAnchor.MiddleCenter);
            screen.Role=JournalUiBuilder.Label(panel,"Class Role","",new Vector2(-272,70),new Vector2(284,80),24,TextAnchor.MiddleCenter);
            screen.Description=JournalUiBuilder.Label(panel,"Identity","",new Vector2(-272,-44),new Vector2(284,140),24,TextAnchor.UpperLeft);
            screen.Progress=JournalUiBuilder.Label(panel,"Class Progress","",new Vector2(-272,-182),new Vector2(284,126),23,TextAnchor.UpperLeft);
            screen.SelectButton=JournalUiBuilder.Action(panel,"Select","class.select",new Vector2(-272,-289),new Vector2(284,76),screen.SelectClass,out screen.SelectLabel,true);
            screen.PurchaseButton=JournalUiBuilder.Action(panel,"Purchase","",new Vector2(-272,-375),new Vector2(284,76),screen.AskPurchase,out screen.PurchaseLabel,true);
            screen.Nodes=new Button[9];screen.NodeNames=new Text[9];screen.NodeViews=new JournalSkillNode[9];screen.Branches=new Text[3];screen.NodeIcons=new Image[9];
            screen.ClassIcons=JournalSymbolAssets.Classes();
            screen.SkillIcons=JournalSymbolAssets.Nodes();
            var root=new Vector2(422,287);Image("Class Tree Root",panel,"white",Vector2.one*.5f,root,new Vector2(58,58));
            screen.TreeIcon=panel.Find("Class Tree Root").GetComponent<Image>();
            screen.ClassIcon.sprite=screen.TreeIcon.sprite=screen.ClassIcons[0];
            for(var b=0;b<3;b++)
            {
                var x=184+b*237;JournalUiBuilder.Rule(panel,root+new Vector2(0,-32),new Vector2(x,224),3);
                screen.Branches[b]=JournalUiBuilder.Label(panel,"Branch "+b,"",new Vector2(x,210),new Vector2(218,40),25,TextAnchor.MiddleCenter);
                for(var r=0;r<3;r++)
                {
                    var i=b*3+r;var p=new Vector2(x+(b-1)*r*9,137-r*145);
                    if(r<2)JournalUiBuilder.Rule(panel,p+new Vector2(0,-78),p+new Vector2((b-1)*9,-99),3);
                    screen.NodeViews[i]=JournalUiBuilder.Node(panel,"Class Skill "+i,p,screen.SkillIcons[i],screen.SelectNode,i,out screen.Nodes[i]);screen.NodeIcons[i]=screen.NodeViews[i].Icon;
                    screen.NodeNames[i]=JournalUiBuilder.Label(panel,"Node Name "+i,"",p+new Vector2(0,-67),new Vector2(216,43),22,TextAnchor.MiddleCenter);

                }
            }
            JournalUiBuilder.Detail(panel,new Vector2(421,-294),new Vector2(665,108));
            screen.NodeDetail=JournalUiBuilder.Label(panel,"Node Detail","",new Vector2(421,-288),new Vector2(665,96),22,TextAnchor.UpperLeft);
            screen.InvestButton=JournalUiBuilder.Action(panel,"Invest","class.invest",new Vector2(602,-375),new Vector2(294,72),screen.Invest,out screen.InvestLabel,true);
            screen.RespecButton=JournalUiBuilder.Action(panel,"Respec","class.respec",new Vector2(240,-375),new Vector2(294,72),screen.Respec,out _);
            screen.Status=JournalUiBuilder.Label(panel,"Status","",new Vector2(-615,-377),new Vector2(300,70),20,TextAnchor.MiddleCenter);
            var confirm=QualityUiBuilder.Surface(screen.View.transform,"Confirm Purchase",new Vector2(760,390),out screen.Confirmation);
            foreach(var image in confirm.GetComponentsInChildren<Image>())image.color=new Color32(225,211,173,255);
            screen.ConfirmText=JournalUiBuilder.Label(confirm.transform,"Price","",new Vector2(0,65),new Vector2(650,160),28,TextAnchor.MiddleCenter);
            JournalUiBuilder.Action(confirm.transform,"Confirm","class.buy",new Vector2(-170,-105),new Vector2(290,76),screen.ConfirmPurchase,out _,true);JournalUiBuilder.Action(confirm.transform,"Cancel","ui.action.back",new Vector2(170,-105),new Vector2(290,76),screen.CancelPurchase,out _);
            screen.Confirmation.SetActive(false);screen.View.SetActive(false);
        }
        public static void BuildHud(GameObject root,Transform safe,PlayerInventory player)
        {
            var hud=root.AddComponent<ClassHud>();hud.Player=player;
            hud.ClassName=Text("Class",safe,"",new Vector2(610,460),new Vector2(280,36),23);
            var active=Image("Class Active",safe,"panel",new Vector2(1,0),new Vector2(-215,425),new Vector2(134,112),true);hud.ActiveView=active.gameObject;hud.ActiveButton=Button(active);hud.GuardHold=active.gameObject.AddComponent<ClassHoldButton>();hud.Ability=Text("Ability",active.transform,"",Vector2.zero,new Vector2(124,100),22);
            var mark=Image("Team Marker Action",safe,"panel",new Vector2(1,0),new Vector2(-400,420),new Vector2(170,76),true);hud.MarkerView=mark.gameObject;hud.MarkButton=Button(mark);hud.MarkerText=Text("Mark",mark.transform,"class.mark",Vector2.zero,new Vector2(160,66),22);
            hud.MeaningButton=QualityUiBuilder.Action(mark.transform,"Meaning","class.meaning.0",new Vector2(0,84),new Vector2(170,62),hud.CycleMeaning,out hud.MeaningText);
            var building=Rect("Building Service",safe,new Vector2(.5f,0),new Vector2(0,215),new Vector2(840,175));hud.BuildingView=building.gameObject;
            hud.BuildingText=Text("Structure",building,"",new Vector2(0,95),new Vector2(820,60),21);
            hud.RepairButton=Button(Image("Repair",building,"panel",Vector2.one*.5f,new Vector2(-155,10),new Vector2(280,68),true));Text("Repair Label",hud.RepairButton.transform,"class.repair",Vector2.zero,new Vector2(268,60),23);hud.RepairHold=hud.RepairButton.gameObject.AddComponent<ClassHoldButton>();
            hud.DismantleButton=Button(Image("Dismantle",building,"panel",Vector2.one*.5f,new Vector2(155,10),new Vector2(280,68),true));Text("Recover Label",hud.DismantleButton.transform,"class.dismantle",Vector2.zero,new Vector2(268,60),23);hud.DismantleHold=hud.DismantleButton.gameObject.AddComponent<ClassHoldButton>();
            hud.ServiceProgress=Text("Hold",building,"",new Vector2(0,-50),new Vector2(810,45),20);
            var medicine=Image("Ally Medicine",safe,"panel",new Vector2(1,0),new Vector2(-415,320),new Vector2(185,68),true);hud.MedicineView=medicine.gameObject;hud.MedicineButton=Button(medicine);Text("Treat Ally",medicine.transform,"class.heal_ally",Vector2.zero,new Vector2(175,60),21);
            hud.QuarryText=Text("Quarry",safe,"",new Vector2(565,300),new Vector2(520,85),23);
            hud.TeamMarkers=new Text[4];for(var i=0;i<4;i++)hud.TeamMarkers[i]=Text("Team Marker "+i,safe,"",new Vector2(0,390-i*38),new Vector2(650,36),21);
            active.gameObject.SetActive(false);mark.gameObject.SetActive(false);building.gameObject.SetActive(false);medicine.gameObject.SetActive(false);
        }
    }
}
