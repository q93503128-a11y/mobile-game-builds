using UnityEngine;
namespace Wildfolk
{
    // A draw/recovery action; trajectory and hit authority reuse the existing projectile.
    public sealed class PlayerRanged : MonoBehaviour
    {
        public StoneProjectile Projectile;
        private PlayerEquipment equipment;private PlayerInventory bag;private PlayerCombat combat;
        private ItemDefinition weapon;private Vector2 direction;private float started,readyAt;private bool fired,busy;
        public bool IsBusy=>busy;
        public int Shots {get;private set;}
        private void Awake(){equipment=GetComponent<PlayerEquipment>();bag=GetComponent<PlayerInventory>();combat=GetComponent<PlayerCombat>();}
        private int AmmoSlot(AmmoKind kind)
        {for(var i=0;i<bag.Model.Capacity;i++){var s=bag.Model.GetSlot(i);if(!s.IsEmpty && s.Item.Ammo==kind)return i;}return -1;}
        public bool Begin()
        {
            if(!SimulationAuthority.Allows(this) || busy || combat.IsAttacking || Time.time<readyAt)return false;
            weapon=equipment.SelectedItem;if(weapon==null || weapon.Ranged==null || AmmoSlot(weapon.Ranged.Ammunition)<0){PlayerFeedback.ShowKey("feedback.no_ammo");return false;}
            if(!GetComponent<Health>().IsAlive || !GetComponent<PlayerInputCommands>().Session.IsRunning)return false;
            GetComponent<PlayerDeath>()?.OffensiveAction();
            direction=GetComponent<PlayerAvatar>().Facing.Direction;started=Time.time;fired=false;busy=true;
            readyAt=started+weapon.Ranged.DrawSeconds+weapon.Ranged.RecoverySeconds;
            GetComponent<NetworkActivities>()?.Publish(ActivityKind.Ranged,direction,weapon.Ranged.DrawSeconds+weapon.Ranged.RecoverySeconds);
            return true;
        }
        private void Update()
        {
            if(!busy)return;
            if(!SimulationAuthority.Allows(this) || equipment.SelectedItem!=weapon || !GetComponent<PlayerInputCommands>().Session.IsRunning || !GetComponent<Health>().IsAlive){Cancel();return;}
            var elapsed=Time.time-started;Present(elapsed,weapon.Ranged);
            if(!fired && elapsed>=weapon.Ranged.DrawSeconds)
            {
                fired=true;var slot=AmmoSlot(weapon.Ranged.Ammunition);
                if(slot<0){Cancel();return;}var ammo=bag.Model.GetSlot(slot).Item;StoneProjectile shot=null;
                var accepted=bag.Model.TryDropSlot(slot,ammo,1,()=>{shot=PrefabPool.Spawn(Projectile,(Vector2)transform.position+direction*.8f,Quaternion.identity);shot.Launch(GetComponent<Health>(),direction,weapon.Ranged.Speed,weapon.Ranged.Range,weapon.Ranged.Damage+ammo.AmmoDamage,bag,ammo);var net=shot.GetComponent<NetworkProjectile>();return net==null || NetworkCreatureWorld.Active.SpawnPendingNow(net.NetworkObject);});
                if(!accepted){if(shot!=null)PrefabPool.Release(shot.gameObject);Cancel();return;}Shots++;GetComponent<Health>().MarkCombat();
            }
            if(Time.time>=readyAt)Cancel();
        }
        public void Present(float elapsed,RangedWeaponDefinition data)
        {var facing=GetComponent<PlayerAvatar>().Facing;var draw=data.Crossbow ? elapsed<data.DrawSeconds ? 0 : Mathf.Sin(Mathf.Clamp01((elapsed-data.DrawSeconds)/data.RecoverySeconds)*Mathf.PI) : Mathf.Clamp01(elapsed/data.DrawSeconds);facing.SetHandPose(1,new Vector2(-.15f,.1f),data.Crossbow ? draw*14 : -draw*8);facing.SetHandPose(0,new Vector2(facing.HandSeparation*1.7f,-draw*.4f));}
        public void Cancel(){busy=false;GetComponent<PlayerAvatar>().Facing.ResetPose();}
        private void OnDisable()=>Cancel();
    }
}
