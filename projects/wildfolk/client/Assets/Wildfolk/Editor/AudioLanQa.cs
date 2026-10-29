using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Wildfolk.Editor
{
 // Isolated two-Editor fixture only. Uses the existing real transport/boss regression.
 [InitializeOnLoad]public static class AudioLanQa
 {
  private static readonly Dictionary<Sound,int> cues=new Dictionary<Sound,int>();
  private static string side;private static int maximumSources;private static float next;
  private static string Root=>Environment.GetEnvironmentVariable("LOWFIRE_AUDIO_RESULTS");
  static AudioLanQa(){EditorApplication.playModeStateChanged+=state=>{if(state!=PlayModeStateChange.EnteredPlayMode)return;side=UnityEditor.SessionState.GetString("Lowfire.AudioLan.Side","");if(string.IsNullOrEmpty(side))return;UnityEditor.SessionState.SetString("Lowfire.AudioLan.Side","");AudioRuntime.Played+=Hear;EditorApplication.update+=Tick;EditorApplication.quitting+=Write;if(side=="host")BossLanQa.Host();};}
  public static void Host(){UnityEditor.SessionState.SetString("Wildfolk.QA.Directory",Path.Combine(Path.GetTempPath(),"LowfireAudioLan",Guid.NewGuid().ToString("N")));UnityEditor.SessionState.SetString("Lowfire.AudioLan.Side","host");UnityEditor.SceneManagement.EditorSceneManager.OpenScene(FrontendSceneBuilder.ScenePath);EditorApplication.isPlaying=true;}
  public static void Client(){UnityEditor.SessionState.SetString("Lowfire.AudioLan.Side","client");BossLanQa.Client();}
  private static void Hear(Sound cue){cues.TryGetValue(cue,out var count);cues[cue]=count+1;}
  private static void Tick(){if(Time.realtimeSinceStartup<next)return;next=Time.realtimeSinceStartup+.5f;var r=AudioRuntime.Current;if(r!=null)maximumSources=Math.Max(maximumSources,r.GetComponentsInChildren<AudioSource>().Length);Write();if(side=="host"&&(BossLanQa.Phase=="complete"||BossLanQa.Phase=="failed"))EditorApplication.Exit(BossLanQa.Phase=="complete"?0:1);}
  private static void Write(){if(string.IsNullOrEmpty(side)||string.IsNullOrEmpty(Root))return;File.WriteAllText(Path.Combine(Root,"lan-"+side+".json"),JsonUtility.ToJson(new Result{Phase=BossLanQa.Phase,MaximumSources=maximumSources,Cues=cues.Select(p=>p.Key+"="+p.Value).ToArray(),Headless=true},true));}
  [Serializable]private sealed class Result{public string Phase;public int MaximumSources;public string[] Cues;public bool Headless;}
 }
}
