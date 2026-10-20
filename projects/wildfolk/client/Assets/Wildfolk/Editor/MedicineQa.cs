using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace Wildfolk.Editor
{
    // Explicit isolated Editor fixture; batch and live runs exercise the same gameplay paths.
    public static class MedicineQa
    {
        public static string Phase{get;private set;}="idle";
        private static string Root=>Path.GetFullPath("Captures/medicine-expansion");
        private static WorldSaveService s;private static PlayerInventory bag;private static NetworkSession host;private static NetworkPlayer actor;
        private static void Log(string text){Directory.CreateDirectory(Root);File.AppendAllText(Root+"/live.tsv",DateTime.UtcNow.ToString("O")+"\t"+Phase+"\t"+text+"\n");}
        private static void Check(bool ok,string label){Log((ok ? "PASS " : "FAIL ")+label);if(!ok)throw new InvalidOperationException(label);}
        private static void Step(string phase){Phase=phase;Log("begin");}
        private static IEnumerator Guard(IEnumerator flow)
        {var stack=new Stack<IEnumerator>();stack.Push(flow);while(stack.Count>0){bool more;object value;try{more=stack.Peek().MoveNext();value=more ? stack.Peek().Current : null;}catch(Exception e){Phase="failed";Log(e.ToString());yield break;}if(!more){stack.Pop();continue;}if(value is IEnumerator child)stack.Push(child);else yield return value;}Phase="complete";Log("complete");}
        private static IEnumerator Capture(string name)
        {if(Application.isBatchMode)yield break;yield return null;ScreenCapture.CaptureScreenshot(Root+"/"+name+".png");yield return new WaitForEndOfFrame();yield return null;}
        public static void Begin()
        {ContentExpansionQa.Prepare();s=ContentExpansionQa.S;bag=s.Player;s.World.Animals.enabled=s.World.Renewal.enabled=false;s.World.Animals.Restore(Array.Empty<AnimalSnapshot>(),new AnimalEcologySnapshot());Application.runInBackground=true;Phase="starting";s.StartCoroutine(Guard(Single()));}
        private static void Select(string id,int amount=0)
        {var item=s.Catalog.Item(id);if(amount>0)bag.Model.Add(item,amount);ContentExpansionQa.Select(id,false);}
        private static ResourcePlacement Habitat(SurfaceBiome biome)
        {
            for(var y=-32;y<=32;y++)for(var x=-32;x<=32;x++)foreach(var p in s.World.Map.Continental.GenerateChunk(new WorldChunk(x,y)))if(p.Rule==12 && s.World.Map.Continental.Environment(p.Position).Biome==biome)return p;
            throw new InvalidOperationException("No herb in "+biome);
        }
        public static string SupplySample()
        {
            var data=UnityEngine.Resources.Load<SaveGameRules>("SaveGameRules").World;var lines=new List<string>{"seed,biome,center_chunks,herb_nodes,herb_units"};
            foreach(var seed in new[]{9472026,17491,-81027})
            {var map=new WorldMap(data,seed,3);var centers=new int[4];var nodes=new int[4];for(var y=-24;y<=24;y+=2)for(var x=-24;x<=24;x+=2){var key=new WorldChunk(x,y);centers[(int)map.Continental.Environment(new Vector2((x+.5f)*data.ChunkSize,(y+.5f)*data.ChunkSize)).Biome]++;foreach(var p in map.Continental.GenerateChunk(key))if(p.Rule==12)nodes[(int)map.Continental.Environment(p.Position).Biome]++;}foreach(SurfaceBiome biome in Enum.GetValues(typeof(SurfaceBiome)))lines.Add(seed+","+biome+","+centers[(int)biome]+","+nodes[(int)biome]+","+nodes[(int)biome]*2);}
            Directory.CreateDirectory(Root);File.WriteAllLines(Root+"/supply.csv",lines);return string.Join("\n",lines);
        }
        private static IEnumerator Gather(SurfaceBiome biome)
        {
            Step("herb-"+biome);var p=Habitat(biome);ContentExpansionQa.Move(p.Position+Vector2.down*.8f);yield return null;
            var node=UnityEngine.Object.FindObjectsByType<ResourceNode>(FindObjectsSortMode.None).First(r=>r.Definition.DropItem.Id=="medicinal_herb" && Vector2.Distance(r.transform.position,p.Position)<.1f);
            foreach(var other in UnityEngine.Object.FindObjectsByType<ResourceNode>(FindObjectsSortMode.None))if(other!=node && Vector2.Distance(other.transform.position,p.Position)<4)other.gameObject.SetActive(false);
            bag.SelectSlot(8);bag.GetComponent<PlayerMotor>().Avatar.Facing.Face(Vector2.up);Physics2D.SyncTransforms();var before=bag.Model.CountItem(s.Catalog.Item("medicinal_herb"));yield return Capture("01-herb-"+biome);
            for(var n=0;n<4 && node!=null && node.Health.IsAlive;n++){Check(bag.GetComponent<PlayerPrimaryAction>().TryUse(),"unarmed strike");yield return new WaitForSeconds(.65f);}
            Check(node==null || !node.Health.IsAlive,"herb harvested "+p.Id+" at "+p.Position);var drop=WorldPickup.Active.FirstOrDefault(d=>d!=null && d.Item!=null && d.Item.Id=="medicinal_herb" && Vector2.Distance(d.transform.position,p.Position)<2);Check(drop!=null,"real herb ground pickup");ContentExpansionQa.Move(drop.transform.position);yield return new WaitForSeconds(.9f);Check(bag.Model.CountItem(s.Catalog.Item("medicinal_herb"))==before+2,"pickup adds authored yield2");yield return Capture("02-pickup-"+biome);
        }
        private static IEnumerator Craft(string id)
        {var craft=bag.GetComponent<PlayerCrafting>();var r=s.Catalog.Recipe(id);foreach(var c in r.Ingredients){var missing=c.Count-bag.Model.CountItem(c.Item);if(missing>0)bag.Model.Add(c.Item,checked((int)missing));}craft.SetSelected(r);yield return null;var before=bag.Model.CountItem(r.Result);UnityEngine.Object.FindFirstObjectByType<CraftingPanel>().Cards.Single(c=>c.Recipe==r).CraftButton.onClick.Invoke();Check(bag.Model.CountItem(r.Result)==before+1,"actual Hand card crafted "+id);yield return Capture("03-craft-"+id);}
        private static IEnumerator Single()
        {
            L.SetLanguage("ko");bag.Model.Restore(new InventoryModel(bag.Model.Capacity).Capture(),s.Catalog);yield return Gather(SurfaceBiome.Forest);yield return Gather(SurfaceBiome.Wetlands);
            Check(s.World.Renewal.Population.Pending.Any(p=>p.Rule==12),"harvest schedules regional recovery");
            Step("craft");ContentExpansionQa.Move(Vector2.zero);ContentExpansionQa.Clear(Vector2.zero,10);s.World.Session.OpenInventory();bag.GetComponent<PlayerCrafting>().OpenHandCrafting();yield return Craft("field_dressing");yield return Craft("healing_salve");yield return Capture("04-inventory-ko");L.SetLanguage("en");bag.GetComponent<PlayerCrafting>().SetSelected(s.Catalog.Recipe("field_dressing"));yield return Capture("04-description-en");bag.GetComponent<PlayerCrafting>().CloseCrafting();s.World.Session.CloseInventory();
            var health=bag.GetComponent<Health>();var survival=bag.GetComponent<PlayerSurvival>();var primary=bag.GetComponent<PlayerPrimaryAction>();
            Step("full-health");health.RestoreHealth(100);Select("field_dressing",3);var dress=bag.Model.CountItem(s.Catalog.Item("field_dressing"));Check(!primary.TryUse(),"full HP rejects dressing");Select("healing_salve",3);var salves=bag.Model.CountItem(s.Catalog.Item("healing_salve"));Check(!primary.TryUse(),"full HP rejects Salve");
            Step("treatment");health.ApplyDamage(65,true);Select("field_dressing");var hp=health.Current;Check(primary.TryUse() && health.Current==hp,"dressing begins without instant burst");Check(bag.Model.CountItem(s.Catalog.Item("field_dressing"))==dress-1,"one dressing consumed");Select("healing_salve");Check(!primary.TryUse(),"dressing blocks immediate Salve");yield return new WaitForSeconds(4.2f);Check(health.Current>=hp+10 && health.Current<hp+15,"gradual half-course delivery");Select("field_dressing");Check(!primary.TryUse() && bag.Model.CountItem(s.Catalog.Item("field_dressing"))==dress-1,"active treatment refuses repeat after cooldown");yield return Capture("05-active-treatment");Select("healing_salve");var before=health.Current;Check(primary.TryUse() && health.Current==before+30,"Salve gives exactly30 instant HP after shared interval");Check(bag.Model.CountItem(s.Catalog.Item("healing_salve"))==salves-1,"one Salve consumed");Check(!primary.TryUse(),"Salve repeat blocked");Select("field_dressing");Check(!primary.TryUse(),"Salve blocks immediate dressing");yield return new WaitForSeconds(4.2f);Check(!survival.Treatment.Active && health.Current>=hp+50 && health.Current<=hp+51,"20 gradual plus30 instant completed");yield return Capture("06-completed");
            Step("save-load");survival.ResetTransient();health.RestoreHealth(40);health.MarkCombat();Select("field_dressing");Check(primary.TryUse(),"new course before Save");yield return new WaitForSeconds(1.2f);var savedHp=health.Current;var savedDress=bag.Model.CountItem(s.Catalog.Item("field_dressing"));Check(s.Save(),"isolated Save11: "+s.LastError);var game=s.Repository.Read(s.Validate);Check(game.Version==11,"Save schema11 preserved");s.Apply(game);yield return null;Check(health.Current==savedHp && !survival.Treatment.Active,"Load retains delivered HP and clears remaining treatment");survival.AdvanceTreatment(20);Check(health.Current==savedHp && bag.Model.CountItem(s.Catalog.Item("field_dressing"))==savedDress,"Load never replays heal or consumed item");Check(s.World.Renewal.Population.Pending.Any(p=>p.Rule==12),"herb recovery survives Save/Load");yield return Capture("07-loaded");
        }
        public static void BeginHost()
        {if(!DevelopmentProfile.Isolated)throw new InvalidOperationException("Isolated QA required");UnityEngine.Object.FindFirstObjectByType<HomeScreen>().OpenMultiplayer();host=NetworkSession.Instance;host.WorldRepository=new SaveRepository(Root+"/host/"+Guid.NewGuid().ToString("N")+"/world.json");host.ProfileOverride=Guid.NewGuid().ToString("N");host.NewWorldGenerationVersion=3;Phase="starting";host.StartCoroutine(Guard(HostFlow()));}
        private static IEnumerator EnterHost()
        {var task=host.Host(ConnectionRoute.LocalNetwork);yield return new WaitUntil(()=>task.IsCompleted);Check(task.Result,"local Host: "+host.LastFailure);Check(host.EnterGame(),"enter multiplayer");var until=Time.realtimeSinceStartup+45;while(Time.realtimeSinceStartup<until){actor=NetworkPlayer.Active.FirstOrDefault(p=>p.IsSpawned && p.NetworkObject.IsPlayerObject);if(actor!=null && NetworkCreatureWorld.Active!=null && NetworkCreatureWorld.Active.Ready)break;yield return null;}Check(actor!=null && NetworkCreatureWorld.Active.Ready,"Host world ready");actor.Motor.enabled=actor.GetComponent<PlayerInputAdapter>().enabled=false;var w=NetworkCreatureWorld.Active;w.Clock.enabled=w.Animals.enabled=w.Goblins.enabled=w.Night.enabled=w.World.Renewal.enabled=false;w.World.Chunks.DrainPending();yield return null;}
        private static IEnumerator HostFlow()
        {
            Step("host");yield return EnterHost();var w=NetworkCreatureWorld.Active;var inventory=actor.GetComponent<PlayerInventory>();var catalog=w.Catalog;var health=actor.Health;var survival=actor.GetComponent<PlayerSurvival>();
            inventory.Model.Restore(new InventoryModel(inventory.Model.Capacity).Capture(),catalog);
            var map=w.World.Map;ResourcePlacement herb=default;var found=false;var center=WorldChunk.At(map.Continental.ForestAnchor,w.World.Definition.ChunkSize);
            for(var y=-8;y<=8 && !found;y++)for(var x=-8;x<=8 && !found;x++)foreach(var p in map.Continental.GenerateChunk(new WorldChunk(center.X+x,center.Y+y)))if(p.Rule==12){herb=p;found=true;break;}
            Check(found,"Host natural herb location");var position=herb.Position+Vector2.down*.8f;actor.transform.position=position;actor.GetComponent<Rigidbody2D>().position=position;w.World.Chunks.Reconcile();w.World.Chunks.DrainPending();w.FlushPending();yield return null;
            var node=UnityEngine.Object.FindObjectsByType<ResourceNode>(FindObjectsSortMode.None).First(n=>n.Definition.DropItem.Id=="medicinal_herb" && Vector2.Distance(n.transform.position,herb.Position)<.1f);
            foreach(var other in UnityEngine.Object.FindObjectsByType<ResourceNode>(FindObjectsSortMode.None))if(other!=node && Vector2.Distance(other.transform.position,herb.Position)<4)other.gameObject.SetActive(false);
            Check(node.GetComponent<NetworkResource>().IsServer,"Host owns actual herb");inventory.SelectSlot(8);actor.Motor.Avatar.Facing.Face(Vector2.up);Physics2D.SyncTransforms();
            for(var n=0;n<4 && node!=null && node.Health.IsAlive;n++){Check(actor.Attack(),"Host unarmed harvest request");yield return new WaitForSeconds(.65f);}actor.transform.position=herb.Position;actor.GetComponent<Rigidbody2D>().position=herb.Position;yield return new WaitForSeconds(1);Check(inventory.Model.CountItem(catalog.Item("medicinal_herb"))==2,"Host herb drops and picks up exactly2");yield return Capture("08-host-herb");
            inventory.Model.Restore(new InventoryModel(inventory.Model.Capacity).Capture(),catalog);inventory.Model.Add(catalog.Item("field_dressing"),4);inventory.Model.Add(catalog.Item("healing_salve"),4);health.RestoreHealth(30);inventory.SelectSlot(0);
            Check(!survival.enabled,"existing MP natural survival stays disabled");Check(actor.ExecutePrimary(100),"Host accepts dressing once");Check(!actor.ExecutePrimary(100),"same request rejected");Check(inventory.Model.CountItem(catalog.Item("field_dressing"))==3,"one network consumption");Check(actor.Status.Value.TreatmentUntil>actor.NetworkManager.ServerTime.Time,"replicated treatment end");inventory.SelectSlot(1);Check(!actor.ExecutePrimary(101),"shared interval blocks Salve");yield return new WaitForSeconds(4.2f);Check(health.Current>=40 && health.Current<45,"Host advances HoT despite disabled survival Update");var before=health.Current;Check(!actor.ExecutePrimary(100),"stale request still rejected after interval");Check(actor.ExecutePrimary(102) && health.Current==before+30,"fresh Salve restores once");Check(!actor.ExecutePrimary(102) && inventory.Model.CountItem(catalog.Item("healing_salve"))==3,"duplicate Salve no item/heal");yield return Capture("08-host-active");var hp=health.Current;var dresses=inventory.Model.CountItem(catalog.Item("field_dressing"));Check(host.TrySaveWorld(),"Host saves during treatment");host.Leave();yield return new WaitUntil(()=>host.State==NetworkSessionState.Idle);yield return new WaitForSecondsRealtime(.3f);Step("rehost");yield return EnterHost();health=actor.Health;survival=actor.GetComponent<PlayerSurvival>();inventory=actor.GetComponent<PlayerInventory>();Check(health.Current==hp && !survival.Treatment.Active,"reHost retains HP and clears transient course");Check(inventory.Model.CountItem(catalog.Item("field_dressing"))==dresses && inventory.Model.CountItem(catalog.Item("healing_salve"))==3,"reHost retains exactly consumed stacks");yield return Capture("09-rehost");
        }
    }
}
