using System.Collections;
using StackfallMobile.Runtime.Combat.Weapons;
using StackfallMobile.Runtime.Enemies;
using StackfallMobile.Runtime.Player;
using StackfallMobile.Runtime.Progression;
using StackfallMobile.Runtime.Rendering;
using StackfallMobile.Runtime.Stage;
using StackfallMobile.Runtime.UI;
using UnityEngine;

namespace StackfallMobile.Runtime.App
{
    public sealed class StackfallAppController : MonoBehaviour
    {
        public const int PlayableStageCap = 2;

        private StackfallShellController _shell;
        private GameObject _combatRoot;
        private StageSessionController _session;
        private PlayerProgression _combatProgression;
        private int _activeStage;
        private Coroutine _transitionRoutine;

        public int HighestClearedStage { get; private set; }
        public int CurrentStage => Mathf.Clamp(HighestClearedStage + 1, 1, PlayableStageCap);
        public int CombatPower => 1000;
        public bool CanAdvanceFrom(int stage) => stage < PlayableStageCap;

        public void Initialize()
        {
            Time.timeScale = 1f;
            Application.targetFrameRate = 60;
            Screen.orientation = ScreenOrientation.Portrait;
            EnsureCamera();

            _shell = gameObject.AddComponent<StackfallShellController>();
            _shell.Initialize(this);
            SetCameraForShell();
            _shell.ShowStartup();
            _transitionRoutine = StartCoroutine(FinishStartup());
        }

        private IEnumerator FinishStartup()
        {
            yield return null;
            yield return new WaitForSecondsRealtime(0.38f);
            _transitionRoutine = null;
            ShowHome();
        }

        public void ShowHome()
        {
            CancelTransition();
            EndCombatRuntime();
            SetCameraForShell();
            _shell.ShowHome(CurrentStage);
        }

        public void ShowShip()
        {
            CancelTransition();
            EndCombatRuntime();
            SetCameraForShell();
            _shell.ShowShip();
        }

        public void ShowMailbox() => ShowShellSection(_shell.ShowMailbox);
        public void ShowMissions() => ShowShellSection(_shell.ShowMissions);
        public void ShowStore() => ShowShellSection(_shell.ShowStore);
        public void ShowRecovery() => ShowShellSection(_shell.ShowRecovery);
        public void ShowSummon() => ShowShellSection(_shell.ShowSummon);
        public void ShowSettings() => ShowShellSection(_shell.ShowSettings);
        public void ShowProfile() => ShowShellSection(_shell.ShowProfile);
        public void ShowAttendance() => ShowShellSection(_shell.ShowAttendance);
        public void ShowEvents() => ShowShellSection(_shell.ShowEvents);
        public void ShowChapters() => ShowShellSection(_shell.ShowChapters);
        public void ShowPartInventory() => ShowShellSection(_shell.ShowPartInventory);
        public void ShowChallenges() => ShowShellSection(_shell.ShowChallenges);
        public void ShowGuild() => ShowShellSection(_shell.ShowGuild);

        private void ShowShellSection(System.Action show)
        {
            CancelTransition();
            EndCombatRuntime();
            SetCameraForShell();
            show();
        }

        public void ShowLockedSection(string section, int unlockStage)
        {
            CancelTransition();
            EndCombatRuntime();
            SetCameraForShell();
            _shell.ShowLockedSection(section, unlockStage);
        }

        public void OpenLoadout(int stage)
        {
            CancelTransition();
            EndCombatRuntime();
            SetCameraForShell();
            _shell.ShowLoadout(Mathf.Clamp(stage, 1, PlayableStageCap));
        }

        public void BeginCombat(int stage)
        {
            CancelTransition();
            _transitionRoutine = StartCoroutine(BeginCombatAfterLoading(stage));
        }

        private IEnumerator BeginCombatAfterLoading(int stage)
        {
            EndCombatRuntime();
            _activeStage = Mathf.Clamp(stage, 1, PlayableStageCap);
            SetCameraForShell();
            _shell.ShowLoading(_activeStage);
            yield return null;
            yield return new WaitForSecondsRealtime(0.45f);
            _shell.Hide();
            SetCameraForCombat();
            BuildCombatRuntime();
            _transitionRoutine = null;
        }

