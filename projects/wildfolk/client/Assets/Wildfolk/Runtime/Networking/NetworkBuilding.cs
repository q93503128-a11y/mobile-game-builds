using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
namespace Wildfolk
{
    public struct BuildingState : INetworkSerializable,IEquatable<BuildingState>
    {
        public FixedString64Bytes Id,Item,Creator;
        public int HP;
        public bool Open;
        public float From;
        public double Started;
        public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter
        {s.SerializeValue(ref Id);s.SerializeValue(ref Item);s.SerializeValue(ref Creator);s.SerializeValue(ref HP);s.SerializeValue(ref Open);s.SerializeValue(ref From);s.SerializeValue(ref Started);}
        public bool Equals(BuildingState o)=>Id.Equals(o.Id)&&Item.Equals(o.Item)&&Creator.Equals(o.Creator)&&HP==o.HP&&Open==o.Open&&From==o.From&&Started==o.Started;
    }
    public sealed class NetworkBuilding : NetworkBehaviour
    {
        public readonly NetworkVariable<BuildingState> State=new NetworkVariable<BuildingState>();
        public readonly NetworkVariable<uint> Revision=new NetworkVariable<uint>();
        public WorldPlaceable Placeable {get;private set;}
        public WorldDoor Door {get;private set;}
        private float from;private double started;
        private void Awake(){Placeable=GetComponent<WorldPlaceable>();Door=GetComponent<WorldDoor>();Placeable.Health.IsReplica=true;}
        public override void OnNetworkSpawn()
        {
            Placeable.Health.IsReplica=!IsServer;State.OnValueChanged+=Changed;
            if(IsServer){Placeable.Health.Changed+=Publish;if(Door!=null)Door.Changed+=DoorChanged;from=Door!=null ? Door.OpenAmount : 0;Publish();}else Apply(State.Value);
            Physics2D.SyncTransforms();WallVisual.RefreshNearby(transform.position);
        }
        private void DoorChanged(){from=Door.OpenAmount;started=NetworkManager.ServerTime.Time;Publish();}
        private void Publish()
        {
            if(!IsSpawned || !IsServer)return;
            var next=new BuildingState{Id=Placeable.SaveId,Item=Placeable.Definition.Item.Id,Creator=Placeable.CreatorId??"",HP=Placeable.Health.Current,Open=Door!=null && Door.IsOpen,From=from,Started=started};
            if(State.Value.Equals(next))return;State.Value=next;Revision.Value++;
        }
        private void Changed(BuildingState old,BuildingState value){if(!IsServer)Apply(value);}
        private void Apply(BuildingState value)
        {
            var definition=NetworkCreatureWorld.Active.Catalog.Item(value.Item.ToString()).Placement;
            Placeable.SaveId=value.Id.ToString();Placeable.CreatorId=value.Creator.ToString();Placeable.Definition=definition;Placeable.Visual.sprite=definition.TierVisual!=null ? definition.TierVisual : definition.Prefab.Visual.sprite;Placeable.Visual.transform.localScale=definition.Prefab.Visual.transform.localScale;Placeable.Health.Definition=definition.Vitality;Placeable.Health.ApplyReplica(value.HP);
            var station=GetComponent<CraftingStation>();if(station!=null){station.Definition=definition;station.NotifyChanged();}
            if(Door!=null)PresentDoor();
        }
        public void PresentDoor()
        {
            var state=State.Value;var amount=Mathf.MoveTowards(state.From,state.Open ? 1 : 0,(float)Math.Max(0,NetworkManager.ServerTime.Time-state.Started)/Door.AnimationSeconds);Door.Present(state.Open,amount);
        }
        public bool SetDoor(NetworkPlayer player,bool desired,uint expected)
        {
            if(!IsSpawned || !IsServer || Door==null || expected!=Revision.Value || desired==Door.IsOpen || !Placeable.Health.IsAlive)return false;
            if(Vector2.Distance(player.transform.position,transform.position)>player.GetComponent<PlayerInventory>().Settings.StationInteractionRange)return false;
            return Door.Toggle();
        }
        public override void OnNetworkDespawn(){Placeable.Health.Changed-=Publish;if(Door!=null)Door.Changed-=DoorChanged;State.OnValueChanged-=Changed;WallVisual.RefreshNearby(transform.position);}
    }
}
