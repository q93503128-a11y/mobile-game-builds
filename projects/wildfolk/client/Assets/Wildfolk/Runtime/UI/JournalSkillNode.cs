using UnityEngine;
using UnityEngine.UI;
namespace Wildfolk
{
    public static class JournalStyle
    {
        public static readonly Color Ink=new Color32(49,63,48,255);
        public static readonly Color Muted=new Color32(111,105,79,255);
        public static readonly Color Ember=new Color32(137,88,56,255);
        public static readonly Color Paper=new Color32(247,235,203,255);
        public static readonly Color Learned=new Color32(197,204,160,255);
        public static readonly Color Danger=new Color32(137,57,43,255);
        public static void Action(Button button,bool primary=false,bool destructive=false)
        {
            var c=button.colors;c.normalColor=destructive?Danger:primary?Ember:new Color32(216,205,173,255);
            c.highlightedColor=c.selectedColor=destructive?new Color32(161,72,52,255):primary?new Color32(157,108,73,255):new Color32(229,219,190,255);
            c.pressedColor=new Color32(111,103,75,255);c.disabledColor=new Color32(183,177,153,255);button.colors=c;
            var label=button.GetComponentInChildren<Text>(true);if(label!=null)label.color=primary||destructive?Paper:Ink;
        }
    }
    // A small state view shared by class and common-growth diagrams; no polling or gameplay truth.
    public sealed class JournalSkillNode:MonoBehaviour
    {
        public Image Frame,Icon;public GameObject Focus,Learned,Locked;
        public void SetState(bool learned,bool unlocked,bool selected)
        {
            Focus.SetActive(selected);Learned.SetActive(learned);Locked.SetActive(!learned && !unlocked);
            Frame.color=learned?JournalStyle.Learned:unlocked?Color.white:new Color32(196,188,164,255);
            Icon.color=learned || unlocked?Color.white:new Color(1,1,1,.72f);
        }
    }
}
