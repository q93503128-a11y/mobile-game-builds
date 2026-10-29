using UnityEngine;
namespace Wildfolk
{
 // Presentation-only observer, reset on pool activation; initial replica/load frames stay silent.
 [DisallowMultipleComponent]public sealed class AudioActor:MonoBehaviour
 {
  private Health health;private ResourceNode resource;private PlayerInventory player;private PlayerCombat combat;private PlayerEquipment equipment;private PlayerDeath death;private CreatureMember creature;private BossEncounter boss;private NetworkPlayer network;private NetworkCreature netCreature;private EnemyController enemy;private NetworkActivities activities;private WorldDoor door;
  private WildlifeActionBrain wildlife;private int wildlifeState;private ItemDefinition selected;private bool dashing;
  private PlayerRanged ranged;private GoblinController goblin;private StationProcessing processing;private int goblinState,output;private bool processingReady;
  private bool ready,bossReady,continuous;private Vector2 previous;private float distance,lastStep;private ushort attack;private uint activity;private EnemyState enemyState;private PlayerLife life;private BossState bossState;private double bossStarted;private int phase;private bool open;
  private void Awake(){health=GetComponent<Health>();resource=GetComponent<ResourceNode>();player=GetComponent<PlayerInventory>();combat=GetComponent<PlayerCombat>();equipment=GetComponent<PlayerEquipment>();death=GetComponent<PlayerDeath>();creature=GetComponent<CreatureMember>();boss=GetComponent<BossEncounter>();network=GetComponent<NetworkPlayer>();netCreature=GetComponent<NetworkCreature>();enemy=GetComponent<EnemyController>();activities=GetComponent<NetworkActivities>();door=GetComponent<WorldDoor>();ranged=GetComponent<PlayerRanged>();goblin=GetComponent<GoblinController>();processing=GetComponent<StationProcessing>();wildlife=GetComponent<WildlifeActionBrain>();continuous=player!=null||door!=null||enemy!=null||goblin!=null||wildlife!=null;}
  private void OnEnable(){ready=bossReady=false;distance=0;health.Damaged+=Hurt;if(combat!=null)combat.AttackStarted+=Swing;if(death!=null)death.Changed+=Life;if(boss!=null)boss.Changed+=Boss;if(ranged!=null)ranged.Released+=Shot;if(processing!=null)processing.Changed+=Processed;}
  private void OnDisable(){health.Damaged-=Hurt;if(combat!=null)combat.AttackStarted-=Swing;if(death!=null)death.Changed-=Life;if(boss!=null)boss.Changed-=Boss;if(ranged!=null)ranged.Released-=Shot;if(processing!=null)processing.Changed-=Processed;}
  private bool Local=>network==null||network.IsSpawned&&network.IsOwner;
  private bool NearBoss=>AudioRuntime.Current!=null&&AudioRuntime.Current.ListenerCamera!=null&&((Vector2)AudioRuntime.Current.ListenerCamera.transform.position-(Vector2)transform.position).sqrMagnitude<35*35;
  private Sound Family()
  {if(creature==null)return Sound.Beast;var id=creature.Definition.Id;if(id.Contains("wolf"))return Sound.Wolf;if(id.Contains("slime"))return Sound.Slime;if(id.Contains("bat"))return Sound.Bat;if(creature.Definition.Family==CreatureFamily.Undead)return Sound.Undead;if(creature.Definition.Family==CreatureFamily.Humanoid)return Sound.Goblin;if(creature.Definition.Family==CreatureFamily.Invertebrate)return Sound.Insect;return Sound.Beast;}
  private void Hurt()
  {
   if(!ready)return;var dead=!health.IsAlive;
   if(boss!=null){if(!dead)AudioRuntime.Play(Sound.BossHurt,transform.position);return;}
   if(resource!=null){var kind=resource.Definition.Kind;AudioRuntime.Play(kind==ResourceKind.Wood?(dead?Sound.WoodBreak:Sound.WoodHit):kind==ResourceKind.Stone?(dead?Sound.StoneBreak:Sound.StoneHit):kind==ResourceKind.PlantFiber||kind==ResourceKind.Berry||kind==ResourceKind.Mushroom||kind==ResourceKind.MedicinalHerb?Sound.Plant:dead?Sound.OreBreak:Sound.OreHit,transform.position);return;}
   if(player!=null){if(!dead||death==null)AudioRuntime.Play(dead?Sound.PlayerDeath:Sound.PlayerHurt,transform.position);return;}
   var cue=creature==null?Sound.WoodHit:creature.Definition.Family==CreatureFamily.Undead?Sound.BoneHit:creature.Definition.Family==CreatureFamily.Invertebrate?Sound.ChitinHit:Sound.FleshHit;
   AudioRuntime.Play(dead?Sound.BodyFall:cue,transform.position);if(creature!=null&&(creature.Definition.Family!=CreatureFamily.Animal||Family()==Sound.Wolf))AudioRuntime.Play(Family(),transform.position);
  }
  private void Shot()=>AudioRuntime.Play(equipment.SelectedItem!=null&&equipment.SelectedItem.Ranged!=null&&equipment.SelectedItem.Ranged.Crossbow?Sound.Crossbow:Sound.BowRelease,transform.position);
  private void Processed(){if(ready&&processing.IsReady&&(!processingReady||processing.OutputCount>output))AudioRuntime.Play(Sound.CraftComplete,transform.position);processingReady=processing.IsReady;output=processing.OutputCount;}
  private void Swing(){if(network==null)SwingCue();}
  private void SwingCue()=>AudioRuntime.Play(equipment!=null&&equipment.ActiveTool!=null&&(equipment.ActiveTool.Kind==ToolKind.Sword||equipment.ActiveTool.Kind==ToolKind.Spear)?Sound.Blade:Sound.Swing,transform.position);
  private void Life(){if(!ready)return;var next=death.State;if(next==life)return;Vector2? at=Local?null:(Vector2?)transform.position;if(next==PlayerLife.Alive&&life!=PlayerLife.Alive)AudioRuntime.Play(Sound.Revive,at);else if(next==PlayerLife.Downed)AudioRuntime.Play(Sound.Downed,at);else if(next==PlayerLife.Dead)AudioRuntime.Play(Sound.PlayerDeath,at);life=next;}
  private void Boss()
  {
   if(boss.World==null)return;var state=boss.State;var fresh=state!=bossState||boss.Started!=bossStarted;
   if(!bossReady){bossReady=true;phase=boss.Phase;if(state==BossState.Awakening&&boss.Age<.5f){AudioRuntime.Play(Sound.BossIntro,transform.position);if(NearBoss)AudioRuntime.Current?.Landscape.BossIntro();}}
   else if(fresh){if(state==BossState.Awakening&&boss.Age<.5f){AudioRuntime.Play(Sound.BossIntro,transform.position);if(NearBoss)AudioRuntime.Current?.Landscape.BossIntro();}if(boss.Phase>phase)AudioRuntime.Play(Sound.BossPhase,transform.position);if(state==BossState.Windup){var shape=boss.Attack==null?BossAttackShape.Fan:boss.Attack.Shape;AudioRuntime.Play(shape==BossAttackShape.Circle?Sound.BossWarningCircle:shape==BossAttackShape.Line?Sound.BossWarningLine:Sound.BossWarningFan,transform.position);if(NearBoss)AudioRuntime.Current?.Landscape.Telegraph();}else if(state==BossState.Strike){var shape=boss.Attack==null?BossAttackShape.Fan:boss.Attack.Shape;AudioRuntime.Play(shape==BossAttackShape.Circle?Sound.BossSlam:shape==BossAttackShape.Line?Sound.BossCharge:Sound.BossStrike,transform.position);}else if(state==BossState.Ritual)AudioRuntime.Play(Sound.BossSummon,transform.position);else if(state==BossState.Dying){AudioRuntime.Play(Sound.BossDeath,transform.position);if(NearBoss)AudioRuntime.Current?.Landscape.BossDefeated(boss.Definition.DeathSeconds+boss.Definition.LootPause);}}
   bossState=state;bossStarted=boss.Started;phase=boss.Phase;
  }
  private void LateUpdate()
  {
   if(ready&&!continuous)return;var p=(Vector2)transform.position;if(!ready){previous=p;attack=network==null? (ushort)0:network.AttackState.Value.Sequence;activity=activities==null?0:activities.Activity.Value.Sequence;life=death==null?PlayerLife.Alive:death.State;open=door!=null&&door.IsOpen;enemyState=enemy==null?EnemyState.Idle:enemy.State;goblinState=goblin==null?0:(int)goblin.State;if(processing!=null){processingReady=processing.IsReady;output=processing.OutputCount;}selected=equipment==null?null:equipment.SelectedItem;dashing=network!=null&&network.Motion.Value.Dashing;wildlifeState=wildlife==null?0:(int)wildlife.Phase;ready=true;if(boss!=null)Boss();return;}
   if(player!=null&&health.IsAlive){var moved=Vector2.Distance(previous,p);if(moved<1.2f){distance+=moved;if(moved>.001f&&distance>=1.05f&&Time.unscaledTime-lastStep>.15f){distance%=1.05f;lastStep=Time.unscaledTime;AudioRuntime.Play(AudioRuntime.Current==null?Sound.StepGrass:AudioRuntime.Current.Landscape.Step(p),p);}}else distance=0;
    if(network!=null&&network.IsSpawned&&attack!=network.AttackState.Value.Sequence){attack=network.AttackState.Value.Sequence;if(network.NetworkManager.ServerTime.Time-network.AttackState.Value.Started<.7)SwingCue();}
   }
   if(door!=null&&door.IsOpen!=open){open=door.IsOpen;AudioRuntime.Play(open?Sound.DoorOpen:Sound.DoorClose,p);}
   if(enemy!=null){var state=netCreature!=null&&netCreature.IsSpawned&&!netCreature.IsServer?(EnemyState)netCreature.Frame.Value.Brain:enemy.State;if(state!=enemyState){if(state==EnemyState.Windup)AudioRuntime.Play(Family(),p);else if(state==EnemyState.Swing)AudioRuntime.Play(Sound.Swing,p);enemyState=state;}}
   if(activities!=null&&activities.IsSpawned&&activity!=activities.Activity.Value.Sequence){var s=activities.Activity.Value;activity=s.Sequence;if(s.Kind==ActivityKind.Ranged&&activities.NetworkManager.ServerTime.Time-s.Started<.5)AudioRuntime.Play(Sound.BowDraw,p);else if(s.Kind==ActivityKind.Fishing)AudioRuntime.Play(Sound.Cast,p);}
   if(goblin!=null){var state=netCreature!=null&&netCreature.IsSpawned&&!netCreature.IsServer?(int)netCreature.Frame.Value.Brain:(int)goblin.State;if(state!=goblinState&&state==(int)GoblinState.Windup)AudioRuntime.Play(Sound.Goblin,p);goblinState=state;}
   if(equipment!=null&&selected!=equipment.SelectedItem){selected=equipment.SelectedItem;if(Local)AudioRuntime.Play(Sound.Equip);}
   if(network!=null&&network.IsSpawned&&!network.IsServer){var dash=network.Motion.Value.Dashing;if(dash&&!dashing)AudioRuntime.Play(Sound.Dash,p);dashing=dash;}
   if(wildlife!=null){var state=netCreature!=null&&netCreature.IsSpawned&&!netCreature.IsServer?(int)netCreature.Frame.Value.Brain:(int)wildlife.Phase;if(state!=wildlifeState){var id=creature.Definition.Id;if(state==(int)WildlifePhase.Windup&&(creature.Definition.Family==CreatureFamily.Invertebrate||id.Contains("wolf")||id.Contains("boar")||id.Contains("bat")))AudioRuntime.Play(Family(),p);if(state==(int)WildlifePhase.Action)AudioRuntime.Play(Sound.Swing,p);wildlifeState=state;}}
   previous=p;
  }
 }
}
