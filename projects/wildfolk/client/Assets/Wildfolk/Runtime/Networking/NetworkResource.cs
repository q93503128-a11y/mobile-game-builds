using System;
using System.Collections;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
namespace Wildfolk
{
    public struct ResourceNetworkState : INetworkSerializable,IEquatable<ResourceNetworkState>
    {
        public FixedString64Bytes Id;
        public int Rule,HP;
        public ushort Hits;
        public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter
        {s.SerializeValue(ref Id);s.SerializeValue(ref Rule);s.SerializeValue(ref HP);s.SerializeValue(ref Hits);}
        public bool Equals(ResourceNetworkState other)=>Id.Equals(other.Id)&&Rule==other.Rule&&HP==other.HP&&Hits==other.Hits;
    }
    [DefaultExecutionOrder(200)]
    public sealed class NetworkResource : NetworkBehaviour
    {
        public readonly NetworkVariable<ResourceNetworkState> State=new NetworkVariable<ResourceNetworkState>();
        public bool Broken {get;private set;}
        private ResourceNode node;
        private ushort hits;
        private Collider2D[] hitboxes;
        private void Awake(){node=GetComponent<ResourceNode>();node.Health.IsReplica=true;hitboxes=GetComponents<Collider2D>();}
        public override void OnNetworkSpawn()
        {
            node.Health.IsReplica=!IsServer;
            if(IsServer){node.Health.Changed+=Publish;node.Health.Damaged+=Hit;Publish();}else Apply(State.Value,false);
            State.OnValueChanged+=Changed;
        }
        private void Hit(){hits++;Publish();}
        private void Publish()
        {
            if(!IsSpawned || !IsServer)return;
            var identity=GetComponent<ResourceIdentity>();if(identity==null)return;
            var next=new ResourceNetworkState{Id=identity.Id,Rule=identity.Rule,HP=node.Health.Current,Hits=hits};if(!State.Value.Equals(next))State.Value=next;
        }
        private void Changed(ResourceNetworkState old,ResourceNetworkState next){if(!IsServer)Apply(next,old.Hits!=next.Hits);}
        private void Apply(ResourceNetworkState state,bool hit)
        {
            node.Health.ApplyReplica(state.HP,hit);foreach(var box in hitboxes)box.enabled=state.HP>0;
            var world=NetworkCreatureWorld.Active;if(world==null || state.Rule<0 || state.Rule>=world.World.Definition.Resources.Length)return;
            var rule=world.World.Definition.Resources[state.Rule];
            var id=state.Id.ToString();if(rule.AlternateSprite!=null && id.Length>1 && int.TryParse(id.Substring(1),out var number) && number%3==0)node.Visual.sprite=rule.AlternateSprite;
        }
        public void Break()
        {
            if(!IsSpawned || !IsServer || Broken)return;Broken=true;
            NetworkGroundItem.Drop(node.Definition.DropItem,node.Definition.DropCount,transform.position,null,false);
            EconomyTelemetry.Record(node.Health.LastAttacker,"harvests");foreach(var box in hitboxes)box.enabled=false;Publish();StartCoroutine(Remove());
        }
        private IEnumerator Remove(){yield return null;if(IsSpawned && IsServer)NetworkObject.Despawn();}
        public override void OnNetworkDespawn(){node.Health.Changed-=Publish;node.Health.Damaged-=Hit;State.OnValueChanged-=Changed;}
    }
}
