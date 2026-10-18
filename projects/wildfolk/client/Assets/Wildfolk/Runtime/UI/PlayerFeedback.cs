using UnityEngine;
using UnityEngine.UI;
namespace Wildfolk
{
    public sealed class PlayerFeedback : MonoBehaviour
    {
        public Text Label;
        private static PlayerFeedback current;
        private float until;private string key;private ResourceDefinition requirement;
        private ItemDefinition pickupItem;private int pickupCount;
        private void OnEnable()=>L.Changed+=Refresh;
        private void OnDisable()=>L.Changed-=Refresh;
        private void Refresh(){if(Time.unscaledTime<until){if(pickupItem!=null)Label.text="+"+pickupCount+" "+pickupItem.LocalizedName;else if(!string.IsNullOrEmpty(key))Label.text=requirement!=null ? requirement.Requirement : L.Text(key);}}
        private void Awake(){current=this;Label.text=string.Empty;}
        private void OnDestroy(){if(current==this)current=null;}
        public static void ShowKey(string value){if(current==null)return;current.pickupItem=null;current.requirement=null;current.key=value;current.Label.text=L.Text(value);current.until=Time.unscaledTime+2.5f;}
        public static void ShowPickup(ItemDefinition item,int count)
        {if(current==null || item==null || count<=0)return;current.pickupCount=current.pickupItem==item && Time.unscaledTime<current.until ? current.pickupCount+count : count;current.pickupItem=item;current.requirement=null;current.key=null;current.until=Time.unscaledTime+1.5f;current.Refresh();}

        public static void ShowRequirement(ResourceDefinition value)
        {if(current==null || value==null || current.requirement==value && Time.unscaledTime<current.until)return;current.pickupItem=null;current.requirement=value;current.key="feedback.requires_tool";current.Label.text=value.Requirement;current.until=Time.unscaledTime+2.5f;}
        private void Update(){if(Time.unscaledTime>=until)Label.text=string.Empty;}
    }
}
