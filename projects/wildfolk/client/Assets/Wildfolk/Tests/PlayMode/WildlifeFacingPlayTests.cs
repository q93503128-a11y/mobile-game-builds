#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Wildfolk.Tests
{
    public sealed class WildlifeFacingPlayTests
    {
        private WorldSaveService world;
        private GameObject liveActor;
        private static readonly Vector2[] Directions = { Vector2.right, Vector2.left, Vector2.up, Vector2.down,
            new Vector2(1, 1).normalized, new Vector2(-1, 1).normalized,
            new Vector2(1, -1).normalized, new Vector2(-1, -1).normalized };
        // Independent art axes: head/tail geometry in generate_creature_loot_art.py,
        // confirmed against the actual prefab gallery. Do not derive these from the offset under test.
        [TestCase("Bat", 1)]
        [TestCase("StoneCrawler", -1)]
        [TestCase("BogLeech", -1)]
        [TestCase("Wolf", 1)]
        [TestCase("MountainGoat", 1)]
        [TestCase("Crow", 1)]
        [TestCase("GiantBoar", -1)]
        [TestCase("CrystalCrawler", -1)]
        public void AuthoredHeadFollowsIntentForLocalAndReplicaInEightDirections(string species, int headY)
        {
            var present = typeof(WildlifeActionBrain).GetMethod("Present", BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (var network in new[] { false, true })
            {
                var path = "Assets/Wildfolk/Prefabs/" + (network ? "Network" : "") + species + ".prefab";
                var root = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path));
                try
                {
                    var brain = root.GetComponent<WildlifeActionBrain>();
                    brain.enabled = false;
                    foreach (var phase in new[] { WildlifePhase.Idle, WildlifePhase.Warning,
                        WildlifePhase.Windup, WildlifePhase.Action, WildlifePhase.Recovery })
                    foreach (var direction in Directions)
                    {
                        if (network) brain.PresentReplica((byte)phase, 9.9, direction, 10, true);
                        else
                        {
                            // Awaken camouflage through the public replica boundary, then exercise
                            // the exact presentation method used by authority Tick, without duplicating its math.
                            brain.PresentReplica((byte)phase, 9.9, direction, 10, true);
                            present.Invoke(brain, new object[] { 10f, (byte)phase, 9.9f, direction });
                        }
                        var head = (Vector2)brain.Pose.TransformDirection(new Vector3(0, headY, 0));
                        Assert.That(Vector2.Dot(head.normalized, direction), Is.GreaterThan(.999f),
                            path + " / " + phase + " / " + direction);
                        Assert.That(brain.Body.flipX || brain.Body.flipY, Is.False, path);
                    }
                }
                finally { Object.DestroyImmediate(root); }
            }
        }

        [Test]
        public void DormantCamouflageKeepsItsRestPoseAndAwakenedPoseUsesFacing()
        {
            var root = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/StoneCrawler.prefab"));
            try
            {
                var brain = root.GetComponent<WildlifeActionBrain>();
                brain.enabled = false;
                brain.PresentReplica((byte)WildlifePhase.Idle, 0, Vector2.right, 10, false);
                Assert.That(brain.Body.sprite, Is.SameAs(brain.RestSprite));
                Assert.That(Quaternion.Angle(brain.Pose.localRotation, Quaternion.identity), Is.LessThan(.01f));
                brain.PresentReplica((byte)WildlifePhase.Idle, 0, Vector2.right, 10, true);
                Assert.That(brain.Body.sprite, Is.SameAs(brain.ActiveSprite));
                Assert.That(Vector2.Dot(brain.Pose.TransformDirection(Vector3.down), Vector2.right), Is.GreaterThan(.999f));
            }
            finally { Object.DestroyImmediate(root); }
        }

        private IEnumerator EnterWorld()
        {
            Time.timeScale = 1;
            WorldGenerator.SelectedSeed = 9472026;
            WorldGenerator.SelectedGenerationVersion = 3;
            yield return SceneManager.LoadSceneAsync("Wildfolk");
            yield return null;
            world = Object.FindFirstObjectByType<WorldSaveService>();
            world.AutoSaveEnabled = false;
            world.Clock.enabled = world.World.Animals.enabled = false;
            world.World.GetComponent<ZombieSpawner>().enabled = world.World.GetComponent<GoblinSpawner>().enabled = false;
            world.World.Animals.Restore(System.Array.Empty<AnimalSnapshot>(), new AnimalEcologySnapshot());
            world.Player.GetComponent<PlayerInputAdapter>().enabled = world.Player.GetComponent<PlayerMotor>().enabled = false;
            world.Player.GetComponent<PlayerSurvival>().enabled = false;
            world.World.Chunks.DrainPending();
            foreach (var node in Object.FindObjectsByType<ResourceNode>(FindObjectsSortMode.None))
                if (Vector2.Distance(node.transform.position, Vector2.zero) < 20) node.gameObject.SetActive(false);
        }

        private void MovePlayer(Vector2 position)
        {
            world.Player.transform.position = position;
            world.Player.GetComponent<Rigidbody2D>().position = position;
            Physics2D.SyncTransforms();
        }

        private WildlifeActionBrain Spawn(string species, Vector2 position)
        {
            var prefab = world.World.Animals.Definition.Rules.Single(r => r.Prefab.Member.Definition.Id == species).Prefab;
            var animal = PrefabPool.Spawn(prefab, position, Quaternion.identity);
            liveActor = animal.gameObject;
            animal.Session = world.Player.GetComponent<PlayerInputCommands>().Session;
            animal.Player = animal.Separation.Player = world.Player.transform;
            animal.Member.Anchor = position;
            animal.Member.GroupId = "facing-regression";
            var loot = animal.GetComponent<LootOnDeath>();
            loot.Player = world.Player; loot.Settings = world.World.Settings; loot.Pickup = world.World.Pickup;
            var brain = animal.GetComponent<WildlifeActionBrain>();
            brain.Bind(world.World.GetComponent<WorldNavigation>());
            return brain;
        }

        [UnityTest]
        public IEnumerator CrowHeadFollowsActualWanderAndFleeDisplacement()
        {
            yield return EnterWorld();
            MovePlayer(new Vector2(0, -15));
            var brain = Spawn("crow", new Vector2(0, 4));
            yield return new WaitForSeconds(.15f);
            var before = (Vector2)brain.transform.position;
            yield return new WaitForSeconds(.3f);
            var delta = (Vector2)brain.transform.position - before;
            Assert.That(delta.magnitude, Is.GreaterThan(.05f), "actual wander displacement");
            Assert.That(Vector2.Dot(delta.normalized, brain.Pose.TransformDirection(Vector3.up)), Is.GreaterThan(.98f));
            TestContext.WriteLine("wander displacement=" + delta + " rendered-head=" + brain.Pose.up);
            foreach (var direction in Directions)
            {
                MovePlayer((Vector2)brain.transform.position - direction * 3);
                yield return new WaitForSeconds(.15f);
                before = brain.transform.position;
                yield return new WaitForSeconds(.2f);
                delta = (Vector2)brain.transform.position - before;
                Assert.That(delta.magnitude, Is.GreaterThan(.2f), "actual flee displacement " + direction);
                Assert.That(Vector2.Dot(delta.normalized, direction), Is.GreaterThan(.98f), "flee AI unchanged");
                Assert.That(Vector2.Dot(delta.normalized, brain.Pose.TransformDirection(Vector3.up)), Is.GreaterThan(.98f), "rendered head " + direction);
                TestContext.WriteLine("flee displacement=" + delta + " rendered-head=" + brain.Pose.up);
            }
        }

        [UnityTest]
        public IEnumerator DeathAndPoolReuseRestoreLivingVisualsForCorrectedSpecies()
        {
            yield return EnterWorld();
            MovePlayer(new Vector2(0, -15));
            foreach (var species in new[] { "bat", "wolf", "mountain_goat", "crow" })
            {
                var brain = Spawn(species, new Vector2(0, 4));
                var animal = brain.Animal;
                var rest = animal.Visual.localScale;
                brain.PresentReplica((byte)WildlifePhase.Idle, 0, Vector2.right, 10, true);
                animal.Health.ApplyDamage(10000, true, world.Player);
                yield return new WaitForSeconds(.28f);
                Assert.That(animal.Visual.localScale.y, Is.LessThan(rest.y * .6f), species + " real corpse squash");
                yield return new WaitForSeconds(.15f);
                Assert.That(animal.gameObject.activeSelf, Is.False, species + " actual death release");
                var reused = Spawn(species, new Vector2(0, 4));
                Assert.That(reused, Is.SameAs(brain), species + " real pool reuse");
                Assert.That(reused.Animal.Health.Current, Is.EqualTo(reused.Animal.Health.Maximum));
                Assert.That(reused.Animal.Visual.localScale, Is.EqualTo(rest));
                Assert.That(reused.enabled && reused.GetComponent<Collider2D>().enabled, Is.True);
                reused.PresentReplica((byte)WildlifePhase.Idle, 0, Vector2.right, 10, true);
                Assert.That(Vector2.Dot(reused.Pose.TransformDirection(Vector3.up), Vector2.right), Is.GreaterThan(.999f));
                TestContext.WriteLine(species + " death/reuse: same instance, full HP, living scale, right-facing head");
                Object.DestroyImmediate(liveActor); liveActor = null;
            }
        }

        [UnityTearDown]
        public IEnumerator Exit()
        {
            if (liveActor != null) Object.DestroyImmediate(liveActor);
            WorldGenerator.SelectedSeed = null;
            Time.timeScale = 1;
            yield return null;
        }
    }
}
#endif
