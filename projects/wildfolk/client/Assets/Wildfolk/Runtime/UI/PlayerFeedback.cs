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
        private Image catchIcon;private string catchKey;private FishingWaterContext? water;
        private void OnEnable()=>L.Changed+=Refresh;
        private void OnDisable()=>L.Changed-=Refresh;
        private void Refresh(){if(Time.unscaledTime<until){if(water.HasValue){var c=water.Value;Label.text=L.Text("fishing.water."+c.Water)+(c.Deep && c.ShoreInset>=2.25f ? " · "+L.Text("fishing.deep") : "")+(c.Night ? " · "+L.Text("fishing.night") : "");}else if(catchKey!=null){Label.text=L.Format("feedback.fishing_catch",L.Text(catchKey),pickupCount);catchIcon.rectTransform.anchoredPosition=new Vector2(-Mathf.Min(Label.preferredWidth,Label.rectTransform.rect.width)*.5f-8,0);}else if(pickupItem!=null)Label.text="+"+pickupCount+" "+pickupItem.LocalizedName;else if(!string.IsNullOrEmpty(key))Label.text=requirement!=null ? requirement.Requirement : L.Text(key);}}
        private void Awake(){current=this;Label.text=string.Empty;var go=new GameObject("Catch icon",typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));go.transform.SetParent(Label.transform,false);catchIcon=go.GetComponent<Image>();catchIcon.preserveAspect=true;catchIcon.raycastTarget=false;catchIcon.enabled=false;var rect=catchIcon.rectTransform;rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);rect.pivot=new Vector2(1,.5f);rect.sizeDelta=new Vector2(40,40);}
        private void ClearCatch(){catchKey=null;water=null;if(catchIcon!=null)catchIcon.enabled=false;}
        public static void ShowWater(FishingWaterContext context){if(current==null)return;current.ClearCatch();current.water=context;current.pickupItem=null;current.key=null;current.until=Time.unscaledTime+2.5f;current.Refresh();}
        public static void ShowCatch(FishingCatchDefinition result,int count){if(current==null || result==null || count<1)return;current.ClearCatch();current.pickupItem=result.Output;current.pickupCount=count;current.catchKey=result.NameKey;current.key=null;current.requirement=null;current.until=Time.unscaledTime+2;current.catchIcon.sprite=result.Output.Icon;current.catchIcon.enabled=true;current.Label.transform.SetAsLastSibling();current.Refresh();Presented?.Invoke(Cue.Pickup);}
        private void OnDestroy(){if(current==this)current=null;}
        public static void ShowKey(string value){if(current==null)return;current.ClearCatch();current.pickupItem=null;current.requirement=null;current.key=value;current.Label.transform.SetAsLastSibling();current.Label.text=L.Text(value);current.until=Time.unscaledTime+2.5f;}
        public static void ShowPickup(ItemDefinition item,int count)
        {if(current==null || item==null || count<=0)return;current.ClearCatch();current.pickupCount=current.pickupItem==item && Time.unscaledTime<current.until ? current.pickupCount+count : count;current.pickupItem=item;current.requirement=null;current.key=null;current.until=Time.unscaledTime+1.5f;current.Label.transform.SetAsLastSibling();current.Refresh();Presented?.Invoke(Cue.Pickup);}

        public static void ShowRequirement(ResourceDefinition value)
        {if(current==null || value==null || current.requirement==value && Time.unscaledTime<current.until)return;current.ClearCatch();current.pickupItem=null;current.requirement=value;current.key="feedback.requires_tool";current.Label.text=value.Requirement;current.until=Time.unscaledTime+2.5f;}
        public static void ShowCrafted(RecipeDefinition recipe)
        {if(current==null)return;ShowKey("feedback.crafted");current.Label.text=L.Format("feedback.crafted_item",recipe.Result.LocalizedName,recipe.ResultCount);Presented?.Invoke(Cue.Crafted);}
        public static void ShowOperation(Cue cue)
        {ShowKey(cue==Cue.Processing ? "feedback.processing_started" : cue==Cue.Upgraded ? "feedback.upgraded" : "feedback.collected");Presented?.Invoke(cue);}
        public static void CraftResult(RecipeDefinition recipe,CraftStatus status)
        {if(status==CraftStatus.Ready)ShowCrafted(recipe);else if(status==CraftStatus.Processing)ShowOperation(Cue.Processing);else if(status==CraftStatus.Collect)ShowOperation(Cue.Collected);else ShowKey(status==CraftStatus.NoSpace ? "feedback.bag_full" : status==CraftStatus.StationRequired ? "feedback.upgrade" : "ui.crafting.missing_materials");}
        private void Update(){if(Time.unscaledTime>=until && Label.text.Length>0){Label.text=string.Empty;ClearCatch();}}
    }
}
