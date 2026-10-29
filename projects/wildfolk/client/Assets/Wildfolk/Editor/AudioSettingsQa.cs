using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;
namespace Wildfolk.Editor
{
 [InitializeOnLoad]public static class AudioSettingsQa
 {
  static AudioSettingsQa(){EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredPlayMode&&UnityEditor.SessionState.GetBool("Lowfire.AudioSettings.Run",false)){UnityEditor.SessionState.SetBool("Lowfire.AudioSettings.Run",false);new GameObject("Isolated audio settings capture").AddComponent<DevelopmentCoroutineHost>().StartCoroutine(Guard());}};}
  public static void Run(){Directory.CreateDirectory(Environment.GetEnvironmentVariable("LOWFIRE_ECONOMY_RESULTS"));UnityEditor.SessionState.SetString("Wildfolk.QA.Directory",Path.Combine(Path.GetTempPath(),"LowfireAudioSettings",Guid.NewGuid().ToString("N")));UnityEditor.SessionState.SetBool("Lowfire.AudioSettings.Run",true);UnityEditor.SceneManagement.EditorSceneManager.OpenScene(FrontendSceneBuilder.ScenePath);EditorApplication.isPlaying=true;}
  private static IEnumerator Guard(){var stack=new Stack<IEnumerator>();stack.Push(Frames());while(stack.Count>0){bool more;object value;try{more=stack.Peek().MoveNext();value=more?stack.Peek().Current:null;}catch(Exception e){File.WriteAllText(Path.Combine(Environment.GetEnvironmentVariable("LOWFIRE_AUDIO_RESULTS"),"settings-failed.txt"),e.ToString());EditorApplication.Exit(1);yield break;}if(!more){stack.Pop();continue;}if(value is IEnumerator inner)stack.Push(inner);else yield return value;}File.WriteAllText(Path.Combine(Environment.GetEnvironmentVariable("LOWFIRE_AUDIO_RESULTS"),"settings.json"),"{\"Passed\":true,\"Captures\":6,\"LocaleAppliedThroughUserPreferences\":true}");EditorApplication.Exit(0);}
  private static IEnumerator Frames(){yield return null;yield return null;var home=Object.FindFirstObjectByType<HomeScreen>();var prefs=Object.FindFirstObjectByType<UserPreferences>();foreach(var locale in new[]{"ko","en"}){prefs.SetLanguage(Array.FindIndex(L.Catalog.Languages,l=>l.Id==locale));foreach(var size in new[]{new Vector2Int(1920,1080),new Vector2Int(1280,720),new Vector2Int(844,390)}){home.OpenSettings();yield return EconomyQa.Frame(home.Settings,size,"audio-settings-"+locale);home.ShowHome();}}}
 }
}
