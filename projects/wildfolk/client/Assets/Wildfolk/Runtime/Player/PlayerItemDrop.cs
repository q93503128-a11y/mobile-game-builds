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
        public bool Drop(InventoryModel source,int index,int amount=0)
        {
            if(Session.State!=SessionState.Inventory || source==null)return false;
            var stack=source.GetSlot(index);if(stack.IsEmpty)return false;
            if(amount==0)amount=stack.Count;if(amount<1 || amount>stack.Count)return false;
            for(var i=0;i<16;i++)
            {
                var angle=i*Mathf.PI/8;var point=(Vector2)transform.position+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*2;
                if(!World.Map.IsWalkable(point,.8f) || Physics2D.OverlapCircle(point,.8f,new ContactFilter2D{useTriggers=false},obstacles)>0)continue;
                var pickup=PrefabPool.Spawn(Prefab,point,Quaternion.identity);pickup.Initialize(stack.Item,amount,Inventory,Settings,true);
                if(source.ConsumeSlot(index,stack.Item,amount))return true;
                PrefabPool.Release(pickup.gameObject);return false;
            }
            return false;
        }
    }
}
