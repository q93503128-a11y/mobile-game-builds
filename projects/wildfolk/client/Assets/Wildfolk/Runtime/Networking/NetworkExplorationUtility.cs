using System;
using Unity.Netcode;
using UnityEngine;
namespace Wildfolk
{
    public enum SurveyPhase:byte { None,Channel,Found,Empty,Crowded,Cancelled }
    public struct SurveyState:INetworkSerializable,IEquatable<SurveyState>
    {
        public SurveyPhase Phase;public int Item;public byte Bearing;public double Until,Ready;
        public void NetworkSerialize<T>(BufferSerializer<T> s)where T:IReaderWriter{s.SerializeValue(ref Phase);s.SerializeValue(ref Item);s.SerializeValue(ref Bearing);s.SerializeValue(ref Until);s.SerializeValue(ref Ready);}
        public bool Equals(SurveyState o)=>Phase==o.Phase&&Item==o.Item&&Bearing==o.Bearing&&Until==o.Until&&Ready==o.Ready;
    }
    public struct ExplorationSignal:INetworkSerializable,IEquatable<ExplorationSignal>
    {
        public Vector2 Point;public double Started,Until;public float Range;
        public void NetworkSerialize<T>(BufferSerializer<T> s)where T:IReaderWriter{s.SerializeValue(ref Point);s.SerializeValue(ref Started);s.SerializeValue(ref Until);s.SerializeValue(ref Range);}
        public bool Equals(ExplorationSignal o)=>Point==o.Point&&Started==o.Started&&Until==o.Until&&Range==o.Range;
    }
    public sealed class NetworkExplorationUtility:NetworkBehaviour
    {
        public readonly NetworkVariable<SurveyState> Survey=new NetworkVariable<SurveyState>(default,NetworkVariableReadPermission.Owner);
        public readonly NetworkVariable<ExplorationSignal> Signal=new NetworkVariable<ExplorationSignal>();
        public readonly NetworkVariable<double> SignalReady=new NetworkVariable<double>(0,NetworkVariableReadPermission.Owner);
        private PlayerExplorationUtility player;
        private void Awake()=>player=GetComponent<PlayerExplorationUtility>();
        public void Publish(){if(!IsSpawned || !IsServer)return;Survey.Value=player.LocalSurvey;Signal.Value=player.LocalSignal;SignalReady.Value=player.LocalSignalReady;}
        public override void OnNetworkDespawn()=>player.ResetTransient();
    }
}
