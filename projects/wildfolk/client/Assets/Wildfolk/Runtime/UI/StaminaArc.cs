using UnityEngine;
using UnityEngine.UI;
namespace Wildfolk
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class StaminaArc : MaskableGraphic
    {
        private float fill;
        public float Fill {get=>fill;set{if(Mathf.Approximately(fill,value))return;fill=Mathf.Clamp01(value);SetVerticesDirty();}}
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var radius=rectTransform.rect.width*.5f;var thickness=4f;const int segments=48;
            for(var i=0;i<segments;i++)
            {
                var a=(210+i*120f/segments)*Mathf.Deg2Rad;var b=(210+(i+1)*120f/segments)*Mathf.Deg2Rad;
                var shade=(i+1f)/segments<=fill ? color : new Color(color.r*.26f,color.g*.26f,color.b*.26f,color.a*.75f);var index=vh.currentVertCount;
                vh.AddVert(new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,shade,Vector2.zero);vh.AddVert(new Vector2(Mathf.Cos(b),Mathf.Sin(b))*radius,shade,Vector2.zero);
                vh.AddVert(new Vector2(Mathf.Cos(b),Mathf.Sin(b))*(radius-thickness),shade,Vector2.zero);vh.AddVert(new Vector2(Mathf.Cos(a),Mathf.Sin(a))*(radius-thickness),shade,Vector2.zero);vh.AddTriangle(index,index+1,index+2);vh.AddTriangle(index,index+2,index+3);
            }
        }
    }
}
