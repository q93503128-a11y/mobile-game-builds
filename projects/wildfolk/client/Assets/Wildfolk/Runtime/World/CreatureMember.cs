using System;
using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    public enum CreatureAlert { Flee,Fight }
    public sealed class CreatureMember : MonoBehaviour
    {
        public CreatureDefinition Definition;
        public string GroupId;
        public Vector2 Anchor;
        public CreatureTrait AdditionalTraits;
        public bool Has(CreatureTrait trait)=>Definition.Has(trait) || (AdditionalTraits&trait)!=0;
        public event Action<Vector2,CreatureAlert> Alerted;
        private static readonly List<CreatureMember> members=new List<CreatureMember>();
        public static IReadOnlyList<CreatureMember> Active=>members;
        public uint SpawnGeneration {get;private set;}
        private void OnEnable(){SpawnGeneration++;members.Add(this);}
        private void OnDisable()=>members.Remove(this);
        public int Broadcast(Vector2 origin,CreatureAlert alert)
        {
            if(!SimulationAuthority.Allows(this))return 0;
            var notified=0;
            foreach(var member in members)
            {
                if(member==this || member.Definition==null || Definition==null || member.Definition.Faction!=Definition.Faction)continue;
                var samePack=!string.IsNullOrEmpty(GroupId) && member.GroupId==GroupId;
                var eligible=alert==CreatureAlert.Flee ? samePack && member.Definition.Has(CreatureTrait.FleeTogether) : member.Definition.Has(CreatureTrait.AllyAssist) || samePack && member.Definition.Has(CreatureTrait.PackAggro);
                if(!eligible || Vector2.Distance(member.transform.position,transform.position)>Definition.AlertRadius || Vector2.Distance(member.transform.position,origin)>Definition.AlertMaxDistance)continue;
                member.Alerted?.Invoke(origin,alert);notified++;
            }
            return notified;
        }
        public Vector2 Cohesion()
        {
            if(!SimulationAuthority.Allows(this) || Definition==null || string.IsNullOrEmpty(GroupId))return Vector2.zero;
            var center=Vector2.zero;var count=0;
            foreach(var member in members)if(member!=this && member.GroupId==GroupId && Vector2.Distance(member.transform.position,transform.position)<Definition.RoamRadius){center+=(Vector2)member.transform.position;count++;}
            if(count==0)return Vector2.zero;var delta=center/count-(Vector2)transform.position;
            return delta.magnitude>Definition.PreferredSpacing ? delta.normalized*.35f : Vector2.zero;
        }
    }
}
