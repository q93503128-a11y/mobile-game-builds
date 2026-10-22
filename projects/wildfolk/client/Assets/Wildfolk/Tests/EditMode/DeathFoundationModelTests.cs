using System;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Unity.Collections;
using Unity.Netcode;
namespace Wildfolk.Tests
{
    public sealed class DeathFoundationModelTests
    {
        [Test]public void Protocol19RoundTripsLifecycleAndDeadlines()
        {
            var a=new PlayerStatusState{HP=0,MaximumHP=100,Life=(byte)PlayerLife.Reviving,LifeRevision=51,DownUntil=120,HoldStarted=93,HoldUntil=97,ProtectedUntil=0,Helping=false};
            using(var writer=new FastBufferWriter(256,Allocator.Temp)){writer.WriteNetworkSerializable(a);using(var reader=new FastBufferReader(writer,Allocator.Temp)){reader.ReadNetworkSerializable(out PlayerStatusState b);Assert.That(b.Equals(a),Is.True);b.LifeRevision++;Assert.That(b.Equals(a),Is.False);}}
            Assert.That(NetworkProtocol.Current,Is.EqualTo(19));
        }
        [Test]public void Save11MigrationPreservesHpAndInitialSeedHomeForAllParticipants()
        {
            var rules=Resources.Load<SaveGameRules>("SaveGameRules");var map=new WorldMap(rules.World,9472026,3,false);
            var host=new PlayerSnapshot{HP=0,Position=map.Spawn};var peer=new PlayerSnapshot{HP=55,Position=map.Spawn+Vector2.right};
            var g=new SavedGame{Version=11,Seed=map.Seed,BaseGenerationVersion=3,Player=host,IsMultiplayer=true,Multiplayer=new MultiplayerSnapshot{HostId="aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",Players=new[]{new SavedParticipant{Id="aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",State=host},new SavedParticipant{Id="bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb",State=peer}}}};
            DeathSaveValidation.Migrate(g,rules.World);Assert.That(g.Version,Is.EqualTo(12));Assert.That(host.HP,Is.Zero);Assert.That(host.Death.Dead,Is.True);Assert.That(peer.HP,Is.EqualTo(55));Assert.That(peer.Death.Dead,Is.False);Assert.That(peer.Death.Home,Is.EqualTo(map.Spawn));Assert.That(host.Position,Is.EqualTo(map.Spawn));DeathSaveValidation.Validate(host,map);DeathSaveValidation.Validate(peer,map);
            peer.Death.Dead=true;Assert.Throws<InvalidDataException>(()=>DeathSaveValidation.Validate(peer,map));peer.Death.Dead=false;peer.Death.Run="bad";Assert.Throws<InvalidDataException>(()=>DeathSaveValidation.Validate(peer,map));
        }
        [Test]public void ReleaseProvidersAreUnavailableAndNoCrystalOrGoldWallet()
        {ReviveServices.Reset();Assert.That(ReviveServices.Wallet.Available,Is.False);Assert.That(ReviveServices.Wallet.Balance,Is.Zero);Assert.That(ReviveServices.Rewarded.Available,Is.False);var callback=false;ReviveServices.Rewarded.Show("x",s=>callback=s);Assert.That(callback,Is.False);Assert.That(ReviveServices.Wallet.TryCommitRevive("x",1,()=>throw new Exception()),Is.False);}

        [Test]public void DevelopmentWalletDuplicateRollbackAndReentrancyAreAtomic()
        {
            var assembly=Array.Find(AppDomain.CurrentDomain.GetAssemblies(),a=>a.GetType("Wildfolk.Editor.DevelopmentPremiumWallet")!=null);var type=assembly.GetType("Wildfolk.Editor.DevelopmentPremiumWallet");var wallet=(IPremiumCurrencyWallet)Activator.CreateInstance(type,new object[]{100L});var calls=0;
            Assert.That(wallet.TryCommitRevive("failed",7,()=>false),Is.False);Assert.That(wallet.Balance,Is.EqualTo(100));
            Assert.That(wallet.TryCommitRevive("once",7,()=>{calls++;Assert.That(wallet.TryCommitRevive("nested",7,()=>true),Is.False);return true;}),Is.True);
            Assert.That(wallet.TryCommitRevive("once",7,()=>{calls++;return true;}),Is.True);Assert.That(calls,Is.EqualTo(1));Assert.That(wallet.Balance,Is.EqualTo(93));
            for(var i=0;i<8;i++)Assert.That(wallet.TryCommitRevive("repeat"+i,7,()=>true),Is.True);Assert.That(wallet.Balance,Is.EqualTo(37));Assert.That(wallet.TryCommitRevive("poor",50,()=>true),Is.False);
        }
        [Test]public void DataIsSharedAndProductionPriceRemainsUndecided()
        {var d=AssetDatabase.LoadAssetAtPath<ReviveDefinition>("Assets/Wildfolk/Data/Revive.asset");Assert.That(d.DownedSeconds,Is.EqualTo(24));Assert.That(d.HoldSeconds,Is.EqualTo(4));Assert.That(d.RevivedHealth,Is.EqualTo(.4f));Assert.That(d.ProtectionSeconds,Is.EqualTo(2));Assert.That(d.ReviveGemCost,Is.Zero);foreach(var p in new[]{"Player","NetworkPlayer"})Assert.That(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/"+p+".prefab").GetComponent<PlayerDeath>().Definition,Is.SameAs(d));}
    }
}
