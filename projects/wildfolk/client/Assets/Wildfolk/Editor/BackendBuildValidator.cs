using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
namespace Wildfolk.Editor
{
    public sealed class BackendBuildValidator:IPreprocessBuildWithReport
    {
        public int callbackOrder=>-500;
        public static List<string> Issues(BackendSettings config,BuildChannel channel,bool developmentBuild)
        {
            var issues=new List<string>();var environment=Array.Find(config.Environments,e=>e.Channel==channel.ToString());
            if(channel!=BuildChannel.Development&&config.AllowDevelopmentIdentity)issues.Add("Development identity is forbidden outside Development");
            if(channel!=BuildChannel.Development&&developmentBuild)issues.Add("Alpha/Production may not use Development build fixtures");
            if(channel==BuildChannel.Development)return issues;
            if(!config.Enabled)issues.Add("Backend configuration is not enabled");
            if(environment==null||!Guid.TryParse(environment.Id,out _)||string.IsNullOrWhiteSpace(environment.Name))issues.Add("Verified channel/environment mapping required");
            if(!BackendPolicy.ValidWeb(config.PrivacyUrl))issues.Add("Public HTTPS privacy policy required");
            if(!BackendPolicy.ValidWeb(config.DeletionWebUrl))issues.Add("Public HTTPS deletion request page required");
            if(string.IsNullOrEmpty(config.GoogleWebClientId)||!long.TryParse(config.GoogleCloudProjectNumber,out var number)||number<=0)issues.Add("Google Play Games / Integrity configuration required");
            return issues;
        }
        public void OnPreprocessBuild(BuildReport report)
        {
            var issues=Issues(BackendSettings.Current,BackendPolicy.Channel,(report.summary.options&BuildOptions.Development)!=0);
            if(BackendPolicy.Channel!=BuildChannel.Development&&report.summary.platform!=BuildTarget.Android)issues.Add("Launch account provider currently supports Android only");
            if(BackendPolicy.Channel!=BuildChannel.Development&&!File.Exists("Assets/Plugins/Android/GooglePlayGamesManifest.androidlib/AndroidManifest.xml"))issues.Add("PGS Android setup manifest is missing");
            if(issues.Count>0)throw new BuildFailedException(string.Join("\n",issues));
        }
    }
}
