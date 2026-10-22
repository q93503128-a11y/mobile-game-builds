using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
namespace Wildfolk
{
    public sealed class NetworkTurret : NetworkBehaviour
    {
        public readonly NetworkVariable<FixedString4096Bytes> Cargo=new NetworkVariable<FixedString4096Bytes>();
        public readonly NetworkVariable<float> Heading=new NetworkVariable<float>();
        public readonly NetworkVariable<uint> Revision=new NetworkVariable<uint>();
        public readonly NetworkVariable<Vector3> OverclockState=new NetworkVariable<Vector3>();
        private WorldTurret turret;private float nextHeading;
        private void Awake()=>turret=GetComponent<WorldTurret>();
        public override void OnNetworkSpawn(){Cargo.OnValueChanged+=Changed;OverclockState.OnValueChanged+=OverclockChanged;if(IsServer){turret.Changed+=Publish;Publish();}else {Apply(Cargo.Value);OverclockChanged(default,OverclockState.Value);}}
        private void Publish(){if(!IsSpawned || !IsServer)return;Cargo.Value=new FixedString4096Bytes(JsonUtility.ToJson(turret.Capture()));Heading.Value=turret.Heading;OverclockState.Value=new Vector3(turret.OverclockRemaining>0?(float)(NetworkManager.ServerTime.Time+turret.OverclockRemaining):0,turret.OverclockReload,turret.OverclockRotation);Revision.Value++;}
        public void PublishHeading(){if(!IsSpawned || !IsServer || Time.unscaledTime<nextHeading)return;nextHeading=Time.unscaledTime+.1f;if(Mathf.Abs(Mathf.DeltaAngle(Heading.Value,turret.Heading))>.5f)Heading.Value=turret.Heading;}
        private void OverclockChanged(Vector3 old,Vector3 value){if(!IsServer)turret.ApplyOverclock(Mathf.Max(0,value.x-(float)NetworkManager.ServerTime.Time),value.y,value.z);}
        private void Changed(FixedString4096Bytes old,FixedString4096Bytes value){if(!IsServer)Apply(value);}
        private void Apply(FixedString4096Bytes value){if(value.Length>0)turret.Restore(JsonUtility.FromJson<TurretSnapshot>(value.ToString()),NetworkCreatureWorld.Active.Catalog);var b=GetComponent<NetworkBuilding>();if(b!=null && b.IsSpawned)GetComponent<Health>().ApplyReplica(b.State.Value.HP);}
        public override void OnNetworkDespawn(){turret.Changed-=Publish;Cargo.OnValueChanged-=Changed;OverclockState.OnValueChanged-=OverclockChanged;}
    }
}
