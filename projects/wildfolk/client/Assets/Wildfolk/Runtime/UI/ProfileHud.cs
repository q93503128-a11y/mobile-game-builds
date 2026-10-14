using UnityEngine;
using UnityEngine.UI;
namespace Wildfolk
{
    public sealed class ProfileHud : MonoBehaviour
    {
        public PlayerInventory Player;
        public Image HP,XP,Stamina;
        public Text Level,HealthValue,SkillNotice;
        public CanvasGroup StaminaGroup;
        private Health health;private PlayerProgression growth;private PlayerLocomotion move;
        private float fullTime;
        private int previousHP=-1,previousMaximum=-1,previousLevel=-1,previousPoints=-1;
        private void Start(){health=Player.GetComponent<Health>();growth=Player.GetComponent<PlayerProgression>();move=Player.GetComponent<PlayerLocomotion>();}
        private void Update()
        {
            HP.fillAmount=health.Ratio;XP.fillAmount=growth.RequiredXP==0 ? 1 : (float)growth.CurrentXP/growth.RequiredXP;
            if(previousHP!=health.Current || previousMaximum!=health.Maximum){HealthValue.text=health.Current+" / "+health.Maximum;previousHP=health.Current;previousMaximum=health.Maximum;}
            if(previousLevel!=growth.Level){Level.text="Lv. "+growth.Level;previousLevel=growth.Level;}
            if(previousPoints!=growth.SkillPoints){SkillNotice.text="+"+growth.SkillPoints;SkillNotice.gameObject.SetActive(growth.SkillPoints>0);previousPoints=growth.SkillPoints;}
            var model=move.Stamina;Stamina.fillAmount=model.Current/model.Maximum;
            if(model.Current<model.Maximum-.01f || move.IsDashing || model.IsSprinting)fullTime=0;else fullTime+=Time.deltaTime;
            var visible=fullTime<move.Definition.FullHideDelay && (model.Current<model.Maximum-.01f || fullTime>0 && StaminaGroup.alpha>0);
            StaminaGroup.alpha=Mathf.MoveTowards(StaminaGroup.alpha,visible ? 1 : 0,Time.unscaledDeltaTime*5);
        }
    }
}
