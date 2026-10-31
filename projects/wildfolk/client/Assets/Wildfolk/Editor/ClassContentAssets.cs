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
            var s=home.gameObject.AddComponent<ClassLobbyScreen>();s.Home=home;s.Rules=home.Rules;
            home.ClassButton=JournalUiBuilder.Action(home.Menu.transform.Find("Field Journal"),"Class Journal Entry","class.title",new Vector2(0,-210),new Vector2(530,120),s.Open,out _);
            var page=JournalUiBuilder.Sheet(home.Menu.transform.parent,"Class Field Journal",out s.View);
            JournalUiBuilder.Label(page,"Heading","journal.classes",new Vector2(-460,405),new Vector2(580,80),44);
            JournalUiBuilder.Action(page,"Back","ui.action.back",new Vector2(690,405),new Vector2(180,120),s.Back,out _);
            Image("Gold Icon",page,"journal_gold",Vector2.one*.5f,new Vector2(45,405),new Vector2(42,42));s.Currency=JournalUiBuilder.Label(page,"Gold","",new Vector2(155,405),new Vector2(160,55),32);
            Image("Gem Icon",page,"journal_gem",Vector2.one*.5f,new Vector2(310,405),new Vector2(42,42));s.PremiumCurrency=JournalUiBuilder.Label(page,"Gems","",new Vector2(430,405),new Vector2(180,55),32);
            JournalUiBuilder.Rule(page,new Vector2(-760,335),new Vector2(760,335));
            var overview=Rect("Class Overview",page,Vector2.one*.5f,Vector2.zero,new Vector2(1720,1000));s.OverviewView=overview.gameObject;
            s.Cards=new Button[6];s.CardNames=new Text[6];s.CardRoles=new Text[6];s.CardStates=new Text[6];s.CardMarks=new Image[6];
            for(var i=0;i<6;i++)
            {
                var card=Image("Class Entry "+i,overview,"white",Vector2.one*.5f,new Vector2(-590+i%2*350,225-i/2*210),new Vector2(330,190),true);s.Cards[i]=Button(card);s.Cards[i].transition=Selectable.Transition.None;UnityEventTools.AddIntPersistentListener(s.Cards[i].onClick,s.SelectCard,i);
                var icon=Image("Class Icon",card.transform,"white",Vector2.one*.5f,new Vector2(-120,50),new Vector2(54,54));icon.sprite=JournalSymbolAssets.Sprite("class_"+ClassCatalog.Current.Classes[i].Id);icon.preserveAspect=true;
                s.CardNames[i]=JournalUiBuilder.Label(card.transform,"Name","",new Vector2(32,50),new Vector2(225,60),34);
                s.CardRoles[i]=JournalUiBuilder.Label(card.transform,"Role","",new Vector2(0,-12),new Vector2(290,64),28);
                s.CardStates[i]=JournalUiBuilder.Label(card.transform,"Ownership","",new Vector2(0,-66),new Vector2(280,40),28);
                s.CardMarks[i]=Image("Selected Stamp",card.transform,"journal_seal",Vector2.one*.5f,new Vector2(135,65),new Vector2(32,32));
                JournalUiBuilder.Rule(card.transform,new Vector2(-160,-95),new Vector2(160,-95));
            }
            s.ClassIcons=JournalSymbolAssets.Classes();s.SkillIcons=JournalSymbolAssets.Nodes();
            s.ClassIcon=Image("Selected Class Symbol",overview,"white",Vector2.one*.5f,new Vector2(155,265),new Vector2(110,110));
            s.Title=JournalUiBuilder.Label(overview,"Class Title","",new Vector2(460,280),new Vector2(480,80),46);
            s.Role=JournalUiBuilder.Label(overview,"Class Role","",new Vector2(460,203),new Vector2(480,85),30);
            s.Description=JournalUiBuilder.Label(overview,"Identity","",new Vector2(430,60),new Vector2(650,170),34,TextAnchor.UpperLeft);
            s.Progress=JournalUiBuilder.Label(overview,"Class Progress","",new Vector2(430,-100),new Vector2(650,140),30);
            JournalUiBuilder.Action(overview,"Class Skill Tree","ui.skill.title",new Vector2(430,-235),new Vector2(650,120),s.OpenTree,out _);
            s.SelectButton=JournalUiBuilder.Action(overview,"Select","class.select",new Vector2(430,-375),new Vector2(650,120),s.SelectClass,out s.SelectLabel,true);
            s.PurchaseButton=JournalUiBuilder.Action(overview,"Purchase","",new Vector2(430,-375),new Vector2(650,120),s.AskPurchase,out s.PurchaseLabel,true);
            var tree=Rect("Class Tree",page,Vector2.one*.5f,Vector2.zero,new Vector2(1720,1000));s.TreeView=tree.gameObject;
            s.TreeTitle=JournalUiBuilder.Label(tree,"Selected Class","",new Vector2(-420,280),new Vector2(650,75),42);s.TreePoints=JournalUiBuilder.Label(tree,"Points","",new Vector2(430,280),new Vector2(650,75),34);
            s.TreeIcon=Image("Class Tree Root",tree,"white",Vector2.one*.5f,new Vector2(-420,200),new Vector2(56,56));
            s.Nodes=new Button[9];s.NodeNames=new Text[9];s.NodeViews=new JournalSkillNode[9];s.Branches=new Text[3];s.NodeIcons=new Image[9];
            for(var b=0;b<3;b++)
            {
                var x=-660+b*240;JournalUiBuilder.Rule(tree,new Vector2(-420,165),new Vector2(x,133),3);s.Branches[b]=JournalUiBuilder.Label(tree,"Branch "+b,"",new Vector2(x,140),new Vector2(220,48),30,TextAnchor.MiddleCenter);
                for(var r=0;r<3;r++)
                {
                    var i=b*3+r;var p=new Vector2(x,70-r*170);if(r<2)JournalUiBuilder.Rule(tree,p+new Vector2(0,-60),p+new Vector2(0,-110),3);
                    s.NodeViews[i]=JournalUiBuilder.Node(tree,"Class Skill "+i,p,s.SkillIcons[i],s.SelectNode,i,out s.Nodes[i]);s.NodeIcons[i]=s.NodeViews[i].Icon;
                    s.NodeNames[i]=JournalUiBuilder.Label(tree,"Node Name "+i,"",p+new Vector2(0,-82),new Vector2(224,52),28,TextAnchor.MiddleCenter);
                }
            }
            s.NodeDetail=JournalUiBuilder.Label(tree,"Node Detail","",new Vector2(430,20),new Vector2(650,370),36,TextAnchor.UpperLeft);
            s.InvestButton=JournalUiBuilder.Action(tree,"Invest","class.invest",new Vector2(430,-250),new Vector2(650,120),s.Invest,out s.InvestLabel,true);
            s.RespecButton=JournalUiBuilder.Action(tree,"Respec","class.respec",new Vector2(430,-390),new Vector2(650,120),s.Respec,out _);
            s.Status=JournalUiBuilder.Label(page,"Status","",new Vector2(-420,-435),new Vector2(680,45),28,TextAnchor.MiddleCenter);
            var confirm=QualityUiBuilder.Surface(s.View.transform,"Confirm Purchase",new Vector2(1000,540),out s.Confirmation);
            s.ConfirmText=JournalUiBuilder.Label(confirm.transform,"Price","",new Vector2(0,80),new Vector2(850,210),36,TextAnchor.MiddleCenter);
            JournalUiBuilder.Action(confirm.transform,"Confirm","class.buy",new Vector2(-220,-140),new Vector2(400,126),s.ConfirmPurchase,out _,true);JournalUiBuilder.Action(confirm.transform,"Cancel","ui.action.back",new Vector2(220,-140),new Vector2(400,126),s.CancelPurchase,out _);
            s.Confirmation.SetActive(false);s.TreeView.SetActive(false);s.View.SetActive(false);
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
            ArrangeHud(hud);active.gameObject.SetActive(false);mark.gameObject.SetActive(false);building.gameObject.SetActive(false);medicine.gameObject.SetActive(false);
        }
        internal static void ArrangeHud(ClassHud hud)
        {
            var name=hud.ClassName.rectTransform;name.anchorMin=name.anchorMax=Vector2.one;InventoryUiBuilder.Caption(hud.ClassName,new Vector2(-390,-70),new Vector2(400,60),28);hud.ClassName.alignment=TextAnchor.MiddleRight;
            InventoryUiBuilder.ActionSize(hud.ActiveButton,new Vector2(-472,310),new Vector2(180,128));InventoryUiBuilder.ActionSize(hud.MarkButton,new Vector2(-472,480),new Vector2(180,128));InventoryUiBuilder.ActionSize(hud.MedicineButton,new Vector2(-682,310),new Vector2(180,128));InventoryUiBuilder.ActionSize(hud.MeaningButton,new Vector2(-210,0),new Vector2(180,128));
            InventoryUiBuilder.ActionSize(hud.RepairButton,new Vector2(-170,0),new Vector2(300,112));InventoryUiBuilder.ActionSize(hud.DismantleButton,new Vector2(170,0),new Vector2(300,112));InventoryUiBuilder.Caption(hud.BuildingText,new Vector2(0,100),new Vector2(820,65),28);InventoryUiBuilder.Caption(hud.ServiceProgress,new Vector2(0,-95),new Vector2(810,55),26);
        }
    }
}
