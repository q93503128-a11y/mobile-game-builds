using System;
using System.Linq;
using NUnit.Framework;
using Unity.Collections;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
namespace Wildfolk.Tests
{
    public sealed class BossFoundationTests
    {
        private BossDefinition D=>AssetDatabase.LoadAssetAtPath<BossDefinition>("Assets/Wildfolk/Data/UndeadKing.asset");
        [TestCase(1f,1)][TestCase(.7001f,1)][TestCase(.7f,2)][TestCase(.3501f,2)][TestCase(.35f,3)][TestCase(0f,3)]
        public void PhaseThresholds(float hp,int expected)=>Assert.That(D.Phase(hp),Is.EqualTo(expected));
        [TestCase(1,2)][TestCase(2,3)][TestCase(3,4)][TestCase(4,5)]
        public void OnlyAuthoredAddCountScales(int players,int expected)
        {Assert.That(D.AddCount(players),Is.EqualTo(expected));Assert.That(D.Vitality.MaximumHealth,Is.EqualTo(9000));Assert.That(D.Attacks.Select(a=>a.Damage),Is.EqualTo(new[]{26,41,21,26,36}));}
        [Test] public void NightIdentitySurvivesKillWipeLoadAndUnlocksNextNight()
        {Assert.That(D.CanSummon(4,4,DayPhase.Night,false),Is.False);Assert.That(D.CanSummon(4,5,DayPhase.Night,false),Is.True);Assert.That(D.CanSummon(4,5,DayPhase.Day,false),Is.False);Assert.That(D.CanSummon(4,5,DayPhase.Night,true),Is.False);}
        [Test] public void OfferUsesExistingBoneAndMeleeTelegraphsRemainReadable()
        {Assert.That(D.Offering.Id,Is.EqualTo("bone"));Assert.That(D.OfferingCount,Is.EqualTo(10));Assert.That(D.Attacks.Select(a=>a.Windup),Is.EqualTo(new[]{.8f,1.15f,.75f,.95f,1.3f}));Assert.That(D.LastPhaseSpeed,Is.EqualTo(1.05f));Assert.That(D.LastPhaseRecovery,Is.EqualTo(.9f));Assert.That(D.Attacks.Single(a=>a.Id=="kings_march").Repeats,Is.EqualTo(3));}
        [Test] public void CoreGuaranteedCrownTwentyPercentAndMinionsHaveNoFarmLoot()
        {Assert.That(D.Rewards.Items.Single(i=>i.Item.Id=="boss_core_t1").Chance,Is.EqualTo(1));var crown=D.Rewards.RareItems.Single();Assert.That(crown.Chance,Is.EqualTo(.2f));Assert.That(crown.Minimum+crown.Maximum,Is.EqualTo(2));Assert.That(D.MinionRewards.Experience+D.MinionRewards.Gold+D.MinionRewards.Items.Length+D.MinionRewards.RareItems.Length,Is.Zero);}
        [Test] public void AccessoryAddsOneSlotAndNoDamageBonus()
        {Assert.That((int)ArmorSlot.Accessory,Is.EqualTo(3));var d=AssetDatabase.LoadAssetAtPath<AccessoryDefinition>("Assets/Wildfolk/Data/LastCrown.asset");Assert.That(d.Threshold,Is.EqualTo(.35f));Assert.That(d.MovementBonus,Is.EqualTo(.08f));Assert.That(d.DamageReduction,Is.EqualTo(.1f));Assert.That(d.Anchor,Is.EqualTo(AccessoryAnchor.AboveHead));}
        [Test] public void SnapshotCopyDoesNotAliasPendingLootOrEligibility()
        {var a=new BossSnapshot{RolledLoot=new[]{new SavedStack{Item="bone",Count=16}},Eligible=new[]{"a"}};var b=a.Copy();b.RolledLoot[0].Count=99;b.Eligible[0]="b";Assert.That(a.RolledLoot[0].Count,Is.EqualTo(16));Assert.That(a.Eligible[0],Is.EqualTo("a"));}
        [Test] public void BossFrameRoundTripsAuthorityPoseAndMark()
        {
            var a=new BossFrame{Ready=true,HP=6300,State=(byte)BossState.Ritual,Phase=2,Attack=3,Direction=Vector2.left,Home=new Vector2(12,23),Started=12.75,Marked=42,Tomb="poi:guard"};
            using(var w=new FastBufferWriter(512,Allocator.Temp)){w.WriteNetworkSerializable(a);using(var r=new FastBufferReader(w,Allocator.Temp)){r.ReadNetworkSerializable(out BossFrame b);Assert.That(b.Equals(a),Is.True);}}
            Assert.That(NetworkProtocol.Current,Is.EqualTo(22));
        }
        [Test] public void RosterLabelIsClassInEnglish()
        {var old=L.Language;try{L.SetLanguage("en");Assert.That(L.Text("class.title"),Is.EqualTo("Class"));}finally{L.SetLanguage(old);}}
        [Test] public void SegmentsAreDistinctAndPoseOnlyComponentDoesNotOwnSimulation()
        {var d=D.Prefab.GetComponent<UndeadKingVisual>();Assert.That(d.Parts.Length,Is.GreaterThanOrEqualTo(8));Assert.That(d.Parts.Select(p=>p.sprite).Distinct().Count(),Is.GreaterThanOrEqualTo(7));Assert.That(D.Tomb.GetComponentInChildren<BoxCollider2D>().size.x,Is.GreaterThan(2.5f));Assert.That(D.Prefab.GetComponent<EnemyController>(),Is.Null);}
    }
}
