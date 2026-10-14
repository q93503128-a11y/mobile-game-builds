using UnityEngine;
using UnityEngine.UI;
namespace Wildfolk
{
    public sealed class ProfileHud : MonoBehaviour
    {
        public PlayerInventory Player;
        public Image HP,XP;
        public Text Level,HealthValue,SkillNotice;
        private Health health;private PlayerProgression growth;
        private int previousHP=-1,previousMaximum=-1,previousLevel=-1,previousPoints=-1;
        private void OnEnable()=>L.Changed+=LanguageChanged;
        private void OnDisable()=>L.Changed-=LanguageChanged;
        private void LanguageChanged()=>previousLevel=-1;
        private void Start(){health=Player.GetComponent<Health>();growth=Player.GetComponent<PlayerProgression>();}
        private void Update()
        {
            HP.fillAmount=health.Ratio;XP.fillAmount=growth.RequiredXP==0 ? 1 : (float)growth.CurrentXP/growth.RequiredXP;
            if(previousHP!=health.Current || previousMaximum!=health.Maximum){HealthValue.text=health.Current+" / "+health.Maximum;previousHP=health.Current;previousMaximum=health.Maximum;}
            if(previousLevel!=growth.Level){Level.text=L.Format("ui.hud.level",growth.Level);previousLevel=growth.Level;}
            if(previousPoints!=growth.SkillPoints){SkillNotice.text="+"+growth.SkillPoints;SkillNotice.gameObject.SetActive(growth.SkillPoints>0);previousPoints=growth.SkillPoints;}
        }
    }
}
