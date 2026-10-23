using System;
using System.Collections;
using System.IO;
using UnityEngine;
namespace Wildfolk.Editor
{
    public static class BossTransactionQa
    {
        private static void Check(bool condition,string label){var folder=Path.Combine(DevelopmentProfile.Directory,"boss-verification");Directory.CreateDirectory(folder);File.AppendAllText(Path.Combine(folder,"transactions.tsv"),(condition?"PASS\t":"FAIL\t")+label+"\n");if(!condition)throw new InvalidOperationException(label);}
        private static long Core(){long n=BossProductionQa.Player.Model.CountItem(BossProductionQa.S.Catalog.Item("boss_core_t1"));foreach(var p in WorldPickup.Active)if(p.Item!=null && p.Item.Id=="boss_core_t1")n+=p.Count;foreach(var p in BossProductionQa.S.World.Chunks.DormantPickups())if(p.Item=="boss_core_t1")n+=p.Count;return n;}
        public static IEnumerator Run()
        {
            var s=BossProductionQa.S;var w=BossProductionQa.World;var p=s.Player;var good=s.Repository;var hp=p.GetComponent<Health>();var bones=p.Model.CountItem(s.Catalog.Item("bone"));
            var folder=Path.Combine(DevelopmentProfile.Directory,"blocked");File.WriteAllText(folder,"file deliberately blocks directory creation");var bad=new SaveRepository(Path.Combine(folder,"world.json"));
            s.Clock.Model.SetElapsed(301);s.Repository=bad;
            Check(!w.Summon(BossProductionQa.Tomb,p,1) && w.Encounters.Count==0 && w.Record("undead_king").LastNight==0 && p.Model.CountItem(s.Catalog.Item("bone"))==bones,"failed offering rolls back actor, night identity and Bone");
            s.Repository=good;Check(w.Summon(BossProductionQa.Tomb,p,1),"retry offering after write recovery");yield return new WaitForSeconds(3.9f);
            var boss=BossProductionQa.Boss;boss.Health.ApplyDamage(boss.Health.Current,true,p,hp);var pending=s.Capture();var roll=JsonUtility.ToJson(pending.Bosses[0]);Check(pending.Bosses[0].PendingReward,"dying snapshot retains one already-rolled reward");
            s.Apply(pending);yield return null;Check(JsonUtility.ToJson(BossProductionQa.Boss.Record)==roll,"pending save reload keeps exact roll and eligibility");yield return new WaitForSeconds(3.4f);Check(Core()==1 && w.Encounters.Count==0,"pending death reload resolves one physical core");
            var resolved=s.Capture();s.Apply(resolved);yield return null;Check(Core()==1 && w.Encounters.Count==0,"resolved replay cannot spawn another reward");
            s.Clock.Model.SetElapsed(781);ContentExpansionQa.Move((Vector2)BossProductionQa.Tomb.transform.position+Vector2.down*2);Check(w.Summon(BossProductionQa.Tomb,p,2),"second night transaction fixture");yield return new WaitForSeconds(3.9f);boss=BossProductionQa.Boss;
            boss.Health.ApplyDamage(boss.Health.Current,true,p,hp);var stored=string.Join("|",Array.ConvertAll(boss.Record.RolledLoot,v=>v.Item+":"+v.Count));var growth=p.GetComponent<PlayerProgression>();var xp=growth.Experience;var gold=growth.Gold;
            s.Repository=bad;Check(!w.ResolveReward(boss),"failed reward write defers commit");Check(Core()==1 && growth.Experience==xp && growth.Gold==gold && boss.Record.PendingReward && string.Join("|",Array.ConvertAll(boss.Record.RolledLoot,v=>v.Item+":"+v.Count))==stored,"failed reward rollback restores XP, Gold and fixed roll, removes provisional pickups");
            s.Repository=good;yield return new WaitForSeconds(3.4f);Check(Core()==2 && growth.Experience==xp+120 && growth.Gold==gold+35,"write recovery retries once with existing roll");
            resolved=s.Capture();s.Rules.Validate(resolved);s.Apply(resolved);yield return null;Check(Core()==2 && w.Encounters.Count==0,"final reload preserves exactly two night rewards");
        }
    }
}
