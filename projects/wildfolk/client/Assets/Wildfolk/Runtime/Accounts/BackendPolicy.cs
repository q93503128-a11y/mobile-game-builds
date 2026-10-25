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
        public BackendEnvironment Environment=>BackendPolicy.RequireEnvironment(this,BackendPolicy.Channel);
    }
    public sealed class BackendSetupException:InvalidOperationException {public string Code{get;}public BackendSetupException(string code):base(code){Code=code;}}
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
        public static BackendEnvironment RequireEnvironment(BackendSettings config,BuildChannel channel)
        {
            var matches=Array.FindAll(config?.Environments??Array.Empty<BackendEnvironment>(),e=>e!=null&&e.Channel==channel.ToString());
            if(matches.Length!=1)throw new BackendSetupException("environment_missing");var environment=matches[0];
            if(!Guid.TryParse(environment.Id,out var id)||id==Guid.Empty||string.IsNullOrWhiteSpace(environment.Name)||!System.Text.RegularExpressions.Regex.IsMatch(environment.Name,"^[a-zA-Z0-9_-]{1,64}$"))throw new BackendSetupException("environment_missing");
            foreach(var other in config.Environments)if(other!=null&&other.Channel!=environment.Channel&&(other.Id==environment.Id||other.Name==environment.Name))throw new BackendSetupException("environment_mapping_conflict");
            return environment;
        }
        public static bool Fixtures=>Channel==BuildChannel.Development && Debug.isDebugBuild;
        public static bool RealMoneyPurchase=>false;
        public static bool AlphaEnrollment=>Channel==BuildChannel.Alpha;
        public static bool ValidWeb(string value)=>Uri.TryCreate(value,UriKind.Absolute,out var uri)&&uri.Scheme=="https"&&!uri.IsLoopback&&string.IsNullOrEmpty(uri.UserInfo);
    }
}
