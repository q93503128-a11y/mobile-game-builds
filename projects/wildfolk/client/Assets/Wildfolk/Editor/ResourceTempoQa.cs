using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace Wildfolk.Editor
{
    // Editor-only guided input scenario. Later teleports/injections are explicitly logged.
    public static class ResourceTempoQa
    {
        private static WorldSaveService s;private static PlayerInventory bag;private static PlayerInputCommands input;
        private static CraftingStation bench,furnace;private static Vector2 benchSite,furnaceSite;private static float started;public static string Phase {get;private set;}="idle";
        private static string Root=>Path.Combine(Application.dataPath,"../Captures/resource-tempo");
        public static void Begin()
        {
            s=DeveloperHarness.Context();if(!DevelopmentProfile.Isolated)throw new InvalidOperationException("An isolated profile must be selected before Play");
            bag=s.Player;input=bag.GetComponent<PlayerInputCommands>();bag.GetComponent<PlayerInputAdapter>().enabled=false;s.AutoSaveEnabled=false;
            started=Time.realtimeSinceStartup;Directory.CreateDirectory(Root);File.WriteAllText(Path.Combine(Root,"play-steps.tsv"),"realtime_seconds\tgame_seconds\tphase\tdetails\n");s.StartCoroutine(Guard(Flow()));
        }
        private static IEnumerator Guard(IEnumerator flow)
        {
            var stack=new Stack<IEnumerator>();stack.Push(flow);
            while(stack.Count>0){bool next;object value;try{next=stack.Peek().MoveNext();value=next ? stack.Peek().Current : null;}catch(Exception e){Phase="failed";Log(e.ToString());input.SetMove(Vector2.zero);yield break;}if(!next){stack.Pop();continue;}if(value is IEnumerator nested){stack.Push(nested);continue;}yield return value;}
            Phase="complete";Log("scenario complete");
        }
        private static void Log(string details)=>File.AppendAllText(Path.Combine(Root,"play-steps.tsv"),(Time.realtimeSinceStartup-started).ToString("F3",System.Globalization.CultureInfo.InvariantCulture)+"\t"+s.Clock.Model.Elapsed.ToString("F3",System.Globalization.CultureInfo.InvariantCulture)+"\t"+Phase+"\t"+details.Replace('\n',' ')+"\n");
        private static void Mark(string phase,string details=""){Phase=phase;Log(details);}
        private static int Count(string id)=>(int)bag.Model.CountItem(s.Catalog.Item(id));
        private static void Select(string id)
        {
            if(string.IsNullOrEmpty(id)){for(var i=0;i<9;i++)if(bag.Model.GetSlot(i).IsEmpty){bag.SelectSlot(i);return;}throw new InvalidOperationException("No bare-hand slot");}
            var item=s.Catalog.Item(id);for(var i=0;i<bag.Model.Capacity;i++)if(bag.Model.GetSlot(i).Item==item){if(i>=9){bag.Model.MoveStack(i,8);i=8;}bag.SelectSlot(i);return;}throw new InvalidOperationException("Missing "+id);
        }
        private static IEnumerator Walk(Vector2 destination,float stop=1.7f)
        {
            s.World.Session.CloseInventory();var until=Time.realtimeSinceStartup+90;var nav=bag.GetComponent<PlayerMotor>().Terrain;
            while(Vector2.Distance(bag.transform.position,destination)>stop)
            {
                if(Time.realtimeSinceStartup>until)throw new InvalidOperationException("Guided walk blocked at "+bag.transform.position+" toward "+destination);
                var p=(Vector2)bag.transform.position;var direction=(destination-p).normalized;var selected=direction;var found=false;
                foreach(var angle in new[]{0f,35,-35,70,-70,110,-110}){var d=(Vector2)(Quaternion.Euler(0,0,angle)*direction);if(nav.CanMove(p,d,.65f,.28f,bag.GetComponent<Collider2D>())){selected=d;found=true;break;}}
                input.SetMove(found ? selected : Vector2.zero);yield return null;
            }
            input.SetMove(Vector2.zero);yield return null;
        }
        private static IEnumerator Gather(string item,int needed,string tool)
        {
            Select(tool);
            while(Count(item)<needed)
            {
                var nodes=UnityEngine.Object.FindObjectsByType<ResourceIdentity>(FindObjectsSortMode.None).Where(n=>n.Node.Definition.DropItem.Id==item && n.Node.Definition.CanHarvest(bag.GetComponent<PlayerEquipment>().ActiveTool));
                var target=nodes.OrderBy(n=>Vector2.Distance(n.transform.position,bag.transform.position)).FirstOrDefault();
                if(target==null)throw new InvalidOperationException("No loaded source for "+item);
                if((item=="iron_ore" || item=="coal") && Count(item)==0)Log("first visible "+item+" distance="+Vector2.Distance(target.transform.position,bag.transform.position));
                yield return Walk(target.transform.position);var pos=(Vector2)target.transform.position;var deadline=Time.realtimeSinceStartup+45;
                while(target!=null && target.Node.Health.IsAlive){if(Time.realtimeSinceStartup>deadline)throw new InvalidOperationException("Harvest input failed "+item);bag.GetComponent<PlayerMotor>().Avatar.Facing.Face(pos-(Vector2)bag.transform.position);input.Attack();yield return null;}
                yield return new WaitForSeconds(.8f);var drops=WorldPickup.Active.Where(p=>p.Item!=null && p.Item.Id==item && Vector2.Distance(p.transform.position,pos)<4).ToArray();
                foreach(var drop in drops){if(drop!=null)yield return Walk(drop.transform.position,.35f);yield return new WaitForSeconds(.2f);}
                Log("harvested "+item+" total="+Count(item));
            }
        }
        private static void Craft(string id,CraftingStation station=null,bool inject=false)
        {
            s.World.Session.CloseInventory();var c=bag.GetComponent<PlayerCrafting>();if(station!=null){if(!c.OpenStation(station))throw new InvalidOperationException("Station unavailable");}else{s.World.Session.OpenInventory();c.OpenHandCrafting();}
            var r=s.Catalog.Recipe(id);if(inject)foreach(var a in r.Ingredients){var n=a.Count-(int)bag.Model.CountItem(a.Item);if(n>0){bag.Model.Add(a.Item,n);Log("fixture injection "+a.Item.Id+":"+n);}}
            var result=c.Craft(r);if(result!=CraftStatus.Ready)throw new InvalidOperationException("Craft "+id+" "+result);s.World.Session.CloseInventory();Log("crafted "+id);
        }
        private static WorldPlaceable Place(string id)
        {
            Select(id);var facing=bag.GetComponent<PlayerMotor>().Avatar.Facing;foreach(var direction in new[]{Vector2.down,Vector2.left,Vector2.right,Vector2.up}){facing.Face(direction);if(input.Attack())return WorldPlaceable.Active.Last(p=>p.Definition.Item.Id==id);}throw new InvalidOperationException("Placement blocked "+id);
        }
        private static IEnumerator Flow()
        {
            Mark("guided-input","fresh seed="+s.World.Seed+"; no teleport/material injection/time acceleration; exact source selection is scripted");
            yield return Gather("wood",6,"");Craft("workbench");bench=Place("workbench").GetComponent<CraftingStation>();benchSite=bench.transform.position;
            yield return Gather("wood",5,"");yield return Walk(bench.transform.position,2);Craft("axe",bench);
            yield return Gather("wood",5,"axe");yield return Walk(bench.transform.position,2);Craft("pickaxe",bench);
            yield return Gather("stone",4,"pickaxe");yield return Walk(bench.transform.position,2);Craft("stone_pickaxe",bench);Mark("stone-pickaxe");
            yield return Gather("iron_ore",12,"stone_pickaxe");Mark("iron-secured","12+ iron from two nodes; natural input stage complete");Capture("01-iron-harvest");yield return new WaitForEndOfFrame();
            Mark("fixture-progression","following travel uses teleports; remaining ingredients explicitly injected; processing time advanced through StationProcessing.Advance");
            yield return Fixture(benchSite+Vector2.right*2);Craft("furnace",bench,true);furnace=Place("furnace").GetComponent<CraftingStation>();furnaceSite=furnace.transform.position;
            Process("iron_ingot",12,furnace);Mark("iron-ingots");Craft("iron_pickaxe",bench,true);
            var crafting=bag.GetComponent<PlayerCrafting>();crafting.OpenStation(bench);foreach(var a in bench.Definition.UpgradeCosts){var n=a.Count-(int)bag.Model.CountItem(a.Item);if(n>0){bag.Model.Add(a.Item,n);Log("fixture upgrade injection "+a.Item.Id+":"+n);}}if(!crafting.UpgradeWorkbench())throw new InvalidOperationException("Upgrade failed");bench=crafting.Station;s.World.Session.CloseInventory();Mark("workbench-II");
            yield return MineFixture(7,"iron_pickaxe");Mark("coal-secured");yield return MineFixture(5,"iron_pickaxe");Mark("copper-secured");
            yield return Finish();
        }
        public static void ResumeFixture()
        {
            s=DeveloperHarness.Context();if(!DevelopmentProfile.Isolated)throw new InvalidOperationException("Isolated profile required");
            s.Apply(s.Repository.Read(s.Validate));bag=s.Player;input=bag.GetComponent<PlayerInputCommands>();bag.GetComponent<PlayerInputAdapter>().enabled=false;s.AutoSaveEnabled=false;
            var snapshot=s.Capture();benchSite=snapshot.Placeables.First(p=>s.Catalog.Item(p.Item).Placement.Station==StationKind.Workbench).Position;furnaceSite=snapshot.Placeables.First(p=>s.Catalog.Item(p.Item).Placement.Station==StationKind.Furnace).Position;
            started=Time.realtimeSinceStartup-68;Mark("fixture-resume","QA runner resumed from isolated save; setup/import pause excluded from scenario time");s.StartCoroutine(Guard(Finish()));
        }
        private static IEnumerator Finish()
        {
            yield return Fixture(benchSite+Vector2.right*2);Process("copper_ingot",4,furnace);Craft("prospecting_pan",bench,true);
            yield return MineFixture(6,"prospecting_pan");Mark("tin-secured");Capture("02-tin-pan");yield return new WaitForEndOfFrame();
            yield return Fixture(furnaceSite+Vector2.right*2);Process("tin_ingot",1,furnace);Process("bronze",1,furnace);Mark("bronze");
            yield return Fixture(benchSite+Vector2.right*2);Craft("blast_furnace",bench,true);var blast=Place("blast_furnace").GetComponent<CraftingStation>();Mark("blast-furnace");
            yield return Fixture(blast.transform.position+Vector3.right*2);Process("steel",3,blast);Mark("first-steel");
            yield return Fixture(benchSite+Vector2.right*2);Craft("steel_pickaxe",bench,true);Select("steel_pickaxe");Mark("steel-pickaxe");Capture("03-steel-equipment");yield return new WaitForEndOfFrame();
            if(!s.Save())throw new InvalidOperationException(s.LastError);Log("isolated save="+s.Repository.Path);Mark("ready-for-review");
        }
        private static void Process(string id,int n,CraftingStation station)
        {var p=station.GetComponent<StationProcessing>();var r=s.Catalog.Recipe(id);foreach(var a in r.Ingredients){var missing=a.Count*n-(int)bag.Model.CountItem(a.Item);if(missing>0){bag.Model.Add(a.Item,missing);Log("fixture process injection "+a.Item.Id+":"+missing);}}if(!p.Add(r,bag,n))throw new InvalidOperationException("Process rejected "+id);p.Advance(r.ProcessingSeconds*n+.1f);if(!p.Collect(bag))throw new InvalidOperationException("Collect rejected "+id);Log("processed "+id+":"+n);}
        private static IEnumerator Fixture(Vector2 point)
        {s.World.Session.CloseInventory();DeveloperHarness.Teleport(point);s.World.Chunks.Reconcile();s.World.Chunks.DrainPending();Log("fixture teleport "+point);yield return null;bench=WorldPlaceable.Active.FirstOrDefault(p=>p.Definition.Station==StationKind.Workbench)?.GetComponent<CraftingStation>();furnace=WorldPlaceable.Active.FirstOrDefault(p=>p.Definition.Station==StationKind.Furnace)?.GetComponent<CraftingStation>();}
        private static IEnumerator MineFixture(int rule,string tool)
        {var source=s.World.Map.Resources.First(p=>p.Rule==rule && p.Position.magnitude>300);yield return Fixture(source.Position+Vector2.down*2);yield return Gather(s.World.Definition.Resources[rule].Prefab.Definition.DropItem.Id,s.World.Definition.Resources[rule].Prefab.Definition.DropCount,tool);}
        public static void Capture(string name){Directory.CreateDirectory(Root);ScreenCapture.CaptureScreenshot(Path.Combine(Root,name+".png"));}
    }
}
