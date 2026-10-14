using UnityEngine;
using UnityEngine.UI;
namespace Wildfolk
{
    public sealed class TimeHud : MonoBehaviour
    {
        public WorldClock Clock;
        public Image Icon;
        public Text Label;
        private int previousDay;
        private DayPhase previousPhase;
        private bool dirty=true;
        private void OnEnable(){L.Changed+=Dirty;}
        private void OnDisable()=>L.Changed-=Dirty;
        private void Dirty()=>dirty=true;
        private void Update()
        {
            var time=Clock.Model;
            if(!dirty && time.Day==previousDay && time.Phase==previousPhase)return;dirty=false;
            previousDay=time.Day;previousPhase=time.Phase;
            Label.text=L.Format("ui.hud.day",time.Day,L.Text("ui.phase."+time.Phase));
            Icon.sprite=time.Phase==DayPhase.Night || time.Phase==DayPhase.Dawn ? Clock.Definition.MoonIcon : Clock.Definition.SunIcon;
        }
    }
}
