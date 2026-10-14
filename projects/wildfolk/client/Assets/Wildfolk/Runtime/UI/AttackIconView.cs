using UnityEngine;
using UnityEngine.UI;

namespace Wildfolk
{
    public sealed class AttackIconView : MonoBehaviour
    {
        public PlayerPrimaryAction Action;
        public Image Icon;
        public Sprite FistIcon;
        private void Start() {Action.Changed+=Refresh;Refresh();}
        private void OnDestroy() {if(Action!=null) Action.Changed-=Refresh;}
        private void Refresh() => Icon.sprite=Action.ActionIcon(FistIcon);
    }
}
