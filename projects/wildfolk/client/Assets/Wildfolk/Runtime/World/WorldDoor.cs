using UnityEngine;
namespace Wildfolk
{
    public sealed class WorldDoor : MonoBehaviour, IPlayerInteractable
    {
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
        public bool Use(PlayerInteraction actor) => CanInteract(actor) && Toggle();
        public bool Toggle()
        {
            if(!Placeable.Health.IsAlive) return false;
            if(IsOpen)
            {
                var count=Physics2D.OverlapBox(transform.position,Placeable.Definition.Footprint*.85f,transform.eulerAngles.z,new ContactFilter2D{useTriggers=true},occupants);
                for(var i=0;i<count;i++) if(occupants[i]!=Barrier && occupants[i]!=Interaction) return false;
            }
            IsOpen=!IsOpen;return true;
        }
        private void Update(){if(!IsAnimating)return;OpenAmount=Mathf.MoveTowards(OpenAmount,IsOpen ? 1 : 0,Time.deltaTime/AnimationSeconds);ApplyPose();}
        private void ApplyPose(){var eased=Mathf.SmoothStep(0,1,OpenAmount);Hinge.localRotation=Quaternion.Euler(0,0,-90*eased);Barrier.enabled=eased<.7f;}
        public void RestoreOpen(bool open){IsOpen=open;OpenAmount=open ? 1 : 0;ApplyPose();}
    }
}
