using System;
using StackfallMobile.Runtime.Player;
using StackfallMobile.Runtime.Progression;
using StackfallMobile.Runtime.Rendering;
using UnityEngine;
using UnityEngine.Pool;

namespace StackfallMobile.Runtime.Enemies
{
    public sealed class EnemySpawner : MonoBehaviour
    {
        private const float BossTime = 300f;
        private readonly float[] _eliteTimes = { 90f, 180f, 240f };

        private ObjectPool<EnemyActor> _pool;
        private Transform _player;
        private PlayerHealth _playerHealth;
        private ExperienceOrbSpawner _experienceOrbs;
        private Action _bossDefeated;
        private float _elapsed;
        private float _nextNormalSpawn;
        private int _nextEliteIndex;
        private int _activeEnemies;
        private bool _bossSpawned;

        public float Elapsed => _elapsed;
        public float BossSpawnTime => BossTime;
        public bool BossSpawned => _bossSpawned;
        public int ActiveEnemies => _activeEnemies;
        public EnemyActor ActiveBoss { get; private set; }

        public void Initialize(
            Transform player,
            PlayerHealth playerHealth,
            ExperienceOrbSpawner experienceOrbs,
            Action bossDefeated)
        {
            _player = player;
            _playerHealth = playerHealth;
            _experienceOrbs = experienceOrbs;
            _bossDefeated = bossDefeated;
            _pool = new ObjectPool<EnemyActor>(
                CreateEnemy,
                enemy => enemy.gameObject.SetActive(true),
                enemy => enemy.gameObject.SetActive(false),
                enemy => Destroy(enemy.gameObject),
                false,
                48,
                96);
        }

        private void Update()
        {
            if (_player == null || _playerHealth == null || !_playerHealth.IsAlive || _pool == null)
            {
                return;
            }

            _elapsed += Time.deltaTime;

            if (!_bossSpawned && _elapsed >= BossTime)
            {
                _bossSpawned = true;
                SpawnEnemy(EnemyKind.Boss);
            }

            if (_nextEliteIndex < _eliteTimes.Length && _elapsed >= _eliteTimes[_nextEliteIndex])
            {
                SpawnEnemy(EnemyKind.Elite);
                _nextEliteIndex++;
            }

            var activeCap = _bossSpawned ? 24 : 60;
            if (Time.time >= _nextNormalSpawn && _activeEnemies < activeCap)
            {
                SpawnEnemy(EnemyKind.Normal);
                var pressure = Mathf.Clamp01(_elapsed / BossTime);
                var interval = _bossSpawned ? 0.82f : Mathf.Lerp(0.62f, 0.24f, pressure);
                _nextNormalSpawn = Time.time + interval;
            }
        }

        private EnemyActor CreateEnemy()
        {
            var gameObject = new GameObject("Enemy");
            gameObject.transform.SetParent(transform, false);
            var renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeSpriteFactory.Circle;
            renderer.sortingOrder = 2;
            return gameObject.AddComponent<EnemyActor>();
        }

        private void SpawnEnemy(EnemyKind kind)
        {
            var enemy = _pool.Get();
            enemy.transform.position = PickSpawnPosition(kind);
            ConfigurePresentation(enemy, kind);

            var pressure = 1f + Mathf.Clamp01(_elapsed / BossTime) * 1.15f;
            var health = kind switch
            {
                EnemyKind.Elite => 260f * pressure,
                EnemyKind.Boss => 1800f,
                _ => 34f * pressure
            };
            var speed = kind switch
            {
                EnemyKind.Elite => 1.15f,
                EnemyKind.Boss => 0.78f,
                _ => Mathf.Lerp(1.05f, 1.55f, Mathf.Clamp01(_elapsed / BossTime))
            };
            var contactDamage = kind switch
            {
                EnemyKind.Elite => 18f,
                EnemyKind.Boss => 26f,
                _ => 8f
            };
            var contactRadius = kind switch
            {
                EnemyKind.Boss => 0.8f,
                EnemyKind.Elite => 0.52f,
                _ => 0.34f
            };
            var experience = kind switch
            {
                EnemyKind.Elite => 18,
                EnemyKind.Boss => 80,
                _ => 2
            };

            _activeEnemies++;
            enemy.Configure(
                _player,
                _playerHealth,
                kind,
                health,
                speed,
                contactDamage,
                contactRadius,
                experience,
                _experienceOrbs.Spawn,
                ReleaseEnemy,
                OnEnemyDefeated);

            if (kind == EnemyKind.Boss)
            {
                ActiveBoss = enemy;
            }
        }

        private Vector2 PickSpawnPosition(EnemyKind kind)
        {
            var radius = kind == EnemyKind.Boss ? 6.5f : UnityEngine.Random.Range(5.8f, 7.5f);
            var angle = UnityEngine.Random.value * Mathf.PI * 2f;
            return (Vector2)_player.position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }

        private static void ConfigurePresentation(EnemyActor enemy, EnemyKind kind)
        {
            var renderer = enemy.GetComponent<SpriteRenderer>();
            switch (kind)
            {
                case EnemyKind.Elite:
                    renderer.color = new Color(1f, 0.48f, 0.18f, 1f);
                    enemy.transform.localScale = Vector3.one * 0.82f;
                    break;
                case EnemyKind.Boss:
                    renderer.color = new Color(0.92f, 0.15f, 0.32f, 1f);
                    enemy.transform.localScale = Vector3.one * 1.65f;
                    break;
                default:
                    renderer.color = new Color(0.78f, 0.28f, 0.42f, 1f);
                    enemy.transform.localScale = Vector3.one * 0.48f;
                    break;
            }
        }

        private void OnEnemyDefeated(EnemyActor enemy)
        {
            _activeEnemies = Mathf.Max(0, _activeEnemies - 1);
            if (enemy.Kind != EnemyKind.Boss)
            {
                return;
            }

            ActiveBoss = null;
            _bossDefeated?.Invoke();
        }

        private void ReleaseEnemy(EnemyActor enemy)
        {
            _pool.Release(enemy);
        }
    }
}
