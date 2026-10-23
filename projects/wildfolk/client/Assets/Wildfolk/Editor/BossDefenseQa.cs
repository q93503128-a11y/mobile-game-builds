using System;
using System.Collections;
using System.IO;
using UnityEngine;
using Object=UnityEngine.Object;
namespace Wildfolk.Editor
{
    public static class BossDefenseQa
    {
        private static void Check(bool ok,string text){var f=Path.Combine(DevelopmentProfile.Directory,"boss-verification");Directory.CreateDirectory(f);File.AppendAllText(Path.Combine(f,"defenses.tsv"),(ok?"PASS\t":"FAIL\t")+text+"\n");if(!ok)throw new InvalidOperationException(text);}
        private static IEnumerator Delay(float seconds){var end=Time.time+seconds;while(Time.time<end){var h=BossProductionQa.Player.GetComponent<Health>();if(h.IsAlive)h.Heal(100);yield return null;}}
        public static IEnumerator Run()
        {
            var s=BossProductionQa.S;var p=s.Player;var world=BossProductionQa.World;s.Clock.Model.SetElapsed(301);Check(world.Summon(BossProductionQa.Tomb,p,1),"defense fixture summon");yield return Delay(3.9f);var b=BossProductionQa.Boss;b.enabled=false;
            foreach(var id in new[]{"spike_trap","bone_trap"})
            {
                var trap=ContentExpansionQa.Place(id,b.transform.position);Physics2D.SyncTransforms();var hp=b.Health.Current;yield return Delay(.2f);
                Check(b.Health.Current==hp-(id=="spike_trap"?16:32),"existing "+id+" applies real boss damage");if(trap!=null && trap.gameObject.activeSelf)PrefabPool.Release(trap.gameObject);
            }
            foreach(var id in new[]{"ballista","repeater"})
            {
                var gun=ContentExpansionQa.Place(id,(Vector2)b.transform.position+Vector2.down*4).GetComponent<WorldTurret>();ContentExpansionQa.Move((Vector2)gun.transform.position+Vector2.left*2);
                gun.Ammo.Add(gun.Definition.Ammunition,2);Check(gun.Execute(p,TurretOperation.Aim,0,0) && gun.Execute(p,TurretOperation.Reload),"normal turret aim and load "+id);yield return Delay(.2f);
                var hp=b.Health.Current;Check(gun.Execute(p,TurretOperation.Fire),"normal ammunition-consuming shot "+id);yield return Delay(.8f);Check(b.Health.Current==hp-gun.Definition.Damage && gun.AmmoCount()==1,"real projectile damage and exactly one ammo "+id);
                var wall=ContentExpansionQa.Place("wood_wall",(Vector2)b.transform.position+Vector2.down*2);Physics2D.SyncTransforms();Check(!gun.HasLineOfSight(b.Health),"structure blocks turret LOS "+id);PrefabPool.Release(wall.gameObject);
                yield return Delay(gun.Definition.ReloadSeconds);Check(gun.Execute(p,TurretOperation.Reload),"reload second real round "+id);
                ContentExpansionQa.Move(b.Home+Vector2.right*35);b.BeginReturn();hp=b.Health.Current;Check(gun.Fire(),"loaded turret fires while players are outside "+id);yield return Delay(.8f);Check(b.Health.Current==hp && b.State==BossState.Returning,"real projectile cannot damage or cancel Returning "+id);PrefabPool.Release(gun.gameObject);ContentExpansionQa.Move(b.Home+Vector2.down*5);b.enabled=true;yield return Delay(.5f);b.enabled=false;
            }
            var barrier=ContentExpansionQa.Place("wood_wall",b.Home+Vector2.down*2.5f);barrier.PlayerBuilt=true;b.transform.position=b.Home;b.GetComponent<Rigidbody2D>().position=b.Home;Physics2D.SyncTransforms();ContentExpansionQa.Move(b.Home+Vector2.down*5);b.enabled=true;
            var end=Time.time+12;while(barrier!=null && barrier.Health.IsAlive && Time.time<end)yield return Delay(.1f);Check(barrier==null || !barrier.gameObject.activeInHierarchy || !barrier.Health.IsAlive,"boss breaks a blocking player structure through normal Health");
        }
    }
}
