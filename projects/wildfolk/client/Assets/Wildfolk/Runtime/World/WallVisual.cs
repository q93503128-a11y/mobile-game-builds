using UnityEngine;
namespace Wildfolk
{
    public sealed class WallVisual : MonoBehaviour
    {
        public WorldPlaceable Placeable;
        public GameObject LeftEnd,RightEnd;
        private readonly Collider2D[] neighbors=new Collider2D[24];
        private float next;
        private void LateUpdate()
        {
            if(Time.time<next)return;next=Time.time+.35f;var left=false;var right=false;
            var count=Physics2D.OverlapCircle(transform.position,Placeable.Definition.Footprint.x*1.25f,new ContactFilter2D{useTriggers=false},neighbors);
            for(var i=0;i<count;i++)
            {
                var other=neighbors[i].GetComponentInParent<WorldPlaceable>();if(other==null || other==Placeable || !other.Definition.IsBarrier)continue;
                var p=transform.InverseTransformPoint(other.transform.position);
                if(Mathf.Abs(p.y)>.35f || Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.z,other.transform.eulerAngles.z))>10)continue;
                if(p.x<0)left=true;else right=true;
            }
            if(LeftEnd!=null)LeftEnd.SetActive(!left);if(RightEnd!=null)RightEnd.SetActive(!right);
        }
    }
}
