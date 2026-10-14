using UnityEngine;
namespace Wildfolk
{
    public sealed class AnimalIdentity : MonoBehaviour
    {
        public AnimalSnapshot State;
        public AnimalPopulation Owner;
        public NeutralAnimal Animal;
        public void Bind()=>Animal.Health.Died+=Died;
        private void Died(){State.Missing=true;State.HP=0;State.RespawnAt=Owner.Clock.Model.Elapsed+Owner.Definition.Rules[State.Rule].RespawnSeconds;}
        private void OnDestroy(){if(Animal!=null)Animal.Health.Died-=Died;}
    }
}
