using System;
using UnityEngine;
namespace Wildfolk
{
    public sealed class PlayerBuildingService:MonoBehaviour
    {
        public StructureMaintenance Target {get;private set;}
        public BuildingServiceKind Kind {get;private set;}
        public double Until {get;private set;}
        public bool Active=>Target!=null;
        public event Action Changed;
        private PlayerInventory bag;private PlayerClass role;private Health health;private Vector2 origin;
        private bool completed;private double lease;private uint damageRevision,inventoryRevision,revisionAtStart;
        private void Awake(){bag=GetComponent<PlayerInventory>();role=GetComponent<PlayerClass>();health=GetComponent<Health>();}
        private void Start(){bag.Model.Changed+=InventoryChanged;}
        private void OnEnable()=>GetComponent<Health>().Damaged+=Cancel;
        private void OnDisable(){health.Damaged-=Cancel;Cancel();}
        private void OnDestroy(){if(bag?.Model!=null)bag.Model.Changed-=InventoryChanged;}
        private void InventoryChanged()=>inventoryRevision++;
        public bool Hold(StructureMaintenance target,BuildingServiceKind kind,bool held)
        {
            if(!SimulationAuthority.Allows(this))return false;if(!held){completed=false;Cancel();return true;}if(completed)return true;
            if(!Valid(target) || kind!=BuildingServiceKind.Repair && kind!=BuildingServiceKind.Dismantle)return false;
            if(kind==BuildingServiceKind.Repair && !target.CanRepair(role) || kind==BuildingServiceKind.Dismantle && target.HasContents())return false;
            if(Target!=target || Kind!=kind)
            {Cancel();Target=target;Kind=kind;origin=transform.position;damageRevision=target.DamageRevision;revisionAtStart=inventoryRevision;Until=role.Now+(kind==BuildingServiceKind.Repair?role.RepairTime(target.GetComponent<WorldPlaceable>()):2);Changed?.Invoke();}
            lease=role.Now+.5;return true;
        }
        private bool Valid(StructureMaintenance target)=>target!=null && target.GetComponent<WorldPlaceable>().PlayerBuilt && health.IsAlive && GetComponent<PlayerDeath>()?.CanAct!=false && GetComponent<PlayerInputCommands>().Session.IsRunning && !GetComponent<PlayerInputCommands>().IsScouting && target.GetComponent<Health>().IsAlive && Vector2.Distance(transform.position,target.transform.position)<=bag.Settings.StationInteractionRange && ClassActionRules.Clear(health,target.GetComponent<Health>());
        public void Cancel(){if(Target==null)return;Target=null;Until=0;Changed?.Invoke();}
        private void Update()
        {
            if(!Active || !SimulationAuthority.Allows(this))return;
            if(role.Now>lease || !Valid(Target) || GetComponent<PlayerInputCommands>().Move.sqrMagnitude>.01f || Vector2.Distance(origin,transform.position)>.2f || Target.DamageRevision!=damageRevision || inventoryRevision!=revisionAtStart || GetComponent<PlayerCombat>().IsAttacking || GetComponent<PlayerRanged>().IsBusy){Cancel();return;}
            if(role.Now<Until)return;var target=Target;var kind=Kind;completed=true;Cancel();if(kind==BuildingServiceKind.Repair)target.Repair(bag);else target.Dismantle(bag);
        }
    }
}
