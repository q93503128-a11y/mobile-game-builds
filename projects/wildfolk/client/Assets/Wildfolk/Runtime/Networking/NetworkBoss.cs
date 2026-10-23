using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
namespace Wildfolk
{
    public struct BossFrame:INetworkSerializable,IEquatable<BossFrame>
    {
        public bool Ready;public int HP;public byte State,Phase;public short Attack;
        public Vector2 Direction,Home;public double Started;public ulong Marked;public FixedString64Bytes Tomb;
        public void NetworkSerialize<T>(BufferSerializer<T> s)where T:IReaderWriter
        {s.SerializeValue(ref Ready);s.SerializeValue(ref HP);s.SerializeValue(ref State);s.SerializeValue(ref Phase);s.SerializeValue(ref Attack);s.SerializeValue(ref Direction);s.SerializeValue(ref Home);s.SerializeValue(ref Started);s.SerializeValue(ref Marked);s.SerializeValue(ref Tomb);}
        public bool Equals(BossFrame b)=>Ready==b.Ready && HP==b.HP && State==b.State && Phase==b.Phase && Attack==b.Attack && Direction==b.Direction && Home==b.Home && Started==b.Started && Marked==b.Marked && Tomb.Equals(b.Tomb);
    }
    public sealed class NetworkBoss:NetworkBehaviour
    {
        public readonly NetworkVariable<BossFrame> Frame=new NetworkVariable<BossFrame>();
        private BossEncounter encounter;private BossWorld world;private float next;
        // Initialization precedes NetworkObject.Spawn; both sides must use the server epoch.
        public double Clock{get{var manager=Unity.Netcode.NetworkManager.Singleton;return manager!=null && manager.IsListening?manager.ServerTime.Time:Time.timeAsDouble;}}
        private void Awake(){encounter=GetComponent<BossEncounter>();encounter.Health.IsReplica=true;}
        public override void OnNetworkSpawn()
        {
            world=NetworkCreatureWorld.Active.World.GetComponent<BossWorld>();encounter.Health.IsReplica=!IsServer;
            Frame.OnValueChanged+=Changed;if(IsServer){Publish();NetworkManager.NetworkTickSystem.Tick+=Tick;}else Apply(Frame.Value);
        }
        private void Tick(){if(Time.unscaledTime<next)return;next=Time.unscaledTime+.1f;Publish();}
        public void Publish()
        {
            if(!IsSpawned || !IsServer || encounter.Record==null)return;
            var marked=encounter.Marked==null?null:encounter.Marked.GetComponent<NetworkObject>();
            var f=new BossFrame{Ready=true,HP=encounter.Health.Current,State=(byte)encounter.State,Phase=(byte)encounter.Phase,Attack=(short)encounter.AttackIndex,Direction=encounter.Direction,Started=encounter.Started,Home=encounter.Home,Tomb=encounter.Record.Tomb,Marked=marked==null?ulong.MaxValue:marked.NetworkObjectId};
            if(!Frame.Value.Equals(f))Frame.Value=f;
        }
        private void Update(){if(IsSpawned && !IsServer && encounter.World!=null && encounter.Marked==null && Frame.Value.Marked!=ulong.MaxValue)Apply(Frame.Value);}
        private void Changed(BossFrame a,BossFrame b){if(!IsServer)Apply(b);}
        private void Apply(BossFrame f)
        {
            if(!f.Ready)return;
            if(encounter.World==null){var r=new BossSnapshot{Boss=encounter.Definition.Id,Tomb=f.Tomb.ToString(),Home=f.Home,Position=transform.position,HP=f.HP,Active=true};world.Encounters.Add(encounter);encounter.Initialize(world,r,true);}
            Health target=null;if(f.Marked!=ulong.MaxValue && NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(f.Marked,out var player))target=player.GetComponent<Health>();
            encounter.Health.ApplyReplica(f.HP);encounter.ApplyReplica((BossState)f.State,f.Phase,f.Attack,f.Direction,f.Started,target);
        }
        public override void OnNetworkDespawn(){Frame.OnValueChanged-=Changed;if(NetworkManager?.NetworkTickSystem!=null)NetworkManager.NetworkTickSystem.Tick-=Tick;if(world!=null)world.Encounters.Remove(encounter);}
    }
}
