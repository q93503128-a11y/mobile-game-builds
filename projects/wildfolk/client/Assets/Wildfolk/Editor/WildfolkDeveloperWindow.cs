using System;
using UnityEditor;
using UnityEngine;
namespace Wildfolk.Editor
{
    public sealed class WildfolkDeveloperWindow : EditorWindow
    {
        private Vector2 scroll,spawnPosition;
        private string item="wood",message;
        private int quantity=30,hp=100,day=1,seed=9472026;
        private float hunger=90,thirst=90,time=30;
        private long xp,gold;
        private DeveloperTestProfile profile;
        [MenuItem("Wildfolk/Developer/Test Harness")]
        private static void Open(){var window=CreateInstance<WildfolkDeveloperWindow>();window.titleContent=new GUIContent("Wildfolk Developer");window.Show();}
        private void Execute(Action action){try{action();message="Applied";}catch(Exception error){message=error.Message;}Repaint();}
        private void OnGUI()
        {
            scroll=EditorGUILayout.BeginScrollView(scroll);EditorGUILayout.HelpBox("Editor Play Mode tools. Mutations disable automatic saves for this session. Use Save explicitly to keep a test setup.",MessageType.Info);
            item=EditorGUILayout.TextField("Item ID",item);quantity=EditorGUILayout.IntField("Quantity",quantity);
            if(GUILayout.Button("Give Item"))Execute(()=>DeveloperHarness.GiveItem(item,quantity));if(GUILayout.Button("Give All Current Equipment"))Execute(DeveloperHarness.GiveAllEquipment);
            hp=EditorGUILayout.IntField("HP",hp);if(GUILayout.Button("Set HP"))Execute(()=>DeveloperHarness.SetHealth(hp));
            hunger=EditorGUILayout.FloatField("Hunger",hunger);thirst=EditorGUILayout.FloatField("Thirst",thirst);if(GUILayout.Button("Set Hunger / Thirst"))Execute(()=>DeveloperHarness.SetNeeds(hunger,thirst));
            xp=EditorGUILayout.LongField("XP",xp);gold=EditorGUILayout.LongField("Gold",gold);if(GUILayout.Button("Set XP / Gold"))Execute(()=>DeveloperHarness.SetProgression(xp,gold));
            day=EditorGUILayout.IntField("Day",day);time=EditorGUILayout.FloatField("Cycle Time",time);if(GUILayout.Button("Set Day / Time"))Execute(()=>DeveloperHarness.SetTime(day,time));
            spawnPosition=EditorGUILayout.Vector2Field("Position",spawnPosition);if(GUILayout.Button("Teleport Player"))Execute(()=>DeveloperHarness.Teleport(spawnPosition));
            foreach(var kind in new[]{"Zombie","Goblin","Chicken","Boar"})if(GUILayout.Button("Spawn "+kind))Execute(()=>DeveloperHarness.Spawn(kind,spawnPosition));
            if(GUILayout.Button("Clear Nearby Enemies"))Execute(()=>DeveloperHarness.ClearEnemies());
            seed=EditorGUILayout.IntField("Seed",seed);if(GUILayout.Button("Generate World With Seed"))Execute(()=>DeveloperHarness.GenerateWorld(seed));if(GUILayout.Button("Regenerate Test World"))Execute(DeveloperHarness.Regenerate);
            profile=(DeveloperTestProfile)EditorGUILayout.ObjectField("Test Profile",profile,typeof(DeveloperTestProfile),false);if(GUILayout.Button("Load Test Profile"))Execute(()=>{if(profile==null)throw new ArgumentException("Choose a test profile.");DeveloperHarness.LoadProfile(profile);});
            if(GUILayout.Button("Open Combat Sandbox"))Execute(DeveloperHarness.OpenCombatSandbox);
            if(GUILayout.Button("Save"))Execute(()=>{var s=DeveloperHarness.Context();if(!s.Save())throw new InvalidOperationException(s.LastError);});if(GUILayout.Button("Load"))Execute(()=>DeveloperHarness.Context().Load());
            if(GUILayout.Button("Validate Data"))Execute(()=>{var errors=ProjectDataValidator.Validate();message=errors.Count==0 ? "Data valid" : string.Join("\n",errors);});
            if(EditorApplication.isPlaying){var combat=UnityEngine.Object.FindFirstObjectByType<PlayerCombat>();if(combat!=null)EditorGUILayout.LabelField("Last attack",combat.LastHitCount+" targets / "+combat.LastDamage+" damage");}
            EditorGUILayout.HelpBox(message ?? "Choose a tool",MessageType.None);EditorGUILayout.EndScrollView();
        }
    }
}
