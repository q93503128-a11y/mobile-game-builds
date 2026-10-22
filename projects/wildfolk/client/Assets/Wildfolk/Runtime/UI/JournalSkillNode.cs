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
    }
    // A small state view shared by class and common-growth diagrams; no polling or gameplay truth.
    public sealed class JournalSkillNode:MonoBehaviour
    {
        public Image Frame,Icon;public GameObject Focus,Learned,Locked;
        public void SetState(bool learned,bool available,bool selected)
        {
            Focus.SetActive(selected);Learned.SetActive(learned);Locked.SetActive(!learned && !available);
            Frame.color=learned?JournalStyle.Learned:available?Color.white:new Color32(196,188,164,255);
            Icon.color=learned || available?JournalStyle.Ink:JournalStyle.Muted;
        }
    }
}
