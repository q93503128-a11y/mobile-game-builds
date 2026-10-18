using Unity.Netcode;
using UnityEngine;
namespace Wildfolk
{
    public struct WorldClockSample : INetworkSerializable,System.IEquatable<WorldClockSample>
    {
        public double Elapsed,SampledAt;public bool Advancing;
        public double Estimate(double serverTime)=>System.Math.Max(0,Elapsed+(Advancing ? System.Math.Max(0,serverTime-SampledAt) : 0));
        public void NetworkSerialize<T>(BufferSerializer<T> s)where T:IReaderWriter{s.SerializeValue(ref Elapsed);s.SerializeValue(ref SampledAt);s.SerializeValue(ref Advancing);}
        public bool Equals(WorldClockSample other)=>Elapsed==other.Elapsed && SampledAt==other.SampledAt && Advancing==other.Advancing;
    }
    // Scene NetworkObject: terrain seed arrives with NGO scene synchronization.
    public sealed class NetworkWorldState : NetworkBehaviour
    {
        public readonly NetworkVariable<int> Seed=new NetworkVariable<int>();
        public readonly NetworkVariable<int> BaseGenerationVersion=new NetworkVariable<int>(1);
        public readonly NetworkVariable<WorldClockSample> TimeSample=new NetworkVariable<WorldClockSample>();
        private float nextPublish;
        public override void OnNetworkSpawn()
        {
            Seed.OnValueChanged+=Changed;
            if(IsServer){BaseGenerationVersion.Value=NetworkSession.Instance.PreparedWorld?.BaseGenerationVersion??NetworkSession.Instance.NewWorldGenerationVersion;Seed.Value=NetworkSession.Instance.PreparedWorld?.Seed??GetComponent<NetworkCreatureWorld>().World.Definition.DefaultSeed;TimeSample.Value=new WorldClockSample{Elapsed=NetworkSession.Instance.PreparedWorld?.Elapsed??0,SampledAt=NetworkManager.ServerTime.Time,Advancing=true};}
            else GetComponent<NetworkCreatureWorld>().InitializeTerrain(Seed.Value,BaseGenerationVersion.Value);
        }
        private void Changed(int old,int value){if(!IsServer)GetComponent<NetworkCreatureWorld>().InitializeTerrain(value,BaseGenerationVersion.Value);}
        public override void OnNetworkDespawn()=>Seed.OnValueChanged-=Changed;
        private void Update()
        {
            if(!IsSpawned)return;var world=GetComponent<NetworkCreatureWorld>();var clock=world.Clock;
            if(IsServer)
            {
                if(Time.unscaledTime<nextPublish)return;nextPublish=Time.unscaledTime+.5f;
                TimeSample.Value=new WorldClockSample{Elapsed=clock.Model.Elapsed,SampledAt=NetworkManager.ServerTime.Time,Advancing=clock.enabled && clock.Session.WorldIsRunning && Time.timeScale>0};
            }
            else clock.Model.SetElapsed(TimeSample.Value.Estimate(NetworkManager.ServerTime.Time));
        }
    }
}
