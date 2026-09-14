using StackfallMobile.Runtime.Combat.Weapons;
using StackfallMobile.Runtime.Enemies;
using StackfallMobile.Runtime.Player;
using StackfallMobile.Runtime.Progression;
using StackfallMobile.Runtime.Rendering;
using StackfallMobile.Runtime.Stage;
using UnityEngine;

namespace StackfallMobile.Runtime.Bootstrap
{
    public sealed class PrototypeBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureRuntime()
        {
            if (FindFirstObjectByType<PrototypeBootstrap>() != null)
            {
                return;
            }

            var root = new GameObject("StackfallRuntime");
            DontDestroyOnLoad(root);
            root.AddComponent<PrototypeBootstrap>().Build();
        }

        private void Build()
        {
            Time.timeScale = 1f;
            Application.targetFrameRate = 60;
            Screen.orientation = ScreenOrientation.Portrait;

            EnsureCamera();

            var player = CreatePlayer();
            var health = player.GetComponent<PlayerHealth>();
            var progression = player.GetComponent<PlayerProgression>();
            var corePulse = player.GetComponent<CorePulseWeapon>();
            var pulseBlade = player.GetComponent<PulseBladeWeapon>();
            var gravityWell = player.GetComponent<GravityWellWeapon>();

            var progressionRoot = new GameObject("ProgressionRuntime");
            progressionRoot.transform.SetParent(transform, false);
            var orbSpawner = progressionRoot.AddComponent<ExperienceOrbSpawner>();
            orbSpawner.Initialize(player.transform, progression);

            var session = gameObject.AddComponent<StageSessionController>();
            session.Initialize(health);

            var enemyRoot = new GameObject("EnemyRuntime");
            enemyRoot.transform.SetParent(transform, false);
            var enemySpawner = enemyRoot.AddComponent<EnemySpawner>();
            enemySpawner.Initialize(player.transform, health, orbSpawner, session.ClearStage);

            var upgradeDirector = gameObject.AddComponent<StageUpgradeDirector>();
            upgradeDirector.Initialize(progression, corePulse, pulseBlade, gravityWell, true);
        }

        private static void EnsureCamera()
        {
            var camera = Camera.main;
            if (camera == null)
            {
                var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
                camera = cameraObject.AddComponent<Camera>();
            }

            camera.orthographic = true;
            camera.orthographicSize = 9.2f;
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.backgroundColor = new Color(0.025f, 0.035f, 0.075f, 1f);
            camera.clearFlags = CameraClearFlags.SolidColor;
        }

        private static GameObject CreatePlayer()
        {
            var player = new GameObject("Player");
            player.transform.position = Vector3.zero;
            player.transform.localScale = Vector3.one * 0.58f;

            var renderer = player.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeSpriteFactory.Circle;
            renderer.color = new Color(0.35f, 0.9f, 1f, 1f);
            renderer.sortingOrder = 6;

            player.AddComponent<MobilePlayerController>();
            player.AddComponent<PlayerHealth>();
            player.AddComponent<PlayerProgression>();
            player.AddComponent<CorePulseWeapon>();
            player.AddComponent<PulseBladeWeapon>();
            player.AddComponent<GravityWellWeapon>();
            return player;
        }
    }
}
