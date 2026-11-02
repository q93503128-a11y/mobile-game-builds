#if UNITY_EDITOR
using System;
using UnityEngine;
namespace Wildfolk
{
    // Editor assembly only. Nothing changes GameplaySettings, class modifiers, or Save14.
    [DefaultExecutionOrder(1000)]
    public sealed class DeveloperMovement:MonoBehaviour
    {
        public int Multiplier {get;private set;}=1;
        private PlayerMotor motor;private Rigidbody2D body;private Collider2D footprint;
        public void Configure(int multiplier)
        {if(multiplier!=1&&multiplier!=4&&multiplier!=10&&multiplier!=25)throw new ArgumentOutOfRangeException(nameof(multiplier));Multiplier=multiplier;}
        private void Awake(){motor=GetComponent<PlayerMotor>();body=GetComponent<Rigidbody2D>();footprint=GetComponent<Collider2D>();}
        private void FixedUpdate()
        {
            if(Multiplier==1||!motor.enabled||!motor.Session.IsRunning||!GetComponent<Health>().IsAlive||motor.Input.IsScouting)return;
            // Scale only this tick's resolved walking velocity. Dash/stamina/stat data remain canonical.
            if(motor.Locomotion.IsDashing)return;
            var velocity=body.linearVelocity*Multiplier;var distance=velocity.magnitude*Time.fixedDeltaTime;if(distance<=0)return;
            var direction=velocity.normalized;var allowed=distance;var nav=motor.Terrain;
            if(nav!=null){for(var d=.3f;d<distance;d+=.3f)if(!nav.Traversable(body.position+direction*d,.3f)){allowed=Mathf.Max(0,d-.3f);break;}allowed=Mathf.Min(allowed,nav.AllowedDistance(body.position,direction,allowed,.3f,footprint));}
            body.linearVelocity=direction*(allowed/Time.fixedDeltaTime);
        }
        private void OnDisable(){Multiplier=1;if(body!=null)body.linearVelocity=Vector2.zero;}
        private void OnGUI(){if(Multiplier>1){GUI.color=new Color(1,.8f,.35f);GUI.Box(new Rect(Screen.width*.5f-155,8,310,30),"EDITOR QA · movement "+Multiplier+"× · not saved");GUI.color=Color.white;}}
    }
}

#endif
