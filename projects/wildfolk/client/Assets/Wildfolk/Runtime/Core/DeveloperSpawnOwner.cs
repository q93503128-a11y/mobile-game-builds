#if UNITY_EDITOR
using System;
using UnityEngine;
namespace Wildfolk
{
    // Instance lease ends before a pooled actor can be reused by natural population.
    public sealed class DeveloperSpawnOwner:MonoBehaviour
    {
        public Action<GameObject> Released;
        private void OnDisable(){var callback=Released;Released=null;callback?.Invoke(gameObject);}
    }
}
#endif
