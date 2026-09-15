using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace StackfallMobile.Editor
{
    internal static class StackfallProjectBootstrapEditor
    {
        private const string SceneFolder = "Assets/Scenes";
        private const string BootstrapScenePath = "Assets/Scenes/Bootstrap.unity";

        [InitializeOnLoadMethod]
        private static void ScheduleEnsureProjectEntry()
        {
            EditorApplication.delayCall += EnsureProjectEntry;
        }

        private static void EnsureProjectEntry()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (!AssetDatabase.IsValidFolder(SceneFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(BootstrapScenePath) == null)
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                EditorSceneManager.SaveScene(scene, BootstrapScenePath);
                EditorSceneManager.CloseScene(scene, true);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            var current = EditorBuildSettings.scenes;
            if (current.Length > 0 && current[0].enabled && current[0].path == BootstrapScenePath)
            {
                return;
            }

            var remaining = current.Where(scene => scene.path != BootstrapScenePath).ToList();
            remaining.Insert(0, new EditorBuildSettingsScene(BootstrapScenePath, true));
            EditorBuildSettings.scenes = remaining.ToArray();
        }
    }
}
