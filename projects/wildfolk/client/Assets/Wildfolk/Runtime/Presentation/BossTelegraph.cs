using UnityEngine;
namespace Wildfolk
{
    public sealed class BossTelegraph:MonoBehaviour
    {
        public BossEncounter Encounter;public LineRenderer Edge;public SpriteRenderer Crack;
        private readonly Vector3[] points=new Vector3[67];
        private void OnEnable(){Encounter.Changed+=Rebuild;Rebuild();}
        private void OnDisable(){Encounter.Changed-=Rebuild;}
        private void Rebuild()
        {
            var a=Encounter.Attack;Edge.enabled=Encounter.State==BossState.Windup && a!=null;if(!Edge.enabled)return;
            var angle=Mathf.Atan2(Encounter.Direction.y,Encounter.Direction.x);var count=0;
            if(a.Shape==BossAttackShape.Line)
            {var side=new Vector2(-Encounter.Direction.y,Encounter.Direction.x)*a.Width*.5f;points[count++]=side;points[count++]=Encounter.Direction*a.Range+side;points[count++]=Encounter.Direction*a.Range-side;points[count++]=-side;}
            else
            {
                if(a.Shape==BossAttackShape.Fan)points[count++]=Vector3.zero;
                for(var i=0;i<=40;i++){var sweep=a.Shape==BossAttackShape.Circle?Mathf.PI*2:a.Angle*Mathf.Deg2Rad;var t=angle-sweep*.5f+sweep*i/40;points[count++]=new Vector3(Mathf.Cos(t),Mathf.Sin(t))*a.Range;}
            }
            Edge.positionCount=count;for(var i=0;i<count;i++)Edge.SetPosition(i,points[i]);
        }
        private void LateUpdate()
        {
            var a=Encounter.Attack;Edge.transform.localPosition=Vector3.zero;
            if(Edge.enabled){var c=Color.Lerp(new Color(.72f,.68f,.45f,.4f),new Color(.9f,.79f,.54f,.95f),a==null?0:Mathf.Clamp01(Encounter.Age/a.Windup));Edge.startColor=Edge.endColor=c;}
            Crack.enabled=a!=null && a.Shape==BossAttackShape.Line && (Encounter.State==BossState.Strike || Encounter.State==BossState.Recovery && Encounter.Age<.4f);
            if(Crack.enabled){Crack.transform.localPosition=(Vector3)Encounter.Direction*(a.Range*.45f);Crack.transform.localRotation=Quaternion.Euler(0,0,Mathf.Atan2(Encounter.Direction.y,Encounter.Direction.x)*Mathf.Rad2Deg-90);Crack.transform.localScale=new Vector3(.7f,a.Range/2.56f,1);}
        }
    }
}
