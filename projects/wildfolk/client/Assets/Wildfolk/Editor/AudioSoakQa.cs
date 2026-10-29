using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;
namespace Wildfolk.Editor
{
 [InitializeOnLoad]public static class AudioSoakQa
 {
  private static string Root=>Environment.GetEnvironmentVariable("LOWFIRE_AUDIO_RESULTS");
  static AudioSoakQa(){EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredPlayMode&&UnityEditor.SessionState.GetBool("Lowfire.AudioSoak.Run",false)){UnityEditor.SessionState.SetBool("Lowfire.AudioSoak.Run",false);var host=new GameObject("Isolated audio soak").AddComponent<DevelopmentCoroutineHost>();Object.DontDestroyOnLoad(host.gameObject);host.StartCoroutine(Guard());}};}
  public static void Run(){UnityEditor.SessionState.SetString("Wildfolk.QA.Directory",Path.Combine(Path.GetTempPath(),"LowfireAudioSoak",Guid.NewGuid().ToString("N")));UnityEditor.SessionState.SetBool("Lowfire.AudioSoak.Run",true);UnityEditor.SceneManagement.EditorSceneManager.OpenScene(FrontendSceneBuilder.ScenePath);Application.runInBackground=true;EditorApplication.isPlaying=true;}
  private static IEnumerator Guard(){var flow=Flow();while(true){bool more;object current;try{more=flow.MoveNext();current=more?flow.Current:null;}catch(Exception e){File.WriteAllText(Path.Combine(Root,"soak-failed.txt"),e.ToString());EditorApplication.Exit(1);yield break;}if(!more)break;yield return current;}EditorApplication.Exit(0);}
  [Serializable]private sealed class Result{public string StartUTC,EndUTC;public bool Passed,NonGraphicsDisabled,HumanListeningApproved;public double ElapsedSeconds,OutputRMS;public float OutputPeak;public int SourceCount,MaximumSources,MaximumVoices,Attempts,Accepted,Culled,OutputBuffers;public long ClipMemoryStart,ClipMemoryEnd;}
  private static IEnumerator Flow()
  {
   yield return null;WorldGenerator.SelectedSeed=9472026;WorldGenerator.SelectedGenerationVersion=WorldGenerationVersion.Current;SceneFlow.Repository=SaveRepository.Default();yield return SceneManager.LoadSceneAsync(SceneFlow.GameplayScene);yield return null;yield return null;ContentExpansionQa.Prepare();var save=ContentExpansionQa.S;var prefs=Object.FindFirstObjectByType<UserPreferences>();prefs.SetMaster(1);prefs.SetMusic(1);prefs.SetSfx(1);var runtime=AudioRuntime.Current;var player=save.Player;var health=player.GetComponent<Health>();var survival=player.GetComponent<PlayerSurvival>();var clips=runtime.Bank.Sounds.SelectMany(e=>e.Clips).Concat(runtime.Bank.MusicBeds).Concat(runtime.Bank.EnvironmentBeds).Append(runtime.Bank.Fire).Distinct().ToArray();var r=new Result{StartUTC=DateTime.UtcNow.ToString("O"),NonGraphicsDisabled=true,ClipMemoryStart=clips.Sum(c=>UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(c))};var start=Time.realtimeSinceStartup;var next=0f;var snapshot=0f;var region=-1;double power=0;var samples=new float[1024];var count=Enum.GetValues(typeof(Sound)).Length;
   while(Time.realtimeSinceStartup-start<1800)
   {
    var elapsed=Time.realtimeSinceStartup-start;health.Heal(health.Maximum);survival.Model.SetMeters(100,100);var nextRegion=(int)(elapsed/120)%4;if(nextRegion!=region){region=nextRegion;if(region==2||region==3){var cave=save.World.Map.Continental.Caves.First();var room=cave.Rooms.OrderBy(c=>c.Depth).ElementAt(region==2?0:cave.Rooms.Count()-1);ContentExpansionQa.Move(room.Center);}else{ContentExpansionQa.Move(Vector2.zero);save.Clock.Model.SetElapsed(region==0?1:save.Clock.Definition.DaySeconds+save.Clock.Definition.EveningSeconds+1);}}
    if(elapsed>=next){next=elapsed+.15f;AudioRuntime.Play((Sound)(r.Attempts++%count));}
    AudioListener.GetOutputData(samples,0);foreach(var v in samples){power+=v*v;r.OutputPeak=Mathf.Max(r.OutputPeak,Mathf.Abs(v));}r.OutputBuffers++;r.MaximumVoices=Math.Max(r.MaximumVoices,runtime.Voices.ActiveCount);
    if(elapsed>=snapshot){snapshot=elapsed+30;r.ElapsedSeconds=elapsed;r.SourceCount=runtime.GetComponentsInChildren<AudioSource>().Length;r.MaximumSources=Math.Max(r.MaximumSources,r.SourceCount);if(r.SourceCount!=30||Object.FindObjectsByType<AudioRuntime>(FindObjectsSortMode.None).Length!=1)throw new InvalidOperationException("Audio owner/source pool grew");File.WriteAllText(Path.Combine(Root,"soak-progress.json"),JsonUtility.ToJson(r,true));}yield return null;
   }
   r.EndUTC=DateTime.UtcNow.ToString("O");r.ElapsedSeconds=Time.realtimeSinceStartup-start;r.ClipMemoryEnd=clips.Sum(c=>UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(c));r.Accepted=runtime.Voices.Accepted;r.Culled=runtime.Voices.Culled;r.OutputRMS=Math.Sqrt(power/(r.OutputBuffers*samples.Length));r.Passed=r.ElapsedSeconds>=1800&&r.MaximumSources==30&&r.OutputPeak<.999f&&r.OutputRMS>0;File.WriteAllText(Path.Combine(Root,"soak.json"),JsonUtility.ToJson(r,true));if(!r.Passed)throw new InvalidOperationException("Actual30minute maximum-volume soak failed");
  }
 }
}
