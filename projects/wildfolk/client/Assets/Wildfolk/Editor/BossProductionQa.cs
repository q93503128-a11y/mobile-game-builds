using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;
namespace Wildfolk.Editor
{
    // Explicit isolated QA; never attached to a shipped scene or prefab.
    public static class BossProductionQa
    {
        public static string Phase{get;private set;}="idle";
        public static WorldSaveService S=>ContentExpansionQa.S;
        public static PlayerInventory Player=>S.Player;
        public static BossWorld World=>S.World.GetComponent<BossWorld>();
        public static RoyalTomb Tomb=>Object.FindFirstObjectByType<RoyalTomb>();
        public static BossEncounter Boss=>World.Encounters.FirstOrDefault();
        private static string Folder=>Path.Combine(DevelopmentProfile.Directory,"boss-verification");
        private static bool heal;
        private static void Check(bool test,string label)
        {Directory.CreateDirectory(Folder);File.AppendAllText(Path.Combine(Folder,"checks.tsv"),DateTime.UtcNow.ToString("O")+"\t"+(test?"PASS":"FAIL")+"\t"+label+"\n");if(!test)throw new InvalidOperationException(label);}
        private static IEnumerator Wait(float seconds)
        {var end=Time.time+seconds;while(Time.time<end){if(heal && Player.GetComponent<Health>().IsAlive)Player.GetComponent<Health>().Heal(Player.GetComponent<Health>().Maximum);yield return null;}}
        private static IEnumerator Until(Func<bool> test,string label,float seconds=30)
        {var end=Time.time+seconds;while(!test() && Time.time<end)yield return Wait(.05f);Check(test(),label);}
        public static void Start(string scenario="Lifecycle")
        {
            if(!Application.isPlaying || !DevelopmentProfile.Isolated)throw new InvalidOperationException("Isolated Play required");
            var runner=new GameObject("Boss verification").AddComponent<DevelopmentCoroutineHost>();Object.DontDestroyOnLoad(runner.gameObject);runner.StartCoroutine(Guard(scenario));
        }
        public static IEnumerator Guard(string scenario)
        {
            var stack=new System.Collections.Generic.Stack<IEnumerator>();stack.Push(Run(scenario));
            while(stack.Count>0)
            {bool more;object value;try{more=stack.Peek().MoveNext();value=more?stack.Peek().Current:null;}catch(Exception e){Phase="failed";Directory.CreateDirectory(Folder);File.WriteAllText(Path.Combine(Folder,"failed.txt"),e.ToString());Time.timeScale=1;throw;}if(!more){stack.Pop();continue;}if(value is IEnumerator sub)stack.Push(sub);else yield return value;}
            Phase="complete";Time.timeScale=1;Directory.CreateDirectory(Folder);File.WriteAllText(Path.Combine(Folder,scenario+"-complete.txt"),DateTime.UtcNow.ToString("O"));
        }
        public static IEnumerator Run(string scenario)
        {
            if(!DevelopmentProfile.Isolated)throw new InvalidOperationException("Isolated save required");Phase="prepare";heal=false;SceneFlow.Repository=SaveRepository.Default();
            WorldGenerator.SelectedSeed=9472026;WorldGenerator.SelectedGenerationVersion=3;SceneManager.LoadScene(SceneFlow.GameplayScene);yield return null;
            ContentExpansionQa.Prepare();S.World.Animals.enabled=S.World.Renewal.enabled=false;Player.GetComponent<PlayerSurvival>().enabled=false;
            var a=S.World.Map.Continental.Anchors.First(x=>x.Kind==WorldAnchorKind.Graveyard);ContentExpansionQa.Move(a.Position+Vector2.down*3);yield return Wait(.3f);
            Check(Tomb!=null && !Tomb.Unlocked,"existing one guardian keeps tomb sealed");
            foreach(var id in new[]{"steel_sword","steel_head","steel_body"})Player.Model.Add(S.Catalog.Item(id),1);
            var gear=Player.GetComponent<PlayerEquipment>();gear.TryEquip(S.Catalog.Item("steel_head"));gear.TryEquip(S.Catalog.Item("steel_body"));ContentExpansionQa.Select("steel_sword",false);
            var guard=Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None).Single(e=>e.PersistentGuard);
            ContentExpansionQa.Move((Vector2)guard.transform.position+Vector2.down*1.2f);Player.GetComponent<PlayerAvatar>().Facing.Face(Vector2.up);Player.GetComponent<PlayerInputCommands>().Attack();yield return Wait(.5f);
            Check(!guard.OwnHealth.IsAlive && Tomb.Unlocked,"real melee guardian kill permanently unlocks tomb");
            Player.Model.Add(S.Catalog.Item("bone"),50);ContentExpansionQa.Move(a.Position+Vector2.down*2);S.Clock.Model.SetElapsed(1);
            var bones=Player.Model.CountItem(S.Catalog.Item("bone"));Check(!World.Summon(Tomb,Player,1) && bones==Player.Model.CountItem(S.Catalog.Item("bone")),"daytime tomb consumes nothing");
            Check(S.Save(),"unlocked tomb save");var saved=S.Capture();Check(saved.Version==14,"Save14 capture");S.Rules.Validate(saved);
            if(scenario=="Equipment")
            {
                var crown=S.Catalog.Item("undead_kings_crown");Player.Model.Add(crown,1);Check(gear.TryEquip(crown),"accessory equip uses inventory transaction");
                var hp=Player.GetComponent<Health>();hp.RestoreHealth(35);Check(Mathf.Approximately(Player.GetComponent<CharacterStats>().MovementMultiplier,1.08f),"crown movement at35 percent");
                Check(hp.ApplyDamage(28)==18,"armor then crown reduction once");hp.Heal(19);Check(Mathf.Approximately(Player.GetComponent<CharacterStats>().IncomingMultiplier,1),"healing above threshold clears crown effect");
                Check(S.Save(),"accessory save");var snap=S.Capture();PlayerSnapshotCodec.Apply(Player,snap.Player,S.Catalog);Check(gear.Accessory==crown,"accessory restore");Check(gear.Unequip(ArmorSlot.Accessory) && gear.Accessory==null && Player.Model.CountItem(crown)==1,"accessory unequip returns one item");yield break;
            }
            if(scenario=="Ui"){yield return BossUiRegressionQa.Run();yield break;}
            if(scenario=="Defense"){yield return BossDefenseQa.Run();yield break;}
            if(scenario=="Transactions"){yield return BossTransactionQa.Run();yield break;}
            S.Clock.Model.SetElapsed(301);Check(World.Summon(Tomb,Player,1),"Bone10 nighttime offering commits");Check(Player.Model.CountItem(S.Catalog.Item("bone"))==bones-10,"exact10 Bone consumed");
            Check(Boss.State==BossState.Awakening && Boss.Health.ApplyDamage(100,true)==0,"awakening blocks early damage");
            var first=Boss;Check(!World.Summon(Tomb,Player,1) && World.Encounters.Count==1,"same night duplicate summon rejected");
            heal=true;yield return Until(()=>Boss.State!=BossState.Awakening,"awakening finishes without input lock",8);
            Check(BossIdentity.Active!=null,"one continuous boss bar binds");Check(World.Suppresses(Boss.Home) && !World.Suppresses(Boss.Home+Vector2.right*100),"natural spawn suppression is local");
            if(scenario=="Visual"){yield return Visual();heal=false;yield break;}
            Phase="phase two";Boss.Health.ApplyDamage(Boss.Health.Current-6300,true,Player,Player.GetComponent<Health>());yield return Wait(2);
            Check(Boss.Phase==2 && Boss.LivingAdds==2 && Boss.Marked==Player.GetComponent<Health>(),"phase2 solo two adds and skull mark");
            var last=Boss.Marked;yield return Wait(2);Check(Boss.Marked==last,"skull mark has no expiry timer");
            Phase="phase three";Boss.Health.ApplyDamage(Boss.Health.Current-3150,true,Player,Player.GetComponent<Health>());yield return Wait(2);
            Check(Boss.Phase==3 && Boss.LivingAdds==2 && Boss.Marked!=null,"phase3 retains previous adds and mark");
            foreach(var m in Object.FindObjectsByType<BossMinion>(FindObjectsSortMode.None))if(m.Health.IsAlive)m.Health.ApplyDamage(m.Health.Current,true,Player,Player.GetComponent<Health>());
            yield return Wait(.7f);Check(Boss.LivingAdds==0 && Boss.Marked==null,"wave clear removes mark");
            var recycled=PrefabPool.Spawn(Boss.Definition.Minion,Boss.Home+Vector2.right*10,Quaternion.identity);Check(recycled.GetComponent<BossMinion>()!=null && recycled.enabled && recycled.Lifecycle.enabled && recycled.GetComponent<LootOnDeath>().enabled && recycled.PreferredTarget==null && Mathf.Approximately(recycled.MovementMultiplier,1),"pooled former boss add restores ordinary AI, lifecycle, loot and speed");PrefabPool.Release(recycled.gameObject);
            yield return Until(()=>Boss.Attack!=null && Boss.Attack.Id=="crownfall","phase3 Crownfall scheduled",40);
            Phase="return";var home=Boss.Home;ContentExpansionQa.Move(home+Vector2.right*35);yield return Wait(4.5f);Check(Boss.State==BossState.Returning,"grace then Returning after abandonment");
            var prior=Boss.Health.Current;Check(Boss.Health.ApplyDamage(100,true,Player)==0,"outside damage cannot cheese or cancel Returning");yield return Wait(4);
            Check(Boss.Health.Current>prior && Boss.Health.Current<9000,"gradual regeneration, not instant full heal");
            ContentExpansionQa.Move(home+Vector2.down*2);yield return Wait(.5f);Check(Boss.State!=BossState.Returning,"territory re-entry resumes");
            S.Clock.Model.SetElapsed(500);yield return Wait(.3f);Check(Boss!=null && Boss.Health.IsAlive,"live king survives day transition");
            Check(S.Save(),"active encounter save");var live=S.Capture();S.Apply(live);yield return Wait(.5f);Check(World.Encounters.Count==1 && Boss.Record.LastNight==1,"load retains single active encounter and consumed night");
            ContentExpansionQa.Move(home+Vector2.down*2);yield return Wait(.6f);
            // A cloned QA reward table forces proc; the canonical table remains20%.
            var original=Boss.Definition;var copy=Object.Instantiate(original);copy.Rewards=Object.Instantiate(original.Rewards);var rare=copy.Rewards.RareItems;rare[0].Chance=1;copy.Rewards.RareItems=rare;Boss.Definition=copy;
            Phase="death";Boss.Health.ApplyDamage(Boss.Health.Current,true,Player,Player.GetComponent<Health>());Check(Boss.Record.PendingReward,"death rolls one pending transaction");
            var dying=S.Capture();S.Rules.Validate(dying);Check(S.Save(),"dying pending roll persisted");yield return Wait(3.4f);
            Check(World.Encounters.Count==0,"death / ash / loot sequence completes");Check(Total("boss_core_t1")==1 && Total("undead_kings_crown")==1,"one guaranteed core and one forced crown shared physical set");
            Check(S.Save(),"resolved loot persisted");var resolved=S.Capture();S.Apply(resolved);yield return Wait(.5f);Check(Total("boss_core_t1")==1 && Total("undead_kings_crown")==1,"load cannot duplicate resolved loot");
            Check(!World.Summon(Tomb,Player,2),"day still rejects summon");S.Clock.Model.SetElapsed(781);ContentExpansionQa.Move(a.Position+Vector2.down*2);
            Check(World.Summon(Tomb,Player,2),"next night accepts a new Bone10 offering");
            Object.Destroy(copy.Rewards);Object.Destroy(copy);heal=false;Phase="complete";
        }
        private static IEnumerator Visual(){yield return BossVisualQa.Run();}
        private static long Total(string id)
        {long count=Player.Model.CountItem(S.Catalog.Item(id));foreach(var p in WorldPickup.Active)if(p.Item!=null && p.Item.Id==id)count+=p.Count;foreach(var p in S.World.Chunks.DormantPickups())if(p.Item==id)count+=p.Count;return count;}
    }
}
