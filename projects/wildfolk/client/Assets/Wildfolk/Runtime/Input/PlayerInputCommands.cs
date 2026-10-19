using UnityEngine;
namespace Wildfolk
{
    public interface IPlayerMovementAuthority { void SetMove(Vector2 move); bool Dash(); }
    public interface IPlayerCombatAuthority {bool Attack();}
    public sealed class PlayerInputCommands : MonoBehaviour
    {
        public PlayerPrimaryAction Primary;
        public PlayerInteraction Interaction;
        public GameplaySession Session;
        public PlayerLocomotion Locomotion;
        public IPlayerMovementAuthority MovementAuthority {get;set;}
        public IPlayerCombatAuthority CombatAuthority {get;set;}
        public bool AllowInventory=true,AllowPause=true;
        public bool IsScouting=>GetComponent<PlayerTelescope>()!=null && GetComponent<PlayerTelescope>().IsViewing;
        public bool Dash()=>!IsScouting && (MovementAuthority==null ? Locomotion.TryDash() : MovementAuthority.Dash());
        public Vector2 Move {get;private set;}
        public void SetMove(Vector2 move){if(IsScouting){GetComponent<PlayerTelescope>().Aim(move);move=Vector2.zero;}move=Session.IsRunning ? Vector2.ClampMagnitude(move,1) : Vector2.zero;if(MovementAuthority==null)ApplyMove(move);else MovementAuthority.SetMove(move);}
        internal void ApplyMove(Vector2 move)=>Move=move;
        public bool Attack()=>Session.IsRunning && (CombatAuthority!=null ? CombatAuthority.Attack() : Primary!=null && Primary.TryUse());
        public bool Interact()=>!IsScouting && Interaction!=null && Interaction.TryInteract();
        public void Inventory(){if(AllowInventory)Session.ToggleInventory();}
        public void Pause(){if(TierThreeHud.CloseFocused())return;if(IsScouting){GetComponent<PlayerTelescope>().Exit();return;}if(AllowPause)Session.OpenPause();}
    }
}