        private void CancelTransition()
        {
            if (_transitionRoutine == null)
            {
                return;
            }

            StopCoroutine(_transitionRoutine);
            _transitionRoutine = null;
        }

        private void BuildCombatRuntime()
        {
            Time.timeScale = 1f;
            _combatRoot = new GameObject($"CombatRuntime_Stage{_activeStage}");
            _combatRoot.transform.SetParent(transform, false);

            var player = CreatePlayer(_combatRoot.transform);
            var health = player.GetComponent<PlayerHealth>();
            _combatProgression = player.GetComponent<PlayerProgression>();
            var corePulse = player.GetComponent<CorePulseWeapon>();
            var pulseBlade = player.GetComponent<PulseBladeWeapon>();
            var gravityWell = player.GetComponent<GravityWellWeapon>();

            var progressionRoot = new GameObject("ProgressionRuntime");
            progressionRoot.transform.SetParent(_combatRoot.transform, false);
            var orbSpawner = progressionRoot.AddComponent<ExperienceOrbSpawner>();
            orbSpawner.Initialize(player.transform, _combatProgression);

            _session = _combatRoot.AddComponent<StageSessionController>();
            _session.Initialize(health);

            var enemyRoot = new GameObject("EnemyRuntime");
            enemyRoot.transform.SetParent(_combatRoot.transform, false);
            var enemySpawner = enemyRoot.AddComponent<EnemySpawner>();
            enemySpawner.Initialize(player.transform, health, orbSpawner, _session.ClearStage);

            var upgradeDirector = _combatRoot.AddComponent<StageUpgradeDirector>();
            upgradeDirector.Initialize(_combatProgression, corePulse, pulseBlade, gravityWell, false);

            var hud = _combatRoot.AddComponent<PlayerHudController>();
            hud.Initialize(health, _combatProgression, enemySpawner, upgradeDirector, _session);

            _session.StateChanged += OnCombatStateChanged;
        }

        private void OnCombatStateChanged(StageSessionState state)
        {
            if (state == StageSessionState.Running)
            {
                return;
            }

            var finishedStage = _activeStage;
            var combatLevel = _combatProgression != null ? _combatProgression.Level : 1;
            var firstClear = state == StageSessionState.Cleared && finishedStage > HighestClearedStage;
            if (firstClear)
            {
                HighestClearedStage = finishedStage;
            }

            EndCombatRuntime();
            SetCameraForShell();
            _shell.ShowResult(state, finishedStage, combatLevel, firstClear);
        }

        private void EndCombatRuntime()
        {
            if (_session != null)
            {
                _session.StateChanged -= OnCombatStateChanged;
                _session = null;
            }

            _combatProgression = null;
            Time.timeScale = 1f;

            if (_combatRoot != null)
            {
                _combatRoot.SetActive(false);
                Destroy(_combatRoot);
                _combatRoot = null;
            }
        }

        private static GameObject CreatePlayer(Transform parent)
        {
            var player = new GameObject("Player");
            player.transform.SetParent(parent, false);
            player.transform.position = Vector3.zero;
            ShipVisualFactory.BuildCombatShip(player.transform);

            player.AddComponent<MobilePlayerController>();
            player.AddComponent<PlayerHealth>();
            player.AddComponent<PlayerProgression>();
            player.AddComponent<CorePulseWeapon>();
            player.AddComponent<PulseBladeWeapon>();
            player.AddComponent<GravityWellWeapon>();
            return player;
        }

        private static Camera EnsureCamera()
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
            camera.clearFlags = CameraClearFlags.SolidColor;
            return camera;
        }

        private static void SetCameraForCombat()
        {
            var camera = EnsureCamera();
            camera.backgroundColor = new Color(0.025f, 0.035f, 0.075f, 1f);
        }

        private static void SetCameraForShell()
        {
            var camera = EnsureCamera();
            camera.backgroundColor = new Color(0.012f, 0.02f, 0.045f, 1f);
        }
    }
}
