using UnityEngine;
namespace Wildfolk
{
    public sealed class WaterSource : MonoBehaviour
    {
        public WaterSourceDefinition Definition;
        private float availableAt;
        public bool CanInteract(Vector2 actor) => Vector2.Distance(actor,transform.position)<=Definition.InteractionRange;
        public bool Collect(PlayerInventory player)
        {
            if(!CanInteract(player.transform.position) || Time.time<availableAt) return false;
            var added=player.Model.Add(Definition.Water,Definition.PortionCount);
            if(added<=0) return false;
            availableAt=Time.time+Definition.CollectionCooldown;return true;
        }
    }
}
