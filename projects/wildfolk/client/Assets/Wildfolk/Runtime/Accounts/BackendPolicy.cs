using System;
using UnityEngine;
namespace Wildfolk
{
    public enum BuildChannel { Development,Alpha,Production }
    [Serializable]public sealed class BackendEnvironment { public string Channel,Name,Id; }
    [Serializable]public sealed class BackendSettings
    {
        public bool Enabled,AllowDevelopmentIdentity;
        public string PrivacyUrl,DeletionWebUrl,GoogleWebClientId,GoogleCloudProjectNumber;
        public BackendEnvironment[] Environments;
        public static BackendSettings Current=>JsonUtility.FromJson<BackendSettings>(Resources.Load<TextAsset>("BackendSettings").text);
        public BackendEnvironment Environment=>Array.Find(Environments,e=>e.Channel==BackendPolicy.Channel.ToString());
    }
    public static class BackendPolicy
    {
        public static BuildChannel Channel {
            get {
#if LOWFIRE_ALPHA
                return BuildChannel.Alpha;
#elif LOWFIRE_PRODUCTION
                return BuildChannel.Production;
#elif UNITY_EDITOR || DEVELOPMENT_BUILD
                return BuildChannel.Development;
#else
                return BuildChannel.Production;
#endif
            }
        }
        public static bool Fixtures=>Channel==BuildChannel.Development && Debug.isDebugBuild;
        public static bool RealMoneyPurchase=>false;
        public static bool AlphaEnrollment=>Channel==BuildChannel.Alpha;
        public static bool ValidWeb(string value)=>Uri.TryCreate(value,UriKind.Absolute,out var uri)&&uri.Scheme=="https"&&!uri.IsLoopback&&string.IsNullOrEmpty(uri.UserInfo);
    }
}
