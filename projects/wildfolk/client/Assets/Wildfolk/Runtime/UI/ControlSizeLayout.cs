using UnityEngine;
namespace Wildfolk
{
    public sealed class ControlSizeLayout : MonoBehaviour
    {
        public UserPreferences Preferences;
        public GameplaySession Session;
        public RectTransform Joystick, Action, Interact, Dash, Hotbar, InventoryButton;
        private void OnEnable() {Preferences.Changed+=Apply;if(Session!=null)Session.StateChanged+=Apply;}
        private void Start() => Apply();
        private void OnDisable() {Preferences.Changed-=Apply;if(Session!=null)Session.StateChanged-=Apply;}
        public void BindSession(GameplaySession session){if(Session!=null)Session.StateChanged-=Apply;Session=session;if(isActiveAndEnabled&&Session!=null)Session.StateChanged+=Apply;if(Preferences.Values!=null)Apply();}
        private void Apply()
        {
            var scale=Preferences.Values.ControlSize;
            Dash.localScale=Joystick.localScale=Action.localScale=Interact.localScale=Hotbar.localScale=InventoryButton.localScale=new Vector3(scale,scale,1);
            Hotbar.anchoredPosition=new Vector2(51*scale,70*scale);
            var inventoryOpen=Session!=null&&Session.State==SessionState.Inventory;
            InventoryButton.anchorMin=InventoryButton.anchorMax=inventoryOpen ? Vector2.zero : new Vector2(.5f,0);
            InventoryButton.anchoredPosition=inventoryOpen ? new Vector2(40+47*scale,70*scale) : new Vector2(-549*scale,70*scale);
            var extra=Mathf.Clamp01((scale-1)/.4f);
            // The floating base position belongs to the active touch, not control-size layout.
            Action.anchoredPosition=new Vector2(-40-102*scale,160+185*extra);
            Interact.anchoredPosition=Action.anchoredPosition+new Vector2(-35*scale,200*scale);
            Dash.anchoredPosition=Action.anchoredPosition+new Vector2(-175*scale,115*scale);
        }
    }
}
