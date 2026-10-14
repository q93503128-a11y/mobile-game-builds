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
        public bool Drop(InventoryModel source,int index)
        {
            if(Session.State!=SessionState.Inventory || source==null)return false;
            var stack=source.GetSlot(index);if(stack.IsEmpty)return false;
            for(var i=0;i<16;i++)
            {
                var angle=i*Mathf.PI/8;var point=(Vector2)transform.position+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*2;
                if(!World.Map.IsWalkable(point,.8f) || Physics2D.OverlapCircle(point,.8f,new ContactFilter2D{useTriggers=false},obstacles)>0)continue;
                var pickup=Instantiate(Prefab,point,Quaternion.identity);pickup.Initialize(stack.Item,stack.Count,Inventory,Settings,true);
                if(source.ConsumeSlot(index,stack.Item,stack.Count))return true;
                Destroy(pickup.gameObject);return false;
            }
            return false;
        }
    }
}
