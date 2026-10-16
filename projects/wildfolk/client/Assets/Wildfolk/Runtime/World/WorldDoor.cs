using UnityEngine;
namespace Wildfolk
{
    public sealed class WorldDoor : MonoBehaviour, IPlayerInteractable
    {
        public event System.Action Changed;
        public WorldPlaceable Placeable;
        public BoxCollider2D Barrier;
        public Collider2D Interaction;
        public Transform Hinge;
        [Min(.05f)]public float AnimationSeconds=.32f;
        public float OpenAmount {get;private set;}
        public bool IsAnimating=>Mathf.Abs(OpenAmount-(IsOpen ? 1 : 0))>.001f;
        public bool IsOpen { get; private set; }
        private readonly Collider2D[] occupants=new Collider2D[16];
        public Transform Location => transform;
        public Sprite Icon => Placeable.Definition.Item.Icon;
        public bool CanInteract(PlayerInteraction actor) => Placeable.Health.IsAlive && Vector2.Distance(actor.transform.position,transform.position)<=actor.Settings.StationInteractionRange;
        public bool Use(PlayerInteraction actor)
        {var network=GetComponent<NetworkBuilding>();return CanInteract(actor) && (network==null ? Toggle() : actor.GetComponent<NetworkConstruction>().Door(network));}
        public bool Toggle()
        {
            if(!SimulationAuthority.Allows(this) || !Placeable.Health.IsAlive) return false;
            if(IsOpen)
            {
                var count=Physics2D.OverlapBox(transform.position,Placeable.Definition.Footprint*.85f,transform.eulerAngles.z,new ContactFilter2D{useTriggers=true},occupants);
                for(var i=0;i<count;i++) if(occupants[i]!=Barrier && occupants[i]!=Interaction) return false;
            }
            IsOpen=!IsOpen;Changed?.Invoke();return true;
        }
        internal void Present(bool open,float amount){IsOpen=open;OpenAmount=amount;ApplyPose();}
        private void Update(){var network=GetComponent<NetworkBuilding>();if(network!=null && network.IsSpawned && !network.IsServer){network.PresentDoor();return;}if(!IsAnimating)return;OpenAmount=Mathf.MoveTowards(OpenAmount,IsOpen ? 1 : 0,Time.deltaTime/AnimationSeconds);if(!IsAnimating)OpenAmount=IsOpen ? 1 : 0;ApplyPose();}
        private void ApplyPose(){var eased=Mathf.SmoothStep(0,1,OpenAmount);Hinge.localRotation=Quaternion.Euler(0,0,-90*eased);Barrier.enabled=eased<.7f;}
        public void RestoreOpen(bool open){IsOpen=open;OpenAmount=open ? 1 : 0;ApplyPose();}
    }
}
