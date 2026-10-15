using UnityEngine;

namespace Wildfolk
{
    public sealed class PlayerAvatar : MonoBehaviour
    {
        public CharacterFacing Facing;
        public Transform Body;
        public PlayerMotor Motor;
        public bool? MovingOverride {get;set;}
        public float MotionRate {get;set;}=1;
        private Vector3 restBody;
        private Vector3 restScale;

        private void Awake() { restBody = Body.localPosition; restScale = Body.localScale; }
        private void LateUpdate()
        {
            var bob = (MovingOverride ?? Motor.IsMoving) ? Mathf.Sin(Time.time * 11 * MotionRate) : 0;
            Body.localPosition = restBody + Vector3.up * (bob * .022f);
            Body.localScale = new Vector3(restScale.x * (1 + bob * .012f), restScale.y * (1 - bob * .012f), restScale.z);
        }
    }
}
