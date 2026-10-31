using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using static Wildfolk.Editor.GameplayUiFactory;
namespace Wildfolk.Editor
{
    internal static class StackSplitUiBuilder
    {
        internal static void Build(Transform safe,InventoryDragController drag)
        {
            var panel=drag.gameObject.AddComponent<StackSplitPanel>();panel.Drag=drag;drag.SplitPanel=panel;
            var actions=Scrim("Item Actions",safe);panel.Actions=actions.gameObject;
            var actionSurface=Image("Item Actions Panel",actions.transform,"panel",new Vector2(.5f,.5f),Vector2.zero,new Vector2(550,310),true);
            Fit(actionSurface,safe,new Vector2(550,310));panel.ActionIcon=Image("Item",actionSurface.transform,"white",new Vector2(.5f,.5f),new Vector2(-180,80),new Vector2(72,72));panel.ActionIcon.preserveAspect=true;
            panel.ActionTitle=Text("Item Name",actionSurface.transform,string.Empty,new Vector2(25,80),new Vector2(310,48),29,TextAnchor.MiddleLeft);
            panel.ActionSplitButton=Control(actionSurface.transform,"ui.action.split",new Vector2(0,-15),new Vector2(340,68),panel.ChooseSplit);
            Control(actionSurface.transform,"ui.action.cancel",new Vector2(0,-100),new Vector2(340,60),panel.Cancel);actions.gameObject.SetActive(false);
            var overlay=Scrim("Split Stack",safe);panel.View=overlay.gameObject;
            var surface=Image("Split Panel",overlay.transform,"panel",new Vector2(.5f,.5f),Vector2.zero,new Vector2(640,420),true);Fit(surface,safe,new Vector2(640,420));
            panel.Icon=Image("Item",surface.transform,"white",new Vector2(.5f,.5f),new Vector2(-231,141),new Vector2(72,72));panel.Icon.preserveAspect=true;
            panel.Title=Text("Item Name",surface.transform,string.Empty,new Vector2(30,141),new Vector2(408,48),29,TextAnchor.MiddleLeft);
            Text("Instruction",surface.transform,"ui.split.instruction",new Vector2(0,79),new Vector2(560,42),24);
            panel.Quantity=Text("Quantity",surface.transform,string.Empty,new Vector2(0,8),new Vector2(280,62),34);
            panel.Minus=Control(surface.transform,"−1",new Vector2(-224,8),new Vector2(110,70),panel.Decrease);
            panel.Plus=Control(surface.transform,"+1",new Vector2(224,8),new Vector2(110,70),panel.Increase);
            panel.HalfButton=Control(surface.transform,"ui.split.half",new Vector2(0,-73),new Vector2(180,60),panel.Half);
            panel.CancelButton=Control(surface.transform,"ui.action.cancel",new Vector2(-144,-157),new Vector2(220,68),panel.Cancel);
            panel.ConfirmButton=Control(surface.transform,"ui.split.confirm",new Vector2(144,-157),new Vector2(220,68),panel.Confirm);overlay.gameObject.SetActive(false);
            var cancel=Control(safe,"ui.action.cancel",new Vector2(250,-128),new Vector2(150,88),drag.Cancel);var rect=(RectTransform)cancel.transform;rect.anchorMin=rect.anchorMax=new Vector2(.5f,1);drag.HeldCancel=cancel.gameObject;drag.HeldCancel.SetActive(false);Arrange(panel);
        }
        internal static void Arrange(StackSplitPanel panel)
        {
            var actions=panel.ActionTitle.transform.parent;((RectTransform)actions).sizeDelta=new Vector2(1000,620);actions.GetComponent<ModalLayout>().ReferenceSize=new Vector2(1000,620);
            InventoryUiBuilder.Caption(panel.ActionTitle,new Vector2(90,180),new Vector2(650,100),38);InventoryUiBuilder.Position(panel.ActionIcon,new Vector2(-350,180),new Vector2(110,110));
            InventoryUiBuilder.ActionSize(panel.ActionSplitButton,new Vector2(0,-10),new Vector2(760,120));
            foreach(var button in actions.GetComponentsInChildren<Button>(true))if(button!=panel.ActionSplitButton)InventoryUiBuilder.ActionSize(button,new Vector2(0,-170),new Vector2(760,120));
            var surface=panel.Title.transform.parent;((RectTransform)surface).sizeDelta=new Vector2(1000,820);surface.GetComponent<ModalLayout>().ReferenceSize=new Vector2(1000,820);
            InventoryUiBuilder.Caption(panel.Title,new Vector2(70,290),new Vector2(700,100),38);InventoryUiBuilder.Position(panel.Icon,new Vector2(-365,290),new Vector2(100,110));
            InventoryUiBuilder.Caption(surface.Find("Instruction").GetComponent<Text>(),new Vector2(0,180),new Vector2(880,100),30);InventoryUiBuilder.Caption(panel.Quantity,new Vector2(0,25),new Vector2(350,120),44);
            InventoryUiBuilder.ActionSize(panel.Minus,new Vector2(-350,25),new Vector2(220,120));InventoryUiBuilder.ActionSize(panel.Plus,new Vector2(350,25),new Vector2(220,120));InventoryUiBuilder.ActionSize(panel.HalfButton,new Vector2(0,-130),new Vector2(400,120));
            InventoryUiBuilder.ActionSize(panel.CancelButton,new Vector2(-240,-295),new Vector2(420,120));InventoryUiBuilder.ActionSize(panel.ConfirmButton,new Vector2(240,-295),new Vector2(420,120));
        }
        private static void Fit(Image image,Transform safe,Vector2 size){var fit=image.gameObject.AddComponent<ModalLayout>();fit.SafeArea=(RectTransform)safe;fit.ReferenceSize=size;}
        private static Button Control(Transform parent,string label,Vector2 position,Vector2 size,UnityEngine.Events.UnityAction action)
        {var surface=Image(label,parent,"panel",new Vector2(.5f,.5f),position,size,true);Text("Label",surface.transform,label,Vector2.zero,size-new Vector2(10,10),25);var button=Button(surface);UnityEventTools.AddPersistentListener(button.onClick,action);return button;}
    }
}
