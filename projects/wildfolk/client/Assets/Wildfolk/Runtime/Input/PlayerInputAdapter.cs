using UnityEngine;
using UnityEngine.InputSystem;
namespace Wildfolk
{
    public interface IPlayerInputSource {Vector2 Move {get;} bool Primary {get;} bool Interact {get;} bool Inventory {get;} bool Pause {get;}}
    [DefaultExecutionOrder(-50)]
    public sealed class PlayerInputAdapter : MonoBehaviour
    {
        public PlayerInputCommands Commands;
        public VirtualJoystick Joystick;
        public IPlayerInputSource Source {get;set;}
        private void Update()
        {
            if(Source!=null){Commands.SetMove(Source.Move);if(Source.Primary)Commands.Attack();if(Source.Interact)Commands.Interact();if(Source.Inventory)Commands.Inventory();if(Source.Pause)Commands.Pause();return;}
            var move=Joystick==null ? Vector2.zero : Joystick.Value;var keyboard=Keyboard.current;
            if(keyboard!=null)
            {
                if(move.sqrMagnitude<.001f)move=new Vector2((keyboard.dKey.isPressed ? 1 : 0)-(keyboard.aKey.isPressed ? 1 : 0),(keyboard.wKey.isPressed ? 1 : 0)-(keyboard.sKey.isPressed ? 1 : 0));
                if(keyboard.spaceKey.wasPressedThisFrame || keyboard.spaceKey.isPressed && Commands.Primary.Kind==PrimaryActionKind.Attack)Commands.Attack();
                if(keyboard.leftShiftKey.wasPressedThisFrame)Commands.Dash();
                if(keyboard.eKey.wasPressedThisFrame)Commands.Interact();if(keyboard.tabKey.wasPressedThisFrame)Commands.Inventory();if(keyboard.escapeKey.wasPressedThisFrame)Commands.Pause();
            }
            Commands.SetMove(move);
        }
    }
}
