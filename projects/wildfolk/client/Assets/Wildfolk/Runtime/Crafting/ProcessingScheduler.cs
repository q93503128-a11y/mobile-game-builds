using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    // One scene-local clock for running authoritative jobs; idle/blocked facilities have no callback.
    public sealed class ProcessingScheduler : MonoBehaviour
    {
        private static ProcessingScheduler current;
        private readonly Dictionary<StationProcessing,float> jobs=new Dictionary<StationProcessing,float>();
        private readonly List<StationProcessing> due=new List<StationProcessing>();
        private float nextTick;
        public static void Schedule(StationProcessing job)
        {
            if(!Application.isPlaying || !job.NeedsTime){Remove(job);return;}
            if(current==null)current=new GameObject("Facility processing").AddComponent<ProcessingScheduler>();
            if(!current.jobs.ContainsKey(job))current.jobs.Add(job,Time.time);
            current.enabled=true;
        }
        public static void Remove(StationProcessing job){if(current==null)return;current.jobs.Remove(job);if(current.jobs.Count==0)current.enabled=false;}
        private void Update()
        {
            var now=Time.time;if(now<nextTick)return;nextTick=now+.1f;
            due.Clear();foreach(var pair in jobs)due.Add(pair.Key);
            foreach(var job in due)
            {
                if(job==null || !job.NeedsTime){Remove(job);continue;}
                if(!jobs.TryGetValue(job,out var last))continue;
                jobs[job]=now;job.Advance(now-last);if(!job.NeedsTime)Remove(job);
            }
        }
        private void OnDestroy(){if(current==this)current=null;}
    }
}
