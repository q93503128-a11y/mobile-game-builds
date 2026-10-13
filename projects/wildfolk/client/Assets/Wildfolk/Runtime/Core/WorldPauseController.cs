using UnityEngine;
namespace Wildfolk
{
    public sealed class WorldPauseController : MonoBehaviour
    {
        public GameplaySession Session;
        private bool ownsFreeze;
        private void OnEnable() => Session.StateChanged += Refresh;
        private void Start() => Refresh();
        private void OnDisable() { Session.StateChanged -= Refresh; Release(); }
        private void Release() { if (ownsFreeze) Time.timeScale = 1; ownsFreeze = false; }
        private void Refresh()
        {
            if (Session.Mode == SessionMode.Multiplayer) { Release(); return; }
            var freeze = Session.State == SessionState.Paused || Session.State == SessionState.Defeated;
            if (freeze) { Time.timeScale = 0; ownsFreeze = true; }
            else Release();
        }
    }
}
