using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
namespace Wildfolk.Editor
{
 public static class AudioProductionBuilder
 {
  private const BindingFlags Flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static;
  private const string Root="Assets/Wildfolk/",BankPath=Root+"Resources/Audio/LowfireAudio.asset",MixerPath=Root+"Audio/Lowfire.mixer";
  private static AudioClip Clip(string name,string folder="SFX")=>AssetDatabase.LoadAssetAtPath<AudioClip>(Root+"Audio/"+folder+"/"+name+(folder=="SFX"?".wav":".ogg"))??throw new InvalidOperationException("Audio clip missing: "+name);
  private static AudioClip[] Clips(string names)=>names.Split(',').Select(n=>Clip(n.Trim())).ToArray();
  private static AudioClip[] Pool(string prefix)=>AssetDatabase.FindAssets("t:AudioClip",new[]{Root+"Audio/SFX"}).Select(AssetDatabase.GUIDToAssetPath).Where(p=>Path.GetFileNameWithoutExtension(p).StartsWith(prefix+"_",StringComparison.Ordinal)).OrderBy(p=>p,StringComparer.Ordinal).Select(AssetDatabase.LoadAssetAtPath<AudioClip>).ToArray();
  [MenuItem("Wildfolk/Audio/Build Production Audio")]
  public static void Build()
  {
   if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play first");
   Directory.CreateDirectory(Root+"Resources/Audio");AssetDatabase.Refresh();
   var mixer=AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
   if(mixer==null){var type=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("UnityEditor.Audio.AudioMixerController")).First(t=>t!=null);mixer=(AudioMixer)type.GetMethod("CreateMixerControllerAtPath",Flags).Invoke(null,new object[]{MixerPath});var master=(AudioMixerGroup)type.GetProperty("masterGroup",Flags).GetValue(mixer);var groups=new AudioMixerGroup[5];groups[0]=master;
    foreach(var pair in new[]{(1,"Music",0),(2,"SFX",0),(3,"UI",2),(4,"Ambience",2)}){var group=(AudioMixerGroup)type.GetMethod("CreateNewGroup",Flags).Invoke(mixer,new object[]{pair.Item2,false});type.GetMethod("AddChildToParent",Flags).Invoke(mixer,new object[]{group,groups[pair.Item3]});groups[pair.Item1]=group;}
    var property=type.GetProperty("exposedParameters",Flags);var element=property.PropertyType.GetElementType();var exposed=Array.CreateInstance(element,3);for(var i=0;i<3;i++){var entry=Activator.CreateInstance(element);element.GetField("guid",Flags).SetValue(entry,groups[i].GetType().GetMethod("GetGUIDForVolume",Flags).Invoke(groups[i],null));element.GetField("name",Flags).SetValue(entry,new[]{"MasterVolume","MusicVolume","SfxVolume"}[i]);exposed.SetValue(entry,i);}property.SetValue(mixer,exposed);EditorUtility.SetDirty(mixer);AssetDatabase.SaveAssets();
   }
   mixer.updateMode=AudioMixerUpdateMode.UnscaledTime;
   var bank=AssetDatabase.LoadAssetAtPath<AudioBank>(BankPath);if(bank==null){bank=ScriptableObject.CreateInstance<AudioBank>();AssetDatabase.CreateAsset(bank,BankPath);}bank.Mixer=mixer;
   bank.Music=mixer.FindMatchingGroups("Music").Single(g=>g.name=="Music");bank.Sfx=mixer.FindMatchingGroups("SFX").Single(g=>g.name=="SFX");bank.UI=mixer.FindMatchingGroups("UI").Single(g=>g.name=="UI");bank.Ambience=mixer.FindMatchingGroups("Ambience").Single(g=>g.name=="Ambience");
   bank.MusicBeds=new[]{"menu","day","night","cave","cave","graveyard","boss"}.Select(n=>Clip(n,"Music")).ToArray();bank.EnvironmentBeds=new[]{"forest_day","forest_day","forest_night","cave_air","deep_air","graveyard_air","graveyard_air"}.Select(n=>Clip(n,"Ambience")).ToArray();bank.Fire=Clip("campfire","Ambience");
   bank.Sounds=Enum.GetValues(typeof(Sound)).Cast<Sound>().Select(s=>Entry(s)).ToArray();EditorUtility.SetDirty(bank);
   foreach(var guid in AssetDatabase.FindAssets("t:AudioClip",new[]{Root+"Audio"})){var path=AssetDatabase.GUIDToAssetPath(guid);var importer=(AudioImporter)AssetImporter.GetAtPath(path);var serialized=new SerializedObject(importer);var normalize=serialized.FindProperty("m_Normalize");if(normalize!=null&&normalize.boolValue){normalize.boolValue=false;serialized.ApplyModifiedPropertiesWithoutUndo();importer.SaveAndReimport();}}
   foreach(var scenePath in new[]{FrontendSceneBuilder.ScenePath,GameplaySceneBuilder.ScenePath,"Assets/Wildfolk/Scenes/Multiplayer.unity"}){if(!File.Exists(scenePath))continue;var scene=EditorSceneManager.OpenScene(scenePath);foreach(var a in UnityEngine.Object.FindObjectsByType<GameAudio>(FindObjectsInactive.Include,FindObjectsSortMode.None)){foreach(var source in a.GetComponentsInChildren<AudioSource>(true))if(source.clip==null)UnityEngine.Object.DestroyImmediate(source.gameObject);}
    foreach(var b in UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Include,FindObjectsSortMode.None)){if(b.GetComponent<AudioButton>()==null)b.gameObject.AddComponent<AudioButton>();}EditorSceneManager.SaveScene(scene);}
   AssetDatabase.SaveAssets();Validate();
  }
  private static SoundEntry Entry(Sound sound)
  {
   var e=new SoundEntry{Id=sound};string names=null,pool=null;
   switch(sound){
    case Sound.UiClick:names="ui_click";break;case Sound.UiBack:names="ui_back";break;case Sound.UiConfirm:names="ui_confirm";break;case Sound.UiError:names="ui_error";break;case Sound.Journal:names="journal";break;case Sound.Page:names="page";break;case Sound.ClassUnlock:names="bead_reward";break;case Sound.Engineer:names="engineer";break;case Sound.Achievement:names="bead_reward";break;case Sound.Attendance:pool="wood_hit";break;case Sound.Gem:names="ring_reward";break;case Sound.Gold:names="gold";break;case Sound.Pickup:names="pickup";break;
    case Sound.CraftStart:names="craft_start";break;case Sound.CraftComplete:names="craft_complete";break;case Sound.Collect:names="pickup";break;case Sound.Place:pool="wood_break";break;case Sound.Upgrade:names="engineer";break;case Sound.Repair:names="craft_start";break;case Sound.Dismantle:pool="wood_break";break;case Sound.DoorOpen:names="door_open";break;case Sound.DoorClose:names="door_close";break;case Sound.Chest:names="chest";break;case Sound.Equip:names="equip";break;case Sound.Eat:names="food";break;case Sound.Drink:names="drink";break;case Sound.Treat:names="treat";break;
    case Sound.Cast:names="water_splash";break;case Sound.Bite:names="fishing_bite";break;case Sound.Catch:names="fishing_catch";break;case Sound.Dash:pool="swing";break;case Sound.Revive:names="bead_reward";break;case Sound.Downed:pool="body_fall";break;case Sound.PlayerHurt:pool="flesh_hit";break;case Sound.PlayerDeath:pool="body_fall";break;
    case Sound.StepGrass:pool="step_grass";break;case Sound.StepStone:pool="step_stone";break;case Sound.Swing:pool="swing";break;case Sound.Blade:pool="blade";break;case Sound.BowDraw:names="bow_draw";break;case Sound.BowRelease:names="bow_release";break;case Sound.Crossbow:names="crossbow";break;case Sound.ProjectileStone:pool="stone_hit";break;
    case Sound.WoodHit:pool="wood_hit";break;case Sound.WoodBreak:pool="wood_break";break;case Sound.StoneHit:pool="stone_hit";break;case Sound.StoneBreak:pool="stone_hit";break;case Sound.OreHit:pool="ore_hit";break;case Sound.OreBreak:pool="ore_break";break;case Sound.Plant:pool="plant";break;case Sound.FleshHit:pool="flesh_hit";break;case Sound.BoneHit:pool="bone_hit";break;case Sound.ChitinHit:pool="chitin_hit";break;case Sound.BodyFall:pool="body_fall";break;
    case Sound.Undead:pool="undead";break;case Sound.Goblin:pool="goblin";break;case Sound.Slime:pool="slime";break;case Sound.Beast:names="beast";break;case Sound.Wolf:names="wolf";break;case Sound.Bat:pool="bat";break;case Sound.Insect:pool="insect";break;case Sound.Bird:names="bird";break;
    case Sound.BossIntro:names="boss_roar";break;case Sound.BossPhase:names="ring_reward";break;case Sound.BossWarningFan:names="bow_draw";break;case Sound.BossWarningLine:names="engineer";break;case Sound.BossWarningCircle:names="boss_roar";break;case Sound.BossStrike:pool="heavy_hit";break;case Sound.BossSlam:pool="wood_break";break;case Sound.BossCharge:pool="heavy_hit";break;case Sound.BossSummon:names="boss_summon";break;case Sound.BossHurt:pool="bone_hit";break;case Sound.BossDeath:names="boss_roar";break;case Sound.BossVictory:names="victory";break;
   }
   e.Clips=pool==null?Clips(names??throw new InvalidOperationException("Unmapped cue "+sound)):Pool(pool);
   if(sound<=Sound.Pickup||sound==Sound.BossVictory){e.Category=SoundCategory.UI;e.Gain=.5f;e.Priority=20;e.Cooldown=.09f;}
   else if(sound>=Sound.StepGrass&&sound<=Sound.StepStone){e.Category=SoundCategory.Footstep;e.Gain=.7f;e.Range=11;e.Priority=180;e.Cooldown=.055f;}
   else if(sound>=Sound.Undead&&sound<=Sound.Bird){e.Category=SoundCategory.Creature;e.Gain=.6f;e.Range=18;e.Priority=140;e.Cooldown=sound==Sound.Bird?8:.35f;}
   else if(sound>=Sound.BossIntro||sound>=Sound.Revive&&sound<=Sound.PlayerDeath){e.Category=SoundCategory.Critical;e.Gain=.85f;e.Priority=sound>=Sound.BossIntro?8:16;e.Range=38;e.Cooldown=.12f;}
   else{e.Category=SoundCategory.Impact;e.Gain=.65f;e.Priority=80;e.Cooldown=.045f;}
   return e;
  }
  public static void Validate()
  {var bank=AssetDatabase.LoadAssetAtPath<AudioBank>(BankPath);if(bank==null||bank.Mixer==null||bank.Sounds.Length!=Enum.GetValues(typeof(Sound)).Length||bank.Music==null||bank.Sfx==null||bank.UI==null||bank.Ambience==null)throw new Exception("Audio routing incomplete");for(var i=0;i<bank.Sounds.Length;i++){var e=bank.Sounds[i];if((int)e.Id!=i||e.Clips.Length==0||e.Clips.Any(c=>c==null||c.length<=0))throw new Exception("Invalid cue "+i);}if(bank.MusicBeds.Length!=7||bank.EnvironmentBeds.Length!=7||bank.MusicBeds.Any(c=>c==null)||bank.EnvironmentBeds.Any(c=>c==null))throw new Exception("Audio contexts incomplete");var used=bank.Sounds.SelectMany(e=>e.Clips).Concat(bank.MusicBeds).Concat(bank.EnvironmentBeds).Append(bank.Fire).ToHashSet();foreach(var guid in AssetDatabase.FindAssets("t:AudioClip",new[]{Root+"Audio"})){var clip=AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(guid));if(!used.Contains(clip))throw new Exception("Unused audio asset: "+clip.name);}Debug.Log("Lowfire audio bank validated: "+bank.Sounds.Length+" events / "+used.Count+" clips");}
 }
}
