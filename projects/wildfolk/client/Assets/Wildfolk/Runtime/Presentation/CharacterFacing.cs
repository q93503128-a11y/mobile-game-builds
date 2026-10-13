using UnityEngine;

namespace Wildfolk
{
    public sealed class CharacterFacing : MonoBehaviour
    {
        public Transform LeftEye;
        public Transform RightEye;
        public Transform LeftHand;
        public Transform RightHand;
        public float EyeForward = .28f;
        public float EyeSeparation = .18f;
        public float HandForward = .38f;
        public float HandSeparation = .85f;
        public float TurnSpeed = 1080;
        public Vector2 Direction { get; private set; } = Vector2.down;
        public Vector2 DisplayDirection { get; private set; } = Vector2.down;
        private float displayAngle = -90;
        private Vector2 leftOffset;
        private Vector2 rightOffset;
        private float leftArc;
        private float rightArc;

        public void Face(Vector2 direction)
        {
            if (direction.sqrMagnitude > .001f) Direction = direction.normalized;
        }

        public void SetHandPose(int hand, Vector2 offset, float arc = 0)
        {
            if (hand == 0) { leftOffset = offset; leftArc = arc; }
            else { rightOffset = offset; rightArc = arc; }
        }

        public void ResetPose()
        {
            leftOffset = rightOffset = Vector2.zero;
            leftArc = rightArc = 0;
        }

        private void LateUpdate()
        {
            var target = Mathf.Atan2(Direction.y, Direction.x) * Mathf.Rad2Deg;
            displayAngle = Mathf.MoveTowardsAngle(displayAngle, target, TurnSpeed * Time.deltaTime);
            DisplayDirection = new Vector2(Mathf.Cos(displayAngle * Mathf.Deg2Rad), Mathf.Sin(displayAngle * Mathf.Deg2Rad));
            var side = new Vector2(-DisplayDirection.y, DisplayDirection.x);
            LeftEye.localPosition = DisplayDirection * EyeForward - side * EyeSeparation;
            RightEye.localPosition = DisplayDirection * EyeForward + side * EyeSeparation;
            PlaceHand(LeftHand, -HandSeparation, leftOffset, leftArc, side);
            PlaceHand(RightHand, HandSeparation, rightOffset, rightArc, side);
        }

        private void PlaceHand(Transform hand, float separation, Vector2 offset, float arc, Vector2 side)
        {
            hand.localPosition = side * (separation + offset.x) + DisplayDirection * (HandForward + offset.y);
            hand.localRotation = Quaternion.Euler(0, 0, displayAngle - 90 + arc);
        }
    }
}
