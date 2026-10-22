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
                var d=item.Placement;d.RepairMaterials=d.ConstructionMaterials.Where(c=>c.Item.Id=="wood" || c.Item.Id=="stone" || c.Item.Id=="iron_ingot" || c.Item.Id=="bronze_ingot" || c.Item.Id=="steel_ingot").Select(c=>new ItemAmount{Item=c.Item,Count=Mathf.Max(1,Mathf.CeilToInt(c.Count*.25f))}).ToArray();
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

        private static void BuildLobby(HomeScreen home)
        {
            if(home.GetComponent<ClassLobbyScreen>()!=null)throw new InvalidOperationException("Class lobby already installed");
            var screen=home.gameObject.AddComponent<ClassLobbyScreen>();screen.Home=home;screen.Rules=home.Rules;
            var safe=home.Menu.transform.parent;QualityUiBuilder.Action(home.Menu.transform,"CLASS","class.title",new Vector2(0,-330),new Vector2(380,66),screen.Open,out _);
            // Also available before choosing Host/Join; connection admission captures the selected build.
            QualityUiBuilder.Action(home.Multiplayer.transform,"CLASS","class.title",new Vector2(-600,340),new Vector2(230,62),screen.Open,out _);
            var panel=QualityUiBuilder.Surface(safe,"Class Selection",new Vector2(1740,920),out screen.View).transform;
            Text("Heading",panel,"class.title",new Vector2(-600,402),new Vector2(400,50),32);
            QualityUiBuilder.Action(panel,"Back","ui.action.back",new Vector2(730,400),new Vector2(180,64),screen.Back,out _);
            screen.Currency=Text("Gold",panel,"",new Vector2(300,400),new Vector2(550,45),23,TextAnchor.MiddleRight);
            screen.Cards=new Button[6];screen.CardNames=new Text[6];screen.CardRoles=new Text[6];screen.CardStates=new Text[6];
            for(var i=0;i<6;i++)
            {
                var center=new Vector2(-690+(i%2)*245,220-(i/2)*175);var image=Image("Class Card",panel,"panel",Vector2.one*.5f,center,new Vector2(230,155),true);screen.Cards[i]=Button(image);UnityEventTools.AddIntPersistentListener(screen.Cards[i].onClick,screen.SelectCard,i);
                var icon=Image("Class Icon",image.transform,"class_"+ClassCatalog.Current.Classes[i].Id,Vector2.one*.5f,new Vector2(-73,36),new Vector2(48,48));icon.preserveAspect=true;
                screen.CardNames[i]=Text("Class Name",image.transform,"",new Vector2(25,38),new Vector2(145,38),25);
                screen.CardRoles[i]=Text("Class Role",image.transform,"",new Vector2(0,-10),new Vector2(210,40),18);
                screen.CardStates[i]=Text("Ownership",image.transform,"",new Vector2(0,-54),new Vector2(210,26),18);
            }
            QualityUiBuilder.Line(panel,new Vector2(-300,330),new Vector2(-300,-365));
            screen.Title=Text("Class Title",panel,"",new Vector2(235,325),new Vector2(980,48),32,TextAnchor.MiddleLeft);
            screen.Role=Text("Class Role",panel,"",new Vector2(235,275),new Vector2(980,40),23,TextAnchor.MiddleLeft);
            screen.Description=Text("Description",panel,"",new Vector2(235,222),new Vector2(980,70),23,TextAnchor.MiddleLeft);
            screen.Progress=Text("Class Progress",panel,"",new Vector2(235,160),new Vector2(980,40),20,TextAnchor.MiddleLeft);
            screen.Nodes=new Button[9];screen.NodeNames=new Text[9];screen.Branches=new Text[3];
            for(var b=0;b<3;b++)
            {
                var x=-90+b*310;screen.Branches[b]=Text("Branch",panel,"",new Vector2(x,106),new Vector2(280,35),23);
                for(var rank=0;rank<3;rank++)
                {
                    var i=b*3+rank;var y=33-rank*110;if(rank<2)QualityUiBuilder.Line(panel,new Vector2(x,y-45),new Vector2(x,y-65),3);
                    var image=Image("Class Node",panel,"panel",Vector2.one*.5f,new Vector2(x,y),new Vector2(278,88),true);screen.Nodes[i]=Button(image);screen.NodeNames[i]=Text("Node",image.transform,"",Vector2.zero,new Vector2(260,75),22);UnityEventTools.AddIntPersistentListener(screen.Nodes[i].onClick,screen.SelectNode,i);
                }
            }
            screen.NodeDetail=Text("Node Detail",panel,"",new Vector2(235,-280),new Vector2(980,80),22,TextAnchor.MiddleLeft);
            screen.InvestButton=QualityUiBuilder.Action(panel,"Invest","class.invest",new Vector2(630,-375),new Vector2(220,66),screen.Invest,out _);
            screen.RespecButton=QualityUiBuilder.Action(panel,"Respec","class.respec",new Vector2(350,-375),new Vector2(220,66),screen.Respec,out _);
            screen.SelectButton=QualityUiBuilder.Action(panel,"Select","class.select",new Vector2(-685,-355),new Vector2(240,68),screen.SelectClass,out screen.SelectLabel);
            screen.PurchaseButton=QualityUiBuilder.Action(panel,"Purchase","",new Vector2(-430,-355),new Vector2(240,68),screen.AskPurchase,out screen.PurchaseLabel);
            screen.Status=Text("Status",panel,"",new Vector2(-5,-423),new Vector2(1550,35),19);
            var confirmation=QualityUiBuilder.Surface(screen.View.transform,"Confirm Purchase",new Vector2(760,340),out screen.Confirmation).transform;
            screen.ConfirmText=Text("Price",confirmation,"",new Vector2(0,60),new Vector2(690,150),26);
            QualityUiBuilder.Action(confirmation,"Confirm","class.buy",new Vector2(-170,-95),new Vector2(290,70),screen.ConfirmPurchase,out _);QualityUiBuilder.Action(confirmation,"Cancel","ui.action.back",new Vector2(170,-95),new Vector2(290,70),screen.CancelPurchase,out _);
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
