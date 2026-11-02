using UnityEngine;
using UnityEngine.UI;
using static Wildfolk.Editor.GameplayUiFactory;
namespace Wildfolk.Editor
{
    internal static class MaintenanceUiBuilder
    {
        internal static void Arrange(ClassHud hud)
        {
            var root=(RectTransform)hud.BuildingView.transform;root.sizeDelta=new Vector2(840,360);root.anchorMin=root.anchorMax=Vector2.zero;root.anchoredPosition=new Vector2(440,430);
            var backing=root.Find("Context Surface");if(backing==null){var image=Image("Context Surface",root,"white",Vector2.one*.5f,Vector2.zero,root.sizeDelta);image.color=JournalStyle.Field;image.transform.SetAsFirstSibling();}
            QualityUiBuilder.FieldSurface(root.Find("Context Surface").GetComponent<Image>());
            InventoryUiBuilder.Caption(hud.BuildingText,new Vector2(0,130),new Vector2(790,50),32);hud.BuildingText.alignment=TextAnchor.MiddleLeft;
            if(hud.BuildingMaterials==null)hud.BuildingMaterials=Text("Materials and recovery",root,"",new Vector2(0,42),new Vector2(790,112),28,TextAnchor.UpperLeft);
            hud.BuildingMaterials.verticalOverflow=VerticalWrapMode.Truncate;
            var viewport=root.Find("Materials viewport") as RectTransform;
            if(viewport==null)
            {
                viewport=Rect("Materials viewport",root,Vector2.one*.5f,new Vector2(0,42),new Vector2(790,112));viewport.gameObject.AddComponent<Image>().color=Color.clear;viewport.gameObject.AddComponent<RectMask2D>();
                var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.viewport=viewport;scroll.scrollSensitivity=28;
                hud.BuildingMaterials.rectTransform.SetParent(viewport,false);var text=hud.BuildingMaterials.rectTransform;text.anchorMin=text.anchorMax=new Vector2(.5f,1);text.pivot=new Vector2(.5f,1);text.anchoredPosition=Vector2.zero;text.sizeDelta=new Vector2(770,112);
                hud.BuildingMaterials.gameObject.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;scroll.content=text;
                var track=Image("Material scrollbar",root,"white",Vector2.one*.5f,new Vector2(393,42),new Vector2(6,112),true);track.color=JournalStyle.FieldEdge;var bar=track.gameObject.AddComponent<Scrollbar>();bar.direction=Scrollbar.Direction.BottomToTop;
                var handle=Image("Handle",track.transform,"white",Vector2.one*.5f,Vector2.zero,Vector2.zero,true);handle.color=JournalStyle.Learned;bar.handleRect=handle.rectTransform;bar.targetGraphic=handle;scroll.verticalScrollbar=bar;scroll.verticalScrollbarVisibility=ScrollRect.ScrollbarVisibility.AutoHide;
            }
            InventoryUiBuilder.ActionSize(hud.RepairButton,new Vector2(-200,-65),new Vector2(360,102));InventoryUiBuilder.ActionSize(hud.DismantleButton,new Vector2(200,-65),new Vector2(360,102));
            hud.RepairButton.image.sprite=VisualArtImporter.Sprite("white");hud.DismantleButton.image.sprite=VisualArtImporter.Sprite("white");JournalStyle.FieldAction(hud.RepairButton,true);JournalStyle.FieldAction(hud.DismantleButton);
            InventoryUiBuilder.Caption(hud.ServiceProgress,new Vector2(0,-140),new Vector2(790,42),26);
            if(hud.HoldProgress==null){var track=Image("Hold track",root,"white",Vector2.one*.5f,new Vector2(0,-174),new Vector2(790,6));track.color=JournalStyle.FieldEdge;hud.HoldProgress=Image("Hold fill",track.transform,"white",Vector2.one*.5f,Vector2.zero,new Vector2(790,6));hud.HoldProgress.type=UnityEngine.UI.Image.Type.Filled;hud.HoldProgress.fillMethod=UnityEngine.UI.Image.FillMethod.Horizontal;hud.HoldProgress.color=new Color32(209,168,107,255);hud.HoldProgress.fillAmount=0;}
        }
    }
}
