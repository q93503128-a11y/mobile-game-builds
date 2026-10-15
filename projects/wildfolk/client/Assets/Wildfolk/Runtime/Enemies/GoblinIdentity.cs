using UnityEngine;
namespace Wildfolk
{
    public sealed class GoblinIdentity : MonoBehaviour
    {
        public GoblinActorSnapshot State;
        public GoblinController Actor;
        private GoblinSpawner owner;
        public void Bind(GoblinSpawner population,GoblinController actor,GoblinActorSnapshot state)
        {if(Actor!=null)Actor.OwnHealth.Died-=Died;owner=population;Actor=actor;State=state;Actor.OwnHealth.Died+=Died;}
        private void Died()=>owner.Removed(this);
        private void OnDestroy(){if(Actor!=null)Actor.OwnHealth.Died-=Died;}
    }
}
