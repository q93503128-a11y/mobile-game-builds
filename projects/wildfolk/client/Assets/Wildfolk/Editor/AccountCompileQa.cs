using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Player;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace Wildfolk.Editor
{
    public static class AccountCompileQa
    {
        [Serializable]private sealed class Result {public bool Passed,AlphaFixtureExcluded,ProductionFixtureExcluded;public int AlphaAssemblies,ProductionAssemblies;public double Seconds;public string Error;}
        public static void Android()
        {
            var destination=Environment.GetEnvironmentVariable("LOWFIRE_BACKEND_RESULTS");if(string.IsNullOrEmpty(destination))throw new InvalidOperationException("Explicit QA result directory required");Directory.CreateDirectory(destination);
            var result=new Result();var timer=System.Diagnostics.Stopwatch.StartNew();
            try{
                foreach(var channel in new[]{"ALPHA","PRODUCTION"}){
                    var folder=Path.Combine(destination,"android-"+channel.ToLowerInvariant());Directory.CreateDirectory(folder);
                    var settings=new ScriptCompilationSettings{target=BuildTarget.Android,group=BuildTargetGroup.Android,options=ScriptCompilationOptions.None,extraScriptingDefines=new[]{"LOWFIRE_"+channel}};
                    var compiled=PlayerBuildInterface.CompilePlayerScripts(settings,folder);
                    var runtime=compiled.assemblies.FirstOrDefault(p=>Path.GetFileName(p)=="Wildfolk.Runtime.dll");
                    if(runtime==null)throw new InvalidOperationException("Android runtime assembly missing");
                    var file=File.Exists(runtime)?runtime:Path.Combine(folder,Path.GetFileName(runtime));
                    var native=compiled.assemblies.Any(p=>Path.GetFileName(p)=="Wildfolk.AndroidAccounts.dll");
                    var excluded=!System.Text.Encoding.UTF8.GetString(File.ReadAllBytes(file)).Contains("DevelopmentIdentityProvider");
                    if(!native||!excluded)throw new InvalidOperationException("Android provider missing or development adapter included");
                    if(channel=="ALPHA"){result.AlphaAssemblies=compiled.assemblies.Count;result.AlphaFixtureExcluded=excluded;}else{result.ProductionAssemblies=compiled.assemblies.Count;result.ProductionFixtureExcluded=excluded;}
                }result.Passed=true;
            }catch(Exception e){result.Error=e.GetType().Name+": "+e.Message;throw;}
            finally{timer.Stop();result.Seconds=timer.Elapsed.TotalSeconds;File.WriteAllText(Path.Combine(destination,"android-scripts.json"),JsonUtility.ToJson(result,true));}
        }
        [Serializable]private sealed class BundleResult
        {
            public bool Passed,Development,BackendEnabled,CustomKeystore;
            public string Package,EnvironmentName,EnvironmentId,Backend,Architecture,Result,Error;
            public string[] MissingPgsConfiguration;
            public double Seconds;public ulong Bytes;public int Errors,Warnings;
        }
        public static void DevelopmentBundle()
        {
            var destination=Environment.GetEnvironmentVariable("LOWFIRE_BACKEND_RESULTS");
            var isolatedProject=Environment.GetEnvironmentVariable("LOWFIRE_QA_PROJECT");
            var project=Path.GetFullPath(Path.Combine(Application.dataPath,".."));
            if(!Application.isBatchMode||string.IsNullOrEmpty(destination)||string.IsNullOrEmpty(isolatedProject)||!string.Equals(project,Path.GetFullPath(isolatedProject),StringComparison.OrdinalIgnoreCase)||!project.Replace('\\','/').EndsWith("/tests/artifacts/unity",StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Explicit isolated batch QA project and result directory required");
            if(project.Any(c=>c>127)||Path.GetFullPath(destination).Any(c=>c>127))throw new InvalidOperationException("Android tools require ASCII project and result paths");
            Directory.CreateDirectory(destination);var result=new BundleResult();var timer=System.Diagnostics.Stopwatch.StartNew();
            var previousBundle=EditorUserBuildSettings.buildAppBundle;
            try{
                var config=BackendSettings.Current;
                var defines=PlayerSettings.GetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.Android).Split(';');
                if(EditorUserBuildSettings.activeBuildTarget!=BuildTarget.Android||defines.Contains("LOWFIRE_ALPHA")||defines.Contains("LOWFIRE_PRODUCTION"))throw new InvalidOperationException("Android Development channel required");
                if(config.Enabled||config.AllowDevelopmentIdentity||PlayerSettings.Android.useCustomKeystore)throw new InvalidOperationException("Disabled backend/fixture identity and standard development signing required");
                var environment=BackendPolicy.RequireEnvironment(config,BuildChannel.Development);
                result.Development=true;result.BackendEnabled=config.Enabled;result.CustomKeystore=PlayerSettings.Android.useCustomKeystore;
                result.Package=PlayerSettings.GetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android);result.EnvironmentName=environment.Name;result.EnvironmentId=environment.Id;
                result.Backend=PlayerSettings.GetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android).ToString();result.Architecture=PlayerSettings.Android.targetArchitectures.ToString();
                result.MissingPgsConfiguration=BackendBuildValidator.PgsIssues(config).ToArray();
                if(result.Backend!="IL2CPP"||PlayerSettings.Android.targetArchitectures!=AndroidArchitecture.ARM64)throw new InvalidOperationException("Existing IL2CPP/ARM64 configuration required");
                var scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray();
                if(scenes.Length==0)throw new InvalidOperationException("Enabled product scenes required");
                EditorUserBuildSettings.buildAppBundle=true;
                var output=Path.Combine(destination,"Lowfire-development.aab");
                var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=scenes,locationPathName=output,target=BuildTarget.Android,options=BuildOptions.Development});
                result.Result=report.summary.result.ToString();result.Bytes=report.summary.totalSize;result.Errors=(int)report.summary.totalErrors;result.Warnings=(int)report.summary.totalWarnings;
                result.Passed=report.summary.result==BuildResult.Succeeded&&File.Exists(output);
                if(!result.Passed)throw new InvalidOperationException("Development bundle build "+result.Result);
            }catch(Exception e){result.Error=e.GetType().Name+": "+e.Message;throw;}
            finally{EditorUserBuildSettings.buildAppBundle=previousBundle;timer.Stop();result.Seconds=timer.Elapsed.TotalSeconds;File.WriteAllText(Path.Combine(destination,"android-development-bundle.json"),JsonUtility.ToJson(result,true));}
        }
    }
}
