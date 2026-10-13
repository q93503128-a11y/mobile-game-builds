using UnityEngine;

namespace Wildfolk
{
    public sealed class PlayerAvatar : MonoBehaviour
    {
        public Transform FacingRoot;
        public Transform Body;
        public Transform LeftHand;
        public Transform RightHand;
        public PlayerMotor Motor;
        public Vector2 Facing { get; private set; } = Vector2.down;
        private Vector3 restBody;
        private Vector3 restScale;

        private void Awake() { restBody = Body.localPosition; restScale = Body.localScale; }

        public void Face(Vector2 direction)
        {
            if (direction.sqrMagnitude < .001f) return;
            Facing = direction.normalized;
            FacingRoot.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(Facing.y, Facing.x) * Mathf.Rad2Deg + 90);
        }

        private void LateUpdate()
        {
            var bob = Motor.IsMoving ? Mathf.Sin(Time.time * 11) : 0;
            Body.localPosition = restBody + Vector3.up * (bob * .022f);
            Body.localScale = new Vector3(restScale.x * (1 + bob * .012f), restScale.y * (1 - bob * .012f), restScale.z);
        }
    }
}
