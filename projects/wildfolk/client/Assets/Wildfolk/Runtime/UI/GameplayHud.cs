using UnityEngine;
using UnityEngine.UI;

namespace Wildfolk
{
    public sealed class GameplayHud : MonoBehaviour
    {
        public GameplaySession Session;
        public VirtualJoystick Joystick;
        public HoldAttackInput Attack;
        public CanvasGroup Controls;
        public Button InventoryButton;
        public GameObject DefeatPanel;

        private void OnEnable() => Session.StateChanged+=Refresh;
        private void OnDisable() => Session.StateChanged-=Refresh;
        private void Start() => Refresh();

        private void Refresh()
        {
            Controls.interactable=Session.IsRunning;
            Controls.blocksRaycasts=Session.IsRunning;
            Controls.alpha=Session.IsRunning ? 1 : .35f;
            Joystick.ResetInput();
            Attack.ResetInput();
            InventoryButton.interactable=Session.State!=SessionState.Defeated;
            DefeatPanel.SetActive(Session.State==SessionState.Defeated);
        }
    }
}
