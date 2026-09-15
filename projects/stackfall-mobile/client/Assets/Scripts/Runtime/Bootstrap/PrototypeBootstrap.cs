using StackfallMobile.Runtime.App;
using UnityEngine;

namespace StackfallMobile.Runtime.Bootstrap
{
    public sealed class PrototypeBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureRuntime()
        {
            if (FindFirstObjectByType<StackfallAppController>() != null)
            {
                return;
            }

            var root = new GameObject("StackfallRuntime");
            DontDestroyOnLoad(root);
            root.AddComponent<StackfallAppController>().Initialize();
        }
    }
}
