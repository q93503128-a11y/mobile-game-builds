using System;
using StackfallMobile.Runtime.Combat;
using StackfallMobile.Runtime.Player;
using UnityEngine;

namespace StackfallMobile.Runtime.Enemies
{
    public enum EnemyKind
    {
        Normal,
        Elite,
        Boss
    }

    public sealed class EnemyActor : MonoBehaviour
    {
        private Transform _target;
        private PlayerHealth _playerHealth;
        private Action<EnemyActor> _release;
        private Action<Vector2, int> _spawnExperience;
        private Action<EnemyActor> _onDefeated;
        private float _maxHealth;
        private float _health;
        private float _moveSpeed;
        private float _contactDamage;
        private float _contactRadius;
        private float _nextContactTime;
        private int _experienceReward;

        public EnemyKind Kind { get; private set; }
        public bool IsAlive => _health > 0f;
        public float Health => _health;
        public float MaxHealth => _maxHealth;

        public void Configure(
            Transform target,
            PlayerHealth playerHealth,
            EnemyKind kind,
            float maxHealth,
            float moveSpeed,
            float contactDamage,
            float contactRadius,
            int experienceReward,
            Action<Vector2, int> spawnExperience,
            Action<EnemyActor> release,
            Action<EnemyActor> onDefeated)
        {
            _target = target;
            _playerHealth = playerHealth;
            Kind = kind;
            _maxHealth = Mathf.Max(1f, maxHealth);
            _health = _maxHealth;
            _moveSpeed = Mathf.Max(0f, moveSpeed);
            _contactDamage = Mathf.Max(0f, contactDamage);
            _contactRadius = Mathf.Max(0.05f, contactRadius);
            _experienceReward = Mathf.Max(1, experienceReward);
            _spawnExperience = spawnExperience;
            _release = release;
            _onDefeated = onDefeated;
            _nextContactTime = 0f;
            CombatTargetRegistry.Register(this);
        }

        private void OnDisable()
        {
            CombatTargetRegistry.Unregister(this);
        }

        private void Update()
        {
            if (!IsAlive || _target == null)
            {
                return;
            }

            var position = (Vector2)transform.position;
            var targetPosition = (Vector2)_target.position;
            var delta = targetPosition - position;
            var distance = delta.magnitude;

            if (distance > 0.001f)
            {
                var step = Mathf.Min(distance, _moveSpeed * Time.deltaTime);
                transform.position = position + delta / distance * step;
            }

            if (distance <= _contactRadius && Time.time >= _nextContactTime)
            {
                _nextContactTime = Time.time + 0.7f;
                _playerHealth?.TryApplyDamage(_contactDamage);
            }
        }

        public void ApplyDamage(float amount)
        {
            if (!IsAlive || amount <= 0f)
            {
                return;
            }

            _health = Mathf.Max(0f, _health - amount);
            if (_health <= 0f)
            {
                Defeat();
            }
        }

        public void PullTowards(Vector2 center, float strength, float deltaTime)
        {
            if (!IsAlive || Kind == EnemyKind.Boss || strength <= 0f)
            {
                return;
            }

            var position = (Vector2)transform.position;
            var delta = center - position;
            if (delta.sqrMagnitude < 0.0001f)
            {
                return;
            }

            transform.position = position + Vector2.ClampMagnitude(delta, strength * deltaTime);
        }

        private void Defeat()
        {
            CombatTargetRegistry.Unregister(this);
            _spawnExperience?.Invoke(transform.position, _experienceReward);
            _onDefeated?.Invoke(this);
            _release?.Invoke(this);
        }
    }
}
