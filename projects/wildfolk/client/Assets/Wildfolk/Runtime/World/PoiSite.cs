using System;
using UnityEngine;
namespace Wildfolk
{
    // Active chunk representation; the authoritative snapshot belongs to WorldChunkRuntime.
    public sealed class PoiSite:MonoBehaviour,IPlayerInteractable,IWorldSupplies
    {
        public string Id;public WorldAnchorKind Kind;public PoiSiteSnapshot State;
        public WorldGenerator World{get;private set;}public ContentCatalog Catalog{get;private set;}
        public SpriteRenderer Visual;public Sprite ClosedSprite,OpenedSprite,WaterIcon;
        public bool ReplicaSecured;
        public event Action Changed;
        public bool IsAvailable=>this!=null && State!=null;
        public Transform Location=>Kind==WorldAnchorKind.Well || Visual==null ? transform : Visual.transform;public Sprite Icon=>Kind==WorldAnchorKind.Well ? WaterIcon : ClosedSprite;
        public string TitleKey=>"poi."+Kind;public string ExplainKey=>"ui.poi.supplies";
        public SavedStack[] Loot=>State?.Loot;public uint Revision=>State==null ? 0 : State.Revision;
        public string GuardId=>Id+":guard";
        public bool Secured=>Kind!=WorldAnchorKind.Graveyard || State!=null && State.Legacy || (World!=null ? World.Chunks.FeatureCleared(GuardId) : ReplicaSecured);
        public void Initialize(WorldGenerator world,WorldAnchor anchor,PoiSiteSnapshot state)
        {World=world;Catalog=world.Definition.ProgressionCatalog;Id=anchor.Id;Kind=anchor.Kind;State=state;Notify();}
        public void ApplyReplica(string id,WorldAnchorKind kind,PoiSiteSnapshot state,bool secured,ContentCatalog catalog)
        {Id=id;Kind=kind;State=state;Catalog=catalog;ReplicaSecured=secured;Notify();}
        public bool InRange(PlayerInventory p)=>p!=null && p.GetComponent<Health>().IsAlive && Vector2.Distance(p.transform.position,Location.position)<=p.Settings.StationInteractionRange;
        public bool CanInteract(PlayerInteraction p)=>State!=null && InRange(p.GetComponent<PlayerInventory>());
        public bool Use(PlayerInteraction p)
        {
            var visitor=p.GetComponent<PlayerInventory>();
            if(Kind==WorldAnchorKind.Well)
            {var net=visitor.GetComponent<NetworkFacilityInteractor>();return net==null ? Fill(visitor,Revision) : net.PoiInteract(this,-2,Revision);}
            if(!Secured){if(World!=null)World.Chunks.TryPoiGuard(this);PlayerFeedback.ShowKey("ui.poi.guarded");return true;}
            var ok=RequestTake(visitor,-1);if(ok)CampLootPanel.Open(this,visitor);return ok;
        }
        public bool RequestTake(PlayerInventory p,int slot){var net=p.GetComponent<NetworkFacilityInteractor>();return net==null ? Take(p,slot,Revision) : net.PoiInteract(this,slot,Revision);}
        private bool Allowed(PlayerInventory p,uint revision)
        {return SimulationAuthority.Allows(this) && State!=null && Revision==revision && InRange(p) && !p.GetComponent<PlayerInputCommands>().IsScouting && (p.GetComponent<PlayerInputCommands>().Session.IsRunning || p.GetComponent<PlayerInputCommands>().Session.State==SessionState.Inventory);}
        public bool Take(PlayerInventory p,int slot,uint revision)
        {
            if(Kind==WorldAnchorKind.Well || !Allowed(p,revision) || !Secured || slot< -1 || slot>=State.Loot.Length)return false;
            if(slot>=0){var stack=State.Loot[slot];if(stack==null || stack.Count<=0 || p.Model.TryExchange(Array.Empty<ItemAmount>(),Catalog.Item(stack.Item),stack.Count)!=ExchangeResult.Success)return false;State.Loot[slot]=new SavedStack();}
            State.Opened=true;State.Revision++;Notify();return true;
        }
        public bool Fill(PlayerInventory p,uint revision)
        {
            if(Kind!=WorldAnchorKind.Well || !Allowed(p,revision))return false;
            var config=World.Definition.PoiContent;AdvanceWater(State,World.Renewal.Clock.Model.Elapsed,config.WellCapacity,config.WellRefillSeconds);
            if(State.Water<=0){Feedback(p,"ui.poi.well_empty");Notify();return false;}
            var selected=p.SelectedStack;bool ok;
            if(!selected.IsEmpty && selected.Item.WaterCapacity>0)
                ok=selected.Charges<selected.Item.WaterCapacity && (selected.Charges==0 || selected.Quality==LiquidState.Clean) && p.Model.SetWater(p.SelectedIndex,selected.Item,selected.Charges+1,LiquidState.Clean);
            else ok=p.Model.TryExchange(new[]{new ItemAmount{Item=Catalog.Item("empty_cup"),Count=1}},Catalog.Item("clean_water_cup"),1)==ExchangeResult.Success;
            if(!ok){Feedback(p,"ui.poi.well_container");Notify();return false;}
            if(State.Water==config.WellCapacity)State.RefillAt=World.Renewal.Clock.Model.Elapsed+config.WellRefillSeconds;
            State.Water--;State.Revision++;Notify();Feedback(p,"ui.poi.well_filled");return true;
        }
        public static void AdvanceWater(PoiSiteSnapshot s,double now,int capacity,double interval)
        {
            if(s.Water>=capacity){s.RefillAt=0;return;}if(s.RefillAt<=0)s.RefillAt=now+interval;
            if(now<s.RefillAt)return;var count=(int)Math.Min(capacity-s.Water,1+Math.Floor((now-s.RefillAt)/interval));s.Water+=count;s.RefillAt=s.Water>=capacity ? 0 : s.RefillAt+count*interval;
        }
        private static void Feedback(PlayerInventory p,string key){var net=p.GetComponent<NetworkPlayer>();if(net==null || net.IsOwner)PlayerFeedback.ShowKey(key);}
        public void Notify(){var trigger=GetComponent<CircleCollider2D>();if(trigger!=null)trigger.offset=Kind==WorldAnchorKind.Well ? Vector2.zero : Vector2.right*2;if(Visual!=null){Visual.enabled=Kind!=WorldAnchorKind.Well;Visual.sprite=State!=null && State.Opened ? OpenedSprite : ClosedSprite;}Changed?.Invoke();GetComponent<NetworkPoiSite>()?.Publish();}
    }
}
