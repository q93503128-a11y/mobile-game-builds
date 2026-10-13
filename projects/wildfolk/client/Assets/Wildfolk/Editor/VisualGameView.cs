using System;
using System.Reflection;
using UnityEditor;

namespace Wildfolk.Editor
{
    public static class VisualGameView
    {
        private const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        [MenuItem("Wildfolk/View 1920x1080")]
        public static void ShowReference() => Show(1920,1080);

        [MenuItem("Wildfolk/View 2400x1080")]
        public static void ShowWide() => Show(2400,1080);

        public static void Show(int width,int height)
        {
            var assembly=typeof(UnityEditor.Editor).Assembly;
            var sizesType=assembly.GetType("UnityEditor.GameViewSizes",true);
            var sizeType=assembly.GetType("UnityEditor.GameViewSize",true);
            var instance=typeof(ScriptableSingleton<>).MakeGenericType(sizesType).GetProperty("instance").GetValue(null);
            var group=sizesType.GetMethod("GetGroup",Flags).Invoke(instance,new[] { Enum.ToObject(assembly.GetType("UnityEditor.GameViewSizeGroupType",true),0) });
            var groupType=group.GetType();
            var labels=(string[])groupType.GetMethod("GetDisplayTexts",Flags).Invoke(group,null);
            var label=$"Wildfolk {width}x{height}";
            var index=Array.FindIndex(labels,item=>item.StartsWith(label,StringComparison.Ordinal));
            if(index<0)
            {
                var size=Activator.CreateInstance(sizeType,new[] { Enum.ToObject(assembly.GetType("UnityEditor.GameViewSizeType",true),1),width,height,(object)label });
                groupType.GetMethod("AddCustomSize",Flags).Invoke(group,new[] {size});
                index=(int)groupType.GetMethod("GetTotalCount",Flags).Invoke(group,null)-1;
            }
            var gameViewType=assembly.GetType("UnityEditor.GameView",true);
            var windows=UnityEngine.Resources.FindObjectsOfTypeAll(gameViewType);
            if(windows.Length==0)
                throw new InvalidOperationException("Open a Game View tab before selecting its resolution.");
            var window=(EditorWindow)windows[0];
            gameViewType.GetProperty("selectedSizeIndex",Flags).SetValue(window,index);
            window.Repaint();
        }
    }
}
