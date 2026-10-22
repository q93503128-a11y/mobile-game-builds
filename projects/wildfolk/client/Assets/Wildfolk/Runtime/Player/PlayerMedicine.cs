using System;
using UnityEngine;
namespace Wildfolk
{
    // Item category and delivery data determine behavior; no medicine item IDs are used.
    public sealed class PlayerMedicine:MonoBehaviour
    {
        public bool Applying=>target!=null;
        public double Until {get;private set;}
        private PlayerInventory bag;private Health own,target;private PlayerSurvival survival;
        private ItemDefinition item;private int slot;private uint revision,startRevision;private Vector2 origin;
        private PlayerClass role;
        private void Awake(){bag=GetComponent<PlayerInventory>();own=GetComponent<Health>();survival=GetComponent<PlayerSurvival>();role=GetComponent<PlayerClass>();}
        private void Start()=>bag.Model.Changed+=InventoryChanged;
        private void InventoryChanged()=>revision++;
        private void OnEnable()=>GetComponent<Health>().Damaged+=Cancel;
        private void OnDisable(){own.Damaged-=Cancel;Cancel();}
        private void OnDestroy(){if(bag?.Model!=null)bag.Model.Changed-=InventoryChanged;}
        private bool Valid(Health other)
        {
            if(other==null || !own.IsAlive || !other.IsAlive || !GetComponent<PlayerInputCommands>().Session.IsRunning || GetComponent<PlayerDeath>()?.CanAct==false || GetComponent<PlayerInputCommands>().IsScouting || other.GetComponent<PlayerSurvival>()==null || other.GetComponent<PlayerDeath>()?.State!=PlayerLife.Alive)return false;
            if(other!=own && (role?.CanHealAlly!=true || Vector2.Distance(transform.position,other.transform.position)>2.6f || !ClassActionRules.Clear(own,other)))return false;
            var net=other.GetComponent<NetworkPlayer>();if(other!=own && (net==null || !net.IsSpawned))return false;
            return true;
        }
        public bool Begin(Health other)
        {
            if(!SimulationAuthority.Allows(this) || Applying || !Valid(other) || Time.time<survival.UseReadyAt)return false;
            var stack=bag.SelectedStack;var medicine=stack.Item?.Consumable;
            if(stack.IsEmpty || medicine?.Kind!=ConsumableKind.Medicine || other.Current>=other.Maximum || medicine.GradualHealthRestore>0 && other.GetComponent<PlayerSurvival>().Treatment.Active)return false;
            item=stack.Item;slot=bag.SelectedIndex;target=other;origin=transform.position;startRevision=revision;
            Until=role.Now+medicine.ApplicationSeconds;
            if(medicine.ApplicationSeconds<=0)return Commit();return true;
        }
        private bool Commit()
        {
            if(!Valid(target) || slot!=bag.SelectedIndex || revision!=startRevision || bag.SelectedStack.Item!=item || Time.time<survival.UseReadyAt){Cancel();return false;}
            var medicine=item.Consumable;var recipient=target.GetComponent<PlayerSurvival>();if(target.Current>=target.Maximum || medicine.GradualHealthRestore>0 && recipient.Treatment.Active){Cancel();return false;}
            var stats=target.GetComponent<PlayerClass>();var received=stats?.MedicineReceived??1+(target.GetComponent<PlayerProgression>()?.Bonus(SkillEffect.MedicineReceived)??0);
            var multiplier=(role?.HealingOutgoing(target)??1)*received;
            var success=bag.Model.TryDropSlot(slot,item,1,()=>
            {target.Heal(Mathf.RoundToInt(medicine.HealthRestore*multiplier));if(medicine.GradualHealthRestore>0)recipient.ReceiveTreatment(Mathf.RoundToInt(medicine.GradualHealthRestore*multiplier),medicine.HealingDuration*(role?.HotDurationMultiplier??1));return true;});
            if(success){survival.UseReadyAt=Time.time+medicine.UseCooldownSeconds*(role?.MedicineCooldownMultiplier??1);GetComponent<NetworkClassActions>()?.MedicineFeedback(target);}
            Cancel();return success;
        }
        public void Cancel(){target=null;item=null;Until=0;}
        private void Update()
        {if(!Applying || !SimulationAuthority.Allows(this))return;if(!Valid(target) || GetComponent<PlayerInputCommands>().Move.sqrMagnitude>.01f || Vector2.Distance(origin,transform.position)>.2f || revision!=startRevision || bag.SelectedIndex!=slot){Cancel();return;}if(role.Now>=Until)Commit();}
    }
}
