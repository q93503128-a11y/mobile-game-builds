using UnityEngine;
namespace Wildfolk
{
    // Permanent growth and a single temporary meal share one modifier boundary.
    public sealed class CharacterStats : MonoBehaviour
    {
        public PlayerProgression Progression;
        public Health Health;
        public int PersistentHealthBonus=>Mathf.RoundToInt(Progression.Bonus(SkillEffect.MaximumHealth));
        private float MealBonus(MealEffect effect)=>GetComponent<PlayerSurvival>()?.Meal.Bonus(effect)??0;
        public int HealthBonus=>PersistentHealthBonus+Mathf.RoundToInt(MealBonus(MealEffect.MaximumHealth));
        public float MovementMultiplier=>1+(GetComponent<PlayerEquipment>()?.Accessory?.Accessory is AccessoryDefinition a && a.Active(Health)?a.MovementBonus:0);
        public float IncomingMultiplier=>GetComponent<PlayerEquipment>()?.Accessory?.Accessory is AccessoryDefinition a && a.Active(Health)?1-a.DamageReduction:1;
        public float StaminaBonus=>Progression.Bonus(SkillEffect.MaximumStamina);
        public float RecoveryMultiplier=>1+Progression.Bonus(SkillEffect.StaminaRecovery)+(GetComponent<PlayerEquipment>()?.BodyArmor?.Armor.StaminaRecoveryBonus??0)+(GetComponent<PlayerEquipment>()?.HeadArmor?.Armor.StaminaRecoveryBonus??0)+MealBonus(MealEffect.StaminaRecovery)+(GetComponent<PlayerClass>()?.RecoveryMultiplier??1)-1;
        public float SprintCostMultiplier=>1/(1+Progression.Bonus(SkillEffect.SprintEfficiency));
        private void Start(){Progression.Changed+=Apply;Apply();}
        private void OnDestroy(){if(Progression!=null)Progression.Changed-=Apply;}
        public void Apply()=>Health.SetMaximumBonus(HealthBonus);
    }
}
