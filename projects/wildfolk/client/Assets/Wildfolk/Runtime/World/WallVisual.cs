using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    public sealed class WallVisual : MonoBehaviour
    {
        public WorldPlaceable Placeable;
        public GameObject LeftEnd,RightEnd;
        private static readonly List<WallVisual> active=new List<WallVisual>();
        private readonly Collider2D[] neighbors=new Collider2D[24];
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]private static void Reset()=>active.Clear();
        private void OnEnable(){active.Add(this);RefreshNearby(transform.position);}
        private void OnDisable(){active.Remove(this);RefreshNearby(transform.position);}
        public static void RefreshNearby(Vector2 position){Physics2D.SyncTransforms();foreach(var wall in active)if(wall!=null && Vector2.Distance(wall.transform.position,position)<6)wall.Refresh();}
        public static void RefreshAll(){Physics2D.SyncTransforms();foreach(var wall in active)if(wall!=null)wall.Refresh();}
        private void Refresh()
        {
            var left=false;var right=false;var count=Physics2D.OverlapCircle(transform.position,Placeable.Definition.Footprint.x*1.25f,new ContactFilter2D{useTriggers=true},neighbors);
            for(var i=0;i<count;i++){var other=neighbors[i].GetComponentInParent<WorldPlaceable>();if(other==null || other==Placeable || !other.Definition.IsBarrier)continue;var p=transform.InverseTransformPoint(other.transform.position);if(Mathf.Abs(p.y)>.35f || Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.z,other.transform.eulerAngles.z))>10)continue;if(p.x<0)left=true;else right=true;}
            if(LeftEnd!=null)LeftEnd.SetActive(!left);if(RightEnd!=null)RightEnd.SetActive(!right);
        }
    }
}
