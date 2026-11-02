using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace Wildfolk.Tests
{
    public sealed class PostRcQualityPlayTests
    {
        private WorldSaveService save;private PlayerInventory player;
        [UnitySetUp]public IEnumerator Setup()
        {
            Time.timeScale=1;WorldGenerator.SelectedGenerationVersion=3;WorldGenerator.SelectedSeed=9472026;yield return SceneManager.LoadSceneAsync("Wildfolk");yield return null;yield return null;
            save=Object.FindFirstObjectByType<WorldSaveService>();save.AutoSaveEnabled=false;save.Clock.enabled=false;player=save.Player;player.GetComponent<PlayerSurvival>().enabled=false;player.GetComponent<PlayerInputAdapter>().enabled=false;
            save.GetComponent<ZombieSpawner>().enabled=false;save.GetComponent<GoblinSpawner>().enabled=false;
        }
        [UnityTearDown]public IEnumerator Teardown(){Time.timeScale=1;WorldGenerator.SelectedSeed=null;WorldGenerator.SelectedGenerationVersion=WorldGenerationVersion.Current;yield return null;}
        [UnityTest]public IEnumerator HandArmorMatchesHandCoverageAndBodyAxisThroughoutFacingAndAttack()
        {
            var e=player.GetComponent<PlayerEquipment>();var f=player.GetComponent<CharacterFacing>();var avatar=player.GetComponent<PlayerAvatar>();
            Assert.That(e.LeftHandGear.enabled,Is.False);
            foreach(var id in new[]{"leather_body","iron_body","steel_body","diamond_body"})
            {
                var item=save.Catalog.Item(id);player.Model.Add(item,1);Assert.That(e.TryEquip(item),Is.True);
                foreach(var direction in new[]{Vector2.down,Vector2.up,Vector2.right,Vector2.left,new Vector2(1,1)})
                {
                    f.Face(direction);avatar.MovingOverride=true;yield return new WaitForSeconds(.2f);
                    foreach(var gear in new[]{e.LeftHandGear,e.RightHandGear})
                    {
                        var hand=gear.transform.parent.GetComponent<SpriteRenderer>();Assert.That(gear.enabled,Is.True);
                        Assert.That(Vector3.Distance(gear.transform.position,hand.transform.position),Is.LessThan(.0001f));
                        var size=Vector3.Scale(gear.sprite.bounds.size,gear.transform.localScale);Assert.That(size.x,Is.EqualTo(hand.sprite.bounds.size.x).Within(.001f));Assert.That(size.y,Is.EqualTo(hand.sprite.bounds.size.y).Within(.001f));
                        Assert.That(Quaternion.Angle(gear.transform.rotation,e.ArmorVisual.transform.rotation),Is.LessThan(.1f));
                    }
                }
            }
            avatar.MovingOverride=false;var sword=save.Catalog.Item("iron_sword");player.Model.Add(sword,1);var slot=Enumerable.Range(0,9).First(i=>player.Model.GetSlot(i).Item==sword);player.SelectSlot(slot);player.GetComponent<PlayerInputCommands>().Attack();
            for(var i=0;i<18;i++){yield return null;Assert.That(Vector3.Distance(e.RightHandGear.transform.position,f.RightHand.position),Is.LessThan(.0001f));Assert.That(Quaternion.Angle(e.RightHandGear.transform.localRotation,Quaternion.Euler(0,0,180)),Is.LessThan(.001f));}
            Assert.That(e.HeldVisual.transform.IsChildOf(f.RightHand),Is.True);Assert.That(e.Unequip(ArmorSlot.Body),Is.True);Assert.That(e.LeftHandGear.enabled,Is.False);Assert.That(e.RightHandGear.enabled,Is.False);
        }
        [UnityTest]public IEnumerator RepairAndDismantleUseRealHoldCancellationCostsAndCompletion()
        {
            var point=(Vector2)player.transform.position+Vector2.left*2;foreach(var r in Object.FindObjectsByType<ResourceNode>(FindObjectsSortMode.None))if(Vector2.Distance(r.transform.position,point)<4)r.gameObject.SetActive(false);
            var place=Object.Instantiate(save.Catalog.Item("wood_wall").Placement.Prefab,point,Quaternion.identity);place.PlayerBuilt=true;place.Health.ApplyDamage(40,true);yield return new WaitForSeconds(3.2f);
            var hud=Object.FindFirstObjectByType<ClassHud>();var service=player.GetComponent<PlayerBuildingService>();var maintenance=place.GetComponent<StructureMaintenance>();var costs=maintenance.RepairCosts();foreach(var c in costs){var owned=player.Model.CountItem(c.Item);if(owned>0)player.Model.TryConsumeCosts(new[]{new ItemAmount{Item=c.Item,Count=(int)owned}});}yield return new WaitForSeconds(.25f);
            Assert.That(hud.RepairButton.interactable,Is.False);var unavailable=new PointerEventData(EventSystem.current);hud.RepairHold.OnPointerDown(unavailable);yield return new WaitForSeconds(.25f);Assert.That(service.Active,Is.False,"disabled/missing-cost repair must not start a hold");Assert.That(hud.HoldProgress.fillAmount,Is.Zero);hud.RepairHold.OnPointerUp(unavailable);
            foreach(var c in costs)player.Model.Add(c.Item,c.Count);yield return new WaitForSeconds(.25f);
            Assert.That(hud.BuildingView.activeSelf,Is.True);Assert.That(hud.BuildingMaterials.text,Does.Contain("/"));save.World.Session.OpenPause();Assert.That(hud.BuildingView.activeSelf,Is.False,"maintenance must disappear in the same frame as Pause");save.World.Session.Resume();yield return new WaitForSeconds(.25f);var hp=place.Health.Current;
            var pointer=new PointerEventData(EventSystem.current);hud.RepairHold.OnPointerDown(pointer);yield return new WaitForSeconds(.4f);Assert.That(hud.HoldProgress.fillAmount,Is.GreaterThan(0));hud.RepairHold.OnPointerUp(pointer);yield return null;
            Assert.That(service.Active,Is.False);Assert.That(hud.HoldProgress.fillAmount,Is.Zero);Assert.That(place.Health.Current,Is.EqualTo(hp));
            hud.RepairHold.OnPointerDown(pointer);yield return new WaitForSeconds(2.3f);hud.RepairHold.OnPointerUp(pointer);Assert.That(place.Health.Current,Is.EqualTo(place.Health.Maximum));
            var refund=maintenance.Refund();var before=refund.Select(c=>player.Model.CountItem(c.Item)).ToArray();hud.DismantleHold.OnPointerDown(pointer);yield return new WaitForSeconds(2.3f);hud.DismantleHold.OnPointerUp(pointer);yield return null;
            Assert.That(place==null||!place.gameObject.activeSelf,Is.True);for(var i=0;i<refund.Length;i++)Assert.That(player.Model.CountItem(refund[i].Item),Is.EqualTo(before[i]+refund[i].Count));
            Assert.That(Object.FindFirstObjectByType<PlayerFeedback>().Label.text,Does.Contain(L.Text("maintenance.salvaged")));
            var empty=new SavedStack[player.Model.Capacity];for(var i=0;i<empty.Length;i++)empty[i]=new SavedStack();player.Model.Restore(empty,save.Catalog);var sword=save.Catalog.Item("iron_sword");player.Model.Add(sword,player.Model.Capacity);
            var blocked=Object.Instantiate(save.Catalog.Item("wood_wall").Placement.Prefab,point,Quaternion.identity);blocked.PlayerBuilt=true;yield return new WaitForSeconds(.25f);hud.DismantleHold.OnPointerDown(pointer);yield return new WaitForSeconds(2.3f);hud.DismantleHold.OnPointerUp(pointer);
            Assert.That(blocked.Health.IsAlive,Is.True);Assert.That(blocked.gameObject.activeSelf,Is.True);Assert.That(player.Model.CountItem(sword),Is.EqualTo(player.Model.Capacity));Assert.That(Object.FindFirstObjectByType<PlayerFeedback>().Label.text,Is.EqualTo(L.Text("feedback.bag_full")));
        }
        [UnityTest]public IEnumerator HpTextLivesInsideHealthComponentAndPauseRetainsActions()
        {
            var hud=Object.FindFirstObjectByType<ProfileHud>();Assert.That(hud.HealthValue.transform.parent,Is.EqualTo(hud.HP.transform.parent));Assert.That(hud.HealthValue.rectTransform.anchoredPosition,Is.EqualTo(Vector2.zero));
            save.World.Session.OpenPause();yield return null;var menu=Object.FindFirstObjectByType<PauseMenu>();Assert.That(((RectTransform)menu.Menu.transform).rect.width,Is.LessThan(800));
            Assert.That(menu.Menu.GetComponentsInChildren<Button>().Count(),Is.EqualTo(4));menu.Menu.transform.Find("Settings").GetComponent<Button>().onClick.Invoke();Assert.That(menu.Settings.activeSelf,Is.True);menu.Back();
            menu.Menu.transform.Find("Continue").GetComponent<Button>().onClick.Invoke();Assert.That(save.World.Session.IsRunning,Is.True);
        }
        [UnityTest]public IEnumerator EditorSpeedRestoresWithoutChangingStatsAndShowcaseUsesNearPlayerGround()
        {
            var traversal=System.Type.GetType("Wildfolk.Editor.DeveloperTraversal, Assembly-CSharp-Editor",true);var settings=player.Settings;var speed=settings.MoveSpeed;
            var instance=(MonoBehaviour)traversal.GetMethod("Set").Invoke(null,new object[]{25});Assert.That(instance.GetType().GetProperty("Multiplier").GetValue(instance),Is.EqualTo(25));Assert.That(settings.MoveSpeed,Is.EqualTo(speed));
            instance.enabled=false;Assert.That(instance.GetType().GetProperty("Multiplier").GetValue(instance),Is.EqualTo(1));Assert.That(settings.MoveSpeed,Is.EqualTo(speed));
            var showcase=System.Type.GetType("Wildfolk.Editor.DeveloperShowcase, Assembly-CSharp-Editor",true);var args=new object[]{(Vector2)player.transform.position+Vector2.down*3.5f,.7f,null};
            Assert.That(showcase.GetMethod("Ground").Invoke(null,args),Is.EqualTo(true));var point=(Vector2)args[2];Assert.That(Vector2.Distance(point,player.transform.position),Is.LessThan(7));Assert.That(save.World.Map.IsWalkable(point,.7f),Is.True);yield return null;
        }
        [UnityTest]public IEnumerator ShowcaseClearDoesNotClaimAnActorAfterItsPooledInstanceIsReused()
        {
            var showcase=System.Type.GetType("Wildfolk.Editor.DeveloperShowcase, Assembly-CSharp-Editor",true);var harness=System.Type.GetType("Wildfolk.Editor.DeveloperHarness, Assembly-CSharp-Editor",true);
            var first=(Component)showcase.GetMethod("NearPlayer").Invoke(null,new object[]{"Zombie"});var instance=first.gameObject;PrefabPool.Release(instance);
            var reused=(Component)harness.GetMethod("Spawn").Invoke(null,new object[]{"Zombie",(Vector2)player.transform.position+Vector2.down*5,false});Assert.That(reused.gameObject,Is.SameAs(instance));
            Assert.That(showcase.GetMethod("Clear").Invoke(null,null),Is.EqualTo(0));Assert.That(reused.gameObject.activeSelf,Is.True,"a new population lease must survive clearing old QA actors");PrefabPool.Release(reused.gameObject);yield return null;
        }
    }
}
