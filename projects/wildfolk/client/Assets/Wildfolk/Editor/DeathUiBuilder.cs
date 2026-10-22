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
            var scrim=Scrim("Revive Choices",modal);hud.DeadView=scrim.gameObject;
            var panel=Image("Recovery",scrim.transform,"panel",new Vector2(.5f,.5f),Vector2.zero,new Vector2(760,620),true);
            hud.Title=Text("Title",panel.transform,"death.title",new Vector2(0,240),new Vector2(670,60),34);
            hud.Message=Text("Choices",panel.transform,"death.choices",new Vector2(0,115),new Vector2(650,150),23);
            hud.Ad=Action(panel.transform,"Rewarded","death.ad",new Vector2(0,-10),out _);
            hud.Gem=Action(panel.transform,"Premium Gem","death.gem",new Vector2(0,-105),out var label);hud.GemLabel=label;
            hud.Home=Action(panel.transform,"Free Home","death.home",new Vector2(0,-215),out _);
            var banner=Image("Rescue Status",safe,"panel",new Vector2(.5f,1),new Vector2(0,-200),new Vector2(650,140));hud.DownView=banner.gameObject;
            hud.DownLabel=Text("Status",banner.transform,"",Vector2.zero,new Vector2(610,116),26);hud.DownLabel.raycastTarget=false;
            scrim.gameObject.SetActive(false);banner.gameObject.SetActive(false);return hud;
        }
        private static Button Action(Transform p,string name,string key,Vector2 at,out Text label)
        {var image=Image(name,p,"panel",new Vector2(.5f,.5f),at,new Vector2(610,78),true);label=Text("Label",image.transform,key,Vector2.zero,new Vector2(580,66),27);return Button(image);}
    }
}
