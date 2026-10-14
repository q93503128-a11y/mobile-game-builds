using UnityEngine;
namespace Wildfolk
{
    public sealed class ResourceIdentity : MonoBehaviour
    {
        public string Id;
        public int Rule;
        public ResourceNode Node;
        public ResourceRegeneration Owner;
        public void Bind(){Node.Health.Changed+=Changed;Node.Health.Died+=Died;}
        private void Changed()=>Owner.Record(this,false);
        private void Died()=>Owner.Record(this,true);
        private void OnDestroy(){if(Node!=null){Node.Health.Changed-=Changed;Node.Health.Died-=Died;}}
    }
}
