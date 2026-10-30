using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace Wildfolk
{
    public sealed class InteractButtonView : MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler
    {
        public PlayerInteraction Interaction;
        public Button Button;
        public Image Icon,Surface;
        public Sprite DefaultIcon;
        private bool held,suppressClick;private float nextProbe;private bool hadDowned;
        private PlayerInputCommands Commands=>Interaction.GetComponent<PlayerInputCommands>();
        private void Start(){Interaction.Changed+=Refresh;Interaction.Equipment.Changed+=Refresh;Interaction.Session.StateChanged+=Refresh;Button.onClick.AddListener(Click);Refresh();}
        private void OnDestroy(){if(Interaction!=null){Interaction.Changed-=Refresh;Interaction.Equipment.Changed-=Refresh;Interaction.Session.StateChanged-=Refresh;}Button.onClick.RemoveListener(Click);}
        public void OnPointerDown(PointerEventData e){if(Interaction==null)return;suppressClick=Commands.HasDownedTeammate;held=suppressClick;if(held)Commands.HoldInteract(true);}
        public void OnPointerUp(PointerEventData e){if(held)Commands.HoldInteract(false);held=false;}
        public void OnPointerExit(PointerEventData e){OnPointerUp(e);}
        private void Click(){if(!suppressClick)Interaction.Use();suppressClick=false;}
        private void Update(){if(Interaction==null)return;if(held)Commands.HoldInteract(true);if(Time.unscaledTime<nextProbe)return;nextProbe=Time.unscaledTime+.15f;var nearby=Commands.HasDownedTeammate;if(nearby!=hadDowned){hadDowned=nearby;Refresh();}}
        private void OnDisable(){if(held && Interaction!=null)Commands.HoldInteract(false);held=false;}
        private void Refresh(){var armor=Interaction.CanEquipSelected || Interaction.UsesSelectedWaterContainer;var rescue=Commands.HasDownedTeammate;var available=rescue || armor || Interaction.Target!=null;Button.interactable=available && (Interaction.Session.IsRunning || rescue && Interaction.Session.State==SessionState.Rescuing);Icon.sprite=rescue ? DefaultIcon : armor ? Interaction.Equipment.SelectedItem.Icon : available ? Interaction.Target.Icon : DefaultIcon;Surface.color=available ? Color.white : new Color(1,1,1,.4f);}
    }
}
