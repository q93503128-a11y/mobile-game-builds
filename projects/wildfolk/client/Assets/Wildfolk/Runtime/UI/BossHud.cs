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

        private void OnEnable() { BossIdentity.ActiveChanged+=Bind; Bind(); }
        private void OnDisable()
        {
            BossIdentity.ActiveChanged-=Bind;
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
            Name.text=bound.DisplayName;
            Fill.fillAmount=bound.Health.Ratio;
        }
    }
}
