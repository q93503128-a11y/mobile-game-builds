using UnityEngine;

namespace Wildfolk
{
    public sealed class WorldPauseController : MonoBehaviour
    {
        public GameplaySession Session;
        public bool FreezeWorldForPause = true;
        public bool FreezeWorldForDefeat = true;
        private void OnEnable() => Session.StateChanged += Refresh;
        private void Start() => Refresh();
        private void OnDisable() { Session.StateChanged -= Refresh; Time.timeScale = 1; }
        private void Refresh()
        {
            var freeze = (FreezeWorldForPause && Session.State == SessionState.Paused) ||
                (FreezeWorldForDefeat && Session.State == SessionState.Defeated);
            Time.timeScale = freeze ? 0 : 1;
        }
    }
}
