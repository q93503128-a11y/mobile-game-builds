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
        private float hunger=90,thirst=90,time=30,stamina=100;
        private long xp,gold;
        private float recoverySeconds=60;
        [MenuItem("Wildfolk/Developer/Open Test Console")]
        private static void Open(){var window=CreateInstance<WildfolkDeveloperWindow>();window.titleContent=new GUIContent("Wildfolk Test Console");window.Show();}
        private void Execute(Action action){try{action();message="Applied";}catch(Exception error){message=error.Message;}Repaint();}
        private void Section(string title){EditorGUILayout.Space(14);EditorGUILayout.LabelField(title,EditorStyles.boldLabel);}
        private void OnGUI()
        {
            scroll=EditorGUILayout.BeginScrollView(scroll);EditorGUILayout.HelpBox("Editor Play Mode only. Changes disable automatic saves. Save explicitly to keep a test state.",MessageType.Info);
            Section("Player");hp=EditorGUILayout.IntField("HP",hp);if(GUILayout.Button("Set HP"))Execute(()=>DeveloperHarness.SetHealth(hp));
            hunger=EditorGUILayout.FloatField("Hunger",hunger);thirst=EditorGUILayout.FloatField("Thirst",thirst);if(GUILayout.Button("Set Needs"))Execute(()=>DeveloperHarness.SetNeeds(hunger,thirst));
            xp=EditorGUILayout.LongField("Total earned XP",xp);gold=EditorGUILayout.LongField("Gold",gold);if(GUILayout.Button("Set XP / Gold (reset skills)"))Execute(()=>DeveloperHarness.SetProgression(xp,gold));
            level=EditorGUILayout.IntField("Level",level);if(GUILayout.Button("Set Level (reset skills)"))Execute(()=>DeveloperHarness.SetLevel(level));
            points=EditorGUILayout.IntField("Unspent Skill Points",points);if(GUILayout.Button("Set Points (sets matching level, resets skills)"))Execute(()=>DeveloperHarness.SetSkillPoints(points));
            stamina=EditorGUILayout.FloatField("Stamina",stamina);if(GUILayout.Button("Set Stamina"))Execute(()=>DeveloperHarness.SetStamina(stamina));
            Section("Equipment");foreach(var iron in new[]{false,true}){if(GUILayout.Button("Give "+(iron ? "Iron" : "Leather")+" Set"))Execute(()=>DeveloperHarness.GiveArmorSet(iron,false));if(GUILayout.Button("Give & Equip "+(iron ? "Iron" : "Leather")+" Set"))Execute(()=>DeveloperHarness.GiveArmorSet(iron,true));}
            if(GUILayout.Button("Clear Armor Into Inventory"))Execute(DeveloperHarness.ClearArmor);
            item=EditorGUILayout.TextField("Item ID",item);quantity=EditorGUILayout.IntField("Quantity",quantity);if(GUILayout.Button("Give Item"))Execute(()=>DeveloperHarness.GiveItem(item,quantity));if(GUILayout.Button("Give All Tools"))Execute(DeveloperHarness.GiveAllEquipment);
            Section("Progression");for(var rank=1;rank<=3;rank++){var index=rank;var label=rank==1 ? "Early" : rank==2 ? "Stone" : "Iron";if(GUILayout.Button(label+" Profile"))Execute(()=>DeveloperHarness.LoadProfile(AssetDatabase.LoadAssetAtPath<DeveloperTestProfile>("Assets/Wildfolk/Data/DeveloperProfile"+index+".asset")));}
            if(GUILayout.Button("Workbench II Profile"))Execute(DeveloperHarness.WorkbenchIIProfile);if(GUILayout.Button("Combat Sandbox"))Execute(DeveloperHarness.OpenCombatSandbox);
            Section("World");day=EditorGUILayout.IntField("Day",day);time=EditorGUILayout.FloatField("Cycle Time",time);if(GUILayout.Button("Set Day / Time"))Execute(()=>DeveloperHarness.SetTime(day,time));
            spawnPosition=EditorGUILayout.Vector2Field("Position",spawnPosition);if(GUILayout.Button("Teleport"))Execute(()=>DeveloperHarness.Teleport(spawnPosition));foreach(var kind in new[]{"Zombie","Goblin","Chicken","Boar"})if(GUILayout.Button("Spawn "+kind))Execute(()=>DeveloperHarness.Spawn(kind,spawnPosition));
            if(GUILayout.Button("Clear Nearby Enemies"))Execute(()=>DeveloperHarness.ClearEnemies());seed=EditorGUILayout.IntField("Seed",seed);if(GUILayout.Button("Generate With Seed"))Execute(()=>DeveloperHarness.GenerateWorld(seed));
            Section("Resource Population");if(GUILayout.Button("Force Recovery Tick"))Execute(()=>DeveloperHarness.ForceResourceRecovery());foreach(var seconds in new[]{60,300,600}){var delta=seconds;if(GUILayout.Button("Advance +"+delta/60+"m"))Execute(()=>DeveloperHarness.AdvanceResourceRecovery(delta));}recoverySeconds=EditorGUILayout.FloatField("Custom seconds",recoverySeconds);if(GUILayout.Button("Advance custom"))Execute(()=>DeveloperHarness.AdvanceResourceRecovery(recoverySeconds));if(GUILayout.Button("Show Population / Targets / Pending / Exclusions")){try{message=DeveloperHarness.ResourcePopulationReport();}catch(System.Exception error){message=error.Message;}}
            Section("Save");if(GUILayout.Button("Save"))Execute(()=>{var s=DeveloperHarness.Context();if(!s.Save())throw new InvalidOperationException(s.LastError);});if(GUILayout.Button("Load"))Execute(()=>DeveloperHarness.Context().Load());if(GUILayout.Button("New Test State"))Execute(DeveloperHarness.NewTestState);
            Section("Validation");if(GUILayout.Button("Validate Data"))Execute(()=>{var errors=ProjectDataValidator.Validate();if(errors.Count>0)throw new InvalidOperationException(string.Join("\n",errors));});
            if(EditorApplication.isPlaying){var combat=UnityEngine.Object.FindFirstObjectByType<PlayerCombat>();if(combat!=null)EditorGUILayout.LabelField("Last attack",combat.LastHitCount+" targets / "+combat.LastDamage+" damage");}
            EditorGUILayout.HelpBox(message ?? "Choose a tool",MessageType.None);EditorGUILayout.EndScrollView();
        }
    }
}
