using UnityEngine;
using UnityEngine.UI;
using static Wildfolk.Editor.GameplayUiFactory;
namespace Wildfolk.Editor
{
    internal static class DeathUiBuilder
    {
        internal static DeathHud Build(GameObject root,Transform modal,Transform safe,PlayerDeath player)
        {
            var hud=root.GetComponent<DeathHud>()??root.AddComponent<DeathHud>();hud.Player=player;
            var panel=JournalUiBuilder.Sheet(modal,"Revive Choices",out hud.DeadView);
            hud.Title=JournalUiBuilder.Label(panel,"Title","death.title",new Vector2(0,365),new Vector2(1400,85),46,TextAnchor.MiddleCenter);
            hud.Message=JournalUiBuilder.Label(panel,"Choices","death.choices",new Vector2(0,150),new Vector2(1400,260),36,TextAnchor.MiddleCenter);
            hud.Ad=Action(panel,"Rewarded","death.ad",new Vector2(-390,-100),out _);
            hud.Gem=Action(panel,"Premium Gem","death.gem",new Vector2(390,-100),out var label);hud.GemLabel=label;
            hud.Home=JournalUiBuilder.Action(panel,"Free Home","death.home",new Vector2(0,-320),new Vector2(1050,132),null,out _,true);
            var banner=Image("Rescue Status",safe,"panel",new Vector2(.5f,1),new Vector2(0,-200),new Vector2(650,140));hud.DownView=banner.gameObject;
            hud.DownLabel=Text("Status",banner.transform,"",Vector2.zero,new Vector2(610,116),26);hud.DownLabel.raycastTarget=false;
            hud.DeadView.SetActive(false);banner.gameObject.SetActive(false);return hud;
        }
        private static Button Action(Transform p,string name,string key,Vector2 at,out Text label)
        {return JournalUiBuilder.Action(p,name,key,at,new Vector2(720,140),null,out label);}
    }
}
