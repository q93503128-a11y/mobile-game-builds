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
            text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text=value;
            text.fontSize=fontSize;
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
    }
}
