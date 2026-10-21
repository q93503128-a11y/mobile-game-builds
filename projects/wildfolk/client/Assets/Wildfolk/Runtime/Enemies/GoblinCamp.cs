using UnityEngine;
namespace Wildfolk
{
    public sealed class GoblinCamp : MonoBehaviour,IPlayerInteractable,IWorldSupplies
    {
        public CampSnapshot State;
        public GoblinCampDefinition Definition;
        public Transform Supplies;
        public Sprite SupplyIcon;
        public SpriteRenderer SupplyVisual;
        public Sprite OpenedSprite;
        public bool IsAvailable=>this!=null && State!=null;
        public SavedStack[] Loot=>State.Loot;
        public uint Revision=>State.LootRevision;
        public string TitleKey=>"ui.camp.title";public string ExplainKey=>"ui.camp.explain";
        public bool RequestTake(PlayerInventory visitor,int slot){var net=visitor.GetComponent<NetworkFacilityInteractor>();return net==null ? Take(visitor,slot,Revision) : net.CampLoot(this,slot,Revision);}
        public string LootId=>State.Id+":supplies";
        public GoblinSpawner Owner {get;private set;}
        public ContentCatalog Catalog {get;private set;}
        public event System.Action Changed;
        public Transform Location=>Supplies==null ? transform : Supplies;
        public Sprite Icon=>SupplyIcon;
        public bool Secured=>Owner!=null ? Owner.CampSecured(State.Id) : GetComponent<NetworkCamp>()!=null && GetComponent<NetworkCamp>().Secured.Value;
        public void InitializeLoot(GoblinSpawner owner,ContentCatalog catalog,int seed)
        {
            Owner=owner;Catalog=catalog;
            if(State.LootInitialized){Notify();return;}
            var random=new System.Random(WorldSeeds.Derive(seed,SeedDomain.POI,WorldSeeds.StableText(State.Id+":supplies")));
            State.Loot=new InventoryModel(6).Capture();
            State.Loot[0]=new SavedStack{Item="bone_arrow",Count=random.Next(8,17)};
            State.Loot[1]=new SavedStack{Item="healing_salve",Count=random.Next(1,3)};
            State.Loot[2]=new SavedStack{Item="bronze",Count=random.Next(1,3)};
            State.Loot[3]=new SavedStack{Item="leather",Count=random.Next(1,3)};
            if(random.NextDouble()<.2)State.Loot[4]=new SavedStack{Item="mechanical_part",Count=1};
            if(random.NextDouble()<.35)State.Loot[5]=new SavedStack{Item="barbed_bolt",Count=8};
            State.LootInitialized=true;Notify();
        }
        public bool InRange(PlayerInventory player)=>player!=null && player.GetComponent<Health>().IsAlive && Vector2.Distance(player.transform.position,Location.position)<=player.Settings.StationInteractionRange;
        public bool CanInteract(PlayerInteraction actor)=>State!=null && Supplies!=null && InRange(actor.GetComponent<PlayerInventory>());
        public bool Use(PlayerInteraction actor)
        {
            if(!Secured){PlayerFeedback.ShowKey("ui.camp.guarded");return true;}
            var player=actor.GetComponent<PlayerInventory>();var authority=player.GetComponent<NetworkFacilityInteractor>();
            var ok=authority==null ? Take(player,-1,State.LootRevision) : authority.CampLoot(this,-1,State.LootRevision);
            if(ok)CampLootPanel.Open(this,player);return ok;
        }
        public bool Take(PlayerInventory player,int slot,uint expectedRevision)
        {
            var net=GetComponent<NetworkCamp>();if(net!=null && (!net.IsSpawned || !net.IsServer))return false;
            if(State==null || !State.LootInitialized || !InRange(player) || !Secured || State.LootRevision!=expectedRevision)return false;
            var session=player.GetComponent<PlayerInputCommands>().Session;if(!session.IsRunning && session.State!=SessionState.Inventory)return false;
            if(slot< -1 || slot>=State.Loot.Length)return false;
            if(slot>=0)
            {
                var stack=State.Loot[slot];if(stack==null || stack.Count<=0)return false;
                if(player.Model.TryExchange(System.Array.Empty<ItemAmount>(),Catalog.Item(stack.Item),stack.Count)!=ExchangeResult.Success)return false;
                State.Loot[slot]=new SavedStack();
            }
            State.LootOpened=true;State.LootRevision++;Notify();net?.PublishLoot();return true;
        }
        public void Notify(){if(SupplyVisual!=null)SupplyVisual.sprite=State!=null && State.LootOpened ? OpenedSprite : SupplyIcon;Changed?.Invoke();}
        public void ApplyReplica(ContentCatalog catalog,SavedStack[] slots,bool opened,uint revision)
        {Catalog=catalog;if(State==null)State=new CampSnapshot();State.Loot=slots;State.LootInitialized=true;State.LootOpened=opened;State.LootRevision=revision;Notify();}
    }
}
