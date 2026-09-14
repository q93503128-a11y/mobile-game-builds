using System;
using UnityEngine;

namespace StackfallMobile.Runtime.Player
{
    public sealed class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float damageInvulnerabilitySeconds = 0.18f;

        private float _currentHealth;
        private float _invulnerableUntil;

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsAlive => _currentHealth > 0f;

        public event Action<float, float> HealthChanged;
        public event Action Died;

        private void Awake()
        {
            _currentHealth = maxHealth;
        }

        public bool TryApplyDamage(float amount)
        {
            if (!IsAlive || amount <= 0f || Time.time < _invulnerableUntil)
            {
                return false;
            }

            _invulnerableUntil = Time.time + damageInvulnerabilitySeconds;
            _currentHealth = Mathf.Max(0f, _currentHealth - amount);
            HealthChanged?.Invoke(_currentHealth, maxHealth);

            if (_currentHealth <= 0f)
            {
                Died?.Invoke();
            }

            return true;
        }

        public void Heal(float amount)
        {
            if (!IsAlive || amount <= 0f)
            {
                return;
            }

            _currentHealth = Mathf.Min(maxHealth, _currentHealth + amount);
            HealthChanged?.Invoke(_currentHealth, maxHealth);
        }

        public void ResetHealth()
        {
            _currentHealth = maxHealth;
            _invulnerableUntil = 0f;
            HealthChanged?.Invoke(_currentHealth, maxHealth);
        }
    }
}
