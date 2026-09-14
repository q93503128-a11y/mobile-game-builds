using System;
using StackfallMobile.Runtime.Combat;
using StackfallMobile.Runtime.Player;
using StackfallMobile.Runtime.Rendering;
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
        private enum SpecialState
        {
            None,
            Windup,
            Dash,
            BossPulseWindup
        }

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
        private float _nextSpecialTime;
        private float _stateUntil;
        private int _experienceReward;
        private int _bossAttackIndex;
        private Vector2 _dashDirection;
        private Vector3 _baseScale;
        private SpecialState _specialState;
        private Transform _telegraph;
        private SpriteRenderer _telegraphRenderer;

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
            _specialState = SpecialState.None;
            _baseScale = transform.localScale;
            _bossAttackIndex = 0;
            _nextSpecialTime = Time.time + (kind == EnemyKind.Boss ? 2.4f : UnityEngine.Random.Range(1.8f, 3.2f));
            HideTelegraph();
            CombatTargetRegistry.Register(this);
        }

        private void OnDisable()
        {
            CombatTargetRegistry.Unregister(this);
            HideTelegraph();
        }

        private void Update()
        {
            if (!IsAlive || _target == null)
            {
                return;
            }

            switch (Kind)
            {
                case EnemyKind.Elite:
                    UpdateElite();
                    break;
                case EnemyKind.Boss:
                    UpdateBoss();
                    break;
                default:
                    MoveToward(_target.position, _moveSpeed);
                    break;
            }

            TryContactDamage();
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
            if (!IsAlive || Kind == EnemyKind.Boss || _specialState == SpecialState.Dash || strength <= 0f)
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

        private void UpdateElite()
        {
            if (_specialState == SpecialState.None && Time.time >= _nextSpecialTime)
            {
                BeginDashWindup(0.5f);
            }

            if (_specialState == SpecialState.Windup)
            {
                PulseScale(0.08f, 8f);
                if (Time.time >= _stateUntil)
                {
                    _specialState = SpecialState.Dash;
                    _stateUntil = Time.time + 0.34f;
                    transform.localScale = _baseScale;
                }
                return;
            }

            if (_specialState == SpecialState.Dash)
            {
                transform.position = (Vector2)transform.position + _dashDirection * (6.4f * Time.deltaTime);
                if (Time.time >= _stateUntil)
                {
                    _specialState = SpecialState.None;
                    _nextSpecialTime = Time.time + UnityEngine.Random.Range(2.6f, 3.8f);
                }
                return;
            }

            MoveToward(_target.position, _moveSpeed);
        }

        private void UpdateBoss()
        {
            if (_specialState == SpecialState.None && Time.time >= _nextSpecialTime)
            {
                if ((_bossAttackIndex++ & 1) == 0)
                {
                    BeginBossPulse();
                }
                else
                {
                    BeginDashWindup(0.72f);
                }
            }

            if (_specialState == SpecialState.BossPulseWindup)
            {
                UpdateBossPulseTelegraph();
                if (Time.time >= _stateUntil)
                {
                    ResolveBossPulse();
                    _specialState = SpecialState.None;
                    _nextSpecialTime = Time.time + 2.9f;
                }
                return;
            }

            if (_specialState == SpecialState.Windup)
            {
                PulseScale(0.12f, 7f);
                if (Time.time >= _stateUntil)
                {
                    _specialState = SpecialState.Dash;
                    _stateUntil = Time.time + 0.5f;
                    transform.localScale = _baseScale;
                }
                return;
            }

            if (_specialState == SpecialState.Dash)
            {
                transform.position = (Vector2)transform.position + _dashDirection * (5.2f * Time.deltaTime);
                if (Time.time >= _stateUntil)
                {
                    _specialState = SpecialState.None;
                    _nextSpecialTime = Time.time + 2.5f;
                }
                return;
            }

            var distance = Vector2.Distance(transform.position, _target.position);
            if (distance > 2.7f)
            {
                MoveToward(_target.position, _moveSpeed);
            }
        }

        private void BeginDashWindup(float duration)
        {
            var delta = (Vector2)_target.position - (Vector2)transform.position;
            _dashDirection = delta.sqrMagnitude > 0.0001f ? delta.normalized : Vector2.down;
            _specialState = SpecialState.Windup;
            _stateUntil = Time.time + duration;
        }

        private void BeginBossPulse()
        {
            EnsureTelegraph();
            _specialState = SpecialState.BossPulseWindup;
            _stateUntil = Time.time + 0.95f;
            _telegraph.gameObject.SetActive(true);
            _telegraph.localScale = Vector3.one * 0.2f;
        }

        private void UpdateBossPulseTelegraph()
        {
            EnsureTelegraph();
            const float pulseRadius = 3.15f;
            var remaining = Mathf.Max(0f, _stateUntil - Time.time);
            var progress = 1f - remaining / 0.95f;
            _telegraph.localScale = Vector3.one * Mathf.Lerp(0.2f, pulseRadius * 2f, progress);
            var alpha = Mathf.Lerp(0.14f, 0.38f, progress);
            _telegraphRenderer.color = new Color(1f, 0.35f, 0.12f, alpha);
        }

        private void ResolveBossPulse()
        {
            const float pulseRadius = 3.15f;
            if (Vector2.Distance(transform.position, _target.position) <= pulseRadius)
            {
                _playerHealth?.TryApplyDamage(24f);
            }
            HideTelegraph();
        }

        private void MoveToward(Vector2 targetPosition, float speed)
        {
            var position = (Vector2)transform.position;
            var delta = targetPosition - position;
            var distance = delta.magnitude;
            if (distance <= 0.001f)
            {
                return;
            }

            var step = Mathf.Min(distance, speed * Time.deltaTime);
            transform.position = position + delta / distance * step;
        }

        private void TryContactDamage()
        {
            var distance = Vector2.Distance(transform.position, _target.position);
            if (distance > _contactRadius || Time.time < _nextContactTime)
            {
                return;
            }

            _nextContactTime = Time.time + 0.7f;
            _playerHealth?.TryApplyDamage(_contactDamage);
        }

        private void PulseScale(float amplitude, float frequency)
        {
            var scale = 1f + Mathf.Sin(Time.time * frequency) * amplitude;
            transform.localScale = _baseScale * scale;
        }

        private void EnsureTelegraph()
        {
            if (_telegraph != null)
            {
                return;
            }

            var gameObject = new GameObject("BossPulseTelegraph");
            gameObject.transform.SetParent(transform, false);
            _telegraph = gameObject.transform;
            _telegraphRenderer = gameObject.AddComponent<SpriteRenderer>();
            _telegraphRenderer.sprite = RuntimeSpriteFactory.Circle;
            _telegraphRenderer.color = new Color(1f, 0.35f, 0.12f, 0.2f);
            _telegraphRenderer.sortingOrder = 0;
            gameObject.SetActive(false);
        }

        private void HideTelegraph()
        {
            transform.localScale = _baseScale == Vector3.zero ? transform.localScale : _baseScale;
            if (_telegraph != null)
            {
                _telegraph.gameObject.SetActive(false);
            }
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
