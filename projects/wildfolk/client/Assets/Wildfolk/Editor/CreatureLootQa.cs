using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Wildfolk.Editor
{
    // Explicit isolated, live Game View fixture; never runs in player builds.
    public static class CreatureLootQa
    {
        public static string Phase {get;private set;}="idle";
        private static WorldSaveService s;private static PlayerInventory bag;
        private static string Root=>Path.GetFullPath("Captures/creature-loot-expansion");
        private static void Log(string text){Directory.CreateDirectory(Root);File.AppendAllText(Root+"/live.tsv",DateTime.UtcNow.ToString("O")+"\t"+Phase+"\t"+text.Replace('\n',' ')+"\n");}
        private static void Check(bool value,string text){Log((value?"PASS ":"FAIL ")+text);if(!value)throw new InvalidOperationException(text);}
        private static void Step(string name){Phase=name;Log("begin");}
        public static void Begin(){ContentExpansionQa.Prepare();s=ContentExpansionQa.S;bag=s.Player;s.World.Animals.enabled=false;s.World.Renewal.enabled=false;Application.runInBackground=true;Directory.CreateDirectory(Root);File.WriteAllText(Root+"/live.tsv","utc\tphase\tresult\n");s.StartCoroutine(Guard(Flow()));}
        private static IEnumerator Guard(IEnumerator flow){var stack=new Stack<IEnumerator>();stack.Push(flow);while(stack.Count>0){bool more;object value;try{more=stack.Peek().MoveNext();value=more ? stack.Peek().Current : null;}catch(Exception e){Phase="failed";Log(e.ToString());yield break;}if(!more){stack.Pop();continue;}if(value is IEnumerator child)stack.Push(child);else yield return value;}Phase="complete";Log("live flow complete");}
        private static IEnumerator Capture(string name){ScreenCapture.CaptureScreenshot(Root+"/"+name+".png");yield return new WaitForEndOfFrame();yield return null;}
        private static void Move(Vector2 p)=>ContentExpansionQa.Move(p);
        private static void Empty(){bag.Model.Restore(new InventoryModel(bag.Model.Capacity).Capture(),s.Catalog);bag.SelectSlot(0);}
        private static void Give(string id,int count=1)=>bag.Model.Add(s.Catalog.Item(id),count);
        private static WildlifeActionBrain Spawn(string id,Vector2 p,string group="qa")
        {var rule=s.World.Animals.Definition.Rules.Single(r=>r.Prefab.Member.Definition.Id==id);var a=UnityEngine.Object.Instantiate(rule.Prefab,p,Quaternion.identity);a.Player=bag.transform;a.Session=s.World.Session;a.Member.Anchor=p;a.Member.GroupId=group;a.Separation.Player=bag.transform;var brain=a.GetComponent<WildlifeActionBrain>();brain.Bind(s.World.GetComponent<WorldNavigation>());var loot=a.GetComponent<LootOnDeath>();loot.Player=bag;loot.Pickup=s.World.Pickup;loot.Settings=s.World.Settings;return brain;}
        private static IEnumerator WaitPhase(WildlifeActionBrain a,WildlifePhase phase,float seconds=8){var until=Time.time+seconds;while(a!=null && a.Phase!=phase && Time.time<until)yield return null;Check(a!=null && a.Phase==phase,a.Animal.Member.Definition.Id+" reached "+phase);}
        private static Vector2 Habitat(CreatureDefinition d)
        {
            if((d.Habitat&(CreatureHabitat.Caverns|CreatureHabitat.DeepCaverns|CreatureHabitat.CrystalPocket))!=0)foreach(var cave in s.World.Map.Continental.Caves)foreach(var room in cave.Rooms)for(var x=-3;x<=3;x++)for(var y=-3;y<=3;y++){var p=room.Center+new Vector2(x,y);if(d.Matches(s.World.Map,p))return p;}
            for(var ring=0;ring<=2400;ring+=24)for(var n=0;n<Math.Max(1,ring/5);n++){var p=new Vector2(Mathf.Cos(n*2.399963f),Mathf.Sin(n*2.399963f))*ring;if(d.Matches(s.World.Map,p))return p;}
            throw new InvalidOperationException("No authored habitat for "+d.Id);
        }
        private static IEnumerator Craft(string id)
        {var recipe=s.Catalog.Recipe(id);foreach(var c in recipe.Ingredients)bag.Model.Add(c.Item,c.Count);var craft=bag.GetComponent<PlayerCrafting>();craft.SetSelected(recipe);yield return null;var panel=UnityEngine.Object.FindFirstObjectByType<CraftingPanel>();panel.Cards.Single(c=>c.Recipe==recipe).CraftButton.onClick.Invoke();if(recipe.ProcessingSeconds>0){Check(craft.Processor.IsBusy,"queue began "+id);yield return Capture("processing-"+id);yield return new WaitForSeconds(recipe.ProcessingSeconds/craft.Station.Definition.ProcessingSpeed+.25f);Check(craft.Processor.Collect(bag),"collected "+id);}Check(bag.Model.CountItem(recipe.Result)>0,"UI crafted "+id);}
        private static IEnumerator Flow()
        {
            Step("gallery");L.SetLanguage("ko");Move(Vector2.zero);ContentExpansionQa.Clear(Vector2.zero,20);s.World.Animals.Restore(Array.Empty<AnimalSnapshot>(),new AnimalEcologySnapshot());Empty();var gallery=new List<WildlifeActionBrain>();var ids=new[]{"bat","stone_crawler","bog_leech","wolf","mountain_goat","crow","giant_boar","crystal_crawler"};
            for(var i=0;i<ids.Length;i++){var a=Spawn(ids[i],new Vector2(-6+i%4*4,4-i/4*5));a.enabled=false;a.PresentReplica(0,0,Vector2.down,Time.time,false);gallery.Add(a);}yield return new WaitForSeconds(.3f);yield return Capture("01-eight-species");foreach(var a in gallery)UnityEngine.Object.Destroy(a.gameObject);yield return null;
            foreach(var id in ids)
            {
                Step(id);var definition=s.World.Animals.Definition.Rules.Single(r=>r.Prefab.Member.Definition.Id==id).Prefab.Member.Definition;var p=Habitat(definition);Move(p+Vector2.down*2);ContentExpansionQa.Clear(p,15);s.World.Animals.Restore(Array.Empty<AnimalSnapshot>(),new AnimalEcologySnapshot());var actor=Spawn(id,p);var env=s.World.Map.Continental.Environment(p);Check(definition.Matches(s.World.Map,p),"authored habitat at "+p+" biome="+env.Biome+" water="+env.WaterDistance+" tags="+env.Tags);bag.GetComponent<Health>().RestoreHealth(bag.GetComponent<Health>().Maximum);
                if(definition.Actions.Attack!=WildlifeAttack.None){yield return WaitPhase(actor,WildlifePhase.Warning);yield return Capture(id+"-warning");if(definition.Actions.Camouflage)Check(actor.Body.sprite==actor.RestSprite,"stone disguise during first warning");yield return WaitPhase(actor,WildlifePhase.Action);yield return Capture(id+"-action");yield return WaitPhase(actor,WildlifePhase.Recovery);Check(actor.Awakened,"unfolded / active");yield return Capture(id+"-recovery");}
                else{var start=actor.transform.position;yield return new WaitForSeconds(.8f);Check(Vector2.Distance(start,actor.transform.position)>.2f,"passive escape displacement");yield return Capture(id+"-flee");}
                if(id=="wolf"){var pack=Spawn(id,(Vector2)actor.transform.position+Vector2.right*4,"pack");actor.Animal.Member.GroupId="pack";actor.Animal.Health.ApplyDamage(1);yield return new WaitForSeconds(.25f);Check(pack.Phase!=WildlifePhase.Idle,"same pack joined");yield return Capture("wolf-pack");UnityEngine.Object.Destroy(pack.gameObject);}
                actor.Animal.Health.ApplyDamage(10000,true);yield return Capture(id+"-death");yield return new WaitForSeconds(HitFeedback.DeathDuration+.05f);foreach(var pickup in WorldPickup.Active.ToArray())if(pickup!=null && pickup.gameObject.activeInHierarchy && pickup.Count>0 && Vector2.Distance(pickup.transform.position,p)<16){var item=pickup.Item;var before=bag.Model.CountItem(item);Log("DROP "+item.Id+" x"+pickup.Count);Move(pickup.transform.position);yield return new WaitForSeconds(.9f);Check(bag.Model.CountItem(item)>before,"automatic pickup "+item.Id);}Check(actor==null || !actor.gameObject.activeInHierarchy,"death returns pooled actor");
            }
            Step("crafting");Move(Vector2.zero);ContentExpansionQa.Clear(Vector2.zero,20);Empty();var crafting=bag.GetComponent<PlayerCrafting>();var bench=ContentExpansionQa.Place("workbench_ii",new Vector2(2,0)).GetComponent<CraftingStation>();crafting.OpenStation(bench);yield return Craft("bone_arrow");yield return Craft("bone_trap");yield return Craft("barbed_bolt");yield return Craft("carapace_vest");yield return Capture("02-workbench-materials");crafting.CloseCrafting();s.World.Session.OpenInventory();crafting.OpenHandCrafting();yield return Craft("healing_salve");yield return Capture("03-salve-hand");crafting.CloseCrafting();
            foreach(var station in new[]{"campfire","cooking_pot","furnace","blast_furnace"})
            {var obj=ContentExpansionQa.Place(station,new Vector2(-2,0));crafting.OpenStation(obj.GetComponent<CraftingStation>());if(station=="campfire")yield return Craft("trail_ration");if(station=="cooking_pot"){yield return Craft("bone_broth");yield return Craft("restorative_broth");}if(station=="furnace")yield return Craft("bronze");if(station=="blast_furnace")yield return Craft("steel");yield return Capture("station-"+station);crafting.CloseCrafting();UnityEngine.Object.Destroy(obj.gameObject);yield return null;}
            Step("consume");bag.GetComponent<Health>().ApplyDamage(40,true);ContentExpansionQa.Select("healing_salve");var hp=bag.GetComponent<Health>().Current;Check(bag.GetComponent<PlayerPrimaryAction>().TryUse(),"salve through primary action");Check(bag.GetComponent<Health>().Current==hp+30,"salve restored30HP");Check(!bag.GetComponent<PlayerSurvival>().TryConsume(),"shared4s consumption cooldown");yield return Capture("04-food-effects");
            Step("camp");Empty();var spawner=s.World.GetComponent<GoblinSpawner>();var camp=spawner.SpawnCamp(new Vector2(-4,2),"qa-live-camp",0,false);Move(camp.Location.position+Vector3.down);var guard=spawner.SpawnActor(false,new Vector2(-4,4),"qa-live-camp",new Vector2(-4,2),"qa-live-camp");Check(!camp.Secured && !camp.Take(bag,0,0),"living guard blocks loot");yield return Capture("05-camp-guarded");guard.OwnHealth.ApplyDamage(10000,true);yield return new WaitForSeconds(.4f);Check(camp.Use(bag.GetComponent<PlayerInteraction>()),"open take-only camp modal");yield return Capture("06-camp-supplies");var revision=camp.State.LootRevision;var amount=camp.State.Loot[0].Count;Check(UnityEngine.Object.FindFirstObjectByType<CampLootPanel>().Take(0),"loot button claimed entire stack");Check(!camp.Take(bag,0,revision),"replayed claim rejected");Check(bag.Model.CountItem(s.Catalog.Item("bone_arrow"))==amount,"claimed ammunition in bag exactly once");yield return Capture("07-camp-remaining");CampLootPanel.CloseFocused();var saved=camp.State.LootRevision;Check(s.Save(),"isolated Save11 writes camp state: "+s.LastError);var game=s.Repository.Read(s.Validate);s.Apply(game);yield return null;s.World.Chunks.DrainPending();camp=spawner.Camps.Single(c=>c.State.Id=="qa-live-camp");Check(camp.State.LootRevision==saved && camp.State.LootOpened && (camp.State.Loot[0]==null || camp.State.Loot[0].Count==0),"load keeps opened state and spent slot");yield return Capture("08-loaded-camp");
        }
        private static NetworkSession host;private static NetworkPlayer hostPlayer;
        public static void BeginCombat()
        {ContentExpansionQa.Prepare();s=ContentExpansionQa.S;bag=s.Player;s.World.Animals.enabled=s.World.Renewal.enabled=false;s.StartCoroutine(Guard(CombatFlow(s.World,bag,s.World.Session,"sp")));}
        public static void BeginHostCombat()
        {
            if(!DevelopmentProfile.Isolated)throw new InvalidOperationException("Isolated QA required");
            UnityEngine.Object.FindFirstObjectByType<HomeScreen>().OpenMultiplayer();host=NetworkSession.Instance;
            host.WorldRepository=new SaveRepository(Root+"/combat-host/"+Guid.NewGuid().ToString("N")+"/world.json");host.ProfileOverride=Guid.NewGuid().ToString("N");host.NewWorldGenerationVersion=3;
            host.StartCoroutine(Guard(HostCombatFlow()));
        }
        private static IEnumerator HostCombatFlow()
        {yield return Host();yield return CombatFlow(NetworkCreatureWorld.Active.World,hostPlayer.GetComponent<PlayerInventory>(),NetworkCreatureWorld.Active.Session,"host");}
        private static IEnumerator CombatFlow(WorldGenerator world,PlayerInventory player,GameplaySession session,string mode)
        {
            Step(mode+"-actual-damage");world.Animals.enabled=world.Renewal.enabled=false;world.Animals.Restore(Array.Empty<AnimalSnapshot>(),new AnimalEcologySnapshot());
            var p=world.Map.Spawn;player.GetComponent<PlayerSurvival>().enabled=false;var health=player.GetComponent<Health>();health.DamageReduction=0;
            foreach(var id in new[]{"bat","stone_crawler","bog_leech","wolf","giant_boar","crystal_crawler"})
            {
                player.transform.position=p;player.GetComponent<Rigidbody2D>().position=p;world.Chunks.Reconcile();world.Chunks.DrainPending();
                foreach(var r in UnityEngine.Object.FindObjectsByType<ResourceNode>(FindObjectsSortMode.None))if(Vector2.Distance(p,r.transform.position)<20)r.gameObject.SetActive(false);
                health.RestoreHealth(health.Maximum);var hp=health.Current;
                var rule=world.Animals.Definition.Rules.Single(r=>r.Prefab.Member.Definition.Id==id);
                var position=p+Vector2.right*(id=="stone_crawler" ? 1.6f : id=="giant_boar" ? 3 : 2);
                var actor=NetworkSpawnFactory.Spawn(rule.Prefab,position,Quaternion.identity);actor.Player=player.transform;actor.Session=session;actor.Region=world.Region;actor.Member.Anchor=position;actor.Member.GroupId="qa-combat";actor.Separation.Player=player.transform;
                var brain=actor.GetComponent<WildlifeActionBrain>();brain.Bind(world.GetComponent<WorldNavigation>());NetworkCreatureWorld.Active?.FlushPending();Physics2D.SyncTransforms();
                var until=Time.time+5;while(brain.Impacts==0 && Time.time<until)yield return null;
                Check(brain.Impacts==1 && health.Current==hp-brain.Definition.Damage,id+" damage="+(hp-health.Current)+" expected="+brain.Definition.Damage+" impacts="+brain.Impacts);
                yield return Capture(mode+"-hit-"+id);
                while(brain.Phase==WildlifePhase.Action && Time.time<until)yield return null;
                Check(brain.Impacts==1,id+" one hit through recovery");
                var net=actor.GetComponent<NetworkCreature>();if(net!=null && net.IsSpawned)net.NetworkObject.Despawn();else UnityEngine.Object.Destroy(actor.gameObject);yield return null;
            }
        }
        public static void BeginHost()
        {if(!DevelopmentProfile.Isolated)throw new InvalidOperationException("Isolated QA required");UnityEngine.Object.FindFirstObjectByType<HomeScreen>().OpenMultiplayer();host=NetworkSession.Instance;host.WorldRepository=new SaveRepository(Root+"/host/"+Guid.NewGuid().ToString("N")+"/world.json");host.ProfileOverride=Guid.NewGuid().ToString("N");host.NewWorldGenerationVersion=3;host.StartCoroutine(Guard(HostFlow()));}
        private static IEnumerator Host()
        {var task=host.Host(ConnectionRoute.LocalNetwork);yield return new WaitUntil(()=>task.IsCompleted);Check(task.Result,"local Host starts: "+host.LastFailure);Check(host.EnterGame(),"enter multiplayer world");var until=Time.realtimeSinceStartup+45;while(Time.realtimeSinceStartup<until){hostPlayer=NetworkPlayer.Active.FirstOrDefault(p=>p.IsSpawned && p.NetworkObject.IsPlayerObject);if(hostPlayer!=null && NetworkCreatureWorld.Active!=null && NetworkCreatureWorld.Active.Ready)break;yield return null;}Check(hostPlayer!=null && NetworkCreatureWorld.Active.Ready,"authoritative world ready");hostPlayer.Motor.enabled=hostPlayer.GetComponent<PlayerInputAdapter>().enabled=false;var world=NetworkCreatureWorld.Active;world.Clock.enabled=world.Animals.enabled=world.Goblins.enabled=world.Night.enabled=false;world.World.Chunks.DrainPending();yield return null;}
        private static IEnumerator HostFlow()
        {
            Step("local-host");yield return Host();var world=NetworkCreatureWorld.Active;var p=(Vector2)hostPlayer.transform.position;var camp=world.Goblins.SpawnCamp(p+Vector2.left*2,"qa-mcp-host-camp",0,false);world.FlushPending();yield return null;var net=camp.GetComponent<NetworkCamp>();Check(net.IsServer && net.IsSpawned && net.Loot.Count==6,"network camp and server loot list");hostPlayer.transform.position=camp.Location.position+Vector3.down;hostPlayer.GetComponent<Rigidbody2D>().position=hostPlayer.transform.position;Physics2D.SyncTransforms();var bag=hostPlayer.GetComponent<PlayerInventory>();var request=hostPlayer.GetComponent<NetworkFacilityInteractor>();var inventory=hostPlayer.GetComponent<NetworkInventory>();var revision=camp.State.LootRevision;var oldBag=inventory.Revision.Value;var amount=camp.State.Loot[0].Count;var before=bag.Model.CountItem(world.Catalog.Item("bone_arrow"));Check(camp.Use(hostPlayer.GetComponent<PlayerInteraction>()),"network loot modal opens");yield return Capture("09-host-loot");Check(request.CampLoot(camp,0,camp.State.LootRevision),"owner Host claim accepted");Check(!request.ExecuteCampLoot(net.NetworkObjectId,0,revision,oldBag),"old target and bag revisions reject duplicate");Check(bag.Model.CountItem(world.Catalog.Item("bone_arrow"))==before+amount,"Host inventory receives one roll");CampLootPanel.CloseFocused();var state=JsonUtility.ToJson(camp.State);Check(host.TrySaveWorld(),"Host isolated save11");yield return Capture("10-host-claimed");host.Leave();yield return new WaitUntil(()=>host.State==NetworkSessionState.Idle);yield return new WaitForSecondsRealtime(.3f);Step("rehost");yield return Host();camp=NetworkCreatureWorld.Active.Goblins.Camps.Single(c=>c.State.Id=="qa-mcp-host-camp");Check(JsonUtility.ToJson(camp.State)==state,"reHost retains opened/revision/all remaining slots");Check(hostPlayer.GetComponent<PlayerInventory>().Model.CountItem(NetworkCreatureWorld.Active.Catalog.Item("bone_arrow"))==before+amount,"reHost keeps inventory reward exactly once");yield return Capture("11-rehost-camp");
        }
    }
}
