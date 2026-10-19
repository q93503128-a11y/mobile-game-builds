using UnityEngine;
namespace Wildfolk
{
    public sealed class StoneProjectile : MonoBehaviour,IPoolReset
    {
        public Vector2 Velocity=>velocity;
        private Health owner,target;private Sprite originalSprite;private WorldGenerator turretWorld;
        private void Awake()=>originalSprite=GetComponentInChildren<SpriteRenderer>().sprite;
        private Vector2 velocity;
        private float remaining;
        private int damage;private bool playerShot;private PlayerInventory credit;
        public short VisualIndex {get;private set;}=-1;
        private readonly RaycastHit2D[] hits=new RaycastHit2D[24];
        public void ResetForSpawn(){remaining=0;owner=target=null;playerShot=false;credit=null;turretWorld=null;VisualIndex=-1;}
        public void Launch(Health source,Health destination,Vector2 direction,SlingshotDefinition definition)
        {GetComponentInChildren<SpriteRenderer>().sprite=originalSprite;transform.rotation=Quaternion.identity;owner=source;target=destination;velocity=direction.normalized*definition.ProjectileSpeed;remaining=definition.ProjectileLifetime;damage=definition.Damage;}
        public void Launch(Health source,Vector2 direction,float speed,float range,int amount,PlayerInventory player,ItemDefinition ammo)
        {owner=source;target=null;playerShot=true;credit=player;turretWorld=source.GetComponent<WorldTurret>()?.SimulationWorld;velocity=direction.normalized*speed;remaining=range/speed;damage=amount;var catalog=player==null ? NetworkCreatureWorld.Active?.Catalog : player.GetComponent<NetworkPlayer>()?.Catalog;VisualIndex=catalog==null ? (short)-1 : (short)System.Array.IndexOf(catalog.Items,ammo);GetComponentInChildren<SpriteRenderer>().sprite=ammo.Icon;transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg-90);}
        private void Update()
        {
            var network=GetComponent<NetworkProjectile>();if(network!=null && (!network.IsSpawned || !network.IsServer))return;
            if(turretWorld!=null && turretWorld.Map.Streamed && !turretWorld.Chunks.Wants(transform.position)){PrefabPool.Release(gameObject);return;}
            if(remaining<=0){PrefabPool.Release(gameObject);return;}
            var elapsed=Mathf.Min(Time.deltaTime,remaining);var step=velocity*elapsed;var origin=(Vector2)transform.position;
            var count=Physics2D.CircleCast(origin,.08f,velocity.normalized,new ContactFilter2D{useTriggers=true},hits,step.magnitude);
            Collider2D hit=null;Health victim=null;var nearest=float.MaxValue;
            for(var i=0;i<count;i++)
            {
                var collider=hits[i].collider;var health=collider.GetComponentInParent<Health>();
                if(health==owner || collider.GetComponentInParent<StoneProjectile>()!=null)continue;
                var member=collider.GetComponentInParent<CreatureMember>();if(!playerShot && member!=null)continue;
                if(playerShot && health!=null && health.GetComponent<PlayerInventory>()!=null)continue;
                var valid=(playerShot ? CombatAffiliation.CanDamage(owner,health) && health.GetComponent<ResourceNode>()==null : health==target) || !collider.isTrigger && (health==null || health.GetComponent<WorldPlaceable>()!=null || playerShot && health.GetComponent<ResourceNode>()!=null);
                if(!valid || hits[i].distance>=nearest)continue;nearest=hits[i].distance;hit=collider;victim=health;
            }
            if(hit!=null){if(victim!=null && victim.IsAlive && (!playerShot || victim.GetComponent<ResourceNode>()==null && CombatAffiliation.CanDamage(owner,victim)))victim.ApplyDamage(damage,false,credit);PrefabPool.Release(gameObject);return;}
            transform.position=origin+step;remaining-=elapsed;
        }
    }
}
