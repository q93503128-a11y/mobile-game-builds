using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace Wildfolk.Editor
{
    // Reauthors only existing UI from its canonical builders; gameplay/data/prefabs are retained.
    public static class UiPresentationBuilder
    {
        [MenuItem("Wildfolk/Build UI Presentation")]
        public static void Build()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Clean edit mode required");
            FrontendSceneBuilder.Build();JournalContentAssets.Rebuild();EconomyUiBuilder.Build();
            foreach(var path in new[]{FrontendSceneBuilder.ScenePath,GameplaySceneBuilder.ScenePath,MultiplayerSceneBuilder.ScenePath})
            {
                var scene=EditorSceneManager.OpenScene(path);
                foreach(var old in UnityEngine.Object.FindObjectsByType<VirtualJoystick>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                {
                    var next=GameplayHudBuilder.BuildJoystick(old.transform.parent,old.Session);
                    foreach(var motor in UnityEngine.Object.FindObjectsByType<PlayerMotor>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(motor.Session==old.Session)motor.Joystick=next;
                    foreach(var adapter in UnityEngine.Object.FindObjectsByType<PlayerInputAdapter>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(adapter.GetComponent<PlayerMotor>().Session==old.Session)adapter.Joystick=next;
                    foreach(var hud in UnityEngine.Object.FindObjectsByType<GameplayHud>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(hud.Joystick==old)hud.Joystick=next;
                    foreach(var layout in UnityEngine.Object.FindObjectsByType<ControlSizeLayout>(FindObjectsInactive.Include,FindObjectsSortMode.None))layout.Joystick=next.Visual;
                    foreach(var arena in UnityEngine.Object.FindObjectsByType<MultiplayerArena>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(arena.Joystick==old){arena.Joystick=next;next.gameObject.SetActive(false);}
                    UnityEngine.Object.DestroyImmediate(old.gameObject);
                }
                foreach(var hotbar in UnityEngine.Object.FindObjectsByType<HotbarView>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                {
                    var parent=hotbar.transform.parent;var owner=hotbar.Owner;var session=hotbar.Session;var wasActive=hotbar.gameObject.activeSelf;UnityEngine.Object.DestroyImmediate(hotbar.gameObject);
                    var next=GameplayHudBuilder.BuildHotbar(parent,session,owner);next.gameObject.SetActive(wasActive);
                    foreach(var layout in UnityEngine.Object.FindObjectsByType<ControlSizeLayout>(FindObjectsInactive.Include,FindObjectsSortMode.None))layout.Hotbar=(RectTransform)next.transform;
                }
                foreach(var pause in UnityEngine.Object.FindObjectsByType<PauseMenu>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                {var parent=pause.Settings.transform.parent;UnityEngine.Object.DestroyImmediate(pause.Settings);pause.Settings=SurvivalUiBuilder.Settings(parent,pause.Preferences,pause.Back);pause.Settings.SetActive(false);}
                foreach(var hud in UnityEngine.Object.FindObjectsByType<DeathHud>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                {var modal=hud.DeadView.transform.parent;var safe=hud.DownView.transform.parent;var player=hud.Player;UnityEngine.Object.DestroyImmediate(hud.DeadView);UnityEngine.Object.DestroyImmediate(hud.DownView);DeathUiBuilder.Build(hud.gameObject,modal,safe,player);}
                GameplayHudBuilder.ConfigureTouchLayout();
                foreach(var button in UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(button.GetComponent<AudioButton>()==null)button.gameObject.AddComponent<AudioButton>();
                foreach(var component in UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(PrefabUtility.IsPartOfPrefabInstance(component))PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();EditorSceneManager.OpenScene(FrontendSceneBuilder.ScenePath);
        }
    }
}
