using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Wildfolk
{
    public enum BuildingServiceKind:byte { Repair,Dismantle }
    // Buffers live only for this generated world session; sleep/wake reuses stable SaveId.
    public sealed class StructureMaintenance:MonoBehaviour
    {
        private static readonly Dictionary<string,int> buffers=new Dictionary<string,int>();
        public uint DamageRevision {get;private set;}
        public int Buffer {get;private set;}
        public event Action Changed;
        private WorldPlaceable place;
        private void Awake()=>place=GetComponent<WorldPlaceable>();
        private void OnEnable(){DamageRevision=0;Buffer=0;place.Health.Damaged+=Damaged;place.Health.Died+=Destroyed;}
        private void OnDisable(){place.Health.Damaged-=Damaged;place.Health.Died-=Destroyed;if(place.Health.IsAlive && Buffer>0)buffers[place.SaveId]=Buffer;}
        private void Damaged(){DamageRevision++;Changed?.Invoke();}
        private void Destroyed(){buffers.Remove(place.SaveId);Buffer=0;}
        public static void NewWorld()=>buffers.Clear();
        public void WakeBuffer(){Buffer=buffers.TryGetValue(place.SaveId,out var n)?n:0;Changed?.Invoke();}
        public int Absorb(int amount)
        {if(Buffer<=0)return amount;var used=Math.Min(Buffer,amount);Buffer-=used;if(Buffer==0)buffers.Remove(place.SaveId);else buffers[place.SaveId]=Buffer;Changed?.Invoke();return amount-used;}
        public void ReplicaBuffer(int value){Buffer=value;Changed?.Invoke();}
        public bool CanRepair(PlayerClass role)=>place.PlayerBuilt && place.Health.IsAlive && Time.time-place.Health.LastCombatTime>=(role?.RepairLock??3) && (place.Health.Current<place.Health.Maximum || role!=null && Buffer<Mathf.FloorToInt(role.BufferCap(place)));
        public ItemAmount[] RepairCosts()
        {
            var missing=1-place.Health.Ratio;
            return place.Definition.RepairMaterials.Select(c=>new ItemAmount{Item=c.Item,Count=Mathf.Max(1,Mathf.CeilToInt(c.Count*missing))}).ToArray();
        }
        public bool Repair(PlayerInventory player)
        {
            var role=player.GetComponent<PlayerClass>();if(!SimulationAuthority.Allows(this) || !CanRepair(role))return false;
            if(place.Health.Current>=place.Health.Maximum)
            {if(role==null || !role.FreeRepair)return false;Buffer=Mathf.FloorToInt(role.BufferCap(place));buffers[place.SaveId]=Buffer;Changed?.Invoke();return true;}
            if(role?.FreeRepair==true)place.Health.Heal(place.Health.Maximum-place.Health.Current);
            else if(place.Definition.RepairMaterials.Length==0 || !player.Model.TryConsumeCosts(RepairCosts(),()=>{place.Health.Heal(place.Health.Maximum-place.Health.Current);return true;}))return false;
            Changed?.Invoke();return true;
        }
        public bool HasContents()
        {
            var chest=GetComponent<StorageChest>();if(chest!=null)for(var i=0;i<chest.Model.Capacity;i++)if(!chest.Model.GetSlot(i).IsEmpty)return true;
            var process=GetComponent<StationProcessing>();if(process!=null && process.IsBusy)return true;
            var turret=GetComponent<WorldTurret>();return turret!=null && (turret.AmmoCount()>0 || turret.Loaded || turret.Modules.Any(m=>m!=null));
        }
        public ItemAmount[] Refund()
        {
            var ratio=place.Health.Ratio>=.9f?1:place.Health.Ratio;
            if(place.Definition.RecoverPortable)return ratio>=1?new[]{new ItemAmount{Item=place.Definition.Item,Count=1}}:Array.Empty<ItemAmount>();
            return place.Definition.ConstructionMaterials.Select(c=>new ItemAmount{Item=c.Item,Count=Mathf.FloorToInt(c.Count*ratio)}).Where(c=>c.Count>0).ToArray();
        }
        public bool Dismantle(PlayerInventory player)
        {
            if(!SimulationAuthority.Allows(this) || !place.PlayerBuilt || !place.Health.IsAlive || HasContents())return false;
            var result=Refund();if(!player.Model.TryAddAmounts(result))return false;
            buffers.Remove(place.SaveId);Buffer=0;PrefabPool.Release(gameObject);return true;
        }
    }
}
