using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    public sealed class ResourceRegeneration : MonoBehaviour
    {
        public WorldGenerator World;public WorldClock Clock;public WorldNavigation Navigation;
        private readonly Dictionary<string,ResourceSnapshot> changes=new Dictionary<string,ResourceSnapshot>();
        private readonly Dictionary<string,ResourceIdentity> live=new Dictionary<string,ResourceIdentity>();
        public ResourcePopulation Population {get;private set;}
        private double nextAttempt;
        public bool HasChanges=>changes.Count>0;
        public void ResetWorld(){changes.Clear();live.Clear();Population=new ResourcePopulation(World.Map);nextAttempt=0;}
        public void Register(ResourceNode node,string id,int rule)
        {var identity=node.gameObject.AddComponent<ResourceIdentity>();identity.Id=id;identity.Rule=rule;identity.Node=node;identity.Owner=this;identity.Bind();live[id]=identity;}
        public void Record(ResourceIdentity identity,bool destroyed)
        {
            if(!SimulationAuthority.Allows(this))return;
            var state=new ResourceSnapshot{Id=identity.Id,Rule=identity.Rule,Position=identity.transform.position,Scale=identity.transform.localScale.x,HP=identity.Node.Health.Current,Destroyed=destroyed};
            if(destroyed){if(!live.Remove(state.Id))return;Population.Remove(state.Rule,state.Position,Clock.Model.Elapsed,true);if(state.Id.StartsWith("p")){changes.Remove(state.Id);return;}}
            changes[state.Id]=state;
        }
        public ResourceSnapshot[] Capture()=>new List<ResourceSnapshot>(changes.Values).ToArray();
        public ResourceRecoverySnapshot CaptureRecovery()=>Population.Capture();
        private void Update(){if(SimulationAuthority.Allows(this) && World.Session.WorldIsRunning && Clock.Model.Elapsed>=nextAttempt){nextAttempt=Clock.Model.Elapsed+5;Tick();}}
        public int Tick()
        {
            if(!SimulationAuthority.Allows(this))return 0;
            var now=Clock.Model.Elapsed;Population.Expire(now);var spawned=0;var hardLimit=0;foreach(var rule in World.Definition.Resources)hardLimit+=Mathf.CeilToInt(rule.Count*1.25f);
            foreach(var ticket in Population.Pending.ToArray())
            {
                if(Population.Count(ticket.Rule,ticket.Region)>=Population.Target(ticket.Rule,ticket.Region)){Population.Pending.Remove(ticket);continue;}
                if(ticket.Due>now || live.Count>=hardLimit)continue;
                var rule=World.Definition.Resources[ticket.Rule];if(!FindPosition(ticket.Rule,ticket.Region,out var position))continue;
                var id="p"+(++Population.Serial);var scale=Mathf.Lerp(rule.ScaleRange.x,rule.ScaleRange.y,.5f);var node=World.CreateResource(id,ticket.Rule,position,scale);Population.Add(ticket.Rule,position);
                changes[id]=new ResourceSnapshot{Id=id,Rule=ticket.Rule,Position=position,Scale=scale,HP=node.Health.Maximum};Population.Pending.Remove(ticket);Population.Schedule(ticket.Rule,ticket.Region,now);if(++spawned>=2)break;
            }
            return spawned;
        }
        private bool FindPosition(int index,int region,out Vector2 point)
        {
            var rule=World.Definition.Resources[index];
            for(var attempt=0;attempt<96;attempt++)
            {
                point=Population.Candidate(index,region);
                if(!World.Map.ResourceSiteAllowed(index,point) || Population.Excluded(index,point,Clock.Model.Elapsed) || !Navigation.CanSpawn(point,rule.Spacing*.55f,rule.Rarity==ResourceRarity.Rare ? 24 : 12))continue;
                var density=World.Map.ResourceDensity(index,point);if(density<.25f)continue;
                var clear=true;foreach(var other in live.Values)if(other!=null && World.Definition.Resources[other.Rule].Family==rule.Family && Vector2.Distance(other.transform.position,point)<rule.ClusterSpacing){clear=false;break;}if(clear)return true;
            }
            point=default;return false;
        }
        public void Restore(ResourceSnapshot[] states,ResourceRecoverySnapshot recovery=null)
        {
            if(!SimulationAuthority.Allows(this))return;
            foreach(var state in states)
            {
                if(live.TryGetValue(state.Id,out var old)){Population.Remove(old.Rule,old.transform.position,Clock.Model.Elapsed,false);PrefabPool.Release(old.gameObject);live.Remove(state.Id);}
                changes[state.Id]=state;
                if(!state.Destroyed){var node=World.CreateResource(state.Id,state.Rule,state.Position,state.Scale);Population.Add(state.Rule,state.Position);node.Health.RestoreHealth(state.HP);}
                else if(recovery==null)Population.Harvests.Add(new HarvestExclusion{Rule=state.Rule,Position=state.Position,Until=Clock.Model.Elapsed+World.Definition.Resources[state.Rule].HarvestExclusionSeconds});
            }
            Population.Restore(recovery);
        }
    }
}
