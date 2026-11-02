using System;
using UnityEditor;
using UnityEngine;
namespace Wildfolk.Editor
{
    public sealed class WildfolkDeveloperWindow : EditorWindow
    {
        private Vector2 scroll,spawnPosition;
        private string item="wood",message;
        private int quantity=30,hp=100,day=1,seed=9472026,level=3,points=2;
        private float hunger=90,thirst=90,time=30,stamina=100,recoverySeconds=60;
        private long xp,gold;
        private string[] overviewTargets;private int overviewTarget;private WorldOverview overview;private int creatureIndex;private string[] creatureFamilies;
        private void OnDisable(){overview?.Dispose();overview=null;if(EditorApplication.isPlaying){var movement=UnityEngine.Object.FindFirstObjectByType<DeveloperMovement>();if(movement!=null)movement.Configure(1);}}
        private void InspectWorld(){var next=new WorldOverview(DeveloperHarness.Context().World.Map);overview?.Dispose();overview=next;overviewTargets=new System.Collections.Generic.List<string>(overview.Targets.Keys).ToArray();overviewTarget=0;message="Actual world seed "+overview.Map.Seed+" · "+overview.Map.Roads.Count+" roads · "+overview.Map.Continental.Anchors.Count+" POIs";}
        private void QaTools()
        {
            EditorGUILayout.Space(14);EditorGUILayout.LabelField("World / Creature Inspector · Editor only",EditorStyles.boldLabel);
            if(!EditorApplication.isPlaying){if(GUILayout.Button("Start Isolated QA Play"))Execute(()=>{IsolatedQaProfile.Begin("Codex_QA_DeveloperInspector");EditorApplication.isPlaying=true;});if(!string.IsNullOrEmpty(UnityEditor.SessionState.GetString("Wildfolk.QA.Directory","")))if(GUILayout.Button("Return to normal profile (Play stopped)"))Execute(IsolatedQaProfile.End);return;}
            var context=UnityEngine.Object.FindFirstObjectByType<WorldSaveService>();if(context==null||context.World.Map==null){EditorGUILayout.HelpBox("World not ready in this scene.",MessageType.Info);return;}
            if(GUILayout.Button("Inspect actual current seed"))Execute(InspectWorld);
            EditorGUILayout.LabelField(OverviewSeedBatch.Status,EditorStyles.wordWrappedMiniLabel);
            if(GUILayout.Button("Export 12 consecutive seeds (keeps current Play world)"))Execute(()=>OverviewSeedBatch.Begin(DeveloperHarness.Context().World.Definition,seed,12));
            if(overview!=null)
            {
                if(overview.Map!=DeveloperHarness.Context().World.Map)EditorGUILayout.HelpBox("World changed. Refresh before selecting/exporting.",MessageType.Warning);
                else
                {
                    var rect=GUILayoutUtility.GetAspectRect(1);GUI.DrawTexture(rect,overview.Texture,ScaleMode.ScaleToFit);
                    if(Event.current.type==EventType.MouseDown&&rect.Contains(Event.current.mousePosition))
                    {var p=Event.current.mousePosition;var point=new Vector2((p.x-rect.x)/rect.width-.5f,.5f-(p.y-rect.y)/rect.height)*overview.Map.Size.x;var nearest=point;var distance=overview.Map.Size.x/overview.Texture.width*4;foreach(var marker in overview.Targets){var d=Vector2.Distance(point,marker.Value);if(d<distance){distance=d;nearest=marker.Value;}}Execute(()=>message=DeveloperShowcase.Teleport(nearest));Event.current.Use();}
                    EditorGUILayout.LabelField("Grassland olive · Forest green · Wetlands teal · Rocky gray\nWater blue · Roads sand · Spawn cream · Cave violet · Graveyard white · Goblin red",EditorStyles.wordWrappedMiniLabel);
                    if(GUILayout.Button("Export PNG + labelled SVG + seed data"))Execute(()=>message=overview.Export(System.IO.Path.GetFullPath("Captures/world-overview")));
                    overviewTarget=EditorGUILayout.Popup("Actual destination",overviewTarget,overviewTargets);if(GUILayout.Button("Teleport selected actual target"))Execute(()=>message=DeveloperShowcase.Teleport(overview.Targets[overviewTargets[overviewTarget]]));
                }
            }
            EditorGUILayout.LabelField("QA walking speed (not saved)",EditorStyles.boldLabel);EditorGUILayout.BeginHorizontal();foreach(var n in new[]{1,4,10,25}){var speed=n;if(GUILayout.Button(n+"×"))Execute(()=>{DeveloperTraversal.Set(speed);message="QA movement "+speed+"× · not saved";});}EditorGUILayout.EndHorizontal();
            if(creatureFamilies==null)creatureFamilies=DeveloperShowcase.Families;creatureIndex=EditorGUILayout.Popup("Actual prefab family",creatureIndex,creatureFamilies);
            if(GUILayout.Button("Spawn selected near player"))Execute(()=>{DeveloperShowcase.NearPlayer(creatureFamilies[creatureIndex]);message=DeveloperShowcase.LastStatus;});
            if(GUILayout.Button("Spawn selected group ×3"))Execute(()=>{DeveloperShowcase.Group(new[]{creatureFamilies[creatureIndex],creatureFamilies[creatureIndex],creatureFamilies[creatureIndex]});message=DeveloperShowcase.LastStatus;});
            if(GUILayout.Button("Clear only QA spawned actors"))Execute(()=>{DeveloperShowcase.Clear();message=DeveloperShowcase.LastStatus;});
        }
        [MenuItem("Wildfolk/Developer/Open Test Console")]
        private static void Open(){var window=CreateInstance<WildfolkDeveloperWindow>();window.titleContent=new GUIContent(L.Text("dev.title"));window.Show();}
        private void Execute(Action action){try{message=null;action();if(message==null)message=L.Text("dev.applied");}catch(Exception error){message=error.Message;}Repaint();}
        private void Section(string key){EditorGUILayout.Space(14);EditorGUILayout.LabelField(L.Text("dev."+key),EditorStyles.boldLabel);EditorGUILayout.LabelField(L.Text("dev."+key+".note"),EditorStyles.wordWrappedMiniLabel);}
        private void Control(string key,Action action,bool warning=true,string description=null,string title=null)
        {
            var note=L.Text("dev."+(description ?? key)+".note");var tooltip=note+(warning ? "\n"+L.Text("dev.warning") : "");
            if(GUILayout.Button(new GUIContent(title ?? L.Text("dev."+key),tooltip)))Execute(action);
            EditorGUILayout.LabelField(note,EditorStyles.wordWrappedMiniLabel);
        }
        private void OnGUI()
        {
            titleContent.text=L.Text("dev.title");scroll=EditorGUILayout.BeginScrollView(scroll);EditorGUILayout.HelpBox(L.Text("dev.intro"),MessageType.Warning);QaTools();
            using(new EditorGUI.DisabledScope(!EditorApplication.isPlaying))
            {
                Section("quick");Control("vitals",DeveloperHarness.RestoreVitals);Control("gear",DeveloperHarness.GiveCurrentTierGear);Control("armor",()=>DeveloperHarness.GiveArmorSet(true,false));Control("point",DeveloperHarness.AddSkillPoint);
                Control("day",()=>DeveloperHarness.SetTime(DeveloperHarness.Context().Clock.Model.Day,30));Control("night",()=>{var s=DeveloperHarness.Context();DeveloperHarness.SetTime(s.Clock.Model.Day,s.Clock.Definition.DaySeconds+s.Clock.Definition.EveningSeconds+1);});
                Control("sandbox",DeveloperHarness.OpenCombatSandbox);Control("resource",()=>DeveloperHarness.ForceResourceRecovery());Control("save",()=>{var s=DeveloperHarness.Context();if(!s.Save())throw new InvalidOperationException(s.LastError);});Control("load",()=>DeveloperHarness.Context().Load());
                Section("player");hp=EditorGUILayout.IntField(new GUIContent(L.Text("dev.hp"),L.Text("dev.hp.note")),hp);Control("hp",()=>DeveloperHarness.SetHealth(hp));
                hunger=EditorGUILayout.FloatField(L.Text("dev.field.hunger"),hunger);thirst=EditorGUILayout.FloatField(L.Text("dev.field.thirst"),thirst);Control("needs",()=>DeveloperHarness.SetNeeds(hunger,thirst));
                xp=EditorGUILayout.LongField(L.Text("dev.field.xp"),xp);gold=EditorGUILayout.LongField(L.Text("dev.field.gold"),gold);Control("xp",()=>DeveloperHarness.SetProgression(xp,gold));
                level=EditorGUILayout.IntField(L.Text("dev.level"),level);Control("level",()=>DeveloperHarness.SetLevel(level));points=EditorGUILayout.IntField(L.Text("dev.points"),points);Control("points",()=>DeveloperHarness.SetSkillPoints(points));stamina=EditorGUILayout.FloatField(L.Text("dev.stamina"),stamina);Control("stamina",()=>DeveloperHarness.SetStamina(stamina));
                Section("equipment");Control("leather",()=>DeveloperHarness.GiveArmorSet(false,false));Control("leather_equip",()=>DeveloperHarness.GiveArmorSet(false,true));Control("iron_equip",()=>DeveloperHarness.GiveArmorSet(true,true));Control("clear_armor",DeveloperHarness.ClearArmor);
                item=EditorGUILayout.TextField(L.Text("dev.field.item"),item);quantity=EditorGUILayout.IntField(L.Text("dev.field.quantity"),quantity);Control("item",()=>DeveloperHarness.GiveItem(item,quantity));Control("tools",DeveloperHarness.GiveAllEquipment);
                Section("profiles");for(var i=1;i<=4;i++){var rank=i;Control("profile",()=>DeveloperHarness.LoadProfile(AssetDatabase.LoadAssetAtPath<DeveloperTestProfile>("Assets/Wildfolk/Data/DeveloperProfile"+rank+".asset")),title:L.Format("dev.profile",rank));}Control("sandbox",DeveloperHarness.OpenCombatSandbox);
                Section("world");day=EditorGUILayout.IntField(L.Text("dev.day"),day);time=EditorGUILayout.FloatField(L.Text("dev.time"),time);Control("time",()=>DeveloperHarness.SetTime(day,time));spawnPosition=EditorGUILayout.Vector2Field(L.Text("dev.position"),spawnPosition);Control("teleport",()=>message=DeveloperShowcase.Teleport(spawnPosition));
                var kinds=new[]{"Chicken","Boar","Rabbit","Deer","Sheep","Zombie","Skeleton","Goblin","SlingshotGoblin"};
                foreach(var kind in kinds){var local=kind;var id=kind=="SlingshotGoblin" ? "slingshot_goblin" : kind.ToLowerInvariant();Control("spawn",()=>{DeveloperShowcase.NearPlayer(local);message=DeveloperShowcase.LastStatus;},title:L.Format("dev.spawn",L.Text("creature."+id+".name")));}
                Control("spawn",()=>{DeveloperShowcase.NearPlayer("Skeleton (sword)");message=DeveloperShowcase.LastStatus;},title:L.Format("dev.spawn",L.Text("creature.skeleton_sword.name")));Control("warband",()=>{var count=DeveloperHarness.SpawnWarband(spawnPosition);if(count==0)throw new InvalidOperationException("Warband unavailable: population cap or authority. No actor spawned.");message="Spawned Warband ×"+count+" at "+spawnPosition;});Control("camp",()=>{DeveloperHarness.SpawnCamp(spawnPosition);message="Spawned Goblin Camp at "+spawnPosition;});Control("assist",DeveloperHarness.TriggerAllyAssist);Control("flee",DeveloperHarness.TriggerHerdFlee);
                Control("clear_enemies",()=>DeveloperHarness.ClearEnemies());seed=EditorGUILayout.IntField(L.Text("dev.field.seed"),seed);Control("seed",()=>{DeveloperHarness.GenerateWorld(seed);message="Generated world seed "+DeveloperHarness.Context().World.Seed+"; player returned to Spawn. Refresh overview.";});
                Section("population");Control("resource",()=>DeveloperHarness.ForceResourceRecovery());foreach(var seconds in new[]{60,300,600}){var delta=seconds;Control("advance",()=>DeveloperHarness.AdvanceResourceRecovery(delta),title:L.Format("dev.advance",delta/60));}recoverySeconds=EditorGUILayout.FloatField(L.Text("dev.seconds"),recoverySeconds);Control("custom",()=>DeveloperHarness.AdvanceResourceRecovery(recoverySeconds),description:"advance");Control("report",()=>message=DeveloperHarness.ResourcePopulationReport(),false);Control("animals",()=>DeveloperHarness.ForceAnimalPopulation());Control("camps",()=>DeveloperHarness.ForceCampPopulation());
                Section("persistence");Control("save",()=>{var s=DeveloperHarness.Context();if(!s.Save())throw new InvalidOperationException(s.LastError);});Control("load",()=>DeveloperHarness.Context().Load());Control("new",DeveloperHarness.NewTestState);
            }
            Section("validation");Control("validate",()=>{var errors=ProjectDataValidator.Validate();if(errors.Count>0)throw new InvalidOperationException(string.Join("\n",errors));},false);
            EditorGUILayout.HelpBox(message ?? L.Text("dev.choose"),MessageType.None);EditorGUILayout.EndScrollView();
        }
    }
}
