using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Player;
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
    }
}
