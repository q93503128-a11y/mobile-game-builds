using UnityEngine;

namespace Wildfolk
{
    public sealed class PlayerAvatar : MonoBehaviour
    {
        public CharacterFacing Facing;
        public Transform Body;
        public PlayerMotor Motor;
        public bool? MovingOverride {get;set;}
        public float MotionRate {get;set;}=1;
        private PlayerDeath death;
        private Vector3 restBody;
        private Vector3 restScale;

        private void Awake() { death=GetComponent<PlayerDeath>(); restBody = Body.localPosition; restScale = Body.localScale; }
        private void LateUpdate()
        {
            if(death!=null && death.State!=PlayerLife.Alive){Body.localPosition=restBody+Vector3.down*.12f;Body.localScale=new Vector3(restScale.x*1.05f,restScale.y*.48f,restScale.z);return;}
            var role=GetComponent<PlayerClass>();var net=GetComponent<NetworkClassActions>();var guard=net!=null && net.IsSpawned && !net.IsServer?net.State.Value.Guarding:role?.Guarding==true;
            if(guard && !Motor.Combat.IsAttacking && GetComponent<PlayerRanged>()?.IsBusy!=true){Facing.SetHandPose(0,new Vector2(.06f,.14f),-12);Facing.SetHandPose(1,new Vector2(-.08f,.24f),16);}
            var bob = (MovingOverride ?? Motor.IsMoving) ? Mathf.Sin(Time.time * 11 * MotionRate) : 0;
            Body.localPosition = restBody + Vector3.up * (bob * .022f);
            Body.localScale = new Vector3(restScale.x * (1 + bob * .012f), restScale.y * (1 - bob * .012f), restScale.z);
        }
    }
}
