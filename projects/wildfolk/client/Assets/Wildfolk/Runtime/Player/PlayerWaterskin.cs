using UnityEngine;
namespace Wildfolk
{
    public sealed class PlayerWaterskin : MonoBehaviour
    {
        private float readyAt;
        public bool Fill()
        {
            if(!SimulationAuthority.Allows(this) || Time.time<readyAt)return false;
            var bag=GetComponent<PlayerInventory>();var s=bag.SelectedStack;
            if(s.IsEmpty || s.Item.WaterCapacity==0 || s.Charges==s.Item.WaterCapacity)return false;
            var p=(Vector2)transform.position;var world=FindFirstObjectByType<WorldGenerator>();if(world==null)return false;
            var found=false;for(var i=0;i<16;i++){var a=i*Mathf.PI/8;var at=p+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*2.5f;if(world.Map.Contains(at) && world.Map.IsWater(at)){found=true;break;}}
            if(!found)return false;
            return bag.Model.SetWater(bag.SelectedIndex,s.Item,s.Item.WaterCapacity,LiquidState.Raw);
        }
        public bool Drink()
        {
            if(!SimulationAuthority.Allows(this) || Time.time<readyAt)return false;
            var bag=GetComponent<PlayerInventory>();var s=bag.SelectedStack;
            if(s.IsEmpty || s.Item.WaterCapacity==0 || s.Charges<=0)return false;
            var survival=GetComponent<PlayerSurvival>();var food=s.Quality==LiquidState.Clean ? s.Item.CleanDrink : s.Item.RawDrink;
            if(!survival.Model.CanRestore(food))return false;
            if(!bag.Model.SetWater(bag.SelectedIndex,s.Item,s.Charges-1,s.Charges==1 ? LiquidState.Empty : s.Quality))return false;
            readyAt=Time.time+.5f;survival.Model.Restore(food);PlayerActionAudio.Committed(this,ConfirmedPlayerAction.Drink);return true;
        }
    }
}
