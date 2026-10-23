using UnityEngine;
using UnityEngine.UI;

namespace Wildfolk
{
    public sealed class BossHud : MonoBehaviour
    {
        public GameObject View;
        public Text Name;
        public Image Fill;
        private BossIdentity bound;

        private void OnEnable() { BossIdentity.ActiveChanged+=Bind; L.Changed+=Refresh; Bind(); }
        private void OnDisable()
        {
            BossIdentity.ActiveChanged-=Bind;L.Changed-=Refresh;
            if(bound!=null) bound.Health.Changed-=Refresh;
        }

        private void Bind()
        {
            if(bound!=null) bound.Health.Changed-=Refresh;
            bound=BossIdentity.Active;
            if(bound!=null) bound.Health.Changed+=Refresh;
            Refresh();
        }

        private void Refresh()
        {
            View.SetActive(bound!=null && bound.Health.IsAlive);
            if(bound==null) return;
            var encounter=bound.GetComponent<BossEncounter>();
            Name.text=encounter==null?bound.DisplayName:L.Text(encounter.Definition.NameKey);
            Fill.color=encounter!=null && encounter.State==BossState.Returning?new Color32(121,122,101,255):new Color32(169,105,68,255);
            Fill.fillAmount=bound.Health.Ratio;
        }
    }
}
