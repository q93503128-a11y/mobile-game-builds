using UnityEngine;
using UnityEngine.UI;
namespace Wildfolk
{
    public sealed class PlayerFeedback : MonoBehaviour
    {
        public Text Label;
        private static PlayerFeedback current;
        private float until;
        private void Awake(){current=this;Label.text=string.Empty;}
        private void OnDestroy(){if(current==this)current=null;}
        public static void Show(string message){if(current==null)return;current.Label.text=message;current.until=Time.unscaledTime+2.5f;}
        private void Update(){if(Time.unscaledTime>=until)Label.text=string.Empty;}
    }
}
