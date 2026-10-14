using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    public sealed class AnimalPopulation : MonoBehaviour
    {
        public AnimalPopulationDefinition Definition;
        public WorldGenerator World;
        public WorldClock Clock;
        public WorldNavigation Navigation;
        private readonly List<AnimalSnapshot> slots=new List<AnimalSnapshot>();
        private readonly Dictionary<string,AnimalIdentity> live=new Dictionary<string,AnimalIdentity>();
        private System.Random random;private double nextAttempt;
        public void Generate()
        {
            slots.Clear();live.Clear();random=new System.Random(World.Seed^19447);nextAttempt=0;
            for(var rule=0;rule<Definition.Rules.Length;rule++)for(var i=0;i<Definition.Rules[rule].Population;i++)
            {
                var state=new AnimalSnapshot{Id="a"+rule+"_"+i,Rule=rule,Home=Definition.Rules[rule].Home,Missing=true};slots.Add(state);
                if(FindPosition(state.Home,false,out var position))Spawn(state,position);
            }
        }
        private bool FindPosition(Vector2 home,bool outside,out Vector2 p)
        {
            for(var i=0;i<64;i++){p=home+new Vector2((float)random.NextDouble()*2-1,(float)random.NextDouble()*2-1)*(outside ? 24 : 8);if(Navigation.CanSpawn(p,.8f,outside ? 12 : 5,outside))return true;}
            p=default;return false;
        }
        private void Spawn(AnimalSnapshot state,Vector2 position)
        {
            var actor=Instantiate(Definition.Rules[state.Rule].Prefab,position,Quaternion.identity,World.Environment);
            actor.Player=World.Player.transform;actor.Session=World.Session;actor.Region=World.Region;
            var loot=actor.GetComponent<LootOnDeath>();loot.Player=World.Player;loot.Pickup=World.Pickup;loot.Settings=World.Settings;
            state.Missing=false;state.Position=position;state.HP=actor.Health.Current;state.RespawnAt=0;
            var identity=actor.gameObject.AddComponent<AnimalIdentity>();identity.State=state;identity.Animal=actor;identity.Owner=this;identity.Bind();live[state.Id]=identity;
        }
        private void Update(){if(World.Session.WorldIsRunning && Clock.Model.Elapsed>=nextAttempt){nextAttempt=Clock.Model.Elapsed+5;Tick();}}
        public int Tick()
        {var count=0;foreach(var state in slots)if(state.Missing && Clock.Model.Elapsed>=state.RespawnAt && FindPosition(state.Home,true,out var p)){Spawn(state,p);count++;}return count;}
        public AnimalSnapshot[] Capture()
        {var result=new List<AnimalSnapshot>();foreach(var state in slots){if(!state.Missing && live.TryGetValue(state.Id,out var identity) && identity!=null){state.Position=identity.transform.position;state.HP=identity.Animal.Health.Current;}result.Add(new AnimalSnapshot{Id=state.Id,Rule=state.Rule,HP=state.HP,Position=state.Position,Home=state.Home,Missing=state.Missing,RespawnAt=state.RespawnAt});}return result.ToArray();}
        public void Restore(AnimalSnapshot[] states)
        {
            foreach(var identity in live.Values)if(identity!=null){identity.gameObject.SetActive(false);Destroy(identity.gameObject);}slots.Clear();live.Clear();
            foreach(var state in states){slots.Add(state);if(state.Missing)continue;var hp=state.HP;Spawn(state,state.Position);live[state.Id].Animal.Health.RestoreHealth(hp);state.HP=hp;}
        }
    }
}
