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
        private WorldGenerator world;private PlayerTelescope telescope;
        private Bounds previousBounds;
        private Vector3 previousTarget;
        private bool initialized;
        private void Awake(){view=GetComponent<Camera>();world=FindFirstObjectByType<WorldGenerator>();}

        private void LateUpdate()
        {
            if (Target == null) return;
            var bounds=world!=null && world.Map!=null && world.Map.Streamed ? world.Map.RegionBounds(Target.position) : Ground.bounds;
            if(telescope==null || telescope.transform!=Target)telescope=Target.GetComponent<PlayerTelescope>();
            var follow=telescope!=null && telescope.IsViewing ? (Vector3)telescope.ViewPoint : Target.position;
            var halfHeight = view.orthographicSize;
            var halfWidth = halfHeight * view.aspect;
            var desired = new Vector3(
                Mathf.Clamp(follow.x, bounds.min.x + halfWidth, bounds.max.x - halfWidth),
                Mathf.Clamp(follow.y, bounds.min.y + halfHeight, bounds.max.y - halfHeight), -10);
            // A portal/load is a discrete region change, not a pan across the space between maps.
            var jump=Vector2.Distance(follow,previousTarget)>halfWidth*2;
            if(!initialized || bounds!=previousBounds || jump){transform.position=desired;velocity=Vector3.zero;}
            else transform.position=Vector3.SmoothDamp(transform.position,desired,ref velocity,Settings.CameraSmoothTime);
            initialized=true;previousBounds=bounds;previousTarget=follow;
            WorldSorting.OriginY=transform.position.y;
        }
    }
}
