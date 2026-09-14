using System;
using UnityEngine;

namespace StackfallMobile.Runtime.Combat.Weapons
{
    public sealed class GravityWellField : MonoBehaviour
    {
        private float _radius;
        private float _pullStrength;
        private float _damagePerTick;
        private float _remainingDuration;
        private float _tickInterval;
        private float _tickTimer;
        private Action<GravityWellField> _release;

        public void Configure(
            Vector2 position,
            float radius,
            float pullStrength,
            float damagePerTick,
            float duration,
            float tickInterval,
            Action<GravityWellField> release)
        {
            transform.position = position;
            _radius = Mathf.Max(0.1f, radius);
            _pullStrength = Mathf.Max(0f, pullStrength);
            _damagePerTick = Mathf.Max(0f, damagePerTick);
            _remainingDuration = Mathf.Max(0.1f, duration);
            _tickInterval = Mathf.Max(0.05f, tickInterval);
            _tickTimer = 0f;
            _release = release;

            // RuntimeSpriteFactory.Circle has a 1-unit local radius, so scale == radius keeps
            // the visible edge aligned with the actual pull/damage boundary.
            transform.localScale = Vector3.one * _radius;
        }

        private void Update()
        {
            var deltaTime = Time.deltaTime;
            _remainingDuration -= deltaTime;
            _tickTimer -= deltaTime;

            ApplyPull(deltaTime);

            if (_tickTimer <= 0f)
            {
                _tickTimer += _tickInterval;
                ApplyDamage();
            }

            if (_remainingDuration <= 0f)
            {
                _release?.Invoke(this);
            }
        }

        private void ApplyPull(float deltaTime)
        {
            var targets = CombatTargetRegistry.ActiveTargets;
            var center = (Vector2)transform.position;
            var radiusSqr = _radius * _radius;

            for (var i = targets.Count - 1; i >= 0; i--)
            {
                var enemy = targets[i];
                if (enemy == null || !enemy.IsAlive || !enemy.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (((Vector2)enemy.transform.position - center).sqrMagnitude <= radiusSqr)
                {
                    enemy.PullTowards(center, _pullStrength, deltaTime);
                }
            }
        }

        private void ApplyDamage()
        {
            var targets = CombatTargetRegistry.ActiveTargets;
            var center = (Vector2)transform.position;
            var radiusSqr = _radius * _radius;

            for (var i = targets.Count - 1; i >= 0; i--)
            {
                var enemy = targets[i];
                if (enemy == null || !enemy.IsAlive || !enemy.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (((Vector2)enemy.transform.position - center).sqrMagnitude <= radiusSqr)
                {
                    enemy.ApplyDamage(_damagePerTick);
                }
            }
        }
    }
}
