using UnityEngine;
namespace Wildfolk
{
    public sealed class UndeadKingVisual:MonoBehaviour
    {
        public BossEncounter Encounter;
        public Transform Pose,SwordArm,OffArm,Sword,Legs;
        public SpriteRenderer Crown,Dust,Skull;
        public Sprite BrokenCrown,WholeCrown;
        public SpriteRenderer[] Parts;
        private float flashUntil;
        private void OnEnable(){Encounter.Health.Damaged+=Hit;}
        private void OnDisable(){Encounter.Health.Damaged-=Hit;}
        private void Hit()=>flashUntil=Time.time+.08f;
        private void LateUpdate()
        {
            var state=Encounter.State;var age=Encounter.Age;var attack=Encounter.Attack;
            var facing=Mathf.Atan2(Encounter.Direction.y,Encounter.Direction.x)*Mathf.Rad2Deg-90;
            var arm=facing-20;var poseY=1f;var alpha=1f;
            Pose.localPosition=Vector3.zero;Pose.localRotation=Quaternion.identity;OffArm.localRotation=Quaternion.identity;Legs.localRotation=Quaternion.identity;
            Sword.localPosition=new Vector3(0,.75f,0);
            if(state==BossState.Awakening)
            {
                var rise=Mathf.SmoothStep(0,1,Mathf.InverseLerp(1,2.9f,age));poseY=Mathf.Lerp(.18f,1,rise);Pose.localPosition=Vector3.down*(1-rise)*.8f;
                arm=Mathf.Lerp(-125,facing-20,Mathf.InverseLerp(.5f,2,age));alpha=Mathf.InverseLerp(.4f,1.1f,age);
            }
            else if(state==BossState.Windup && attack!=null)
            {
                var p=Mathf.SmoothStep(0,1,age/attack.Windup);
                var heavy=attack.Shape==BossAttackShape.Circle || attack.Shape==BossAttackShape.Line || attack.Id=="execution";
                arm=heavy?Mathf.Lerp(facing-20,12,p):facing-Mathf.Lerp(20,115,p);
                Pose.localRotation=Quaternion.Euler(0,0,(heavy?3:-7)*p);OffArm.localRotation=Quaternion.Euler(0,0,heavy?-48*p:12*p);
            }
            else if(state==BossState.Strike)arm=attack!=null && (attack.Shape==BossAttackShape.Circle)?facing-115+Mathf.Clamp01(age/.22f)*360:Mathf.Lerp(facing-115,facing+70,Mathf.Clamp01(age/.16f));
            else if(state==BossState.Ritual || state==BossState.Transition){arm=170;Pose.localPosition=Vector3.down*.1f;OffArm.localRotation=Quaternion.Euler(0,0,-30);}
            else if(state==BossState.Chase || state==BossState.Returning){Pose.localPosition=Vector3.up*(Mathf.Sin(Time.time*6)*.025f);Legs.localRotation=Quaternion.Euler(0,0,Mathf.Sin(Time.time*6)*3);}
            else if(state==BossState.Dying)
            {
                var collapse=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.12f,1.3f,age));poseY=1-collapse*.65f;
                Pose.localRotation=Quaternion.Euler(0,0,-collapse*14);Pose.localPosition=Vector3.down*collapse*.45f;
                arm=Mathf.Lerp(facing,100,Mathf.InverseLerp(.2f,.7f,age));Sword.localPosition=new Vector3(.3f,-.7f,0);
                alpha=1-Mathf.InverseLerp(1.1f,2.2f,age);
            }
            Pose.localScale=new Vector3(1,poseY,1);SwordArm.localRotation=Quaternion.Euler(0,0,arm);
            Crown.sprite=Encounter.Phase>=3?BrokenCrown:WholeCrown;
            Crown.transform.localRotation=Quaternion.Euler(0,0,Encounter.Phase>=3?-17:0);
            foreach(var part in Parts)part.color=new Color(1,Time.time<flashUntil ? .8f : 1,Time.time<flashUntil ? .65f : 1,alpha);
            if(state==BossState.Dying){Crown.color=new Color(1,1,1,1-Mathf.InverseLerp(1.9f,2.45f,age));Crown.transform.localPosition=new Vector3(0,1.5f/Mathf.Max(.3f,poseY),0);}
            else Crown.transform.localPosition=new Vector3(0,1.5f,0);
            Dust.enabled=state==BossState.Dying && age>1.9f;Dust.color=new Color(1,1,1,state==BossState.Dying?1-Mathf.InverseLerp(2.1f,2.7f,age):0);
            Skull.enabled=Encounter.Marked!=null;if(Encounter.Marked!=null)Skull.transform.position=Encounter.Marked.transform.position+Vector3.up*1.25f;
        }
    }
}
