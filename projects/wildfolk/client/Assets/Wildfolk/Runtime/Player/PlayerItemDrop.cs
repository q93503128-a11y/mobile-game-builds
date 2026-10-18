using UnityEngine;
namespace Wildfolk
{
    public sealed class PlayerItemDrop : MonoBehaviour
    {
        public PlayerInventory Inventory;
        public GameplaySession Session;
        public WorldPickup Prefab;
        public GameplaySettings Settings;
        public WorldGenerator World;
        private readonly Collider2D[] obstacles=new Collider2D[16];
        public void BindWorld(WorldGenerator world){World=world;Prefab=world.Pickup;Settings=world.Settings;}
        public bool Drop(InventoryModel source,int index,int amount=0)
        {
            var facility=GetComponent<NetworkFacilityInteractor>();
            if(facility!=null && !facility.IsApplying && source!=Inventory.Model)return facility.Drop(index,amount);
            if(Inventory.Authority!=null && !Inventory.Authority.IsApplying && (facility==null || !facility.IsApplying))return source==Inventory.Model && Inventory.Authority.Drop(index,amount);
            if(Session.State!=SessionState.Inventory || source==null || index<0 || index>=source.Capacity)return false;
            var stack=source.GetSlot(index);if(stack.IsEmpty)return false;
            if(amount==0)amount=stack.Count;if(amount<1 || amount>stack.Count)return false;
            for(var i=0;i<16;i++)
            {
                var angle=i*Mathf.PI/8;var point=(Vector2)transform.position+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*2;
                if(!World.Map.IsWalkable(point,.8f) || Physics2D.OverlapCircle(point,.8f,new ContactFilter2D{useTriggers=false},obstacles)>0)continue;
                WorldPickup pickup=null;
                try
                {
                    var committed=source.TryDropSlot(index,stack.Item,amount,()=>
                    {
                        pickup=PrefabPool.Spawn(Prefab,point,Quaternion.identity);pickup.Initialize(stack.Item,amount,Inventory,Settings,true,stack.Charges,stack.Quality);
                        var network=pickup.GetComponent<NetworkGroundItem>();return network==null || NetworkCreatureWorld.Active.SpawnPendingNow(network.NetworkObject);
                    });
                    if(!committed && pickup!=null)PrefabPool.Release(pickup.gameObject);return committed;
                }
                catch{if(pickup!=null)PrefabPool.Release(pickup.gameObject);throw;}
            }
            return false;
        }
    }
}
