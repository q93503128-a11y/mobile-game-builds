using UnityEngine;
namespace Wildfolk
{
 // Five persistent sources: two musical beds, two environmental beds and one nearby fire.
 public sealed class AudioLandscape:MonoBehaviour
 {
  private readonly AudioSource[] music=new AudioSource[2],air=new AudioSource[2];private AudioSource fire;
  private AudioRuntime runtime;private WorldGenerator world;private WorldClock clock;private GameplaySession session;
  private int musicSlot,airSlot;private bool initialized;private float poll,changedAt,fade=1.5f,nextBird,bossUntil,victoryAt=-1,duckUntil;
  private AudioRegion candidate;private bool bossDead;public AudioRegion Region {get;private set;}public int Transitions {get;private set;}
  private void Awake(){runtime=GetComponent<AudioRuntime>();for(var i=0;i<2;i++){music[i]=Source("Music "+i,true);air[i]=Source("Air "+i,false);}fire=Source("Nearby fire",false);fire.clip=runtime.Bank.Fire;fire.volume=0;}
  private AudioSource Source(string name,bool score){var go=new GameObject(name);go.transform.SetParent(transform,false);var s=go.AddComponent<AudioSource>();s.loop=true;s.playOnAwake=false;s.outputAudioMixerGroup=score?runtime.Bank.Music:runtime.Bank.Ambience;s.spatialBlend=0;return s;}
  public void BindScene(){world=FindFirstObjectByType<WorldGenerator>();clock=FindFirstObjectByType<WorldClock>();session=FindFirstObjectByType<GameplaySession>();poll=0;candidate=world==null?AudioRegion.Menu:AudioRegion.Day;changedAt=Time.unscaledTime-4;nextBird=Time.unscaledTime+Random.Range(12,24);if(world==null)Change(AudioRegion.Menu);}
  public void ApplyMute(){foreach(var s in music)s.mute=runtime.Settings.Master<=0||runtime.Settings.Music<=0;foreach(var s in air)s.mute=runtime.Settings.Master<=0||runtime.Settings.Sfx<=0;fire.mute=runtime.Settings.Master<=0||runtime.Settings.Sfx<=0;}
  public void BossIntro(){bossDead=false;bossUntil=Time.unscaledTime+5;Change(AudioRegion.Boss);}
  public void BossDefeated(float delay){bossDead=true;bossUntil=Time.unscaledTime+delay;victoryAt=Time.unscaledTime+delay;duckUntil=victoryAt+4.5f;}
  public void Telegraph()=>duckUntil=Time.unscaledTime+.6f;
  public Sound Step(Vector2 position)
  {if(world!=null&&world.Map!=null&&world.Map.Streamed){var c=world.Map.Continental;if(c.Underground(position)||c.Environment(position).Rockiness>.7f)return Sound.StepStone;}return Sound.StepGrass;}
  private AudioRegion Context(Vector2 position)
  {
   if(world==null||world.Map==null)return AudioRegion.Menu;
   var boss=BossIdentity.Active;if(Time.unscaledTime<bossUntil||boss!=null&&Vector2.Distance(position,boss.transform.position)<35)return AudioRegion.Boss;
   if(world.Map.Streamed){var c=world.Map.Continental;if(c.Underground(position))return c.CaveAt(position).Nearest(position).Depth>=.6f?AudioRegion.Deep:AudioRegion.Cave;foreach(var a in c.Anchors)if(a.Kind==WorldAnchorKind.Graveyard&&(a.Position-position).sqrMagnitude<16*16)return AudioRegion.Graveyard;}
   return clock!=null&&clock.Model!=null&&(clock.Model.Phase==DayPhase.Night||clock.Model.Phase==DayPhase.Evening)?AudioRegion.Night:AudioRegion.Day;
  }
  public void Change(AudioRegion region)
  {
   if(initialized&&Region==region)return;Region=region;if(region!=AudioRegion.Boss)bossDead=false;initialized=true;Transitions++;fade=region==AudioRegion.Boss?.6f:1.5f;
   var clip=runtime.Bank.MusicBeds[(int)region];if(music[musicSlot].clip!=clip){musicSlot=1-musicSlot;music[musicSlot].Stop();music[musicSlot].clip=clip;music[musicSlot].volume=0;music[musicSlot].Play();}
   var ambience=runtime.Bank.EnvironmentBeds[(int)region];if(air[airSlot].clip!=ambience){airSlot=1-airSlot;air[airSlot].Stop();air[airSlot].clip=ambience;air[airSlot].volume=0;air[airSlot].Play();}ApplyMute();
  }
  private void Update()
  {
   if(runtime.Suspended)return;var now=Time.unscaledTime;
   if(victoryAt>=0&&now>=victoryAt){victoryAt=-1;AudioRuntime.Play(Sound.BossVictory);}
   if(now>=poll){poll=now+.3f;var p=runtime.ListenerCamera==null?Vector2.zero:(Vector2)runtime.ListenerCamera.transform.position;var next=Context(p);if(next!=candidate){candidate=next;changedAt=now;}if(!initialized||next==AudioRegion.Boss||now-changedAt>=3)Change(next);
    var gain=0f;var pan=0f;foreach(var placeable in WorldPlaceable.Active){if(placeable==null||placeable.Definition==null||placeable.Definition.Station!=StationKind.Campfire||!placeable.Health.IsAlive)continue;var delta=(Vector2)placeable.transform.position-p;var v=Mathf.Clamp01(1-delta.magnitude/10)*.45f;if(v>gain){gain=v;pan=Mathf.Clamp(delta.x/10,-.7f,.7f);}}fire.volume=gain;fire.panStereo=pan;if(gain>0&&!fire.isPlaying)fire.Play();else if(gain<=0&&fire.isPlaying)fire.Stop();
    if(now>=nextBird){nextBird=now+Random.Range(16,34);if(Region==AudioRegion.Day||Region==AudioRegion.Menu)AudioRuntime.Play(Sound.Bird,p+new Vector2(Random.Range(-8,8),4));}
   }
   var factor=session!=null&&(session.State==SessionState.Paused||session.State==SessionState.Defeated||session.State==SessionState.Downed)?.32f:1;if(now<duckUntil)factor*=.5f;if(bossDead)factor=0;
   for(var i=0;i<2;i++){music[i].volume=Mathf.MoveTowards(music[i].volume,i==musicSlot?.7f*factor:0,Time.unscaledDeltaTime/fade);air[i].volume=Mathf.MoveTowards(air[i].volume,i==airSlot?(Region==AudioRegion.Menu?.24f:.65f):0,Time.unscaledDeltaTime/1.5f);if(i!=musicSlot&&music[i].volume==0&&music[i].isPlaying)music[i].Stop();if(i!=airSlot&&air[i].volume==0&&air[i].isPlaying)air[i].Stop();}
  }
 }
}
