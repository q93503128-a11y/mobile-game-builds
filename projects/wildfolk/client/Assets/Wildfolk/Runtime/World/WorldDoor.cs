using UnityEngine;
namespace Wildfolk
{
    public sealed class WorldDoor : MonoBehaviour, IPlayerInteractable
    {
        public WorldPlaceable Placeable;
        public BoxCollider2D Barrier;
        public Collider2D Interaction;
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
            RestoreOpen(!IsOpen);return true;
        }
        public void RestoreOpen(bool open){IsOpen=open;Barrier.enabled=!open;Placeable.Visual.transform.localRotation=Quaternion.Euler(0,0,open ? 90 : 0);}
    }
}
