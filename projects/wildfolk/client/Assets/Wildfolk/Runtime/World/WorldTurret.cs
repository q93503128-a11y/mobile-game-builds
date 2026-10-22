using System;
using UnityEngine;
namespace Wildfolk
{
    public enum TurretOperation { Aim,Fire,Reload,Deposit,Withdraw,Install,Remove }
    public sealed class WorldTurret : MonoBehaviour,IPlayerInteractable,IPoolReset
    {
        public TurretDefinition Definition;
        public Transform Head;
        public InventoryModel Ammo {get;private set;}=new InventoryModel(3);
        public readonly ItemDefinition[] Modules=new ItemDefinition[5];
        public float Heading {get;private set;}
        public float DesiredHeading {get;private set;}
        public float ReloadRemaining=>Mathf.Max(0,readyAt-Time.time);
        public bool Loaded {get;private set;}
        public int Shots {get;private set;}
        public int AmmoSlots=>Mathf.Clamp(1+(Modules[2]?.TurretModule.ExtraAmmoSlots??0),1,3);
        public Health Target {get;private set;}
        public event Action Changed;
        public uint Revision {get;private set;}
        private float readyAt,lastManual;private double overclockUntil;private float reloadBonus,rotationBonus;
        public double OverclockUntil=>overclockUntil;
        internal float OverclockReload=>reloadBonus;
        internal float OverclockRotation=>rotationBonus;
        internal void ApplyOverclock(float seconds,float reload,float rotation){overclockUntil=seconds>0?Time.timeAsDouble+seconds:0;reloadBonus=reload;rotationBonus=rotation;}
        public float OverclockRemaining=>Mathf.Max(0,(float)(overclockUntil-Time.timeAsDouble));
        public bool Overclock(float duration,float reload,float rotation)
        {if(!Authoritative || !place.Health.IsAlive || duration<=0 || Time.timeAsDouble<overclockUntil)return false;overclockUntil=Time.timeAsDouble+duration;reloadBonus=reload;rotationBonus=rotation;Notify();return true;}
        public float ReloadMultiplier=>Time.timeAsDouble<overclockUntil?1+reloadBonus:1;
        public float RotationMultiplier=>Time.timeAsDouble<overclockUntil?1+rotationBonus:1;
        private WorldPlaceable place;private WorldGenerator world;private PlayerInventory credit;
        internal WorldGenerator SimulationWorld=>world;
        private readonly Collider2D[] nearby=new Collider2D[64];private readonly RaycastHit2D[] lineHits=new RaycastHit2D[32];
        public Transform Location=>transform;
        public Sprite Icon=>place.Definition.Item.Icon;
        private bool Authoritative=>SimulationAuthority.Allows(this);
        private void Awake(){place=GetComponent<WorldPlaceable>();Ammo.Changed+=Notify;}
        private void OnEnable(){TurretScheduler.Register(this);place.Health.Died+=ReleaseCargo;}
        private void OnDisable(){TurretScheduler.Unregister(this);place.Health.Died-=ReleaseCargo;Target=null;}
        private void Notify(){if(AmmoCount()==0)Loaded=false;Revision++;Changed?.Invoke();}
        public void ResetForSpawn(){Ammo=new InventoryModel(3);Ammo.Changed+=Notify;Array.Clear(Modules,0,5);Heading=DesiredHeading=transform.eulerAngles.z;Loaded=false;readyAt=0;overclockUntil=0;reloadBonus=rotationBonus=0;credit=null;Target=null;place.Health.SetMaximumBonus(0);}
        public bool CanInteract(PlayerInteraction actor)=>place.Health.IsAlive && actor.Session.IsRunning;
        public bool Use(PlayerInteraction actor)=>TierThreeHud.OpenTurret(this,actor.GetComponent<PlayerInventory>());
        public bool InRange(PlayerInventory player)=>player!=null && player.GetComponent<Health>().IsAlive && Vector2.Distance(player.transform.position,transform.position)<=player.Settings.StationInteractionRange;
        public bool Execute(PlayerInventory player,TurretOperation operation,int index=0,float angle=0)
        {
            if(!Authoritative || !place.Health.IsAlive || !InRange(player) || !float.IsFinite(angle))return false;
            credit=player;
            if(operation==TurretOperation.Aim){DesiredHeading=Mathf.Repeat(angle,360);lastManual=Time.time+1;return true;}
            if(operation==TurretOperation.Fire)return Fire();
            if(operation==TurretOperation.Reload){if(Loaded || Time.time<readyAt || AmmoCount()==0)return false;Loaded=true;Notify();return true;}
            if(operation==TurretOperation.Deposit)
            {
                if(index<0 || index>=player.Model.Capacity)return false;var s=player.Model.GetSlot(index);if(s.IsEmpty || s.Item!=Definition.Ammunition)return false;
                for(var i=0;i<AmmoSlots;i++){var dest=Ammo.GetSlot(i);if((dest.IsEmpty || dest.Count<dest.Item.MaxStack) && InventoryModel.Transfer(player.Model,index,Ammo,i))return true;}return false;
            }
            if(operation==TurretOperation.Withdraw)
            {if(index<0 || index>=AmmoSlots || Ammo.GetSlot(index).IsEmpty)return false;for(var i=0;i<player.Model.Capacity;i++){var dest=player.Model.GetSlot(i);if((dest.IsEmpty || dest.Item==Definition.Ammunition && dest.Count<dest.Item.MaxStack) && InventoryModel.Transfer(Ammo,index,player.Model,i))return true;}return false;}
            if(operation==TurretOperation.Install)
            {
                if(index<0 || index>=player.Model.Capacity)return false;var s=player.Model.GetSlot(index);if(s.IsEmpty || s.Item.TurretModule==null)return false;var slot=(int)s.Item.TurretModule.Slot;var old=Modules[slot];if(old==s.Item)return false;
                if(slot==2 && 1+s.Item.TurretModule.ExtraAmmoSlots<AmmoSlots)for(var i=1+s.Item.TurretModule.ExtraAmmoSlots;i<3;i++)if(!Ammo.GetSlot(i).IsEmpty)return false;
                var costs=new[]{new ItemAmount{Item=s.Item,Count=1}};var accepted=old==null ? player.Model.TryConsumeCosts(costs) : player.Model.TryExchange(costs,old,1)==ExchangeResult.Success;
                if(!accepted)return false;Modules[slot]=s.Item;ApplyFrame();Notify();return true;
            }
            if(operation==TurretOperation.Remove)
            {
                if(index<0 || index>=5 || Modules[index]==null)return false;if(index==2)for(var i=1;i<3;i++)if(!Ammo.GetSlot(i).IsEmpty)return false;
                if(player.Model.Add(Modules[index],1)!=1)return false;Modules[index]=null;ApplyFrame();Notify();return true;
            }
            return false;
        }
        private void ApplyFrame()=>place.Health.SetMaximumBonus(Modules[4]==null ? 0 : Mathf.RoundToInt(place.Health.Definition.MaximumHealth*(Modules[4].TurretModule.Multiplier-1)));
        public int AmmoCount(){var n=0;for(var i=0;i<AmmoSlots;i++)n+=Ammo.GetSlot(i).Count;return n;}
        public void Tick(float seconds)
        {
            if(!place.Health.IsAlive)return;
            if(Authoritative && overclockUntil>0 && Time.timeAsDouble>=overclockUntil){overclockUntil=0;Notify();}
            if(!Authoritative){var net=GetComponent<NetworkTurret>();if(net!=null && net.IsSpawned)Heading=Mathf.MoveTowardsAngle(Heading,net.Heading.Value,Definition.RotationSpeed*seconds*2);Present();return;}
            if(world==null)world=FindFirstObjectByType<WorldGenerator>();
            if(world!=null && world.Map.Streamed && !world.Chunks.Wants(transform.position)){Target=null;return;}
            if(Modules[1]!=null && Time.time>=lastManual){Target=Acquire();if(Target!=null){var d=(Vector2)(Target.transform.position-transform.position);DesiredHeading=Mathf.Atan2(d.y,d.x)*Mathf.Rad2Deg-90;}}
            Heading=Mathf.MoveTowardsAngle(Heading,DesiredHeading,Definition.RotationSpeed*(Modules[3]?.TurretModule.Multiplier??1)*RotationMultiplier*seconds);Present();GetComponent<NetworkTurret>()?.PublishHeading();
            if(Modules[0]!=null && !Loaded && Time.time>=readyAt && AmmoCount()>0){Loaded=true;Notify();}
            if(Modules[0]!=null && Modules[1]!=null && Target!=null && Mathf.Abs(Mathf.DeltaAngle(Heading,DesiredHeading))<5 && HasLineOfSight(Target))Fire();
        }
        public bool HasLineOfSight(Health target)
        {
            if(target==null)return false;var origin=(Vector2)transform.position;var delta=(Vector2)target.transform.position-origin;
            var count=Physics2D.Raycast(origin,delta.normalized,new ContactFilter2D{useTriggers=true},lineHits,delta.magnitude);
            for(var i=0;i<count;i++){var c=lineHits[i].collider;var h=c.GetComponentInParent<Health>();if(h==place.Health || h==target || c.isTrigger)continue;return false;}return true;
        }
        private Health Acquire()
        {
            var count=Physics2D.OverlapCircle(transform.position,Definition.Range,new ContactFilter2D{useTriggers=true},nearby);Health best=null;var distance=float.MaxValue;
            for(var i=0;i<count;i++){var h=nearby[i].GetComponentInParent<Health>();if(h==null || !h.IsAlive || !CombatAffiliation.CanDamage(place.Health,h))continue;var faction=h.GetComponent<CombatAffiliation>();if(faction==null || faction.Relation!=EntityRelation.Hostile && faction.Relation!=EntityRelation.NeutralGoblin)continue;if(world!=null && world.Map.Streamed && !world.Chunks.Wants(h.transform.position))continue;var d=(h.transform.position-transform.position).sqrMagnitude;if(d>=distance || !HasLineOfSight(h))continue;best=h;distance=d;}return best;
        }
        public bool Fire()
        {
            if(!Authoritative || !Loaded || Time.time<readyAt || !place.Health.IsAlive || AmmoCount()==0)return false;
            if(world==null)world=FindFirstObjectByType<WorldGenerator>();if(world!=null && world.Map.Streamed && !world.Chunks.Wants(transform.position))return false;
            var slot=-1;for(var i=0;i<AmmoSlots;i++)if(!Ammo.GetSlot(i).IsEmpty){slot=i;break;}if(slot<0)return false;
            StoneProjectile shot=null;var direction=(Vector2)(Quaternion.Euler(0,0,Heading)*Vector3.up);
            var accepted=Ammo.TryDropSlot(slot,Definition.Ammunition,1,()=>{shot=PrefabPool.Spawn(Definition.Projectile,(Vector2)transform.position+direction*1.1f,Quaternion.identity);shot.Launch(place.Health,direction,Definition.ProjectileSpeed,Definition.Range,Definition.Damage,credit,Definition.Ammunition);var net=shot.GetComponent<NetworkProjectile>();return net==null || NetworkCreatureWorld.Active.SpawnPendingNow(net.NetworkObject);});
            if(!accepted){if(shot!=null)PrefabPool.Release(shot.gameObject);return false;}Loaded=false;readyAt=Time.time+Definition.ReloadSeconds/ReloadMultiplier;Shots++;Notify();return true;
        }
        private void Present(){if(Head!=null)Head.localRotation=Quaternion.Euler(0,0,Heading-transform.eulerAngles.z);}
        public TurretSnapshot Capture(){var ids=new string[5];for(var i=0;i<5;i++)ids[i]=Modules[i]?.Id;return new TurretSnapshot{Ammo=Ammo.Capture(),Modules=ids,Heading=Heading,Loaded=Loaded,ReloadRemaining=ReloadRemaining};}
        public void Restore(TurretSnapshot state,ContentCatalog catalog)
        {if(state==null)return;TurretState.Validate(state,place.Definition,catalog);for(var i=0;i<5;i++)Modules[i]=string.IsNullOrEmpty(state.Modules[i]) ? null : catalog.Item(state.Modules[i]);ApplyFrame();Heading=DesiredHeading=state.Heading;Loaded=state.Loaded;readyAt=Time.time+state.ReloadRemaining;Ammo.Restore(state.Ammo,catalog);Present();Notify();}
        private void ReleaseCargo()
        {if(!Authoritative)return;var actor=world==null ? FindFirstObjectByType<WorldGenerator>() : world;if(actor==null)return;for(var i=0;i<3;i++){var s=Ammo.GetSlot(i);if(!s.IsEmpty)Drop(actor,s.Item,s.Count);}foreach(var item in Modules)if(item!=null)Drop(actor,item,1);}
        private void Drop(WorldGenerator actor,ItemDefinition item,int count){var drop=PrefabPool.Spawn(actor.Pickup,transform.position,Quaternion.identity);drop.Initialize(item,count,actor.Player,actor.Settings);}
    }
}
