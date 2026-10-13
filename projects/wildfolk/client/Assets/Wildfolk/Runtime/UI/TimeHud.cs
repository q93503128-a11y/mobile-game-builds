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
        private void Update()
        {
            var time=Clock.Model;
            if(time.Day==previousDay && time.Phase==previousPhase)return;
            previousDay=time.Day;previousPhase=time.Phase;
            Label.text="Day "+time.Day+" · "+time.Phase;
            Icon.sprite=time.Phase==DayPhase.Night || time.Phase==DayPhase.Dawn ? Clock.Definition.MoonIcon : Clock.Definition.SunIcon;
        }
    }
}
