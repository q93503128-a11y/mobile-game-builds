using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Wildfolk
{
 // Owns routing/lifecycle. Voices and music/context have separate bounded responsibilities.
 public sealed class AudioRuntime:MonoBehaviour
 {
  public static AudioRuntime Current {get;private set;}
  public AudioBank Bank {get;private set;}public AudioVoices Voices {get;private set;}public AudioLandscape Landscape {get;private set;}
  public LocalSettings Settings {get;private set;}public Camera ListenerCamera {get;private set;}
  public bool Suspended=>background||unfocused||overlays>0;public static event Action<Sound> Played;
  private bool background,unfocused,started;private int overlays;
  public static AudioRuntime Ensure(){if(Current!=null)return Current;return new GameObject("Lowfire Audio").AddComponent<AudioRuntime>();}
  private void Awake()
  {
   if(Current!=null&&Current!=this){Destroy(gameObject);return;}Current=this;DontDestroyOnLoad(gameObject);Bank=Resources.Load<AudioBank>("Audio/LowfireAudio");
   if(Bank==null)throw new InvalidOperationException("Lowfire audio bank missing");
   Settings=LocalSettings.Load();Voices=new AudioVoices(transform,Bank);Landscape=gameObject.AddComponent<AudioLandscape>();SceneManager.sceneLoaded+=SceneLoaded;SceneLoaded(SceneManager.GetActiveScene(),LoadSceneMode.Single);
  }
  private void Start(){started=true;Apply(Settings);}
  private void SceneLoaded(Scene scene,LoadSceneMode mode){ListenerCamera=Camera.main;if(Landscape!=null)Landscape.BindScene();}
  public void Apply(LocalSettings settings)
  {Settings=settings;if(!started)return;Bank.Mixer.SetFloat("MasterVolume",ToDb(settings.Master));Bank.Mixer.SetFloat("MusicVolume",ToDb(settings.Music));Bank.Mixer.SetFloat("SfxVolume",ToDb(settings.Sfx));Voices.ApplyMute(settings);Landscape.ApplyMute();}
  public static float ToDb(float linear)=>linear<=0?-80:Mathf.Clamp(20*Mathf.Log10(linear),-80,0);
  public static bool Play(Sound sound,Vector2? position=null)
  {var r=Current;if(r==null||r.Suspended||!r.isActiveAndEnabled)return false;if(!r.Voices.Play(sound,position,r.ListenerCamera,r.Settings))return false;Played?.Invoke(sound);return true;}
  public static async Task<T> DuringOverlay<T>(Func<Task<T>> show)
  {var r=Current;if(r!=null){r.overlays++;r.UpdateSuspension();}try{return await show();}finally{if(r!=null){r.overlays=Math.Max(0,r.overlays-1);r.UpdateSuspension();}}}
  private void UpdateSuspension(){AudioListener.pause=Suspended;if(Suspended)Voices.Stop();}
  private void OnApplicationPause(bool paused){background=paused;UpdateSuspension();}
  private void OnApplicationFocus(bool focused){if(Application.isEditor)return;unfocused=!focused;UpdateSuspension();}
  private void OnDestroy(){if(Current!=this)return;SceneManager.sceneLoaded-=SceneLoaded;AudioListener.pause=false;Current=null;}
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]private static void Reset(){Current=null;Played=null;AudioListener.pause=false;}
 }
 // Play has no per-hit object/array/receipt allocation or scene scans.
 public sealed class AudioVoices
 {
  public const int Capacity=25;private static readonly int[] Starts={0,4,12,17,21},Counts={4,8,5,4,4};
  private readonly AudioSource[] sources=new AudioSource[Capacity];private readonly int[] priorities=new int[Capacity];private readonly float[] ends=new float[Capacity],next;private readonly int[] variants;private readonly AudioBank bank;
  public int Accepted {get;private set;}public int Culled {get;private set;}public int ActiveCount {get{var n=0;foreach(var s in sources)if(s.isPlaying)n++;return n;}}
  public AudioVoices(Transform parent,AudioBank data)
  {bank=data;next=new float[bank.Sounds.Length];variants=new int[next.Length];for(var i=0;i<next.Length;i++)variants[i]=-1;for(var i=0;i<Capacity;i++){var go=new GameObject("Voice "+i);go.transform.SetParent(parent,false);var s=go.AddComponent<AudioSource>();s.playOnAwake=false;s.spatialBlend=0;s.dopplerLevel=0;s.outputAudioMixerGroup=i<4?bank.UI:bank.Sfx;sources[i]=s;}}
  public void ApplyMute(LocalSettings settings){foreach(var s in sources)s.mute=settings.Master<=0||settings.Sfx<=0;}
  public bool Play(Sound id,Vector2? position,Camera camera,LocalSettings settings)
  {
   var e=bank.Entry(id);var now=Time.unscaledTime;var n=(int)id;if(settings.Master<=0||settings.Sfx<=0||now<next[n]){Culled++;return false;}var gain=e.Gain;var pan=0f;
   if(position.HasValue&&camera!=null){var delta=position.Value-(Vector2)camera.transform.position;var d=delta.magnitude;if(d>=e.Range){Culled++;return false;}gain*=Mathf.Pow(Mathf.Clamp01(1-d/e.Range),1.35f);pan=Mathf.Clamp(delta.x/e.Range,-.75f,.75f);}
   var category=(int)e.Category;var chosen=-1;var worst=-1;for(var i=Starts[category];i<Starts[category]+Counts[category];i++){if(!sources[i].isPlaying||ends[i]<=now){chosen=i;break;}if(worst<0||priorities[i]>priorities[worst])worst=i;}
   if(chosen<0){if(worst<0||priorities[worst]<e.Priority){Culled++;return false;}chosen=worst;}
   var v=e.Clips.Length==1?0:UnityEngine.Random.Range(0,variants[n]<0?e.Clips.Length:e.Clips.Length-1);if(e.Clips.Length>1&&variants[n]>=0&&v>=variants[n])v++;v%=e.Clips.Length;variants[n]=v;
   var clip=e.Clips[v];var source=sources[chosen];source.Stop();source.clip=clip;source.panStereo=pan;source.volume=gain;source.pitch=e.Category==SoundCategory.UI?1:UnityEngine.Random.Range(.97f,1.03f);source.priority=e.Priority;source.mute=false;source.Play();priorities[chosen]=e.Priority;ends[chosen]=now+clip.length/source.pitch;next[n]=now+e.Cooldown;Accepted++;return true;
  }
  public void Stop(){foreach(var source in sources)source.Stop();}
 }
}
