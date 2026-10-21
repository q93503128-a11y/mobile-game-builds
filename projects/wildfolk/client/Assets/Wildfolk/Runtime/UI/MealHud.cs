using UnityEngine;
using UnityEngine.UI;
namespace Wildfolk
{
    public sealed class MealHud : MonoBehaviour
    {
        public PlayerSurvival Player;
        public Text Label;
        public Image Icon;
        private ItemDefinition previousItem;
        private int previousSeconds=-1;
        private bool previousVisible;
        private float next;
        private void OnEnable()=>L.Changed+=Language;
        private void OnDisable()=>L.Changed-=Language;
        private void Language(){previousSeconds=-1;next=0;}
        private void Update()
        {
            if(Time.unscaledTime<next)return;next=Time.unscaledTime+.2f;
            var item=Player==null ? null : Player.ActiveMealItem;var seconds=Player==null ? 0 : Mathf.CeilToInt(Player.MealRemaining);
            var active=item!=null && seconds>0 && Player.Session.IsRunning;
            if(item==previousItem && seconds==previousSeconds && active==previousVisible)return;previousItem=item;previousSeconds=seconds;previousVisible=active;
            Label.enabled=Icon.enabled=active;if(!active)return;
            Icon.sprite=item.Icon;Label.text=L.Format("ui.meal.active",item.LocalizedName,seconds/60,(seconds%60).ToString("00"));
        }
    }
}
