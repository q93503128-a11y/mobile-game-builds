using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace Wildfolk.Tests
{
 public sealed class AudioDataTests
 {
  [Test]public void ProductionBankHasCompleteRoutingAndNoEmptyCues(){var bank=Resources.Load<AudioBank>("Audio/LowfireAudio");Assert.That(bank,Is.Not.Null);Assert.That(bank.Mixer,Is.Not.Null);Assert.That(bank.UI,Is.Not.Null);Assert.That(bank.Ambience,Is.Not.Null);Assert.That(bank.Music,Is.Not.Null);Assert.That(bank.Sfx,Is.Not.Null);Assert.That(bank.Sounds.Length,Is.EqualTo(System.Enum.GetValues(typeof(Sound)).Length));for(var i=0;i<bank.Sounds.Length;i++){Assert.That((int)bank.Sounds[i].Id,Is.EqualTo(i));Assert.That(bank.Sounds[i].Clips,Is.Not.Empty);foreach(var clip in bank.Sounds[i].Clips)Assert.That(clip,Is.Not.Null);}}
  [TestCase(0,-80)][TestCase(1,0)][TestCase(.5f,-6.0206f)][TestCase(.1f,-20)]public void MixerUsesLogarithmicDecibels(float slider,float expected)=>Assert.That(AudioRuntime.ToDb(slider),Is.EqualTo(expected).Within(.001));
  [Test]public void ExistingOwnerJsonKeepsPresentationSilentByDefault(){var old=JsonUtility.FromJson<NetworkGrowthSnapshot>("{\"XP\":4,\"Gold\":3}");Assert.That(old.ActionSequence,Is.Zero);Assert.That(old.Action,Is.EqualTo(ConfirmedPlayerAction.None));}
  [Test]public void ImportedBedsStreamAndEffectsAreMono()
  {foreach(var guid in AssetDatabase.FindAssets("t:AudioClip",new[]{"Assets/Wildfolk/Audio"})){var path=AssetDatabase.GUIDToAssetPath(guid);var clip=AssetDatabase.LoadAssetAtPath<AudioClip>(path);var bed=!path.Contains("/SFX/");Assert.That(clip.channels,Is.EqualTo(bed?2:1),path);Assert.That(clip.loadType,Is.EqualTo(bed?AudioClipLoadType.Streaming:AudioClipLoadType.DecompressOnLoad),path);Assert.That(clip.length,Is.GreaterThan(.025f),path);}}
 }
}
