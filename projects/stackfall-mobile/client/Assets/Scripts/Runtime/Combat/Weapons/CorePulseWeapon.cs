using StackfallMobile.Runtime.Rendering;
using UnityEngine;
using UnityEngine.Pool;

namespace StackfallMobile.Runtime.Combat.Weapons
{
    public sealed class CorePulseWeapon : MonoBehaviour
    {
        [SerializeField] private float range = 7.2f;
        [SerializeField] private float fireInterval = 0.68f;
        [SerializeField] private float projectileSpeed = 10.5f;
        [SerializeField] private float damage = 12f;

        private ObjectPool<Projectile> _pool;
        private float _nextFireTime;

        public int Rank { get; private set; } = 1;

        private void Awake()
        {
            _pool = new ObjectPool<Projectile>(
                CreateProjectile,
                projectile => projectile.gameObject.SetActive(true),
                projectile => projectile.gameObject.SetActive(false),
                projectile => Destroy(projectile.gameObject),
                false,
                24,
                96);
        }

        private void Update()
        {
            if (Time.time < _nextFireTime)
            {
                return;
            }

            var target = CombatTargetRegistry.FindNearest(transform.position, range);
            if (target == null)
            {
                return;
            }

            _nextFireTime = Time.time + EffectiveInterval();
            FireAt(target.transform.position);
        }

        public void IncreaseRank()
        {
            Rank = Mathf.Min(6, Rank + 1);
        }

        private float EffectiveInterval()
        {
            var multiplier = Rank switch
            {
                >= 6 => 0.64f,
                >= 4 => 0.76f,
                >= 2 => 0.9f,
                _ => 1f
            };
            return fireInterval * multiplier;
        }

        private void FireAt(Vector2 targetPosition)
        {
            var origin = (Vector2)transform.position;
            var direction = (targetPosition - origin).normalized;
            var projectileCount = Rank >= 5 ? 2 : 1;
            var spread = projectileCount > 1 ? 7f : 0f;

            for (var i = 0; i < projectileCount; i++)
            {
                var angle = projectileCount == 1 ? 0f : Mathf.Lerp(-spread, spread, i / (float)(projectileCount - 1));
                var rotated = Quaternion.Euler(0f, 0f, angle) * direction;
                var projectile = _pool.Get();
                projectile.Configure(
                    origin,
                    rotated * projectileSpeed,
                    damage * DamageMultiplier(),
                    1.15f,
                    Rank >= 3 ? 0.28f : 0.22f,
                    ReleaseProjectile);
            }
        }

        private float DamageMultiplier()
        {
            return Rank switch
            {
                >= 6 => 1.85f,
                >= 4 => 1.45f,
                >= 2 => 1.2f,
                _ => 1f
            };
        }

        private Projectile CreateProjectile()
        {
            var gameObject = new GameObject("CorePulseProjectile");
            var renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeSpriteFactory.Circle;
            renderer.color = new Color(0.45f, 0.9f, 1f, 1f);
            renderer.sortingOrder = 5;
            gameObject.transform.localScale = Vector3.one * 0.17f;
            return gameObject.AddComponent<Projectile>();
        }

        private void ReleaseProjectile(Projectile projectile)
        {
            _pool.Release(projectile);
        }
    }
}
