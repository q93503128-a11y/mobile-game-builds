using System;
using UnityEngine;

namespace StackfallMobile.Runtime.Combat
{
    public sealed class Projectile : MonoBehaviour
    {
        private Vector2 _velocity;
        private float _damage;
        private float _remainingLifetime;
        private float _hitRadius;
        private Action<Projectile> _release;

        public void Configure(Vector2 position, Vector2 velocity, float damage, float lifetime, float hitRadius, Action<Projectile> release)
        {
            transform.position = position;
            _velocity = velocity;
            _damage = Mathf.Max(0f, damage);
            _remainingLifetime = Mathf.Max(0.05f, lifetime);
            _hitRadius = Mathf.Max(0.02f, hitRadius);
            _release = release;
        }

        private void Update()
        {
            var deltaTime = Time.deltaTime;
            transform.position = (Vector2)transform.position + _velocity * deltaTime;
            _remainingLifetime -= deltaTime;

            if (TryHitEnemy() || _remainingLifetime <= 0f)
            {
                _release?.Invoke(this);
            }
        }

        private bool TryHitEnemy()
        {
            var targets = CombatTargetRegistry.ActiveTargets;
            var position = (Vector2)transform.position;
            var radiusSqr = _hitRadius * _hitRadius;

            for (var i = targets.Count - 1; i >= 0; i--)
            {
                var enemy = targets[i];
                if (enemy == null || !enemy.IsAlive || !enemy.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (((Vector2)enemy.transform.position - position).sqrMagnitude > radiusSqr)
                {
                    continue;
                }

                enemy.ApplyDamage(_damage);
                return true;
            }

            return false;
        }
    }
}
