using UnityEngine;
namespace Wildfolk
{
    public sealed class PlayerLocomotion : MonoBehaviour
    {
        public LocomotionDefinition Definition;
        public CharacterStats Stats;
        public PlayerMotor Motor;
        public StaminaModel Stamina {get;private set;}
        public bool IsDashing=>dashRemaining>0;
        public Vector2 DashDirection {get;private set;}
        public float SpeedMultiplier=>Stamina.IsSprinting ? Mathf.Lerp(1,Definition.SprintSpeed,Motor.Survival.SprintEfficiency) : 1;
        private float dashRemaining,nextDash;
        private Rigidbody2D body;
        private Collider2D footprint;
        private void Awake(){Stamina=new StaminaModel(Definition);body=GetComponent<Rigidbody2D>();footprint=GetComponent<Collider2D>();}
        private void Update()
        {
            Stamina.SetMaximum(Definition.MaximumStamina+Stats.StaminaBonus);
            if(!Motor.Session.WorldIsRunning)return;
            var condition=Motor.Survival.Model.ConditionRatio;
            var recovery=Mathf.Lerp(Definition.PoorRecoveryMultiplier,1,Mathf.InverseLerp(0,Motor.Survival.Definition.RegenThreshold,condition))*Stats.RecoveryMultiplier;
            if(IsDashing){if(!Motor.Session.IsRunning)CancelDash();return;}
            Stamina.Advance(Time.deltaTime,Motor.Session.IsRunning ? Motor.Input.Move.magnitude : 0,Motor.Survival.CanSprint && GetComponent<PlayerClass>()?.Guarding!=true,Mathf.Lerp(Motor.Survival.SprintEfficiency,1,GetComponent<PlayerClass>()?.PenaltyRelief??0),Stats.SprintCostMultiplier*(GetComponent<PlayerClass>()?.SprintMultiplier(Motor.Input.Move)??1),recovery);
        }
        public bool TryDash()
        {
            if(GetComponent<PlayerClass>()?.Guarding==true || !Motor.Session.IsRunning || !GetComponent<Health>().IsAlive || IsDashing || Time.time<nextDash || !Stamina.Spend(Definition.DashCost))return false;
            DashDirection=Motor.Input.Move.sqrMagnitude>.01f ? Motor.Input.Move.normalized : Motor.Avatar.Facing.Direction;
            Motor.Combat.CancelAttack();Motor.Avatar.Facing.Face(DashDirection);Stamina.StopSprint();dashRemaining=Definition.DashDuration;nextDash=Time.time+Definition.DashCooldown;AudioRuntime.Play(Sound.Dash,transform.position);return true;
        }
        public void DashFromUI()=>TryDash();
        public bool MoveDash(float seconds)
        {
            if(!IsDashing)return false;
            if(!Motor.Session.IsRunning){CancelDash();return false;}
            var distance=Definition.DashDistance/Definition.DashDuration*Mathf.Min(seconds,dashRemaining);var remaining=distance;
            body.linearVelocity=Vector2.zero;
            // Substeps also inspect map water boundaries, which need not have a physics collider at every bank.
            var p=body.position;
            while(remaining>0){var step=Mathf.Min(.08f,remaining);if(!Motor.Terrain.CanMove(p,DashDirection,step,.55f,footprint)){dashRemaining=0;break;}p+=DashDirection*step;remaining-=step;}
            body.MovePosition(p);dashRemaining=Mathf.Max(0,dashRemaining-seconds);return true;
        }
        public void CancelDash(){dashRemaining=0;body.linearVelocity=Vector2.zero;}
        public void Restore(float current,float delay){Stamina.SetMaximum(Definition.MaximumStamina+Stats.StaminaBonus);Stamina.Restore(current,delay);CancelDash();nextDash=0;}
        private void OnDisable(){if(body!=null)CancelDash();}
    }
}
