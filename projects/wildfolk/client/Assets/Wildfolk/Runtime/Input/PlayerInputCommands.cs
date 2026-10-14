using UnityEngine;
namespace Wildfolk
{
    public sealed class PlayerInputCommands : MonoBehaviour
    {
        public PlayerPrimaryAction Primary;
        public PlayerInteraction Interaction;
        public GameplaySession Session;
        public Vector2 Move {get;private set;}
        public void SetMove(Vector2 move)=>Move=Session.IsRunning ? Vector2.ClampMagnitude(move,1) : Vector2.zero;
        public bool Attack()=>Primary.TryUse();
        public bool Interact()=>Interaction.TryInteract();
        public void Inventory()=>Session.ToggleInventory();
        public void Pause()=>Session.OpenPause();
    }
}
