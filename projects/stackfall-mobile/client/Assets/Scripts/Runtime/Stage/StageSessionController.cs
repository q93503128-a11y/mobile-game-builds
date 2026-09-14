using System;
using StackfallMobile.Runtime.Player;
using UnityEngine;

namespace StackfallMobile.Runtime.Stage
{
    public enum StageSessionState
    {
        Running,
        Cleared,
        Failed
    }

    public sealed class StageSessionController : MonoBehaviour
    {
        private PlayerHealth _playerHealth;

        public StageSessionState State { get; private set; } = StageSessionState.Running;

        public event Action<StageSessionState> StateChanged;

        public void Initialize(PlayerHealth playerHealth)
        {
            _playerHealth = playerHealth;
            _playerHealth.Died += OnPlayerDied;
        }

        private void OnDestroy()
        {
            if (_playerHealth != null)
            {
                _playerHealth.Died -= OnPlayerDied;
            }
        }

        public void ClearStage()
        {
            if (State != StageSessionState.Running)
            {
                return;
            }

            State = StageSessionState.Cleared;
            Time.timeScale = 0f;
            StateChanged?.Invoke(State);
        }

        private void OnPlayerDied()
        {
            if (State != StageSessionState.Running)
            {
                return;
            }

            State = StageSessionState.Failed;
            Time.timeScale = 0f;
            StateChanged?.Invoke(State);
        }
    }
}
