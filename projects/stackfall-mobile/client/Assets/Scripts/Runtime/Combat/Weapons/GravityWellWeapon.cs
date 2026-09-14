using StackfallMobile.Runtime.Rendering;
using UnityEngine;
using UnityEngine.Pool;

namespace StackfallMobile.Runtime.Combat.Weapons
{
    public sealed class GravityWellWeapon : MonoBehaviour
    {
        [SerializeField] private float castRange = 6.8f;
        [SerializeField] private float castInterval = 4.4f;
        [SerializeField] private float baseRadius = 1.45f;
        [SerializeField] private float basePullStrength = 1.9f;
        [SerializeField] private float baseDamagePerTick = 6f;
        [SerializeField] private float duration = 2.6f;

        private ObjectPool<GravityWellField> _pool;
        private float _nextCastTime;

        public int Rank { get; private set; } = 1;

        private void Awake()
        {
            _pool = new ObjectPool<GravityWellField>(
                CreateField,
                field => field.gameObject.SetActive(true),
                field => field.gameObject.SetActive(false),
                field => Destroy(field.gameObject),
                false,
                4,
                12);
        }

        private void Update()
        {
            if (Time.time < _nextCastTime)
            {
                return;
            }

            var target = CombatTargetRegistry.FindNearest(transform.position, castRange);
            if (target == null)
            {
                return;
            }

            _nextCastTime = Time.time + EffectiveInterval();
            Cast(target.transform.position);
        }

        public void IncreaseRank()
        {
            Rank = Mathf.Min(6, Rank + 1);
        }

        private void Cast(Vector2 position)
        {
            var field = _pool.Get();
            var radius = baseRadius * (Rank >= 4 ? 1.28f : Rank >= 2 ? 1.12f : 1f);
            var pull = basePullStrength * (Rank >= 5 ? 1.55f : Rank >= 3 ? 1.28f : 1f);
            var damage = baseDamagePerTick * DamageMultiplier();
            var fieldDuration = duration * (Rank >= 6 ? 1.35f : Rank >= 3 ? 1.15f : 1f);
            field.Configure(position, radius, pull, damage, fieldDuration, 0.3f, ReleaseField);
        }

        private float EffectiveInterval()
        {
            return castInterval * (Rank >= 6 ? 0.72f : Rank >= 3 ? 0.84f : 1f);
        }

        private float DamageMultiplier()
        {
            return Rank switch
            {
                >= 6 => 1.9f,
                >= 4 => 1.5f,
                >= 2 => 1.2f,
                _ => 1f
            };
        }

        private GravityWellField CreateField()
        {
            var gameObject = new GameObject("GravityWell");
            var renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeSpriteFactory.Circle;
            renderer.color = new Color(0.42f, 0.18f, 0.8f, 0.42f);
            renderer.sortingOrder = 1;
            return gameObject.AddComponent<GravityWellField>();
        }

        private void ReleaseField(GravityWellField field)
        {
            _pool.Release(field);
        }
    }
}
