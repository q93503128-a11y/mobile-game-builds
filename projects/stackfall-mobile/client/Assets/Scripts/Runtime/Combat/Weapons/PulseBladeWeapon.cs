using StackfallMobile.Runtime.Rendering;
using UnityEngine;

namespace StackfallMobile.Runtime.Combat.Weapons
{
    public sealed class PulseBladeWeapon : MonoBehaviour
    {
        [SerializeField] private float orbitRadius = 1.05f;
        [SerializeField] private float rotationDegreesPerSecond = 190f;
        [SerializeField] private float damage = 8f;
        [SerializeField] private float damageTickSeconds = 0.2f;
        [SerializeField] private float hitRadius = 0.42f;

        private readonly Transform[] _blades = new Transform[3];
        private float _angle;
        private float _nextDamageTick;

        public int Rank { get; private set; } = 1;

        private void Awake()
        {
            for (var i = 0; i < _blades.Length; i++)
            {
                var blade = new GameObject($"PulseBlade_{i + 1}");
                blade.transform.SetParent(transform, false);
                var renderer = blade.AddComponent<SpriteRenderer>();
                renderer.sprite = RuntimeSpriteFactory.Square;
                renderer.color = new Color(0.72f, 0.85f, 1f, 0.95f);
                renderer.sortingOrder = 5;
                blade.transform.localScale = new Vector3(0.12f, 0.55f, 1f);
                _blades[i] = blade.transform;
            }

            RefreshBladeCount();
        }

        private void Update()
        {
            _angle = Mathf.Repeat(_angle + rotationDegreesPerSecond * RotationMultiplier() * Time.deltaTime, 360f);
            UpdateBladePositions();

            if (Time.time >= _nextDamageTick)
            {
                _nextDamageTick = Time.time + damageTickSeconds;
                ApplyOrbitDamage();
            }
        }

        public void IncreaseRank()
        {
            Rank = Mathf.Min(6, Rank + 1);
            RefreshBladeCount();
        }

        private void RefreshBladeCount()
        {
            var activeCount = Rank >= 6 ? 3 : Rank >= 3 ? 2 : 1;
            for (var i = 0; i < _blades.Length; i++)
            {
                _blades[i].gameObject.SetActive(i < activeCount);
            }
        }

        private void UpdateBladePositions()
        {
            var activeCount = Rank >= 6 ? 3 : Rank >= 3 ? 2 : 1;
            var radius = orbitRadius * (Rank >= 4 ? 1.18f : 1f);

            for (var i = 0; i < activeCount; i++)
            {
                var angle = (_angle + 360f * i / activeCount) * Mathf.Deg2Rad;
                var localPosition = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                var blade = _blades[i];
                blade.localPosition = localPosition;
                blade.localRotation = Quaternion.Euler(0f, 0f, _angle + 360f * i / activeCount + 90f);
            }
        }

        private void ApplyOrbitDamage()
        {
            var activeCount = Rank >= 6 ? 3 : Rank >= 3 ? 2 : 1;
            var targets = CombatTargetRegistry.ActiveTargets;
            var radiusSqr = hitRadius * hitRadius;
            var hitDamage = damage * DamageMultiplier();

            for (var targetIndex = targets.Count - 1; targetIndex >= 0; targetIndex--)
            {
                var enemy = targets[targetIndex];
                if (enemy == null || !enemy.IsAlive || !enemy.gameObject.activeInHierarchy)
                {
                    continue;
                }

                var enemyPosition = (Vector2)enemy.transform.position;
                for (var bladeIndex = 0; bladeIndex < activeCount; bladeIndex++)
                {
                    if ((enemyPosition - (Vector2)_blades[bladeIndex].position).sqrMagnitude > radiusSqr)
                    {
                        continue;
                    }

                    enemy.ApplyDamage(hitDamage);
                    break;
                }
            }
        }

        private float RotationMultiplier() => Rank >= 5 ? 1.35f : Rank >= 2 ? 1.15f : 1f;

        private float DamageMultiplier()
        {
            return Rank switch
            {
                >= 6 => 1.8f,
                >= 4 => 1.48f,
                >= 2 => 1.22f,
                _ => 1f
            };
        }
    }
}
