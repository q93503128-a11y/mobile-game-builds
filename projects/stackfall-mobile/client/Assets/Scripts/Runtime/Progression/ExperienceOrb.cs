using System;
using UnityEngine;

namespace StackfallMobile.Runtime.Progression
{
    public sealed class ExperienceOrb : MonoBehaviour
    {
        private Transform _player;
        private PlayerProgression _progression;
        private Action<ExperienceOrb> _release;
        private int _amount;
        private float _age;

        public void Configure(
            Vector2 position,
            int amount,
            Transform player,
            PlayerProgression progression,
            Action<ExperienceOrb> release)
        {
            transform.position = position;
            _amount = Mathf.Max(1, amount);
            _player = player;
            _progression = progression;
            _release = release;
            _age = 0f;
        }

        private void Update()
        {
            if (_player == null || _progression == null)
            {
                return;
            }

            _age += Time.deltaTime;
            var position = (Vector2)transform.position;
            var target = (Vector2)_player.position;
            var delta = target - position;
            var distance = delta.magnitude;

            if (distance <= 0.28f)
            {
                _progression.GrantExperience(_amount);
                _release?.Invoke(this);
                return;
            }

            if (distance <= 3.4f)
            {
                var speed = Mathf.Lerp(4f, 10f, 1f - Mathf.Clamp01(distance / 3.4f));
                transform.position = position + delta.normalized * (speed * Time.deltaTime);
            }
            else if (_age >= 18f)
            {
                _release?.Invoke(this);
            }
        }
    }
}
