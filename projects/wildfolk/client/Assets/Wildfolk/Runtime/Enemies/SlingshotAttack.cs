using UnityEngine;
namespace Wildfolk
{
    [DefaultExecutionOrder(150)]
    public sealed class SlingshotAttack : MonoBehaviour,IPoolReset
    {
        public SlingshotDefinition Definition;
        public CharacterFacing Facing;
        public Transform Frame,Pose;
        public SpriteRenderer PouchStone;
        public LineRenderer Band;
        public StoneProjectile Projectile;
        public Health Owner,Target;
        public bool IsBusy {get;private set;}
        public bool Released {get;private set;}
        public float Elapsed {get;private set;}
        public int Shots {get;private set;}
        public Vector2 Direction {get;private set;}
        private Vector3 restScale;
        private void Awake()=>restScale=Pose.localScale;
        public void ResetForSpawn(){Cancel();Shots=0;}
        public bool Begin(Health target)
        {if(IsBusy || target==null || !target.IsAlive)return false;Target=target;Direction=((Vector2)target.transform.position-(Vector2)transform.position).normalized;Facing.Face(Direction);IsBusy=true;Released=false;Elapsed=0;return true;}
        public void Cancel(){IsBusy=false;Released=false;Elapsed=0;Facing.ResetPose();if(Pose!=null)Pose.localScale=restScale;PouchStone.enabled=false;Band.enabled=false;}
        private void Update()
        {
            if(!IsBusy)return;Elapsed+=Time.deltaTime;ApplyHands(Elapsed);
            if(!Owner.IsAlive || Target==null || !Target.IsAlive){Cancel();return;}
            if(!Released && Elapsed>=Definition.LaunchAt)
            {
                Released=true;Shots++;
                var stone=PrefabPool.Spawn(Projectile,Frame.TransformPoint(new Vector3(0,.37f,0)),Quaternion.identity);stone.Launch(Owner,Target,Direction,Definition);
            }
            if(Elapsed>=Definition.Duration)Cancel();
        }
        public float DrawAmount(float time)
        {
            if(time<Definition.Ready)return 0;
            if(time<Definition.LaunchAt)return Mathf.SmoothStep(0,1,Mathf.Clamp01((time-Definition.Ready)/Definition.Draw));
            return 1-Mathf.SmoothStep(0,1,Mathf.Clamp01((time-Definition.LaunchAt)/Definition.Release));
        }
        private void ApplyHands(float time)
        {
            var draw=DrawAmount(time);var handDraw=time<Definition.LaunchAt+Definition.Release ? time>=Definition.LaunchAt ? 1 : draw : 1-Mathf.SmoothStep(0,1,(time-Definition.LaunchAt-Definition.Release)/Definition.Recovery);
            var handScale=Facing.RightHand.localScale.y;
            var forkForward=.12f+Frame.localPosition.y*handScale+.37f*Frame.localScale.y*handScale;
            var ready=Mathf.SmoothStep(0,1,Mathf.Clamp01(time/Definition.Ready));
            var recovery=time<=Definition.LaunchAt+Definition.Release ? 1 : 1-Mathf.SmoothStep(0,1,Mathf.Clamp01((time-Definition.LaunchAt-Definition.Release)/Definition.Recovery));
            var poseBlend=ready*recovery;
            Facing.Face(Direction);Facing.SetHandPose(1,new Vector2(-.28f,.12f)*poseBlend);Facing.SetHandPose(0,new Vector2(Facing.HandSeparation*2-.28f,forkForward-handDraw*.88f)*poseBlend);
            Pose.localScale=new Vector3(restScale.x*(1-.018f*handDraw),restScale.y*(1+.018f*handDraw),restScale.z);
        }
        private void DrawBand(float time)
        {
            var draw=DrawAmount(time);
            var a=Frame.TransformPoint(new Vector3(-.18f,.37f,0));var b=Frame.TransformPoint(new Vector3(.18f,.37f,0));
            var pouch=(a+b)*.5f-(Vector3)Direction*draw*.88f;
            Band.enabled=true;Band.SetPosition(0,a);Band.SetPosition(1,pouch);Band.SetPosition(2,b);PouchStone.transform.position=pouch;PouchStone.enabled=time<Definition.LaunchAt;
        }
        private void LateUpdate(){if(IsBusy)DrawBand(Elapsed);}
    }
}
