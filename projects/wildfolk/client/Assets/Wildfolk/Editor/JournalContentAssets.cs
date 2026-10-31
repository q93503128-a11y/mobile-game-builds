using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace Wildfolk.Editor
{
    public static class JournalContentAssets
    {
        public static void Rebuild()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play first");
            VisualArtImporter.Import();var scene=EditorSceneManager.OpenScene("Assets/Wildfolk/Scenes/MainMenu.unity");var home=UnityEngine.Object.FindFirstObjectByType<HomeScreen>();
            var old=home.GetComponent<ClassLobbyScreen>();if(old!=null){UnityEngine.Object.DestroyImmediate(old.View);UnityEngine.Object.DestroyImmediate(old);}
            foreach(var b in home.GetComponentsInChildren<Button>(true))if(b.name=="CLASS" || b.name=="Class Journal Entry" || b.name=="Class Bookmark")UnityEngine.Object.DestroyImmediate(b.gameObject);
            ClassContentAssets.BuildLobby(home);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            foreach(var path in new[]{"Assets/Wildfolk/Scenes/Wildfolk.unity","Assets/Wildfolk/Scenes/Multiplayer.unity"})
            {
                scene=EditorSceneManager.OpenScene(path);var screen=UnityEngine.Object.FindFirstObjectByType<CharacterScreen>(FindObjectsInactive.Include);var tree=UnityEngine.Object.FindFirstObjectByType<SkillTreeScreen>(FindObjectsInactive.Include);
                var root=screen.gameObject;var safe=screen.View.transform.parent;var player=screen.Player;var session=screen.Session;
                UnityEngine.Object.DestroyImmediate(screen.View);UnityEngine.Object.DestroyImmediate(tree.View);UnityEngine.Object.DestroyImmediate(screen);UnityEngine.Object.DestroyImmediate(tree);
                CharacterUiBuilder.BuildInformation(root,safe,session,player);typeof(CharacterUiBuilder).GetMethod("BuildTree",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{root,safe,session,player.GetComponent<PlayerProgression>()});
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();EditorSceneManager.OpenScene("Assets/Wildfolk/Scenes/MainMenu.unity");
        }

    }
}
