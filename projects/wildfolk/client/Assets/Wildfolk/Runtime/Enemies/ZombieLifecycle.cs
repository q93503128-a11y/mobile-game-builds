using UnityEngine;
namespace Wildfolk
{
    public enum ZombiePhase { Emerging, Active, Burrowing }
    public sealed class ZombieLifecycle : MonoBehaviour,IPoolReset
    {
        public WorldClock Clock;
        public ZombieSpawnDefinition Definition;
        public EnemyController Controller;
        public Transform Visual;
        public Collider2D Hitbox;
        public ZombiePhase Phase { get; private set; }
        private float elapsed;
        private Vector3 restPosition,restScale;
        private void Awake(){restPosition=Visual.localPosition;restScale=Visual.localScale;Phase=ZombiePhase.Emerging;Hitbox.enabled=false;SetPose(0);}
        public void ResetForSpawn(){Phase=ZombiePhase.Emerging;elapsed=0;Hitbox.enabled=false;SetPose(0);}
        private void Update()
        {
            if(!Controller.Session.WorldIsRunning)return;
            if(Clock.Model.Phase!=DayPhase.Night && Phase!=ZombiePhase.Burrowing)Burrow();
            if(Phase==ZombiePhase.Active)return;
            elapsed+=Time.deltaTime;var seconds=Phase==ZombiePhase.Emerging ? Definition.EmergeSeconds : Definition.BurrowSeconds;
            var t=Mathf.Clamp01(elapsed/seconds);SetPose(Phase==ZombiePhase.Emerging ? t : 1-t);
            if(t<1)return;
            if(Phase==ZombiePhase.Burrowing)PrefabPool.Release(gameObject);
            else {Phase=ZombiePhase.Active;Hitbox.enabled=true;}
        }
        private void SetPose(float t){Visual.localPosition=restPosition+Vector3.down*((1-t)*.7f);Visual.localScale=new Vector3(restScale.x*(.45f+.55f*t),restScale.y*t,1);}
        public void Burrow(){Phase=ZombiePhase.Burrowing;elapsed=0;Hitbox.enabled=false;Controller.CancelPursuit();}
    }
}
