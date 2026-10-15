using UnityEngine;
namespace Wildfolk
{
    public sealed class AnimalIdentity : MonoBehaviour
    {
        public AnimalSnapshot State;
        public AnimalPopulation Owner;
        public NeutralAnimal Animal;
        public void Bind()=>Animal.Health.Died+=Died;
        private void Died()=>Owner.Removed(this);
        private void OnDestroy(){if(Animal!=null)Animal.Health.Died-=Died;}
    }
}
