using UnityEngine;
using UnityEngine.UI;
using Wildfolk.Presentation;

namespace Wildfolk.Editor
{
    internal static class VisualHudBuilder
    {
        internal static void Build()
        {
            var root = new GameObject("Mobile HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = .5f;
            var safe = Rect("Safe Area", root.transform, Vector2.zero, Vector2.zero, Vector2.zero);
            safe.anchorMax = Vector2.one;
            safe.offsetMin = safe.offsetMax = Vector2.zero;
            safe.gameObject.AddComponent<SafeAreaLayout>();

            Resource(safe, "Wood", "icon_wood", "24", 44);
            Resource(safe, "Stone", "icon_stone", "18", 202);
            Resource(safe, "Food", "icon_food", "06", 360);

            var inventory = Image("Inventory", safe, "panel", new Vector2(1, 1), new Vector2(-90, -82), new Vector2(86, 86));
            Image("Backpack", inventory.transform, "icon_inventory", new Vector2(.5f,.5f), Vector2.zero, new Vector2(57,57));

            var joystick = Image("Joystick Base", safe, "joystick_base", Vector2.zero, new Vector2(166,160), new Vector2(222,222));
            Image("Joystick Thumb", joystick.transform, "joystick_thumb", new Vector2(.5f,.5f), Vector2.zero, new Vector2(95,95));
            var attack = Image("Bare Hand Attack", safe, "attack_button", new Vector2(1,0), new Vector2(-162,150), new Vector2(174,174));
            Image("Fist", attack.transform, "icon_fist", new Vector2(.5f,.5f), Vector2.zero, new Vector2(86,86));
        }

        private static void Resource(Transform parent, string name, string icon, string count, float x)
        {
            var panel = Image(name, parent, "panel", new Vector2(0,1), new Vector2(x+71,-75), new Vector2(142,62));
            Image("Icon", panel.transform, icon, new Vector2(0,.5f), new Vector2(35,0), new Vector2(46,46));
            var text = Rect("Count", panel.transform, new Vector2(1,.5f), new Vector2(-44,0), new Vector2(64,44)).gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = count;
            text.fontSize = 29;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color32(227,226,200,255);
            text.raycastTarget = false;
        }

        private static Image Image(string name, Transform parent, string sprite, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var image = Rect(name, parent, anchor, position, size).gameObject.AddComponent<Image>();
            image.sprite = VisualArtImporter.Sprite(sprite);
            image.type = sprite == "panel" ? UnityEngine.UI.Image.Type.Sliced : UnityEngine.UI.Image.Type.Simple;
            image.raycastTarget = false;
            return image;
        }

        private static RectTransform Rect(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = new Vector2(.5f,.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }
    }
}
