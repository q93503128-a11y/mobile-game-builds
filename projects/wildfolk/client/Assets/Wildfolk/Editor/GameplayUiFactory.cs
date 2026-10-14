using UnityEngine;
using UnityEngine.UI;

namespace Wildfolk.Editor
{
    internal static class GameplayUiFactory
    {
        internal static RectTransform Rect(string name,Transform parent,Vector2 anchor,Vector2 position,Vector2 size)
        {
            var rect=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent,false);
            rect.anchorMin=rect.anchorMax=anchor;
            rect.pivot=new Vector2(.5f,.5f);
            rect.anchoredPosition=position;
            rect.sizeDelta=size;
            return rect;
        }

        internal static Image Image(string name,Transform parent,string sprite,Vector2 anchor,Vector2 position,Vector2 size,bool raycast=false)
        {
            var image=Rect(name,parent,anchor,position,size).gameObject.AddComponent<Image>();
            image.sprite=VisualArtImporter.Sprite(sprite);
            image.type=sprite=="panel" ? UnityEngine.UI.Image.Type.Sliced : UnityEngine.UI.Image.Type.Simple;
            image.raycastTarget=raycast;
            return image;
        }

        internal static Text Text(string name,Transform parent,string value,Vector2 position,Vector2 size,int fontSize,TextAnchor alignment=TextAnchor.MiddleCenter)
        {
            var text=Rect(name,parent,new Vector2(.5f,.5f),position,size).gameObject.AddComponent<Text>();
            text.font=Resources.Load<Font>("NotoSansKR");
            text.text=L.Has(value) ? L.Text(value) : value;
            if(L.Has(value)){var localized=text.gameObject.AddComponent<LocalizedLabel>();localized.Key=value;}
            text.fontSize=fontSize;text.verticalOverflow=VerticalWrapMode.Overflow;text.lineSpacing=.92f;
            text.alignment=alignment;
            text.color=new Color32(226,222,193,255);
            text.raycastTarget=false;
            return text;
        }

        internal static Button Button(Image image)
        {
            image.raycastTarget=true;
            var button=image.gameObject.AddComponent<Button>();
            button.targetGraphic=image;
            button.navigation=new Navigation {mode=Navigation.Mode.None};
            var colors=button.colors;
            colors.highlightedColor=new Color(.95f,.98f,.86f,1);
            colors.pressedColor=new Color(.70f,.76f,.60f,1);
            button.colors=colors;
            return button;
        }

        internal static Image Scrim(string name,Transform parent)
        {
            var image=Image(name,parent,"white",new Vector2(.5f,.5f),Vector2.zero,Vector2.zero,true);
            var rect=(RectTransform)image.transform;
            rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
            image.color=new Color(.05f,.09f,.06f,.60f);
            return image;
        }

        internal static Image Slot(Transform parent,int index,float size,bool quick)
        {
            var slot=Image("Slot "+(index+1),parent,"panel",new Vector2(.5f,.5f),Vector2.zero,new Vector2(size,size),true);
            var view=slot.gameObject.AddComponent<InventorySlotView>();
            var selection=Image("Selection",slot.transform,"white",new Vector2(.5f,.5f),Vector2.zero,new Vector2(size+6,size+6));
            selection.color=new Color32(210,183,117,255);view.Selection=selection;
            var inset=Image("Inset",selection.transform,"white",new Vector2(.5f,.5f),Vector2.zero,new Vector2(size-2,size-2));inset.color=new Color32(24,40,33,255);selection.enabled=false;
            view.Icon=Image("Icon",slot.transform,"white",new Vector2(.5f,.5f),new Vector2(0,2),new Vector2(size*.68f,size*.72f));view.Icon.enabled=false;view.Icon.preserveAspect=true;
            view.Count=Text("Stack Count",slot.transform,string.Empty,new Vector2(size*.24f,-size*.31f),new Vector2(42,26),21,TextAnchor.MiddleRight);
            if(quick) Text("Quick Slot",slot.transform,(index+1).ToString(),new Vector2(-size*.31f,size*.32f),new Vector2(24,24),15);
            view.DropMarker=Image("Drop Target",slot.transform,"white",new Vector2(.5f,.5f),Vector2.zero,new Vector2(size,size));view.DropMarker.enabled=false;
            return slot;
        }
    }
}
