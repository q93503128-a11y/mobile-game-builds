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
        public void SetMove(Vector2 move){if(move.sqrMagnitude>.001f && (GetComponent<PlayerDeath>()?.RescueTarget!=null || Session.State==SessionState.Rescuing)){GetComponent<PlayerDeath>().InterruptRescue();GetComponent<NetworkPlayer>()?.CancelRevive();}if(IsScouting){GetComponent<PlayerTelescope>().Aim(move);move=Vector2.zero;}move=Session.IsRunning ? Vector2.ClampMagnitude(move,1) : Vector2.zero;if(MovementAuthority==null)ApplyMove(move);else MovementAuthority.SetMove(move);}
        internal void ApplyMove(Vector2 move)=>Move=move;
        public bool Attack(){if(Session.State==SessionState.Rescuing){GetComponent<PlayerDeath>()?.InterruptRescue();GetComponent<NetworkPlayer>()?.CancelRevive();}return Session.IsRunning && (CombatAuthority!=null ? CombatAuthority.Attack() : Primary!=null && Primary.TryUse());}
        public bool Interact()=>!IsScouting && Interaction!=null && Interaction.TryInteract();
        public bool HoldInteract(bool held){var net=GetComponent<NetworkPlayer>();return net!=null && net.HoldRevive(held);}
        public bool HasDownedTeammate=>GetComponent<PlayerDeath>()?.NearbyDowned()!=null;
        public void Inventory(){if(AllowInventory)Session.ToggleInventory();}
        public void Pause(){if(CampLootPanel.CloseFocused() || TierThreeHud.CloseFocused())return;if(IsScouting){GetComponent<PlayerTelescope>().Exit();return;}if(AllowPause)Session.OpenPause();}
    }
}
