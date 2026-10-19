using UnityEngine;
using UnityEngine.UI;
namespace Wildfolk
{
    public sealed class PlayerFeedback : MonoBehaviour
    {
        public Text Label;
        public enum Cue { Pickup,Crafted,Processing,Collected,Upgraded }
        public static event System.Action<Cue> Presented;
        private static PlayerFeedback current;
        private float until;private string key;private ResourceDefinition requirement;
        private ItemDefinition pickupItem;private int pickupCount;
        private void OnEnable()=>L.Changed+=Refresh;
        private void OnDisable()=>L.Changed-=Refresh;
        private void Refresh(){if(Time.unscaledTime<until){if(pickupItem!=null)Label.text="+"+pickupCount+" "+pickupItem.LocalizedName;else if(!string.IsNullOrEmpty(key))Label.text=requirement!=null ? requirement.Requirement : L.Text(key);}}
        private void Awake(){current=this;Label.text=string.Empty;}
        private void OnDestroy(){if(current==this)current=null;}
        public static void ShowKey(string value){if(current==null)return;current.pickupItem=null;current.requirement=null;current.key=value;current.Label.transform.SetAsLastSibling();current.Label.text=L.Text(value);current.until=Time.unscaledTime+2.5f;}
        public static void ShowPickup(ItemDefinition item,int count)
        {if(current==null || item==null || count<=0)return;current.pickupCount=current.pickupItem==item && Time.unscaledTime<current.until ? current.pickupCount+count : count;current.pickupItem=item;current.requirement=null;current.key=null;current.until=Time.unscaledTime+1.5f;current.Label.transform.SetAsLastSibling();current.Refresh();Presented?.Invoke(Cue.Pickup);}

        public static void ShowRequirement(ResourceDefinition value)
        {if(current==null || value==null || current.requirement==value && Time.unscaledTime<current.until)return;current.pickupItem=null;current.requirement=value;current.key="feedback.requires_tool";current.Label.text=value.Requirement;current.until=Time.unscaledTime+2.5f;}
        public static void ShowCrafted(RecipeDefinition recipe)
        {if(current==null)return;ShowKey("feedback.crafted");current.Label.text=L.Format("feedback.crafted_item",recipe.Result.LocalizedName,recipe.ResultCount);Presented?.Invoke(Cue.Crafted);}
        public static void ShowOperation(Cue cue)
        {ShowKey(cue==Cue.Processing ? "feedback.processing_started" : cue==Cue.Upgraded ? "feedback.upgraded" : "feedback.collected");Presented?.Invoke(cue);}
        public static void CraftResult(RecipeDefinition recipe,CraftStatus status)
        {if(status==CraftStatus.Ready)ShowCrafted(recipe);else if(status==CraftStatus.Processing)ShowOperation(Cue.Processing);else if(status==CraftStatus.Collect)ShowOperation(Cue.Collected);else ShowKey(status==CraftStatus.NoSpace ? "feedback.bag_full" : status==CraftStatus.StationRequired ? "feedback.upgrade" : "ui.crafting.missing_materials");}
        private void Update(){if(Time.unscaledTime>=until && Label.text.Length>0)Label.text=string.Empty;}
    }
}
