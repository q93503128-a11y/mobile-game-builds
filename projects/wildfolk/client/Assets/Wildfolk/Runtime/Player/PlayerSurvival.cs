using UnityEngine;
namespace Wildfolk
{
    public sealed class PlayerSurvival : MonoBehaviour
    {
        public SurvivalDefinition Definition;
        public Health Health;
        public GameplaySession Session;
        public PlayerInventory Inventory;
        public SurvivalModel Model { get; private set; }
        public bool CanSprint=>Model.CanSprint;
        public float SprintEfficiency=>Model.SprintEfficiency;
        public float MovementCondition=>Model.MovementCondition;
        private float healing;
        private float nextConsume;
        public TreatmentModel Treatment {get;}=new TreatmentModel();
        public MealModel Meal {get;}=new MealModel();
        public event System.Action TreatmentChanged;
        public event System.Action MealChanged;
        private NetworkPlayer network;
        public float TreatmentRemaining=>network!=null && network.IsSpawned && !network.IsServer ? Mathf.Max(0,(float)(network.Status.Value.TreatmentUntil-network.NetworkManager.ServerTime.Time)) : Treatment.Remaining;
        private bool Replica=>network!=null && network.IsSpawned && !network.IsServer;
        public ItemDefinition ActiveMealItem=>Replica ? network.Status.Value.MealItem>=0 && network.Status.Value.MealItem<network.Catalog.Items.Length ? network.Catalog.Items[network.Status.Value.MealItem] : null : Meal.Item;
        public float MealRemaining=>Replica ? Mathf.Max(0,(float)(network.Status.Value.MealUntil-network.NetworkManager.ServerTime.Time)) : Meal.Remaining;
        private void Awake(){Model=new SurvivalModel(Definition);network=GetComponent<NetworkPlayer>();}
        private void OnEnable()=>Health.Died+=ResetTransient;
        private void OnDisable(){Health.Died-=ResetTransient;ResetTransient();}
        public void ResetTransient(){Treatment.Clear();var hadMeal=Meal.Item!=null;Meal.Clear();healing=nextConsume=0;if(hadMeal){var stats=GetComponent<CharacterStats>();if(stats!=null && stats.Health!=null && stats.Progression!=null)stats.Apply();}TreatmentChanged?.Invoke();MealChanged?.Invoke();}
        public void AdvanceTemporary(float seconds)
        {
            if(!SimulationAuthority.Allows(this))return;
            if(!Health.IsAlive){if(Meal.Active){Meal.Clear();GetComponent<CharacterStats>().Apply();MealChanged?.Invoke();}AdvanceTreatment(seconds);return;}
            if(!Session.WorldIsRunning)return;
            if(network!=null && network.IsSpawned && network.IsServer)Model.AdvanceEffects(seconds);
            if(Meal.Advance(seconds)){GetComponent<CharacterStats>().Apply();MealChanged?.Invoke();}
            AdvanceTreatment(seconds);
        }
        public void AdvanceTreatment(float seconds)
        {
            if(!SimulationAuthority.Allows(this))return;
            if(!Health.IsAlive || Health.Current>=Health.Maximum){if(Treatment.Active){Treatment.Clear();TreatmentChanged?.Invoke();}return;}
            if(!Session.WorldIsRunning)return;
            var active=Treatment.Active;var amount=Treatment.Advance(seconds);if(amount>0)Health.Heal(amount);
            if(active!=Treatment.Active)TreatmentChanged?.Invoke();
        }
        private void Update()
        {
            if(!SimulationAuthority.Allows(this) || !Health.IsAlive || !Session.WorldIsRunning) return;
            AdvanceTemporary(Time.deltaTime);
            var damage=Model.Advance(Time.deltaTime);
            if(damage>0) Health.ApplyDamage(damage,true);
            AdvanceNaturalRecovery(Time.deltaTime);
        }
        public void AdvanceNaturalRecovery(float seconds)
        {
            if(!SimulationAuthority.Allows(this) || !Health.IsAlive || !Session.WorldIsRunning)return;
            if(Model.RecoveryBlockRemaining>0 || Model.ConditionRatio<Definition.RegenThreshold || Time.time-Health.LastCombatTime<Definition.RegenDelay || Health.Current>=Health.Maximum){healing=0;return;}
            healing+=(Definition.RegenPerSecond+Meal.Bonus(MealEffect.NaturalRecovery))*seconds;
            var restored=Mathf.FloorToInt(healing);if(restored>0){Health.Heal(restored);healing-=restored;}
        }
        public bool TryConsume()
        {
            var stack=Inventory.SelectedStack;
            if(!SimulationAuthority.Allows(this) || !Health.IsAlive || !Session.IsRunning || stack.IsEmpty || Time.time<nextConsume)return false;
            var food=stack.Item.Consumable;
            var consumedItem=stack.Item;
            var stamina=GetComponent<PlayerLocomotion>().Stamina;
            if(food==null || food.GradualHealthRestore>0 && Treatment.Active)return false;
            if(food.Meal==null && !Model.CanRestore(food) && !((food.HealthRestore>0 || food.GradualHealthRestore>0) && Health.Current<Health.Maximum) && !(food.StaminaRestore>0 && stamina.Current<stamina.Maximum))return false;
            if(stack.Item.Container!=null)
            {if(!Inventory.Model.ReplaceConsumedSlot(Inventory.SelectedIndex,stack.Item,stack.Item.Container.Empty)){PlayerFeedback.ShowKey("feedback.cup_space");return false;}}
            else if(!Inventory.Model.ConsumeSlot(Inventory.SelectedIndex,stack.Item,1))return false;
            Model.Restore(food);Health.Heal(food.HealthRestore);if(food.StaminaRestore>0)stamina.Restore(Mathf.Min(stamina.Maximum,stamina.Current+food.StaminaRestore),stamina.RecoveryWait);
            if(food.GradualHealthRestore>0){Treatment.Begin(food.GradualHealthRestore,food.HealingDuration);TreatmentChanged?.Invoke();}
            if(food.Meal!=null){Meal.Begin(consumedItem);GetComponent<CharacterStats>().Apply();MealChanged?.Invoke();}
            nextConsume=Time.time+food.UseCooldownSeconds;return true;
        }
    }
}
