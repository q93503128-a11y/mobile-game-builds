using UnityEngine;
namespace Wildfolk
{
    // Selected before entering Play; never redirects a release build or the player's profile.
    public static class DevelopmentProfile
    {
        public static string Directory
        {
            get
            {
#if UNITY_EDITOR
                return UnityEditor.SessionState.GetString("Wildfolk.QA.Directory", "");
#else
                return "";
#endif
            }
        }
        public static bool Isolated => !string.IsNullOrEmpty(Directory);
    }
}
