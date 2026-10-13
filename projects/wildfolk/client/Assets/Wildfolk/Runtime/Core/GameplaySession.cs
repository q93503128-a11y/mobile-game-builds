using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Wildfolk
{
    public enum SessionMode { Singleplayer, Multiplayer }
    public enum SessionState { Running, Inventory, Paused, Defeated }

    public sealed class GameplaySession : MonoBehaviour
    {
        public Health PlayerHealth;
        public SessionMode Mode = SessionMode.Singleplayer;
        public SessionState State { get; private set; }
        public bool IsRunning => State == SessionState.Running;
        public bool WorldIsRunning => (Mode == SessionMode.Multiplayer || State != SessionState.Defeated) && Time.timeScale > 0;
        public event Action StateChanged;

        private void Awake() => State = SessionState.Running;
        private void OnEnable() => PlayerHealth.Died += Defeat;
        private void OnDisable() => PlayerHealth.Died -= Defeat;
        public void ToggleInventory()
        {
            if (State == SessionState.Running) OpenInventory();
            else if (State == SessionState.Inventory) CloseInventory();
        }
        public void OpenInventory() { if (State == SessionState.Running) SetState(SessionState.Inventory); }
        public void CloseInventory() { if (State == SessionState.Inventory) SetState(SessionState.Running); }
        public void OpenPause()
        {
            if (State != SessionState.Defeated && State != SessionState.Paused) SetState(SessionState.Paused);
        }
        public void Resume() { if (State == SessionState.Paused) SetState(SessionState.Running); }
        private void Defeat() => SetState(SessionState.Defeated);
        private void SetState(SessionState value)
        {
            State = value;
            StateChanged?.Invoke();
        }
        public void Restart() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
