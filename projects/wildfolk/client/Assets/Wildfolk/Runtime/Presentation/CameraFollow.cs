using UnityEngine;

namespace Wildfolk
{
    [RequireComponent(typeof(Camera))]
    public sealed class CameraFollow : MonoBehaviour
    {
        public Transform Target;
        public SpriteRenderer Ground;
        public GameplaySettings Settings;
        private Vector3 velocity;
        private Camera view;
        private void Awake() => view = GetComponent<Camera>();

        private void LateUpdate()
        {
            if (Target == null) return;
            var bounds = Ground.bounds;
            var halfHeight = view.orthographicSize;
            var halfWidth = halfHeight * view.aspect;
            var desired = new Vector3(
                Mathf.Clamp(Target.position.x, bounds.min.x + halfWidth, bounds.max.x - halfWidth),
                Mathf.Clamp(Target.position.y, bounds.min.y + halfHeight, bounds.max.y - halfHeight), -10);
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, Settings.CameraSmoothTime);
        }
    }
}
