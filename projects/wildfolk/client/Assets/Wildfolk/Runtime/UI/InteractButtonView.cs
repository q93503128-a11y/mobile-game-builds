using UnityEngine;
using UnityEngine.UI;
namespace Wildfolk
{
    public sealed class InteractButtonView : MonoBehaviour
    {
        public PlayerInteraction Interaction;
        public Button Button;
        public Image Icon,Surface;
        public Sprite DefaultIcon;
        private void Start(){Interaction.Changed+=Refresh;Interaction.Equipment.Changed+=Refresh;Interaction.Session.StateChanged+=Refresh;Button.onClick.AddListener(Interaction.Use);Refresh();}
        private void OnDestroy(){if(Interaction!=null){Interaction.Changed-=Refresh;Interaction.Equipment.Changed-=Refresh;Interaction.Session.StateChanged-=Refresh;}Button.onClick.RemoveListener(Interaction.Use);}
        private void Refresh(){var armor=Interaction.CanEquipSelected || Interaction.Equipment.SelectedItem!=null && Interaction.Equipment.SelectedItem.WaterCapacity>0;var available=armor || Interaction.Target!=null;Button.interactable=available && Interaction.Session.IsRunning;Icon.sprite=armor ? Interaction.Equipment.SelectedItem.Icon : available ? Interaction.Target.Icon : DefaultIcon;Surface.color=available ? Color.white : new Color(1,1,1,.4f);}
    }
}
