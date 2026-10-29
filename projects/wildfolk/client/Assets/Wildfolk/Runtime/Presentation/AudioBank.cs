using System;
using UnityEngine;
using UnityEngine.Audio;
namespace Wildfolk
{
 public enum Sound { UiClick,UiBack,UiConfirm,UiError,Journal,Page,ClassUnlock,Engineer,Achievement,Attendance,Gem,Gold,Pickup,CraftStart,CraftComplete,Collect,Place,Upgrade,Repair,Dismantle,DoorOpen,DoorClose,Chest,Equip,Eat,Drink,Treat,Cast,Bite,Catch,Dash,Revive,Downed,PlayerHurt,PlayerDeath,StepGrass,StepStone,Swing,Blade,BowDraw,BowRelease,Crossbow,ProjectileStone,WoodHit,WoodBreak,StoneHit,StoneBreak,OreHit,OreBreak,Plant,FleshHit,BoneHit,ChitinHit,BodyFall,Undead,Goblin,Slime,Beast,Wolf,Bat,Insect,Bird,BossIntro,BossPhase,BossWarningFan,BossWarningLine,BossWarningCircle,BossStrike,BossSlam,BossCharge,BossSummon,BossHurt,BossDeath,BossVictory }
 public enum SoundCategory { UI,Impact,Creature,Footstep,Critical }
 public enum AudioRegion { Menu,Day,Night,Cave,Deep,Graveyard,Boss }
 [Serializable] public sealed class SoundEntry { public Sound Id;public AudioClip[] Clips;public SoundCategory Category;public float Gain=.75f,Cooldown=.045f,Range=22;public int Priority=80; }
 [CreateAssetMenu(menuName="Wildfolk/Audio Bank")]
 public sealed class AudioBank:ScriptableObject
 {
  public AudioMixer Mixer;public AudioMixerGroup Music,Sfx,UI,Ambience;public AudioClip[] MusicBeds,EnvironmentBeds;public AudioClip Fire;public SoundEntry[] Sounds;
  public SoundEntry Entry(Sound sound)=>Sounds[(int)sound];
 }
}
