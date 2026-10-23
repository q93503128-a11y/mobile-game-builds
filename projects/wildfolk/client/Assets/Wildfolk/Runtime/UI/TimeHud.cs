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
        private bool dirty=true,initialized;private float emphasis;private Color normal;
        private void OnEnable(){L.Changed+=Dirty;}
        private void OnDisable()=>L.Changed-=Dirty;
        private void Dirty()=>dirty=true;
        private void Update()
        {
            if(Clock==null || Clock.Model==null)return;
            if(!initialized){normal=Label.color;initialized=true;}if(emphasis>0){var t=Mathf.Clamp01((emphasis-Time.unscaledTime)/1.6f);Label.color=Color.Lerp(normal,new Color(1,.88f,.58f,normal.a),t);if(t<=0)emphasis=0;}
            var time=Clock.Model;
            if(!dirty && time.Day==previousDay && time.Phase==previousPhase)return;dirty=false;
            if(previousDay>0 && (time.Day!=previousDay || time.Phase!=previousPhase))emphasis=Time.unscaledTime+1.6f;
            previousDay=time.Day;previousPhase=time.Phase;
            Label.text=L.Format("ui.hud.day",time.Day,L.Text("ui.phase."+time.Phase));
            Icon.sprite=time.Phase==DayPhase.Night || time.Phase==DayPhase.Dawn ? Clock.Definition.MoonIcon : Clock.Definition.SunIcon;
        }
    }
}
