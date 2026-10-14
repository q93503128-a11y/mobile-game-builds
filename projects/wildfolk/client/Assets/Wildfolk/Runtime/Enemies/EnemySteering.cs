using UnityEngine;
namespace Wildfolk
{
    public sealed class EnemySteering : MonoBehaviour
    {
        public EnemySteeringDefinition Definition;
        public WorldNavigation Navigation;
        private Rigidbody2D body;
        private readonly Collider2D[] neighbors=new Collider2D[32];
        private readonly RaycastHit2D[] obstacles=new RaycastHit2D[16];
        private void Awake()=>body=GetComponent<Rigidbody2D>();
        private void Start(){if(Navigation==null)Navigation=FindFirstObjectByType<WorldNavigation>();}
        public Vector2 Separate(Vector2 desired)
        {
            var count=Physics2D.OverlapCircle(body.position,Definition.SeparationRadius,new ContactFilter2D{useTriggers=true},neighbors);var repel=Vector2.zero;
            for(var i=0;i<count;i++)
            {
                if(neighbors[i].attachedRigidbody==body || neighbors[i].GetComponentInParent<EnemySteering>()==null)continue;
                var delta=body.position-(Vector2)neighbors[i].transform.position;
                if(delta.sqrMagnitude<.0001f){var angle=(GetInstanceID()%97)*Mathf.PI*2/97;delta=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*.01f;}
                var distance=delta.magnitude;repel+=delta.normalized*Mathf.Clamp01(1-distance/Definition.SeparationRadius);
            }
            return Vector2.ClampMagnitude(desired+repel*Definition.SeparationStrength,1);
        }
        public bool Clear(Vector2 direction,float step)
        {return Navigation!=null ? Navigation.CanMove(body.position,direction,step,Definition.Footprint) : Physics2D.CircleCast(body.position,Definition.Footprint,direction,new ContactFilter2D{useTriggers=false},obstacles,step+.08f)==0;}
        public bool Move(Vector2 desired,float speed,bool avoid=true)
        {
            var direction=Separate(desired);if(direction.sqrMagnitude<.001f)return false;
            var step=speed*Time.fixedDeltaTime;
            if(!Clear(direction.normalized,step))
            {
                if(!avoid)return false;
                var left=(Vector2)(Quaternion.Euler(0,0,55)*direction);var right=(Vector2)(Quaternion.Euler(0,0,-55)*direction);
                if(Clear(left.normalized,step))direction=left;else if(Clear(right.normalized,step))direction=right;else return false;
            }
            body.MovePosition(body.position+direction*step);return true;
        }
    }
}
