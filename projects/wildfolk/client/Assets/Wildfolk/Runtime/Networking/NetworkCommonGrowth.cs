using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
namespace Wildfolk
{
    [Serializable]public sealed class NetworkGrowthSnapshot {public GrowthSnapshot Growth;public long XP,Gold;public uint ActionSequence;public ConfirmedPlayerAction Action;}
    public sealed class NetworkCommonGrowth:NetworkBehaviour
    {
        public readonly NetworkVariable<FixedString4096Bytes> Growth=new NetworkVariable<FixedString4096Bytes>(default,NetworkVariableReadPermission.Owner);
        public bool Applying{get;private set;}
        private PlayerProgression progression;private ulong sequence,lastRequest;private uint actionSequence,lastAction;private ConfirmedPlayerAction action;private bool receiptReady;
        private void Awake()=>progression=GetComponent<PlayerProgression>();
        public override void OnNetworkSpawn(){receiptReady=false;lastAction=0;Growth.OnValueChanged+=Changed;if(IsServer){progression.Changed+=Publish;Publish();}else if(IsOwner)Apply(Growth.Value);}
        public override void OnNetworkDespawn(){Growth.OnValueChanged-=Changed;progression.Changed-=Publish;}
        public void ConfirmAction(ConfirmedPlayerAction committed){if(!IsSpawned||!IsServer||committed==ConfirmedPlayerAction.None)return;action=committed;actionSequence++;Publish();}
        private void Publish(){if(IsServer && IsSpawned)Growth.Value=new FixedString4096Bytes(JsonUtility.ToJson(new NetworkGrowthSnapshot{Growth=progression.Capture(),XP=progression.Experience,Gold=progression.Gold,ActionSequence=actionSequence,Action=action}));}
        private void Changed(FixedString4096Bytes old,FixedString4096Bytes next){if(!IsServer && IsOwner)Apply(next);}
        private void Apply(FixedString4096Bytes value)
        {if(value.Length==0)return;var state=JsonUtility.FromJson<NetworkGrowthSnapshot>(value.ToString());var gained=state.Gold>progression.Gold;progression.Restore(state.Growth,state.XP,state.Gold);GetComponent<PlayerLocomotion>().Stamina.SetMaximum(GetComponent<PlayerLocomotion>().Definition.MaximumStamina+GetComponent<CharacterStats>().StaminaBonus);if(receiptReady&&gained)AudioRuntime.Play(Sound.Gold);if(receiptReady&&state.ActionSequence!=lastAction)PlayerActionAudio.Present(state.Action,transform.position);lastAction=state.ActionSequence;receiptReady=true;}
        public void EmitEconomy(string metric,int amount,string source){if(IsServer&&IsSpawned)EconomyProgressRpc(metric,amount,source);}
        [Rpc(SendTo.Owner,InvokePermission=RpcInvokePermission.Server)]private void EconomyProgressRpc(string metric,int amount,string source){if(IsOwner)EconomyService.Get().Record(metric,amount,source);}
        public bool Invest(int index)
        {if(!IsSpawned || !IsOwner)return false;if(IsServer)return Execute(++sequence,index);InvestRpc(++sequence,index);return true;}
        [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)]private void InvestRpc(ulong request,int index,RpcParams rpc=default){if(rpc.Receive.SenderClientId==OwnerClientId)Execute(request,index);}
        public bool Execute(ulong request,int index)
        {if(!IsSpawned || !IsServer || request==0 || request<=lastRequest || !GetComponent<NetworkInventory>().CanOperate)return false;lastRequest=request;try{Applying=true;return progression.Invest(index);}finally{Applying=false;}}
    }
}
