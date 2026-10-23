using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;
namespace Wildfolk.Editor
{
    // Deterministic input bot, ordinary combat/armor/medicine; no HP or boss-state forcing.
    public static class BossCombatQa
    {
        public static string Phase{get;private set;}="idle";
        private static readonly float[] AvoidAngles={55,-55,100,-100};
        private static string Folder=>Path.Combine(DevelopmentProfile.Directory,"combat");
        public static void Start(string gear="steel")
        {
            if(!Application.isPlaying || !DevelopmentProfile.Isolated)throw new InvalidOperationException("Isolated Play required");
            var runner=new GameObject("Boss combat verification").AddComponent<DevelopmentCoroutineHost>();Object.DontDestroyOnLoad(runner.gameObject);runner.StartCoroutine(Guard(Run(gear)));
        }
        private static IEnumerator Guard(IEnumerator flow)
        {
            var stack=new System.Collections.Generic.Stack<IEnumerator>();stack.Push(flow);
            while(stack.Count>0){bool more;object v;try{more=stack.Peek().MoveNext();v=more?stack.Peek().Current:null;}catch(Exception e){Phase="failed";Directory.CreateDirectory(Folder);File.WriteAllText(Path.Combine(Folder,"failed.txt"),e.ToString());Time.timeScale=1;yield break;}if(!more){stack.Pop();continue;}if(v is IEnumerator sub)stack.Push(sub);else yield return v;}
            Phase="complete";Time.timeScale=1;
        }
        private static IEnumerator Run(string gear)
        {
            Phase="prepare";ClassProfiles.DevelopmentProviders(new LocalAccountProgressionStore(Path.Combine(DevelopmentProfile.Directory,"class-store")));ClassProfiles.Store.Write(ClassProfiles.LocalId,AccountProgression.New());SceneFlow.Repository=SaveRepository.Default();WorldGenerator.SelectedSeed=9472026;WorldGenerator.SelectedGenerationVersion=3;SceneManager.LoadScene(SceneFlow.GameplayScene);yield return null;
            ContentExpansionQa.Prepare();var s=ContentExpansionQa.S;var p=s.Player;var hp=p.GetComponent<Health>();var world=s.World.GetComponent<BossWorld>();
            ClassProfiles.DevelopmentProviders(new LocalAccountProgressionStore(Path.Combine(DevelopmentProfile.Directory,"class-store")));
            s.World.Animals.enabled=s.World.Renewal.enabled=false;
            var a=s.World.Map.Continental.Anchors.First(x=>x.Kind==WorldAnchorKind.Graveyard);ContentExpansionQa.Move(a.Position+Vector2.down*3);yield return null;
            var guard=Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None).Single(e=>e.PersistentGuard);guard.OwnHealth.ApplyDamage(guard.OwnHealth.Current,true,p,hp);yield return new WaitForSeconds(.7f);
            var equip=p.GetComponent<PlayerEquipment>();foreach(var suffix in new[]{"_head","_body","_sword"})p.Model.Add(s.Catalog.Item(gear+suffix),1);equip.TryEquip(s.Catalog.Item(gear+"_head"));equip.TryEquip(s.Catalog.Item(gear+"_body"));
            p.Model.Add(s.Catalog.Item("bone"),10);p.Model.Add(s.Catalog.Item("healing_salve"),12);ContentExpansionQa.Select(gear+"_sword",false);s.Clock.Model.SetElapsed(301);
            ContentExpansionQa.Move(a.Position+Vector2.down*2);var tomb=Object.FindFirstObjectByType<RoyalTomb>();if(!world.Summon(tomb,p,1))throw new InvalidOperationException("Summon rejected");
            var boss=world.Encounters.Single();ContentExpansionQa.Move(boss.Home+Vector2.down*4);p.GetComponent<PlayerMotor>().enabled=true;var commands=p.GetComponent<PlayerInputCommands>();var avatar=p.GetComponent<PlayerAvatar>();
            Directory.CreateDirectory(Folder);var path=Path.Combine(Folder,gear+".tsv");File.WriteAllText(path,"UTC\tgameSeconds\tbossHP\tplayerHP\tadds\tstate\tsalves\n");
            Time.timeScale=2;var began=Time.time;var wall=Time.realtimeSinceStartup;var nextLog=0f;var nextAdds=0f;BossMinion[] adds=Array.Empty<BossMinion>();var hits=0;hp.Damaged+=()=>hits++;
            Phase=gear;
            while(boss!=null && boss.Health.IsAlive && hp.IsAlive && Time.time-began<540)
            {
                var elapsed=Time.time-began;
                if(elapsed>=nextLog){nextLog=elapsed+5;File.AppendAllText(path,DateTime.UtcNow.ToString("O")+"\t"+elapsed+"\t"+boss.Health.Current+"\t"+hp.Current+"\t"+boss.LivingAdds+"\t"+boss.State+"\t"+p.Model.CountItem(s.Catalog.Item("healing_salve"))+"\n");}
                if(hp.Current<=60 && p.Model.CountItem(s.Catalog.Item("healing_salve"))>0 && Time.time>=p.GetComponent<PlayerSurvival>().UseReadyAt)
                {ContentExpansionQa.Select("healing_salve",false);commands.Attack();ContentExpansionQa.Select(gear+"_sword",false);}
                if(Time.time>=nextAdds){nextAdds=Time.time+.25f;adds=Object.FindObjectsByType<BossMinion>(FindObjectsSortMode.None);}
                var origin=(Vector2)p.transform.position;var delta=origin-(Vector2)boss.transform.position;var move=Vector2.zero;var evade=false;var attack=boss.Attack;
                if((boss.State==BossState.Windup || boss.State==BossState.Strike) && attack!=null)
                {
                    var side=new Vector2(-boss.Direction.y,boss.Direction.x);if(Vector2.Dot(side,delta)<0)side=-side;
                    var along=Vector2.Dot(delta,boss.Direction);var across=Mathf.Abs(delta.x*boss.Direction.y-delta.y*boss.Direction.x);
                    if(attack.Shape==BossAttackShape.Circle){evade=delta.magnitude<attack.Range+.6f;move=delta.normalized;}
                    else if(attack.Shape==BossAttackShape.Line){evade=along>-.5f && along<attack.Range+.5f && across<attack.Width*.5f+.65f;move=side;}
                    else {evade=delta.magnitude<attack.Range+.5f && Vector2.Angle(boss.Direction,delta)<attack.Angle*.5f+18;move=side;}
                }
                if(!evade)
                {
                    Health target=boss.Health;var distance=25f;foreach(var m in adds)if(m!=null && m.Health.IsAlive){var d=((Vector2)m.transform.position-origin).sqrMagnitude;if(d<distance){distance=d;target=m.Health;}}
                    var dtarget=(Vector2)target.transform.position-origin;var reach=target==boss.Health?1.9f:1.6f;
                    move=dtarget.magnitude>reach?dtarget.normalized:Vector2.zero;
                    var opening=target!=boss.Health || boss.State==BossState.Recovery || boss.State==BossState.Ritual || boss.State==BossState.Transition;
                    if(opening && dtarget.magnitude<reach+.15f){avatar.Facing.Face(dtarget);commands.Attack();}
                }
                if(move.sqrMagnitude>.01f && !s.World.GetComponent<WorldNavigation>().CanMove(p.transform.position,move,.15f,.3f,p.GetComponent<Collider2D>()))
                {var wanted=move;move=Vector2.zero;foreach(var angle in AvoidAngles){var side=(Vector2)(Quaternion.Euler(0,0,angle)*wanted);if(s.World.GetComponent<WorldNavigation>().CanMove(p.transform.position,side,.15f,.3f,p.GetComponent<Collider2D>())){move=side;break;}}}
                commands.SetMove(move);yield return null;
            }
            commands.SetMove(Vector2.zero);var success=boss!=null && !boss.Health.IsAlive;var duration=Time.time-began;var real=Time.realtimeSinceStartup-wall;
            File.WriteAllText(Path.Combine(Folder,gear+"-result.json"),"{\"clear\":"+(success?"true":"false")+",\"gameSeconds\":"+duration.ToString(System.Globalization.CultureInfo.InvariantCulture)+",\"wallSeconds\":"+real.ToString(System.Globalization.CultureInfo.InvariantCulture)+",\"hits\":"+hits+",\"playerHP\":"+hp.Current+",\"bossHP\":"+(boss==null?0:boss.Health.Current)+",\"salvesUsed\":"+(12-p.Model.CountItem(s.Catalog.Item("healing_salve")))+",\"timeScale\":2}");
            if(!success)throw new InvalidOperationException("Combat bot did not clear; see measured telemetry");yield return new WaitForSeconds(3.5f);
        }
    }
}
