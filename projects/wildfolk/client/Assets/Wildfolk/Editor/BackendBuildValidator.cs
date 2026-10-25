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
        public static List<string> PgsIssues(BackendSettings config)
        {
            var issues=new List<string>();var settings=UnityEngine.Resources.Load<UnityEngine.ScriptableObject>("PlayGamesSettings");
            if(settings==null)issues.Add("PGS PlayGamesSettings resource is missing");
            else {
                var serialized=new SerializedObject(settings);var web=serialized.FindProperty("mWebClientId");var app=serialized.FindProperty("mAppId");
                if(web==null||string.IsNullOrEmpty(web.stringValue)||web.stringValue!=config.GoogleWebClientId)issues.Add("PGS plugin Web client ID must match BackendSettings");
                if(app==null||!long.TryParse(app.stringValue,out var appId)||appId<=0)issues.Add("Actual PGS application ID required");
            }
            if(!File.Exists("Assets/Plugins/Android/GooglePlayGamesManifest.androidlib/AndroidManifest.xml"))issues.Add("PGS Android setup manifest is missing");
            return issues;
        }
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
            if(BackendPolicy.Channel!=BuildChannel.Development){var id=PlayerSettings.GetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android);if(string.IsNullOrWhiteSpace(id)||id.Contains("DefaultCompany")||!System.Text.RegularExpressions.Regex.IsMatch(id,@"^[a-zA-Z][a-zA-Z0-9_]*(\.[a-zA-Z][a-zA-Z0-9_]*)+$"))issues.Add("Real Android application identifier required");}
            if(BackendPolicy.Channel!=BuildChannel.Development&&report.summary.platform!=BuildTarget.Android)issues.Add("Launch account provider currently supports Android only");
            if(BackendPolicy.Channel!=BuildChannel.Development)issues.AddRange(PgsIssues(BackendSettings.Current));
            if(issues.Count>0)throw new BuildFailedException(string.Join("\n",issues));
        }
    }
}
