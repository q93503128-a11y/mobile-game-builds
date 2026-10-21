using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace Wildfolk.Editor
{
    // Explicit isolated fixture, shared by headless checks and the live MCP Game View.
    public static class CreatureSidegradeQa
    {
        public static string Phase{get;private set;}="idle";
        private static string Root=>Path.GetFullPath("Captures/creature-sidegrade-expansion");
        private static WorldSaveService s;private static PlayerInventory bag;private static NetworkSession host;private static NetworkPlayer actor;
        private static void Log(string text){Directory.CreateDirectory(Root);File.AppendAllText(Root+"/live.tsv",DateTime.UtcNow.ToString("O")+"\t"+Phase+"\t"+text+"\n");}
        private static void Check(bool ok,string text){Log((ok ? "PASS " : "FAIL ")+text);if(!ok)throw new InvalidOperationException(text);}
        private static void Step(string text){Phase=text;Log("begin");}
        private static IEnumerator Guard(IEnumerator flow)
        {var stack=new Stack<IEnumerator>();stack.Push(flow);while(stack.Count>0){bool more;object value;try{more=stack.Peek().MoveNext();value=more ? stack.Peek().Current : null;}catch(Exception e){Phase="failed";Log(e.ToString());yield break;}if(!more){stack.Pop();continue;}if(value is IEnumerator child)stack.Push(child);else yield return value;}Phase="complete";Log("complete");}
        private static IEnumerator Capture(string name)
        {if(Application.isBatchMode)yield break;yield return new WaitForSeconds(.3f);ScreenCapture.CaptureScreenshot(Root+"/"+name+".png");yield return new WaitForEndOfFrame();yield return null;}
        public static void Begin()
        {
            ContentExpansionQa.Prepare();s=ContentExpansionQa.S;bag=s.Player;s.World.Animals.enabled=s.World.Renewal.enabled=false;s.World.Animals.Restore(Array.Empty<AnimalSnapshot>(),new AnimalEcologySnapshot());Application.runInBackground=true;Phase="starting";s.StartCoroutine(Guard(Single()));
        }
        private static void Select(string id){ContentExpansionQa.Select(id);}
        private static Health Target(float surface,float x=0)
        {
            var p=(Vector2)bag.transform.position+Vector2.up*surface+Vector2.right*x;var g=s.World.GetComponent<GoblinSpawner>().SpawnActor(false,p,"sidegrade",p);g.enabled=false;var b=g.GetComponent<Rigidbody2D>();b.bodyType=RigidbodyType2D.Kinematic;b.linearVelocity=Vector2.zero;var c=g.GetComponent<Collider2D>();g.transform.position=p+Vector2.up*c.bounds.extents.y;Physics2D.SyncTransforms();return g.OwnHealth;
        }
        private static IEnumerator Strike(string id,Health target,int expected,string capture=null)
        {
            Select(id);s.World.Session.CloseInventory();var combat=bag.GetComponent<PlayerCombat>();var f=bag.GetComponent<PlayerAvatar>().Facing;f.Face(Vector2.up);yield return new WaitForSeconds(1);var before=target.Current;var stamina=bag.GetComponent<PlayerLocomotion>();stamina.Restore(100,0);Check(bag.GetComponent<PlayerPrimaryAction>().TryUse(),"actual Primary attack "+id);Check(!bag.GetComponent<PlayerPrimaryAction>().TryUse(),"same-frame duplicate rejected");
            yield return new WaitForSeconds(combat.Equipment.ActiveTool.Duration*.5f+.025f);
            Check(before-target.Current==expected,id+" damage "+(before-target.Current)+" expected "+expected);Check(combat.LastHitCount==(expected>0 ? 1 : 0),"one hit at most");Check(stamina.Stamina.Current==100,"melee has no stamina cost");
            if(id=="hunters_spear" && capture!=null)
            {
                var visual=bag.GetComponent<PlayerEquipment>().HeldVisual;var tip=visual.transform.TransformPoint(new Vector3(0,visual.sprite.bounds.max.y,0));var delta=tip-bag.transform.position;Check(Mathf.Abs(delta.y-3.35f)<.16f && Mathf.Abs(delta.x)<.12f,"visible tip "+delta+" matches3.35 reach");
                if(!Application.isBatchMode){ScreenCapture.CaptureScreenshot(Root+"/"+capture+".png");yield return new WaitForEndOfFrame();}
            }
            yield return new WaitForSeconds(.95f);Check(before-target.Current==expected,"no later duplicate damage");
        }
        private static IEnumerator Single()
        {
            Step("craft");L.SetLanguage("ko");ContentExpansionQa.Move(Vector2.zero);ContentExpansionQa.Clear(Vector2.zero,15);bag.Model.Restore(new InventoryModel(bag.Model.Capacity).Capture(),s.Catalog);ContentExpansionQa.OpenBench(1);var craft=bag.GetComponent<PlayerCrafting>();var recipe=s.Catalog.Recipe("hunters_spear");Check(craft.Craft(recipe)!=CraftStatus.Ready,"WB I rejects WBII sidegrade");ContentExpansionQa.OpenBench(2);foreach(var cost in recipe.Ingredients)bag.Model.Add(cost.Item,cost.Count);craft.SetSelected(recipe);yield return Capture("01-recipe-ko");L.SetLanguage("en");yield return Capture("02-recipe-en");L.SetLanguage("ko");
            var panel=UnityEngine.Object.FindFirstObjectByType<CraftingPanel>();panel.SelectCategory((int)RecipeCategory.Weapons);panel.ToggleFilter();yield return Capture("02a-recipe-costs-ko");
            Check(ContentExpansionQa.Craft("hunters_spear")==1,"actual craft card creates one Hunter");foreach(var cost in recipe.Ingredients)Check(bag.Model.CountItem(cost.Item)==0,"craft consumed "+cost.Item.Id);craft.CloseCrafting();Select("hunters_spear");s.World.Session.OpenInventory();yield return Capture("03-inventory");s.World.Session.CloseInventory();Check(bag.GetComponent<PlayerEquipment>().ActiveTool==recipe.Result.Tool,"selected gear resolves Tool");
            Step("reach");ContentExpansionQa.Move(new Vector2(0,-7));ContentExpansionQa.Clear(bag.transform.position,10);var t=Target(3.15f);yield return Strike("iron_spear",t,0);yield return Strike("steel_spear",t,0);yield return Strike("hunters_spear",t,27,"04-hunter-impact");t.gameObject.SetActive(false);
            t=Target(2.5f);yield return Strike("steel_spear",t,35);t.gameObject.SetActive(false);t=Target(2.5f);yield return Strike("diamond_spear",t,42);t.gameObject.SetActive(false);
            t=Target(3.42f);yield return Strike("hunters_spear",t,0);t.gameObject.SetActive(false);t=Target(3.34f);yield return Strike("hunters_spear",t,27);t.gameObject.SetActive(false);
            Step("walls");t=Target(3.1f);var wall=ContentExpansionQa.Place("wood_wall",(Vector2)bag.transform.position+Vector2.up*1.7f);yield return Strike("hunters_spear",t,0);yield return Capture("05-wall-block");wall.gameObject.SetActive(false);yield return Strike("hunters_spear",t,27);t.gameObject.SetActive(false);
            t=Target(3.1f);Select("hunters_spear");var combat=bag.GetComponent<PlayerCombat>();bag.GetComponent<PlayerAvatar>().Facing.Face(Vector2.up);Check(combat.TryAttack(),"begin before obstruction");wall=ContentExpansionQa.Place("wood_wall",(Vector2)bag.transform.position+Vector2.up*1.7f);yield return new WaitForSeconds(1);Check(t.Current==t.Maximum && combat.LastHitCount==0,"impact recheck rejects new wall");wall.gameObject.SetActive(false);t.gameObject.SetActive(false);
            Step("multi-target");t=Target(2.8f);var duplicate=new GameObject("Additional hurtbox");duplicate.transform.SetParent(t.transform,false);duplicate.AddComponent<CircleCollider2D>().isTrigger=true;var other=Target(3.1f,.1f);yield return Strike("hunters_spear",t,27,"06-single-target");Check(other.Current==other.Maximum,"second enemy untouched");t.gameObject.SetActive(false);other.gameObject.SetActive(false);t=Target(3,1.5f);yield return Strike("hunters_spear",t,0);t.gameObject.SetActive(false);
            Step("vest");var stats=bag.GetComponent<CharacterStats>();bag.Model.Add(s.Catalog.Item("carapace_vest"),1);Check(bag.GetComponent<PlayerEquipment>().TryEquip(s.Catalog.Item("carapace_vest")),"Vest equip");Check(Mathf.Abs(stats.RecoveryMultiplier-1.15f)<.001f,"Vest still15percent");bag.GetComponent<PlayerSurvival>().Meal.Begin(s.Catalog.Item("trail_broth"));Check(Mathf.Abs(stats.RecoveryMultiplier-1.25f)<.001f,"Vest and Meal add");bag.GetComponent<PlayerSurvival>().ResetTransient();Check(bag.GetComponent<PlayerEquipment>().Unequip(ArmorSlot.Body),"Vest unequip removes modifier");Check(Mathf.Abs(stats.RecoveryMultiplier-1)<.001f,"Vest modifier gone");
            Step("save-load");Select("hunters_spear");var selected=bag.SelectedIndex;Check(s.Save(),"isolated Save11");var game=s.Repository.Read(s.Validate);Check(game.Version==11,"no Save version change");s.Apply(game);yield return null;Check(bag.SelectedIndex==selected && bag.SelectedStack.Item.Id=="hunters_spear" && bag.GetComponent<PlayerEquipment>().ActiveTool==recipe.Result.Tool,"selected Hunter restored from Item ID");yield return Capture("07-loaded");var empty=Enumerable.Range(0,9).First(i=>bag.Model.GetSlot(i).IsEmpty);bag.SelectSlot(empty);Check(bag.GetComponent<PlayerEquipment>().ActiveTool==null && !bag.GetComponent<PlayerEquipment>().HeldVisual.enabled,"empty hand removes weapon");yield return Capture("08-unequipped");
        }
        public static void BeginHost()
        {if(!DevelopmentProfile.Isolated)throw new InvalidOperationException("Isolated QA required");UnityEngine.Object.FindFirstObjectByType<HomeScreen>().OpenMultiplayer();host=NetworkSession.Instance;host.WorldRepository=new SaveRepository(Root+"/host/"+Guid.NewGuid().ToString("N")+"/world.json");host.ProfileOverride=Guid.NewGuid().ToString("N");host.NewWorldGenerationVersion=3;Application.runInBackground=true;Phase="starting";host.StartCoroutine(Guard(HostFlow()));}
        private static IEnumerator EnterHost()
        {var task=host.Host(ConnectionRoute.LocalNetwork);yield return new WaitUntil(()=>task.IsCompleted);Check(task.Result,"local Host "+host.LastFailure);Check(host.EnterGame(),"enter multiplayer");var until=Time.realtimeSinceStartup+45;while(Time.realtimeSinceStartup<until){actor=NetworkPlayer.Active.FirstOrDefault(p=>p.IsSpawned && p.NetworkObject.IsPlayerObject);if(actor!=null && NetworkCreatureWorld.Active!=null && NetworkCreatureWorld.Active.Ready)break;yield return null;}Check(actor!=null && NetworkCreatureWorld.Active.Ready,"Host world ready");actor.Motor.enabled=actor.GetComponent<PlayerInputAdapter>().enabled=false;var w=NetworkCreatureWorld.Active;w.Clock.enabled=w.Animals.enabled=w.Goblins.enabled=w.Night.enabled=w.World.Renewal.enabled=false;w.World.Chunks.DrainPending();yield return null;}
        private static IEnumerator HostFlow()
        {
            Step("host");yield return EnterHost();var w=NetworkCreatureWorld.Active;var c=w.Catalog;bag=actor.GetComponent<PlayerInventory>();bag.Model.Restore(new InventoryModel(bag.Model.Capacity).Capture(),c);var p=(Vector2)actor.transform.position;
            foreach(var r in UnityEngine.Object.FindObjectsByType<ResourceNode>(FindObjectsSortMode.None))if(Vector2.Distance(p,r.transform.position)<10)r.gameObject.SetActive(false);
            var bench=NetworkSpawnFactory.Spawn(c.Item("workbench").Placement.Prefab,p+Vector2.down*2,Quaternion.identity).GetComponent<CraftingStation>();w.FlushPending();yield return null;var craft=actor.GetComponent<PlayerCrafting>();Check(craft.OpenStation(bench),"Host opens WB");foreach(var cost in bench.Definition.UpgradeCosts)bag.Model.Add(cost.Item,cost.Count);Check(craft.UpgradeWorkbench(),"Host upgrades WBII");var recipe=c.Recipe("hunters_spear");foreach(var cost in recipe.Ingredients)bag.Model.Add(cost.Item,cost.Count);Check(craft.Craft(recipe)==CraftStatus.Ready,"Host authority crafts Hunter");craft.CloseCrafting();actor.Commands.Session.CloseInventory();var slot=Enumerable.Range(0,9).First(i=>bag.Model.GetSlot(i).Item==recipe.Result);bag.SelectSlot(slot);actor.Motor.Avatar.Facing.Face(Vector2.up);yield return null;
            Check(actor.Status.Value.Held==Array.IndexOf(c.Items,recipe.Result),"Held index replicated");var g=w.Goblins.SpawnActor(false,p+Vector2.up*3.55f,"sidegrade-host",p+Vector2.up*3.55f);g.enabled=false;g.GetComponent<Rigidbody2D>().bodyType=RigidbodyType2D.Kinematic;w.FlushPending();yield return null;Physics2D.SyncTransforms();var h=g.OwnHealth;var hp=h.Current;
            Check(actor.Attack(),"Host Primary");Check(!actor.Attack(),"duplicate during attack rejected");yield return new WaitForSeconds(.4f);Check(hp-h.Current==27,"Host actual range damage27");Check(actor.AttackState.Value.Tool==Array.IndexOf(c.Items,recipe.Result),"attack Tool reference replicated");Check(g.GetComponent<NetworkCreature>().Frame.Value.HP==h.Current,"damage replicated");yield return Capture("09-host-impact");
            var seq=actor.AttackState.Value.Sequence;Check(!actor.ExecutePrimary(1),"stale Primary sequence rejected");Check(actor.AttackState.Value.Sequence==seq,"replay publishes no attack");yield return new WaitForSeconds(.7f);Check(host.TrySaveWorld(),"Host saves selected Hunter");host.Leave();yield return new WaitUntil(()=>host.State==NetworkSessionState.Idle);yield return new WaitForSecondsRealtime(.3f);
            Step("rehost");yield return EnterHost();bag=actor.GetComponent<PlayerInventory>();Check(bag.SelectedStack.Item!=null && bag.SelectedStack.Item.Id=="hunters_spear","reHost selected Hunter restored");Check(actor.Equipment.ActiveTool==c.Item("hunters_spear").Tool,"reHost exact Tool data");Check(bag.Model.CountItem(c.Item("bone"))==0 && bag.Model.CountItem(c.Item("iron_spear"))==0,"craft costs not refunded/replayed");yield return Capture("10-rehost");
        }
    }
}
