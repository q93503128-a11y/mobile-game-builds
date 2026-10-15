using UnityEngine;
namespace Wildfolk
{
    public sealed class StoneProjectile : MonoBehaviour,IPoolReset
    {
        private Health owner,target;
        private Vector2 velocity;
        private float remaining;
        private int damage;
        private readonly RaycastHit2D[] hits=new RaycastHit2D[24];
        public void ResetForSpawn(){remaining=0;owner=target=null;}
        public void Launch(Health source,Health destination,Vector2 direction,SlingshotDefinition definition)
        {owner=source;target=destination;velocity=direction.normalized*definition.ProjectileSpeed;remaining=definition.ProjectileLifetime;damage=definition.Damage;}
        private void Update()
        {
            if(remaining<=0){PrefabPool.Release(gameObject);return;}
            var step=velocity*Time.deltaTime;var origin=(Vector2)transform.position;
            var count=Physics2D.CircleCast(origin,.08f,velocity.normalized,new ContactFilter2D{useTriggers=true},hits,step.magnitude);
            Collider2D hit=null;Health victim=null;var nearest=float.MaxValue;
            for(var i=0;i<count;i++)
            {
                var collider=hits[i].collider;var health=collider.GetComponentInParent<Health>();
                if(health==owner || collider.GetComponentInParent<StoneProjectile>()!=null)continue;
                var member=collider.GetComponentInParent<CreatureMember>();if(member!=null)continue;
                var valid=health==target || !collider.isTrigger && (health==null || health.GetComponent<WorldPlaceable>()!=null);
                if(!valid || hits[i].distance>=nearest)continue;nearest=hits[i].distance;hit=collider;victim=health;
            }
            if(hit!=null){if(victim!=null && victim.IsAlive)victim.ApplyDamage(damage);PrefabPool.Release(gameObject);return;}
            transform.position=origin+step;remaining-=Time.deltaTime;
        }
    }
}
