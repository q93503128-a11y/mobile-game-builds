using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    public sealed class WildlifeActionScheduler : MonoBehaviour
    {
        private static readonly List<WildlifeActionBrain> actors=new List<WildlifeActionBrain>();
        private static WildlifeActionScheduler current;
        private float next;private int cursor;
        internal static void Register(WildlifeActionBrain brain)
        {if(!actors.Contains(brain))actors.Add(brain);if(current==null){var go=new GameObject("Wildlife actions");current=go.AddComponent<WildlifeActionScheduler>();}}
        internal static void Unregister(WildlifeActionBrain brain)=>actors.Remove(brain);
        private void Update()
        {if(Time.time<next)return;next=Time.time+.05f;var count=Mathf.Min(32,actors.Count);for(var i=0;i<count && actors.Count>0;i++){cursor%=actors.Count;var actor=actors[cursor++];if(actor!=null)actor.Tick();}}
        private void OnDestroy(){if(current==this){actors.Clear();current=null;}}
    }
}
