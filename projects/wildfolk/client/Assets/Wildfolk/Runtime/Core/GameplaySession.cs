using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Wildfolk
{
    public enum SessionState { Running, Inventory, Defeated }

    public sealed class GameplaySession : MonoBehaviour
    {
        public Health PlayerHealth;
        public SessionState State { get; private set; }
        public bool IsRunning => State == SessionState.Running;
        public event Action StateChanged;

        private void Awake() { State = SessionState.Running; Time.timeScale = 1; }
        private void OnEnable() => PlayerHealth.Died += Defeat;
        private void OnDisable() => PlayerHealth.Died -= Defeat;
        private void OnDestroy() => Time.timeScale = 1;

        public void ToggleInventory()
        {
            if (State == SessionState.Defeated) return;
            SetState(State == SessionState.Inventory ? SessionState.Running : SessionState.Inventory);
        }

        public void CloseInventory()
        {
            if (State == SessionState.Inventory) SetState(SessionState.Running);
        }

        private void Defeat() => SetState(SessionState.Defeated);

        private void SetState(SessionState value)
        {
            State = value;
            Time.timeScale = IsRunning ? 1 : 0;
            StateChanged?.Invoke();
        }

        public void Restart()
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
