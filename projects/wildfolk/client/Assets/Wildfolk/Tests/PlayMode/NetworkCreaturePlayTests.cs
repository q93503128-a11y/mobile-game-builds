using System.Collections;
using System.Linq;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.TestTools;
namespace Wildfolk.Tests
{
    public sealed class NetworkCreaturePlayTests
    {
        private NetworkCreatureWorld world;
        private NetworkPlayer player;
        private IEnumerator Host()
        {
            SceneFlow.Home(SaveRepository.Default());yield return null;yield return null;
            Object.FindFirstObjectByType<HomeScreen>().OpenMultiplayer();
            var session=NetworkSession.Instance;
            // Existing small-world combat fixture; generation2 streaming is covered by WorldFoundationHostTests.
            session.NewWorldGenerationVersion=1;session.WorldRepository=new SaveRepository(System.IO.Path.Combine(Application.temporaryCachePath,"lowfire-legacy-creature-"+System.Guid.NewGuid().ToString("N"),"world.json"));session.ProfileOverride=System.Guid.NewGuid().ToString("N");
            var task=session.Host(ConnectionRoute.LocalNetwork);for(var i=0;i<300 && !task.IsCompleted;i++)yield return null;
            Assert.That(task.IsCompleted && task.Result,Is.True,session.LastFailure);Assert.That(session.EnterGame(),Is.True);
            for(var i=0;i<300;i++){yield return new WaitForSecondsRealtime(.02f);player=Object.FindFirstObjectByType<NetworkPlayer>();if(player!=null && player.IsSpawned)break;}
            Assert.That(player,Is.Not.Null);world=NetworkCreatureWorld.Active;Assert.That(world,Is.Not.Null);
            player.GetComponent<PlayerInputAdapter>().enabled=false;player.Commands.SetMove(Vector2.zero);
            world.Goblins.enabled=world.Night.enabled=world.Animals.enabled=world.Clock.enabled=false;
            yield return null;
        }
        private void Place(NetworkPlayer actor,Vector2 p)
        {actor.Motor.enabled=false;actor.GetComponent<Rigidbody2D>().bodyType=RigidbodyType2D.Kinematic;actor.GetComponent<Rigidbody2D>().position=p;actor.transform.position=p;actor.Motor.Avatar.Facing.Face(Vector2.up);Physics2D.SyncTransforms();}
        private NeutralAnimal Animal(Vector2 p,string group)
        {
            var animal=NetworkSpawnFactory.Spawn(world.Animals.Definition.Rules[0].Prefab,p,Quaternion.identity);
            animal.Player=player.transform;animal.Session=world.Session;animal.Region=world.World.Region;animal.Member.GroupId=group;animal.Member.Anchor=p;return animal;
        }
        [UnityTest]public IEnumerator HostSpawnGroupCombatAndDeathHaveOneAuthority()
        {
            yield return Host();
            Assert.That(Object.FindObjectsByType<AnimalIdentity>(FindObjectsSortMode.None).Select(a=>a.State.Species).Distinct().Count(),Is.EqualTo(5));
            foreach(var identity in Object.FindObjectsByType<AnimalIdentity>(FindObjectsSortMode.None)){var net=identity.GetComponent<NetworkCreature>();Assert.That(net.IsSpawned && net.IsServer,Is.True);Assert.That(net.Identity.Value.Group.ToString(),Is.EqualTo(identity.State.GroupId));}
            var origin=world.World.Map.Spawn;Place(player,origin);
            var a=Animal(origin+Vector2.up*.95f,"network-flock");var b=Animal(origin+Vector2.up*3,"network-flock");yield return null;yield return null;
            a.Member.Broadcast(player.transform.position,CreatureAlert.Flee);Assert.That(b.IsFleeing,Is.True);a.enabled=b.enabled=false;
            var hp=a.Health.Current;Assert.That(player.Commands.Attack(),Is.True);Assert.That(player.Commands.Attack(),Is.False);
            yield return new WaitForSeconds(.19f);Assert.That(a.Health.Current,Is.EqualTo(hp-player.GetComponent<PlayerCombat>().Settings.PunchDamage));
            Assert.That(b.Health.Current,Is.EqualTo(b.Health.Maximum));Assert.That(a.GetComponent<NetworkCreature>().Frame.Value.HP,Is.EqualTo(a.Health.Current));
            var second=Object.Instantiate(NetworkSession.Instance.PlayerPrefab,origin+Vector2.right*.1f,Quaternion.identity).GetComponent<NetworkPlayer>();second.GetComponent<NetworkObject>().Spawn();second.GetComponent<PlayerInputAdapter>().enabled=false;Place(second,origin+Vector2.right*.1f);
            yield return new WaitForSeconds(.35f);Place(player,origin);Place(second,origin+Vector2.right*.1f);
            a.Health.RestoreHealth(1);var deaths=0;a.Health.Died+=()=>deaths++;var count=world.DeathCount;var id=a.GetComponent<NetworkObject>().NetworkObjectId;var progression=player.GetComponent<PlayerProgression>();var xp=progression.Experience;var gold=progression.Gold;
            var pickups=Object.FindObjectsByType<WorldPickup>(FindObjectsSortMode.None).Length;
            Assert.That(player.Commands.Attack(),Is.True);Assert.That(second.Commands.Attack(),Is.True);yield return new WaitForSeconds(.20f);
            Assert.That(a.Health.Current,Is.Zero);Assert.That(a.Health.ApplyDamage(999),Is.Zero);Assert.That(deaths,Is.EqualTo(1));Assert.That(world.DeathCount,Is.EqualTo(count+1));Assert.That(world.LastDeath.CreatureId,Is.EqualTo(id));Assert.That(world.LastDeath.Items.Count,Is.GreaterThan(0));
            Assert.That(progression.Experience,Is.EqualTo(xp));Assert.That(progression.Gold,Is.EqualTo(gold));Assert.That(Object.FindObjectsByType<WorldPickup>(FindObjectsSortMode.None).Length,Is.GreaterThan(pickups));
            yield return new WaitForSeconds(.4f);Assert.That(NetworkSession.Instance.Manager.SpawnManager.SpawnedObjects.ContainsKey(id),Is.False);
        }
        [UnityTest]public IEnumerator HostTargetsLivingPlayersAndLaunchesOneAuthoritativeProjectile()
        {
            yield return Host();var origin=world.World.Map.Spawn;Place(player,origin);
            var other=Object.Instantiate(NetworkSession.Instance.PlayerPrefab,origin+Vector2.right*7,Quaternion.identity).GetComponent<NetworkPlayer>();other.GetComponent<NetworkObject>().Spawn();other.GetComponent<PlayerInputAdapter>().enabled=false;Place(other,origin+Vector2.right*7);
            var ranged=world.Goblins.SpawnActor(true,origin+Vector2.up*4,"warband",origin+Vector2.up*4);yield return null;yield return null;ranged.enabled=false;
            Assert.That(SimulationAuthority.Target(ranged,null),Is.SameAs(player.Health));Place(other,origin+Vector2.up*3);Assert.That(SimulationAuthority.Target(ranged,null),Is.SameAs(other.Health));
            other.Health.ApplyDamage(other.Health.Maximum);Assert.That(SimulationAuthority.Target(ranged,null),Is.SameAs(player.Health));
            var shot=ranged.Ranged;var before=player.Health.Current;var initial=shot.Shots;Assert.That(shot.Begin(player.Health),Is.True);
            yield return new WaitForSeconds(shot.Definition.LaunchAt-.10f);Assert.That(shot.Shots,Is.EqualTo(initial));Assert.That(shot.DrawAmount(shot.Elapsed),Is.GreaterThan(.5f));
            yield return new WaitForSeconds(.16f);Assert.That(shot.Shots,Is.EqualTo(initial+1));
            var projectiles=Object.FindObjectsByType<NetworkProjectile>(FindObjectsSortMode.None);Assert.That(projectiles.Length,Is.EqualTo(1));Assert.That(projectiles[0].IsSpawned && projectiles[0].IsServer,Is.True);
            yield return new WaitForSeconds(.7f);Assert.That(player.Health.Current,Is.LessThan(before));Assert.That(shot.Shots,Is.EqualTo(initial+1));
            var band=world.Goblins.SpawnWarband(origin+Vector2.right*18,false);Assert.That(band.Length,Is.InRange(3,5));Assert.That(band.Count(g=>g.Ranged!=null),Is.InRange(0,2));yield return null;yield return null;
            Place(player,origin+Vector2.right*11);band[0].EnterCombat(player.transform.position);Assert.That(band.Any(g=>g!=band[0] && g.InCombat),Is.True);Place(player,origin);
            var skeleton=PrefabPool.Spawn(world.Night.SecondaryPrefab,origin+Vector2.up*1.3f,Quaternion.identity);skeleton.Session=world.Session;skeleton.PlayerHealth=player.Health;skeleton.Clock=world.Clock;skeleton.Lifecycle.Clock=world.Clock;skeleton.Lifecycle.Definition=world.Night.Definition;skeleton.Variant.SetEquipped(true);
            world.Clock.Model.SetElapsed(world.Clock.Definition.DaySeconds+world.Clock.Definition.EveningSeconds+1);yield return null;yield return null;skeleton.Lifecycle.RestoreActive();
            var hp=player.Health.Current;yield return new WaitForSeconds(.95f);Assert.That(player.Health.Current,Is.LessThan(hp));Assert.That(skeleton.GetComponent<NetworkCreature>().Identity.Value.Variant,Is.True);
            other.Health.ResetForSpawn();Place(other,origin+Vector2.right*2);player.Health.ApplyDamage(player.Health.Maximum);yield return new WaitForSeconds(.12f);
            Assert.That(skeleton.PlayerHealth,Is.SameAs(other.Health));Assert.That(skeleton.GetComponent<NetworkCreature>().Frame.Value.Target,Is.EqualTo(other.NetworkObjectId));
        }
        [UnityTest]public IEnumerator UnspawnedReplicaCannotRunBrainDamageAlertsOrSpawnPopulation()
        {
            SceneFlow.Home(SaveRepository.Default());yield return null;yield return null;
            Object.FindFirstObjectByType<HomeScreen>().OpenMultiplayer();
            var prefab=NetworkSession.Instance.EcologyPrefab;var go=Object.Instantiate(prefab);go.SetActive(true);yield return null;
            var replica=go.GetComponent<NetworkCreatureWorld>();Assert.That(replica.IsAuthority,Is.False);Assert.That(replica.Animals.ActiveCount,Is.Zero);Assert.That(replica.Goblins.ActiveCount,Is.Zero);Assert.That(replica.Night.ActiveCount,Is.Zero);
            Assert.That(replica.Animals.Tick(),Is.Zero);Assert.That(replica.Goblins.TickCamps(),Is.Zero);Assert.That(replica.Goblins.TrySpawn(),Is.False);Assert.That(replica.Night.TrySpawn(),Is.False);Assert.That(replica.Clock.enabled,Is.False);
            replica.Animals.Generate();replica.Animals.Restore(new AnimalSnapshot[0]);replica.Goblins.GenerateCamps();replica.Goblins.Restore(null);replica.Night.Restore(new NightCreatureSnapshot[0]);
            Assert.That(Object.FindObjectsByType<NetworkCreature>(FindObjectsSortMode.None),Is.Empty);
            var entry=replica.Prefabs.First(e=>e.Source.GetComponent<GoblinController>()!=null);var actor=Object.Instantiate(entry.NetworkPrefab);var brain=actor.GetComponent<GoblinController>();var health=actor.GetComponent<Health>();var hp=health.Current;
            Assert.That(brain.enabled,Is.False);Assert.That(health.ApplyDamage(50),Is.Zero);Assert.That(health.Current,Is.EqualTo(hp));brain.EnterCombat(Vector2.zero);Assert.That(brain.InCombat,Is.False);Assert.That(brain.Member.Broadcast(Vector2.zero,CreatureAlert.Fight),Is.Zero);Assert.That(brain.Separation.Correction(1),Is.EqualTo(Vector2.zero));
            Assert.Throws<System.InvalidOperationException>(()=>NetworkSpawnFactory.Spawn(entry.Source.GetComponent<GoblinController>(),Vector3.zero,Quaternion.identity));
            Object.Destroy(actor);Object.Destroy(go);yield return null;
        }
        [UnityTest]public IEnumerator LethalReentrantDamageFiresDeathOnceAndPoolResetRestoresSingleplayerHealth()
        {
            var definition=ScriptableObject.CreateInstance<VitalityDefinition>();definition.MaximumHealth=10;var go=new GameObject("Health fixture");go.SetActive(false);var health=go.AddComponent<Health>();health.Definition=definition;go.SetActive(true);
            var deaths=0;health.Died+=()=>deaths++;health.Damaged+=()=>health.ApplyDamage(1);Assert.That(health.ApplyDamage(50),Is.EqualTo(10));Assert.That(health.Current,Is.Zero);Assert.That(deaths,Is.EqualTo(1));
            health.ResetForSpawn();Assert.That(health.IsAlive,Is.True);health.ApplyDamage(50);Assert.That(deaths,Is.EqualTo(2));Object.Destroy(go);Object.Destroy(definition);yield return null;
        }
        [UnityTearDown]public IEnumerator Cleanup()
        {
            if(NetworkSession.Instance!=null && NetworkSession.Instance.State!=NetworkSessionState.Idle){NetworkSession.Instance.Leave();for(var i=0;i<300 && NetworkSession.Instance.State!=NetworkSessionState.Idle;i++)yield return null;}
            SceneFlow.Home(SaveRepository.Default());yield return null;Time.timeScale=1;
        }
    }
}
