using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
namespace Wildfolk
{
    public struct GroundItemState : INetworkSerializable,IEquatable<GroundItemState>
    {
        public FixedString64Bytes Item;
        public int Count,Charges;public LiquidState Quality;
        public Vector2 Origin,Destination;
        public double Started;
        public ulong Dropper;
        public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter
        {s.SerializeValue(ref Item);s.SerializeValue(ref Count);s.SerializeValue(ref Charges);s.SerializeValue(ref Quality);s.SerializeValue(ref Origin);s.SerializeValue(ref Destination);s.SerializeValue(ref Started);s.SerializeValue(ref Dropper);}
        public bool Equals(GroundItemState o)=>Item.Equals(o.Item)&&Count==o.Count&&Charges==o.Charges&&Quality==o.Quality&&Origin==o.Origin&&Destination==o.Destination&&Started==o.Started&&Dropper==o.Dropper;
    }
    public sealed class NetworkGroundItem : NetworkBehaviour
    {
        public readonly NetworkVariable<GroundItemState> State=new NetworkVariable<GroundItemState>();
        private GroundItemState initial;
        private WorldPickup view;
        private GameplaySettings settings;
        private bool claimed;
        private readonly HashSet<NetworkInventory> visitors=new HashSet<NetworkInventory>();
        private void Awake(){view=GetComponent<WorldPickup>();view.enabled=false;}
        public static NetworkGroundItem Drop(ItemDefinition item,int count,Vector2 position,NetworkPlayer dropper,bool playerDropped)
        {
            var world=NetworkCreatureWorld.Active;if(world==null || !world.IsAuthority)throw new InvalidOperationException("Host required for ground drops.");
            var pickup=NetworkSpawnFactory.Spawn(world.World.Pickup,position,Quaternion.identity);
            pickup.Initialize(item,count,dropper==null ? null : dropper.GetComponent<PlayerInventory>(),world.World.Settings,playerDropped);return pickup.GetComponent<NetworkGroundItem>();
        }
        public void Configure(ItemDefinition item,int count,PlayerInventory dropper,GameplaySettings config,bool playerDropped,int charges=0,LiquidState quality=LiquidState.Empty)
        {
            if(item==null || count<1)throw new ArgumentException("Invalid ground stack.");
            settings=config;var origin=(Vector2)transform.position;var direction=UnityEngine.Random.insideUnitCircle.normalized;
            var actor=dropper==null ? null : dropper.GetComponent<NetworkObject>();
            initial=new GroundItemState{Item=item.Id,Count=count,Charges=charges,Quality=quality,Origin=origin,Destination=playerDropped ? origin : origin+direction*config.DropScatter,Started=Unity.Netcode.NetworkManager.Singleton.ServerTime.Time,Dropper=playerDropped && actor!=null ? actor.NetworkObjectId : ulong.MaxValue};
            view.ApplyNetwork(item,count,charges,quality);
        }
        public override void OnNetworkSpawn()
        {
            // The view component is disabled to suppress its SP Update, but the network item is live.
            if(!WorldPickup.Active.Contains(view))WorldPickup.Active.Add(view);
            settings=NetworkCreatureWorld.Active.World.Settings;if(IsServer)State.Value=initial;
            State.OnValueChanged+=Changed;Apply(State.Value);
        }
        private void Apply(GroundItemState state){if(state.Count>0)view.ApplyNetwork(NetworkCreatureWorld.Active.Catalog.Item(state.Item.ToString()),state.Count,state.Charges,state.Quality);}
        private void Changed(GroundItemState old,GroundItemState next){Apply(next);foreach(var visitor in visitors)visitor.InvalidatePickup(this);}
        private void Update()
        {if(IsSpawned)view.PresentNetwork(State.Value.Origin,State.Value.Destination,(float)(NetworkManager.ServerTime.Time-State.Value.Started),settings);}
        public bool Ready(NetworkPlayer player)
        {return IsSpawned && !claimed && State.Value.Count>0 && NetworkManager.ServerTime.Time-State.Value.Started>=settings.DropPickupDelay && Vector2.Distance(player.transform.position,transform.position)<=settings.PickupRadius;}
        public bool TryPickup(NetworkInventory recipient)
        {
            if(!IsServer || !recipient.CanOperate || !Ready(recipient.Player))return false;
            claimed=true;var stack=State.Value;var item=NetworkCreatureWorld.Active.Catalog.Item(stack.Item.ToString());
            if(!recipient.Inventory.Model.TryAddAmounts(new[]{new ItemAmount{Item=item,Count=stack.Count,Charges=stack.Charges,Quality=stack.Quality}})){claimed=false;return false;}
            stack.Count=0;State.Value=stack;NetworkObject.Despawn();return true;
        }
        private void OnTriggerEnter2D(Collider2D other)
        {var inventory=other.GetComponentInParent<NetworkInventory>();if(inventory==null || !inventory.IsOwner)return;visitors.Add(inventory);inventory.Track(this);}
        private void OnTriggerExit2D(Collider2D other)
        {var inventory=other.GetComponentInParent<NetworkInventory>();if(inventory==null)return;visitors.Remove(inventory);inventory.Untrack(this);}
        public override void OnNetworkDespawn()
        {WorldPickup.Active.Remove(view);State.OnValueChanged-=Changed;foreach(var visitor in visitors)visitor.Untrack(this);visitors.Clear();}
    }
}
