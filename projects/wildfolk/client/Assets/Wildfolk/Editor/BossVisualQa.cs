using System.Collections;
using System.IO;
using UnityEngine;
namespace Wildfolk.Editor
{
    public static class BossVisualQa
    {
        public static IEnumerator Run()
        {
            var p=BossProductionQa.Player;var b=BossProductionQa.Boss;var h=p.GetComponent<Health>();
            // Capture a controlled presentation fixture; this is not a timed natural clear.
            var folder=Path.GetFullPath("Captures/undead-king-boss/screens");Directory.CreateDirectory(folder);VisualGameView.ShowReference();
            ContentExpansionQa.Move(b.Home+Vector2.down*3.8f);yield return Frame(folder,"tomb-and-king-1920");
            var until=Time.time+30;while(!(b.State==BossState.Windup && b.Attack.Id=="royal_cleave") && Time.time<until){h.Heal(100);yield return null;}
            yield return Frame(folder,"cleave-1920");
            b.Health.ApplyDamage(b.Health.Current-6300,true,p,h);yield return Delay(1.8f);yield return Frame(folder,"phase2-mark-1920");
            VisualGameView.Show(1280,720);yield return Delay(.3f);yield return Frame(folder,"phase2-mark-1280");
            foreach(var m in Object.FindObjectsByType<BossMinion>(FindObjectsSortMode.None))if(m.Health.IsAlive)m.Health.ApplyDamage(1000,true,p,h);
            b.Health.ApplyDamage(b.Health.Current-3150,true,p,h);yield return Delay(1.7f);until=Time.time+40;
            while(!(b.State==BossState.Windup && b.Attack.Id=="crownfall") && Time.time<until){h.Heal(100);yield return null;}yield return Frame(folder,"crownfall-1280");
            VisualGameView.ShowReference();ContentExpansionQa.Move(b.Home+Vector2.right*30);yield return Delay(4.6f);
            ContentExpansionQa.Move((Vector2)b.transform.position+Vector2.down*25);yield return Frame(folder,"returning-1920");
            ContentExpansionQa.Move(b.Home+Vector2.down*3.8f);yield return Delay(.5f);var crown=BossProductionQa.S.Catalog.Item("undead_kings_crown");p.Model.Add(crown,1);p.GetComponent<PlayerEquipment>().TryEquip(crown);h.RestoreHealth(35);
            p.GetComponent<PlayerInputCommands>().Session.OpenInventory();yield return Frame(folder,"accessory-1920");VisualGameView.Show(1280,720);yield return Delay(.2f);yield return Frame(folder,"accessory-1280");p.GetComponent<PlayerInputCommands>().Session.CloseInventory();
            h.Heal(100);b.Health.ApplyDamage(b.Health.Current,true,p,h);yield return Delay(.8f);yield return Frame(folder,"death-1280");yield return Delay(2.7f);yield return Frame(folder,"loot-1280");VisualGameView.ShowReference();
        }
        private static IEnumerator Delay(float seconds){var end=Time.time+seconds;while(Time.time<end){var h=BossProductionQa.Player.GetComponent<Health>();if(h.IsAlive)h.Heal(100);yield return null;}}
        private static IEnumerator Frame(string folder,string name){yield return null;ScreenCapture.CaptureScreenshot(Path.Combine(folder,name+".png"));yield return null;}
    }
}
