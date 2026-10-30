using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace Wildfolk.Editor
{
    // Release packaging only. Secrets are supplied directly to Gradle, never to Unity settings.
    public static class InternalTestBuild
    {
        [Serializable]private sealed class PreflightResult
        {
            public bool Passed,Development,ScriptDebugging,AutoconnectProfiler,DeepProfiling,BackendEnabled,AdsEnabled,PurchaseEnabled,Fixtures;
            public string UnityVersion,Package,DisplayName,VersionName,Channel,EnvironmentName,EnvironmentId,ScriptingBackend,Architecture;
            public int VersionCode,MinSdk,TargetSdk,AudioClips;public string[] Errors;
        }
        private static string Results
        {
            get{var path=Environment.GetEnvironmentVariable("LOWFIRE_RC1_RESULTS");if(string.IsNullOrWhiteSpace(path)||!Path.IsPathRooted(path)||path.Any(c=>c>127))throw new InvalidOperationException("Explicit ASCII result path required");Directory.CreateDirectory(path);return path;}
        }
        public static void Preflight()
        {
            var config=BackendSettings.Current;var named=NamedBuildTarget.Android;
            var errors=ProjectDataValidator.Validate();var defines=PlayerSettings.GetScriptingDefineSymbols(named).Split(';');
            if(EditorUserBuildSettings.activeBuildTarget!=BuildTarget.Android||BackendPolicy.Channel!=BuildChannel.Alpha||!defines.Contains("LOWFIRE_ALPHA")||defines.Contains("LOWFIRE_PRODUCTION"))errors.Add("Exact Android Alpha channel required");
            errors.AddRange(BackendBuildValidator.Issues(config,BuildChannel.Alpha,false));
            if(config.Enabled||config.AllowDevelopmentIdentity||RewardedSettings.Load().Enabled||RewardedSettings.Load().ProductionLiveEnabled||StoreSettings.Load().PurchaseEnabled||StoreSettings.Load().ProductionLiveEnabled)errors.Add("RC1 account/ads/purchase activation must remain OFF");
            if(PlayerSettings.GetApplicationIdentifier(named)!="com.gltam.lowfire"||PlayerSettings.productName!="Lowfire")errors.Add("RC1 identity mismatch");
            if(PlayerSettings.bundleVersion!="1.0.0-rc1"||PlayerSettings.Android.bundleVersionCode!=2)errors.Add("RC1 version mismatch");
            if(PlayerSettings.GetScriptingBackend(named)!=ScriptingImplementation.IL2CPP||PlayerSettings.Android.targetArchitectures!=AndroidArchitecture.ARM64)errors.Add("IL2CPP ARM64 only required");
            if((int)PlayerSettings.Android.minSdkVersion!=25||(int)PlayerSettings.Android.targetSdkVersion!=36)errors.Add("Explicit min25/target36 required");
            if(PlayerSettings.Android.useCustomKeystore)errors.Add("Use environment-only Gradle upload signing, not Unity credential serialization");
            var template=File.ReadAllText("Assets/Plugins/Android/launcherTemplate.gradle");
            if(!template.Contains("LOWFIRE_UPLOAD_PASSWORD")||template.Contains("signingConfig signingConfigs.debug")||template.Contains("**SIGNCONFIG**"))errors.Add("Release signing template must have no debug fallback");
            var bank=Resources.Load<AudioBank>("Audio/LowfireAudio");var clips=bank==null?Array.Empty<AudioClip>():bank.Sounds.SelectMany(s=>s.Clips).Concat(bank.MusicBeds).Concat(bank.EnvironmentBeds).Append(bank.Fire).Distinct().ToArray();
            if(bank==null||bank.Mixer==null||clips.Length!=101||clips.Any(c=>c==null))errors.Add("Complete 101-clip AudioBank/mixer required");
            EditorUserBuildSettings.development=false;EditorUserBuildSettings.allowDebugging=false;EditorUserBuildSettings.connectProfiler=false;EditorUserBuildSettings.buildWithDeepProfilingSupport=false;UnityEditor.Android.UserBuildSettings.DebugSymbols.level=Unity.Android.Types.DebugSymbolLevel.Full;UnityEditor.Android.UserBuildSettings.DebugSymbols.format=Unity.Android.Types.DebugSymbolFormat.Zip;
            var env=BackendPolicy.RequireEnvironment(config,BuildChannel.Alpha);
            var value=new PreflightResult{Passed=errors.Count==0,UnityVersion=Application.unityVersion,Package=PlayerSettings.GetApplicationIdentifier(named),DisplayName=PlayerSettings.productName,VersionName=PlayerSettings.bundleVersion,VersionCode=PlayerSettings.Android.bundleVersionCode,MinSdk=(int)PlayerSettings.Android.minSdkVersion,TargetSdk=(int)PlayerSettings.Android.targetSdkVersion,Channel=BackendPolicy.Channel.ToString(),EnvironmentName=env.Name,EnvironmentId=env.Id,ScriptingBackend=PlayerSettings.GetScriptingBackend(named).ToString(),Architecture=PlayerSettings.Android.targetArchitectures.ToString(),AudioClips=clips.Length,BackendEnabled=config.Enabled,AdsEnabled=RewardedSettings.Load().Enabled,PurchaseEnabled=StoreSettings.Load().PurchaseEnabled,Fixtures=BackendPolicy.Fixtures,Development=EditorUserBuildSettings.development,ScriptDebugging=EditorUserBuildSettings.allowDebugging,AutoconnectProfiler=EditorUserBuildSettings.connectProfiler,DeepProfiling=EditorUserBuildSettings.buildWithDeepProfilingSupport,Errors=errors.ToArray()};
            File.WriteAllText(Path.Combine(Results,"unity-preflight.json"),JsonUtility.ToJson(value,true));
            if(errors.Count>0)throw new BuildFailedException(string.Join("\n",errors));
        }
        public static void CheapAndroid()
        {Preflight();AccountCompileQa.Android();}
        public static void Export()
        {
            if(!Application.isBatchMode)throw new InvalidOperationException("Isolated batch export required");
            var project=Path.GetFullPath(Path.Combine(Application.dataPath,".."));var expected=Environment.GetEnvironmentVariable("LOWFIRE_RC1_PROJECT");
            if(string.IsNullOrEmpty(expected)||!string.Equals(project,Path.GetFullPath(expected),StringComparison.OrdinalIgnoreCase)||project.Any(c=>c>127))throw new InvalidOperationException("Explicit ASCII isolated project required");
            Preflight();var export=Environment.GetEnvironmentVariable("LOWFIRE_RC1_EXPORT");
            if(string.IsNullOrWhiteSpace(export)||!Path.IsPathRooted(export)||export.Any(c=>c>127))throw new InvalidOperationException("Explicit ASCII export path required");
            EditorUserBuildSettings.exportAsGoogleAndroidProject=true;EditorUserBuildSettings.buildAppBundle=true;
            var started=DateTime.UtcNow;var timer=System.Diagnostics.Stopwatch.StartNew();
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),target=BuildTarget.Android,locationPathName=export,options=BuildOptions.AcceptExternalModificationsToPlayer});
            timer.Stop();File.WriteAllText(Path.Combine(Results,"unity-export.json"),"{\"result\":\""+report.summary.result+"\",\"startedUTC\":\""+started.ToString("O")+"\",\"finishedUTC\":\""+DateTime.UtcNow.ToString("O")+"\",\"wallSeconds\":"+timer.Elapsed.TotalSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture)+",\"errors\":"+report.summary.totalErrors+",\"warnings\":"+report.summary.totalWarnings+"}");
            if(report.summary.result!=BuildResult.Succeeded)throw new BuildFailedException("RC1 Gradle export failed");
        }
    }
}
