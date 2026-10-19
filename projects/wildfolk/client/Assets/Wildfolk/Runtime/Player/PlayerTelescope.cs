using UnityEngine;
namespace Wildfolk
{
    public sealed class PlayerTelescope : MonoBehaviour
    {
        public bool IsViewing {get;private set;}
        public Vector2 Direction {get;private set;}=Vector2.up;
        private PlayerInventory bag;private PlayerInputCommands commands;private Health health;private WorldGenerator world;
        public Vector2 ViewPoint=> (Vector2)transform.position+Direction*(world==null ? 0 : world.Definition.ChunkSize*2.5f);
        private void Awake(){bag=GetComponent<PlayerInventory>();commands=GetComponent<PlayerInputCommands>();health=GetComponent<Health>();}
        private void OnEnable()=>health.Damaged+=Exit;
        private void OnDisable(){health.Damaged-=Exit;Exit();}
        public bool Use()
        {
            if(IsViewing){Exit();return true;}
            if(!health.IsAlive || !commands.Session.IsRunning || bag.SelectedStack.IsEmpty || !bag.SelectedStack.Item.Telescope)return false;
            if(world==null)world=FindFirstObjectByType<WorldGenerator>();if(world==null || !world.Map.Streamed)return false;
            GetComponent<PlayerCombat>().CancelAttack();GetComponent<PlayerLocomotion>().CancelDash();Direction=GetComponent<PlayerAvatar>().Facing.Direction;IsViewing=true;GetComponent<NetworkTierThreePlayer>()?.Scope(true);commands.SetMove(Vector2.zero);return true;
        }
        public void Aim(Vector2 direction){if(IsViewing && direction.sqrMagnitude>.05f)Direction=direction.normalized;}
        public void SetViewing(bool value){IsViewing=value;if(!value)commands.ApplyMove(Vector2.zero);}
        public void Exit(){if(IsViewing)GetComponent<NetworkTierThreePlayer>()?.Scope(false);IsViewing=false;}
        private void LateUpdate()
        {
            if(!IsViewing)return;
            var network=GetComponent<NetworkPlayer>();if(network!=null && !network.IsOwner)return;
            if(!commands.Session.IsRunning || bag.SelectedStack.IsEmpty || !bag.SelectedStack.Item.Telescope){Exit();return;}
            var point=ViewPoint;if(!world.Map.RegionBounds(transform.position).Contains(point)){Exit();return;}world.Chunks.RequestScoutPresentation(point);
        }
    }
}
