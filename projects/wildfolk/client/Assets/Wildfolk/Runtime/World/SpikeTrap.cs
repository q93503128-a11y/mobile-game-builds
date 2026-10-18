using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    public sealed class SpikeTrap:MonoBehaviour
    {
        public float Interval=1.2f;public int Damage=16;
        public int Triggers {get;private set;}
        private float readyAt;private readonly Collider2D[] contacts=new Collider2D[16];private readonly HashSet<Health> struck=new HashSet<Health>();
        private Vector3 rest;private WorldPlaceable placeable;
        private void Awake(){placeable=GetComponent<WorldPlaceable>();rest=placeable.Visual.transform.localScale;}
        private void FixedUpdate()
        {
            if(!SimulationAuthority.Allows(this) || !placeable.Health.IsAlive || Time.time<readyAt)return;
            var count=Physics2D.OverlapBox(transform.position,placeable.Definition.Footprint,transform.eulerAngles.z,new ContactFilter2D{useTriggers=true},contacts);struck.Clear();
            for(var i=0;i<count;i++){var target=contacts[i].GetComponentInParent<Health>();if(target==null || target.GetComponent<ResourceNode>()!=null || !CombatAffiliation.CanDamage(placeable.Health,target) || !struck.Add(target))continue;if(target.ApplyDamage(Damage,false)>0){readyAt=Time.time+Interval;Triggers++;GetComponent<NetworkTrap>()?.Trigger();}}
            if(struck.Count>0)placeable.Visual.transform.localScale=rest*1.12f;
        }
        private void Update(){placeable.Visual.transform.localScale=Vector3.MoveTowards(placeable.Visual.transform.localScale,rest,Time.deltaTime*2);}
    }
}
