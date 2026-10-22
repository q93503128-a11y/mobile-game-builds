using UnityEngine;
namespace Wildfolk
{
    public sealed class PlayerStaminaHud : MonoBehaviour
    {
        public PlayerLocomotion Player;
        public GameplaySession Session;
        public RectTransform Anchor,CanvasRect;
        public StaminaArc Arc;
        public CanvasGroup Group;
        private float fullTime;
        private void LateUpdate()
        {
            if(Player==null || Session==null || !Session.IsRunning){Group.alpha=0;return;}
            var camera=Camera.main;if(camera==null)return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(CanvasRect,camera.WorldToScreenPoint(Player.transform.position),null,out var position);Anchor.anchoredPosition=position;
            var model=Player.Stamina;Arc.Fill=model.Current/model.Maximum;
            var depleted=model.Current<model.Maximum-.01f || Player.IsDashing || model.IsSprinting;if(depleted)fullTime=0;else fullTime+=Time.deltaTime;
            Group.alpha=Mathf.MoveTowards(Group.alpha,depleted || fullTime<Player.Definition.FullHideDelay && Group.alpha>0 ? 1 : 0,Time.unscaledDeltaTime*5);
            // Radius is defined in world space so the gauge stays outside the hands at every resolution.
            var screenRadius=camera.WorldToScreenPoint(Player.transform.position+Vector3.right*1.15f).x-camera.WorldToScreenPoint(Player.transform.position).x;var scale=GetComponentInParent<UnityEngine.Canvas>().scaleFactor;Anchor.sizeDelta=Vector2.one*screenRadius*2/scale;
        }
    }
}
