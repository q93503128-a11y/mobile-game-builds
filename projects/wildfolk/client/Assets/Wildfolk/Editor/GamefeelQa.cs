using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
namespace Wildfolk.Editor
{
    // Guided Play interactions through gameplay commands/UI handlers. No operating-system input.
    public static class GamefeelQa
    {
        public static string Phase {get;private set;}="idle";
        private static WorldSaveService save;private static PlayerInventory bag;private static PlayerInputCommands input;
        private static string Root=>Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/gamefeel"));
        public static void Begin()
        {
            ContentExpansionQa.Prepare();save=ContentExpansionQa.S;bag=save.Player;input=bag.GetComponent<PlayerInputCommands>();
            save.World.Animals.enabled=false;Directory.CreateDirectory(Root);File.WriteAllText(Path.Combine(Root,"play.tsv"),"utc\tstep\tresult\n");
            save.StartCoroutine(Guard(Flow()));
        }
        private static IEnumerator Guard(IEnumerator flow)
        {
            var stack=new Stack<IEnumerator>();stack.Push(flow);
            while(stack.Count>0){bool next;object value;try{next=stack.Peek().MoveNext();value=next ? stack.Peek().Current : null;}catch(Exception e){Phase="failed";Log(e.ToString());input.SetMove(Vector2.zero);yield break;}if(!next){stack.Pop();continue;}if(value is IEnumerator nested)stack.Push(nested);else yield return value;}
            Phase="complete";Log("complete");
        }
        private static void Log(string result)=>File.AppendAllText(Path.Combine(Root,"play.tsv"),DateTime.UtcNow.ToString("O")+"\t"+Phase+"\t"+result.Replace('\n',' ')+"\n");
        private static void Check(bool value,string result){if(!value)throw new InvalidOperationException(result);Log(result);}
        private static void Step(string name){Phase=name;Log("begin");}
        private static void Move(Vector2 p){ContentExpansionQa.Move(p);bag.GetComponent<PlayerSurvival>().Model.SetMeters(100,100);}
        private static void Select(string id)=>ContentExpansionQa.Select(id);
        private static void Costs(RecipeDefinition r){foreach(var c in r.Ingredients)bag.Model.TryAddAmounts(new[]{new ItemAmount{Item=c.Item,Count=c.Count*3,Charges=c.Charges,Quality=c.Quality}});}
        private static IEnumerator Capture(string name){ScreenCapture.CaptureScreenshot(Path.Combine(Root,name+".png"));yield return new WaitForEndOfFrame();yield return null;}
        private static IEnumerator Gather(ResourceKind kind,string tool)
        {
            Step("gather-"+kind);var rule=Array.FindIndex(save.World.Definition.Resources,r=>r.Prefab.Definition.Kind==kind);
            var source=save.World.Map.Resources.First(p=>p.Rule==rule);Move(source.Position+Vector2.down*1.5f);yield return null;
            var node=UnityEngine.Object.FindObjectsByType<ResourceNode>(FindObjectsSortMode.None).Where(n=>n.Definition.Kind==kind).OrderBy(n=>Vector2.Distance(n.transform.position,source.Position)).First();
            foreach(var other in UnityEngine.Object.FindObjectsByType<ResourceNode>(FindObjectsSortMode.None))if(other!=node && Vector2.Distance(other.transform.position,bag.transform.position)<3)other.gameObject.SetActive(false);
            Select(tool);bag.GetComponent<CharacterFacing>().Face((Vector2)node.transform.position-(Vector2)bag.transform.position);Physics2D.SyncTransforms();yield return null;
            var health=node.Health;var before=health.Current;var combat=bag.GetComponent<PlayerCombat>();var limit=Time.time+18;
            while(node!=null && health.IsAlive && Time.time<limit){combat.TryAttack();yield return new WaitForSeconds(.75f);}
            Check(node==null || !health.IsAlive,"command attacks harvested "+kind+"; initial HP="+before+"; fixture teleport/tool injection");
            yield return Capture("gather-"+kind);
        }
        private static StationProcessing Station(string id,Vector2 position)=>ContentExpansionQa.Place(id,position).GetComponent<StationProcessing>();
        private static IEnumerator Process(string item,string recipe,Vector2 position)
        {
            Step(item);Move(position+Vector2.left*2);ContentExpansionQa.Clear(position,3);var station=Station(item,position);var r=save.Catalog.Recipe(recipe);Costs(r);
            Check(bag.GetComponent<PlayerCrafting>().OpenStation(station.Station),"station UI opened");yield return null;
            var panel=UnityEngine.Object.FindFirstObjectByType<CraftingPanel>();panel.Cards.First(c=>c.Recipe==r).CraftButton.onClick.Invoke();yield return new WaitForSeconds(.3f);
            Check(station.Recipe==r && station.Remaining<station.Duration,"processing started and scheduler advanced");yield return Capture("station-"+item);
            station.Advance(r.ProcessingSeconds*4);yield return null;Check(station.IsReady,"fixture time advance reached output");
            var before=bag.Model.CountItem(r.Result);panel.Collect.onClick.Invoke();Check(bag.Model.CountItem(r.Result)>before,"Take button collected result");save.World.Session.CloseInventory();
        }
        private static IEnumerator Flow()
        {
            Step("spawn");Check(bag.GetComponent<Health>().IsAlive && save.World.Map.GenerationVersion==2,"fresh isolated generation2 spawn");
            Move(Vector2.zero);ContentExpansionQa.Clear(Vector2.zero,9);var motor=bag.GetComponent<PlayerMotor>();motor.enabled=true;input.SetMove(Vector2.right*.5f);var start=bag.transform.position;yield return new WaitForSeconds(.45f);
            Step("movement");Check(Vector2.Distance(start,bag.transform.position)>.4f,"movement command moves body");input.SetMove(Vector2.right);yield return new WaitForSeconds(1);
            Step("sprint-stamina");Check(bag.GetComponent<PlayerLocomotion>().Stamina.Current<bag.GetComponent<PlayerLocomotion>().Stamina.Maximum,"held full movement drains sprint stamina");input.SetMove(Vector2.zero);motor.enabled=false;yield return Capture("04-after-world");
            yield return Gather(ResourceKind.Wood,"iron_axe");yield return Gather(ResourceKind.Stone,"iron_pickaxe");yield return Gather(ResourceKind.IronOre,"iron_pickaxe");yield return Gather(ResourceKind.Coal,"iron_pickaxe");yield return Gather(ResourceKind.CopperOre,"iron_pickaxe");yield return Gather(ResourceKind.TinOre,"prospecting_pan");
            yield return Remaining();
        }
        public static void Resume()
        {ContentExpansionQa.Prepare();save=ContentExpansionQa.S;bag=save.Player;input=bag.GetComponent<PlayerInputCommands>();save.StartCoroutine(Guard(Remaining()));}
        private static IEnumerator Remaining()
        {
            yield return new WaitForSeconds(1);
            Step("melee-miss");Move(Vector2.zero);ContentExpansionQa.Clear(Vector2.zero,7);Select("iron_sword");yield return null;bag.GetComponent<CharacterFacing>().Face(Vector2.down);var combat=bag.GetComponent<PlayerCombat>();Check(combat.TryAttack(),"miss swing began");yield return new WaitForSeconds(.8f);Check(combat.LastHitCount==0,"miss has no hit result");
            Step("melee-hit-death");var spawn=save.World.GetComponent<ZombieSpawner>();var enemy=PrefabPool.Spawn(spawn.Prefab,bag.transform.position+Vector3.right*1.5f,Quaternion.identity);enemy.Session=spawn.Session;enemy.Clock=spawn.Clock;enemy.PlayerHealth=bag.GetComponent<Health>();enemy.enabled=false;enemy.GetComponent<ZombieLifecycle>().RestoreActive();enemy.GetComponent<ZombieLifecycle>().enabled=false;enemy.GetComponent<Collider2D>().enabled=true;enemy.GetComponent<Rigidbody2D>().bodyType=RigidbodyType2D.Kinematic;
            bag.GetComponent<CharacterFacing>().Face(Vector2.right);Physics2D.SyncTransforms();var hp=enemy.OwnHealth.Current;Check(combat.TryAttack(),"hit swing began");yield return new WaitForSeconds(.4f);Check(enemy.OwnHealth.Current<hp,"melee contact changed target HP");yield return Capture("combat-hit");yield return new WaitForSeconds(.4f);
            Step("ranged");enemy.OwnHealth.RestoreHealth(enemy.OwnHealth.Maximum);Select("arrow");bag.Model.Add(save.Catalog.Item("arrow"),10);Select("bow");enemy.transform.position=bag.transform.position+Vector3.right*5;Physics2D.SyncTransforms();yield return null;var arrows=bag.Model.CountItem(save.Catalog.Item("arrow"));Check(bag.GetComponent<PlayerRanged>().Begin(),"ranged command began");yield return new WaitForSeconds(.8f);Check(bag.Model.CountItem(save.Catalog.Item("arrow"))==arrows-1,"ranged consumed one arrow");
            Step("enemy-kill");enemy.transform.position=bag.transform.position+Vector3.right*1.5f;enemy.OwnHealth.RestoreHealth(1);Select("iron_sword");Physics2D.SyncTransforms();yield return null;combat.TryAttack();yield return new WaitForSeconds(.2f);yield return Capture("combat-death");yield return new WaitForSeconds(.6f);Check(!enemy.gameObject.activeSelf,"death recovered to pool after short presentation");
            Step("player-damage");var own=bag.GetComponent<Health>();hp=own.Current;own.ApplyDamage(12,true);Check(own.Current==hp-12,"fixture damage changed player HP");yield return Capture("player-damage");own.RestoreHealth(own.Maximum);
            Step("pickup");var wood=save.Catalog.Item("wood");var count=bag.Model.CountItem(wood);var drop=PrefabPool.Spawn(save.World.Pickup,bag.transform.position,Quaternion.identity);drop.Initialize(wood,3,bag,bag.Settings);yield return new WaitForSeconds(1);Check(bag.Model.CountItem(wood)>=count+3,"proximity pickup delivered +3");yield return Capture("pickup-result");
            Step("inventory-drag-drop");input.Inventory();yield return null;var view=UnityEngine.Object.FindFirstObjectByType<InventoryPanel>();Check(view.View.activeSelf,"inventory opened");var from=Enumerable.Range(0,bag.Model.Capacity).First(i=>bag.Model.GetSlot(i).Item==wood);var to=Enumerable.Range(0,bag.Model.Capacity).First(i=>bag.Model.GetSlot(i).IsEmpty);var data=new PointerEventData(EventSystem.current){pointerId=-1,position=RectTransformUtility.WorldToScreenPoint(null,view.Slots[from].transform.position)};var drag=UnityEngine.Object.FindFirstObjectByType<InventoryDragController>();drag.Begin(from,view.Slots[from],data);drag.Drop(to,data);drag.End(data);Check(bag.Model.GetSlot(to).Item==wood,"UI drag handlers moved stack");Check(bag.GetComponent<PlayerItemDrop>().Drop(bag.Model,to,1),"inventory world drop succeeded");
            Step("equipment-utility");var slot=ContentExpansionQa.Select("iron_body");Check(bag.GetComponent<PlayerEquipment>().TryEquipFromSlot(slot),"body equipment installed");slot=ContentExpansionQa.Select("lantern");Check(bag.GetComponent<PlayerEquipment>().TryEquipFromSlot(slot),"Lantern installed in Utility");Select("iron_pickaxe");yield return null;Check(bag.GetComponent<PlayerEquipment>().Utility.Id=="lantern","tool and Utility coexist");yield return Capture("05-inventory-equipment");save.World.Session.CloseInventory();
            Step("craft-failure-success-upgrade");ContentExpansionQa.OpenBench(1);var craft=bag.GetComponent<PlayerCrafting>();var r=save.Catalog.Recipe("compass");Check(craft.Craft(save.Catalog.Recipe("steel_sword"))==CraftStatus.StationRequired,"tier-locked craft rejected without mutation");yield return Capture("06-after-workbench-I");var panel=UnityEngine.Object.FindFirstObjectByType<CraftingPanel>();Check(ContentExpansionQa.Craft("compass")==1,"Craft button produced Compass");yield return Capture("07-craft-result");
            foreach(var c in craft.Station.Definition.UpgradeCosts)bag.Model.Add(c.Item,c.Count);panel.UpgradeButton.onClick.Invoke();yield return null;Check(craft.Station.Definition.WorkbenchTier==2,"Upgrade button changed Workbench I to II");yield return Capture("08-upgrade-result");var home=bag.GetComponent<PlayerNavigation>();Check(home.SelectHome(craft.Station),"Compass Home selected");save.World.Session.CloseInventory();
            yield return Process("furnace","iron_ingot",new Vector2(10,0));yield return Process("blast_furnace","steel",new Vector2(20,0));yield return Process("cooking_pot","mixed_stew",new Vector2(30,0));
            Step("food");bag.GetComponent<PlayerSurvival>().Model.SetMeters(40,40);Select("mixed_stew");Check(bag.GetComponent<PlayerSurvival>().TryConsume(),"cooked food consumed");
            Step("building-valid-invalid");Move(new Vector2(0,-12));ContentExpansionQa.Clear(bag.transform.position,8);Select("wood_wall");bag.Model.Add(save.Catalog.Item("wood_wall"),2);var placement=bag.GetComponent<PlayerPlacement>();placement.Facing.Face(Vector2.up);yield return null;Check(placement.TryPlace(),"valid placement consumed one wall");yield return Capture("placement-invalid");Check(!placement.TryPlace(),"overlapping placement rejected");
            Step("chest-door");Move(new Vector2(0,-22));ContentExpansionQa.Clear(bag.transform.position,8);var chest=ContentExpansionQa.Place("storage_chest",bag.transform.position+Vector3.right*2).GetComponent<StorageChest>();Check(bag.GetComponent<PlayerStorage>().OpenChest(chest),"chest UI opened");from=Enumerable.Range(0,bag.Model.Capacity).First(i=>!bag.Model.GetSlot(i).IsEmpty);Check(InventoryModel.Transfer(bag.Model,from,chest.Model,0,1),"chest transfer delivered one");yield return Capture("chest");save.World.Session.CloseInventory();var gate=ContentExpansionQa.Place("wood_door",bag.transform.position+Vector3.down*3).GetComponent<WorldDoor>();Check(gate.Toggle(),"door open accepted");yield return new WaitForSeconds(.4f);Check(!gate.Barrier.enabled,"door animation removed barrier");Check(gate.Toggle(),"door close accepted");yield return new WaitForSeconds(.4f);Check(gate.Barrier.enabled,"door animation restored barrier");
            Step("fishing-water");Move(new Vector2(save.World.Map.Continental.RiverAt(1000)+7,1000));Select("fishing_rod");bag.GetComponent<CharacterFacing>().Face(Vector2.left);yield return null;var fishing=bag.GetComponent<PlayerFishing>();Check(fishing.Use(),"cast near natural river");yield return Capture("fishing-cast");yield return new WaitForSeconds(3.1f);Check(fishing.Phase==FishingPhase.Bite,"bite phase reached");yield return Capture("fishing-bite");Check(fishing.Use(),"bite action caught fish");yield return new WaitForSeconds(.7f);Check(fishing.Phase==FishingPhase.Idle,"caught settled to idle");
            Select("waterskin");Check(bag.GetComponent<PlayerWaterskin>().Fill(),"Waterskin filled from natural river");bag.GetComponent<PlayerSurvival>().Model.SetMeters(70,20);Check(bag.GetComponent<PlayerWaterskin>().Drink(),"Waterskin consumed one charge");
            Step("torch-compass-cave");Select("torch");yield return null;Check(bag.GetComponent<PlayerLightSource>().Mount==PlayerLightMount.Held,"Torch held light");var cave=save.World.Map.Continental.Caves[0];Move(cave.SurfaceEntrances[0]);Check(CavernPortal.Travel(save.World,bag.gameObject,"cave0:0",false),"cave entry interaction");save.World.Chunks.DrainPending();yield return null;Check(save.World.Map.Continental.Underground(bag.transform.position),"underground region entered");Check(home.HasCompass,"Compass remains carried");yield return Capture("09-cave-torch-navigation");Check(CavernPortal.Travel(save.World,bag.gameObject,"cave0:0",true),"cave exit interaction");save.World.Chunks.DrainPending();yield return null;
            Step("save-load");Check(save.Save(),"isolated save written");Check(save.Load(),"isolated save loaded");save.World.Chunks.DrainPending();yield return null;Check(bag.GetComponent<PlayerEquipment>().Utility.Id=="lantern" && home.HasCompass,"save/load retained Utility/Compass");yield return Capture("10-reloaded");
        }
    }
}
