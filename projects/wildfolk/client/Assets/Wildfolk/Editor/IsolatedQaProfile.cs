using System;
using System.IO;
using UnityEditor;
namespace Wildfolk.Editor
{
    public static class IsolatedQaProfile
    {
        [InitializeOnLoadMethod]private static void BatchIsolation(){if(UnityEngine.Application.isBatchMode && string.IsNullOrEmpty(UnityEditor.SessionState.GetString("Wildfolk.QA.Directory","")))UnityEditor.SessionState.SetString("Wildfolk.QA.Directory",Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../Captures/Codex_QA_ContentExpansion_Batch")));}
        public static string Begin(string label)
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Choose the profile before entering Play.");
            var root=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../Captures",label));
            System.IO.Directory.CreateDirectory(root);UnityEditor.SessionState.SetString("Wildfolk.QA.Directory",root);return root;
        }
        public static void End()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play before leaving the isolated profile.");
            UnityEditor.SessionState.EraseString("Wildfolk.QA.Directory");
        }
    }
}
