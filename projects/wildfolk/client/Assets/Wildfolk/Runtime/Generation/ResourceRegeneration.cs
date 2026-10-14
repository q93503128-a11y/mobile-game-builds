using System;
using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    public sealed class ResourceRegeneration : MonoBehaviour
    {
        public WorldGenerator World;
        public WorldClock Clock;
        public WorldNavigation Navigation;
        private readonly Dictionary<string,ResourceSnapshot> changes=new Dictionary<string,ResourceSnapshot>();
        private readonly Dictionary<string,ResourceIdentity> live=new Dictionary<string,ResourceIdentity>();
        private System.Random random;
        private double nextAttempt;
        public bool HasChanges=>changes.Count>0;
        public void ResetWorld(){changes.Clear();live.Clear();random=new System.Random(World.Map.Seed^31037);nextAttempt=0;}
        public void Register(ResourceNode node,string id,int rule)
        {var identity=node.gameObject.AddComponent<ResourceIdentity>();identity.Id=id;identity.Rule=rule;identity.Node=node;identity.Owner=this;identity.Bind();live[id]=identity;}
        public void Record(ResourceIdentity identity,bool destroyed)
        {
            var state=new ResourceSnapshot{Id=identity.Id,Rule=identity.Rule,Position=identity.transform.position,Scale=identity.transform.localScale.x,HP=identity.Node.Health.Current,Destroyed=destroyed};
            if(destroyed){state.RespawnAt=Clock.Model.Elapsed+World.Definition.Resources[state.Rule].RespawnSeconds;live.Remove(state.Id);}
            changes[state.Id]=state;
        }
        public ResourceSnapshot[] Capture()
        {var result=new List<ResourceSnapshot>();foreach(var s in changes.Values)result.Add(new ResourceSnapshot{Id=s.Id,Rule=s.Rule,HP=s.HP,Position=s.Position,Scale=s.Scale,Destroyed=s.Destroyed,RespawnAt=s.RespawnAt});return result.ToArray();}
        private void Update(){if(World.Session.WorldIsRunning && Clock.Model.Elapsed>=nextAttempt){nextAttempt=Clock.Model.Elapsed+5;Tick();}}
        public int Tick()
        {
            var spawned=0;
            foreach(var state in changes.Values)
            {
                if(!state.Destroyed || Clock.Model.Elapsed<state.RespawnAt)continue;
                if(!FindPosition(state.Position,World.Definition.Resources[state.Rule].Spacing*.55f,out var position))continue;
                var node=World.CreateResource(state.Id,state.Rule,position,state.Scale);
                state.Destroyed=false;state.HP=node.Health.Maximum;state.Position=position;state.RespawnAt=0;
                if(++spawned>=2)break;
            }
            return spawned;
        }
        private bool FindPosition(Vector2 home,float radius,out Vector2 point)
        {
            for(var i=0;i<64;i++)
            {var angle=(float)random.NextDouble()*Mathf.PI*2;var distance=6+(float)random.NextDouble()*20;point=home+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*distance;if(!World.Map.IsRoad(point) && Navigation.CanSpawn(point,radius,12))return true;}
            point=default;return false;
        }
        public void Restore(ResourceSnapshot[] states)
        {
            foreach(var state in states)
            {
                if(live.TryGetValue(state.Id,out var old)){old.gameObject.SetActive(false);Destroy(old.gameObject);live.Remove(state.Id);}
                changes[state.Id]=state;
                if(!state.Destroyed){var node=World.CreateResource(state.Id,state.Rule,state.Position,state.Scale);node.Health.RestoreHealth(state.HP);}
            }
        }
    }
}
