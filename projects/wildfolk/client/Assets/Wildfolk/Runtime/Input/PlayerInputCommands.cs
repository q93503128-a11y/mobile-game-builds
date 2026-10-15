using UnityEngine;
namespace Wildfolk
{
    public interface IPlayerMovementAuthority { void SetMove(Vector2 move); bool Dash(); }
    public sealed class PlayerInputCommands : MonoBehaviour
    {
        public PlayerPrimaryAction Primary;
        public PlayerInteraction Interaction;
        public GameplaySession Session;
        public PlayerLocomotion Locomotion;
        public IPlayerMovementAuthority MovementAuthority {get;set;}
        public bool AllowInventory=true,AllowPause=true;
        public bool Dash()=>MovementAuthority==null ? Locomotion.TryDash() : MovementAuthority.Dash();
        public Vector2 Move {get;private set;}
        public void SetMove(Vector2 move){move=Session.IsRunning ? Vector2.ClampMagnitude(move,1) : Vector2.zero;if(MovementAuthority==null)ApplyMove(move);else MovementAuthority.SetMove(move);}
        internal void ApplyMove(Vector2 move)=>Move=move;
        public bool Attack()=>Primary!=null && Primary.TryUse();
        public bool Interact()=>Interaction!=null && Interaction.TryInteract();
        public void Inventory(){if(AllowInventory)Session.ToggleInventory();}
        public void Pause(){if(AllowPause)Session.OpenPause();}
    }
}
