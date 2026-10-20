using UnityEngine;
namespace Wildfolk
{
    // One aggregation boundary for future status modifiers; no status content is registered here.
    public sealed class CharacterStats : MonoBehaviour
    {
        public PlayerProgression Progression;
        public Health Health;
        public int HealthBonus=>Mathf.RoundToInt(Progression.Bonus(SkillEffect.MaximumHealth));
        public float StaminaBonus=>Progression.Bonus(SkillEffect.MaximumStamina);
        public float RecoveryMultiplier=>1+Progression.Bonus(SkillEffect.StaminaRecovery)+(GetComponent<PlayerEquipment>()?.BodyArmor?.Armor.StaminaRecoveryBonus??0)+(GetComponent<PlayerEquipment>()?.HeadArmor?.Armor.StaminaRecoveryBonus??0);
        public float SprintCostMultiplier=>1/(1+Progression.Bonus(SkillEffect.SprintEfficiency));
        private void Start(){Progression.Changed+=Apply;Apply();}
        private void OnDestroy(){if(Progression!=null)Progression.Changed-=Apply;}
        public void Apply()=>Health.SetMaximumBonus(HealthBonus);
    }
}
