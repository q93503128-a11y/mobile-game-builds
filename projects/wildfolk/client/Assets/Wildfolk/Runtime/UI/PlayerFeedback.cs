using UnityEngine;
using UnityEngine.UI;
namespace Wildfolk
{
    public sealed class PlayerFeedback : MonoBehaviour
    {
        public Text Label;
        private static PlayerFeedback current;
        private float until;private string key;private ResourceDefinition requirement;
        private void OnEnable()=>L.Changed+=Refresh;
        private void OnDisable()=>L.Changed-=Refresh;
        private void Refresh(){if(!string.IsNullOrEmpty(key) && Time.unscaledTime<until)Label.text=requirement!=null ? requirement.Requirement : L.Text(key);}
        private void Awake(){current=this;Label.text=string.Empty;}
        private void OnDestroy(){if(current==this)current=null;}
        public static void ShowKey(string value){if(current==null)return;current.requirement=null;current.key=value;current.Label.text=L.Text(value);current.until=Time.unscaledTime+2.5f;}

        public static void ShowRequirement(ResourceDefinition value)
        {if(current==null || value==null || current.requirement==value && Time.unscaledTime<current.until)return;current.requirement=value;current.key="feedback.requires_tool";current.Label.text=value.Requirement;current.until=Time.unscaledTime+2.5f;}
        private void Update(){if(Time.unscaledTime>=until)Label.text=string.Empty;}
    }
}
