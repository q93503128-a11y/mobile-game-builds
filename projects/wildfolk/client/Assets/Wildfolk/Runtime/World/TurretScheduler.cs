using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    // Bounded physics queries are staggered; sleeping turrets are absent from this list.
    public sealed class TurretScheduler : MonoBehaviour
    {
        private static TurretScheduler current;
        private readonly List<WorldTurret> turrets=new List<WorldTurret>();
        private readonly Dictionary<WorldTurret,float> sampled=new Dictionary<WorldTurret,float>();
        private int cursor;private float next;
        public static void Register(WorldTurret t){if(!Application.isPlaying)return;if(current==null)current=new GameObject("Turret simulation").AddComponent<TurretScheduler>();if(!current.turrets.Contains(t)){current.turrets.Add(t);current.sampled[t]=Time.time;}current.enabled=true;}
        public static void Unregister(WorldTurret t){if(current==null)return;current.turrets.Remove(t);current.sampled.Remove(t);current.enabled=current.turrets.Count>0;}
        private void Update(){if(Time.time<next)return;next=Time.time+.05f;var count=Mathf.Min(16,turrets.Count);for(var i=0;i<count && turrets.Count>0;i++){cursor%=turrets.Count;var t=turrets[cursor++];if(t==null)continue;var elapsed=Time.time-sampled[t];sampled[t]=Time.time;t.Tick(elapsed);}}
        private void OnDestroy(){if(current==this)current=null;}
    }
}
