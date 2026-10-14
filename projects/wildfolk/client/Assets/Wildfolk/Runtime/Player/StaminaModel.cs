using System;
using UnityEngine;
namespace Wildfolk
{
    public sealed class StaminaModel
    {
        public LocomotionDefinition Definition {get;}
        public float Current {get;private set;}
        public float Maximum {get;private set;}
        public float RecoveryWait {get;private set;}
        public bool IsSprinting {get;private set;}
        private float held;
        private bool exhausted;
        public StaminaModel(LocomotionDefinition definition){Definition=definition;Maximum=definition.MaximumStamina;Current=Maximum;}
        public void SetMaximum(float maximum){Maximum=maximum;Current=Mathf.Min(Current,Maximum);}
        public bool Spend(float amount)
        {if(amount<0 || float.IsNaN(amount))throw new ArgumentOutOfRangeException();if(Current<amount)return false;Current-=amount;RecoveryWait=Definition.RecoveryDelay;return true;}
        public void StopSprint(){IsSprinting=false;held=0;}
        public void Advance(float seconds,float magnitude,bool canSprint,float efficiency,float costMultiplier,float recoveryMultiplier)
        {
            if(seconds<0)throw new ArgumentOutOfRangeException();
            if(exhausted && Current>=Definition.ExhaustedRestart)exhausted=false;
            if(!canSprint || magnitude<Definition.SprintMaintain || exhausted)StopSprint();
            else if(!IsSprinting){held=magnitude>=Definition.SprintEnter ? held+seconds : 0;if(held>=Definition.SprintHold)IsSprinting=true;}
            if(IsSprinting)
            {var drain=Definition.SprintDrain*costMultiplier/Mathf.Max(.1f,efficiency)*seconds;Current=Mathf.Max(0,Current-drain);RecoveryWait=Definition.RecoveryDelay;if(Current==0){exhausted=true;StopSprint();}return;}
            var active=Mathf.Max(0,seconds-RecoveryWait);RecoveryWait=Mathf.Max(0,RecoveryWait-seconds);Current=Mathf.Min(Maximum,Current+Definition.Recovery*recoveryMultiplier*active);
        }
        public void Restore(float current,float wait){if(current<0 || current>Maximum || wait<0 || wait>Definition.RecoveryDelay || float.IsNaN(current) || float.IsNaN(wait))throw new ArgumentException();Current=current;RecoveryWait=wait;exhausted=current==0;StopSprint();}
    }
}
