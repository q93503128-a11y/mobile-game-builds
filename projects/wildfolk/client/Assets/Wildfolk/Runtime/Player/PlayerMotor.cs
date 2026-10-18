using UnityEngine;

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
        public PlayerInputCommands Input;
        public WorldNavigation Terrain;
        public PlayerLocomotion Locomotion;
        private Rigidbody2D body;
        private Vector2 input;
        public bool IsMoving => body != null && body.linearVelocity.sqrMagnitude > .02f;

        private void Awake() => body = GetComponent<Rigidbody2D>();

        private void Update()
        {
            input=Session.IsRunning ? Input.Move : Vector2.zero;
            if (input.sqrMagnitude > .01f && !Combat.IsAttacking) Avatar.Facing.Face(input);
        }

        private void FixedUpdate()
        {
            if(Locomotion.MoveDash(Time.fixedDeltaTime))return;
            var velocity=Session.IsRunning ? input*Settings.MoveSpeed*Locomotion.SpeedMultiplier*Survival.MovementCondition*(Terrain==null ? 1 : Terrain.MovementMultiplier(body.position)) : Vector2.zero;
            if(Terrain!=null && !Terrain.Traversable(body.position+velocity*Time.fixedDeltaTime,.3f))velocity=Vector2.zero;body.linearVelocity=velocity;
        }

        private void OnDisable() { if (body != null) body.linearVelocity = Vector2.zero; }
    }
}
