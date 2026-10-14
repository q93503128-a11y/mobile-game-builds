using UnityEngine;
using UnityEngine.InputSystem;

namespace Wildfolk
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        public GameplaySettings Settings;
        public GameplaySession Session;
        public VirtualJoystick Joystick;
        public PlayerAvatar Avatar;
        public PlayerCombat Combat;
        public PlayerSurvival Survival;
        private Rigidbody2D body;
        private Vector2 input;
        public bool IsMoving => body != null && body.linearVelocity.sqrMagnitude > .02f;

        private void Awake() => body = GetComponent<Rigidbody2D>();

        private void Update()
        {
            input = Session.IsRunning ? Joystick.Value : Vector2.zero;
            if (Session.IsRunning && input.sqrMagnitude < .001f && Keyboard.current != null)
            {
                var keyboard = Keyboard.current;
                input = new Vector2((keyboard.dKey.isPressed ? 1 : 0) - (keyboard.aKey.isPressed ? 1 : 0),
                    (keyboard.wKey.isPressed ? 1 : 0) - (keyboard.sKey.isPressed ? 1 : 0));
                input = Vector2.ClampMagnitude(input, 1);
            }
            if (input.sqrMagnitude > .01f && !Combat.IsAttacking) Avatar.Facing.Face(input);
        }

        private void FixedUpdate() => body.linearVelocity = Session.IsRunning ? input * Settings.MoveSpeed * Survival.MovementCondition : Vector2.zero;
        private void OnDisable() { if (body != null) body.linearVelocity = Vector2.zero; }
    }
}
