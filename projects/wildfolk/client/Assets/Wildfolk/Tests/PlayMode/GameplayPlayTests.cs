using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Wildfolk.Tests
{
    public sealed class GameplayPlayTests
    {
        private PlayPointerDriver driver;
        private PlayerMotor player;
        private PlayerInventory inventory;
        private GameplayHud hud;
        private GameplaySettings settings;
        private float leftPunchTravel;
        private float rightPunchTravel;

        [UnitySetUp] public IEnumerator LoadFreshGame()
        {
            Time.timeScale=1;
            driver=new PlayPointerDriver();
            WorldGenerator.SelectedSeed=9472026;yield return SceneManager.LoadSceneAsync("Wildfolk");
            yield return null;
            player=Object.FindFirstObjectByType<PlayerMotor>();
            inventory=player.GetComponent<PlayerInventory>();
            hud=Object.FindFirstObjectByType<GameplayHud>();
            settings=player.Settings;
            for(var i=0;i<inventory.Model.Capacity;i++) Assert.That(inventory.Model.GetSlot(i).IsEmpty,Is.True);
            // Scope the prior tool regression to test fixtures; the real new game starts empty.
            foreach(var recipe in player.GetComponent<PlayerCrafting>().Catalog.Recipes)
                if(recipe.Result.Id=="axe" || recipe.Result.Id=="pickaxe") inventory.Model.Add(recipe.Result,1);
            inventory.SelectSlot(2);
            player.Avatar.Facing.Face(Vector2.up);
            yield return new WaitForSeconds(.2f);
        }

        [UnityTearDown] public IEnumerator ReleaseInput()
        {
            driver?.Dispose();
            Time.timeScale=1;
            yield return null;
        }

        private static Vector2 ScreenPoint(Component component) => PlayPointerDriver.Point(component);
        private void Pointer(Vector2 position,bool pressed) => driver.Pointer(position,pressed);
        private IEnumerator Click(Component component) => driver.Click(component);

        private IEnumerator Walk(Vector2 direction,float seconds)
        {
            var point=ScreenPoint(player.Joystick);
            Pointer(point,true);yield return null;
            Pointer(point+direction.normalized*110,true);yield return null;
            yield return new WaitForSeconds(seconds);
            Pointer(point+direction.normalized*110,false);yield return null;yield return null;
        }

        private void PositionPlayer(Vector2 position)
        {
            var body=player.GetComponent<Rigidbody2D>();
            body.position=position;body.linearVelocity=Vector2.zero;
            player.transform.position=position;
            Physics2D.SyncTransforms();
        }

        private static ResourceNode Resource(string itemId,float x,float y)
        {
            ResourceNode selected=null;var closest=float.MaxValue;
            foreach(var node in Object.FindObjectsByType<ResourceNode>(FindObjectsSortMode.None))
            {
                if(node.Definition.DropItem.Id!=itemId) continue;
                var distance=Vector2.Distance(node.transform.position,new Vector2(x,y));
                if(distance>=closest) continue;
                selected=node;closest=distance;
            }
            WorldTestScene.MoveResource(selected,new Vector2(x,y));return selected;
        }

        private int Total(string id)
        {
            var total=0;
            for(var i=0;i<inventory.Model.Capacity;i++)
            { var slot=inventory.Model.GetSlot(i);if(!slot.IsEmpty && slot.Item.Id==id) total+=slot.Count; }
            return total;
        }

        [UnityTest] public IEnumerator PointerDragMovesDiagonallyReturnsThumbAndCameraFollows()
        {
            var start=player.transform.position;var camera=Camera.main.transform.position;
            yield return Walk(new Vector2(-1,1),.45f);
            Assert.That(player.transform.position.x,Is.LessThan(start.x-.3f));
            Assert.That(player.transform.position.y,Is.GreaterThan(start.y+.3f));
            Assert.That(player.Joystick.Value.sqrMagnitude,Is.LessThan(.0001f));
            Assert.That(player.Joystick.Thumb.anchoredPosition.sqrMagnitude,Is.LessThan(.0001f));
            yield return new WaitForSeconds(.3f);
            Assert.That(Vector2.Distance(camera,Camera.main.transform.position),Is.GreaterThan(.3f));
            Assert.That(Vector2.Distance(player.transform.position,Camera.main.transform.position),Is.LessThan(.25f));
            Assert.That(player.GetComponent<Rigidbody2D>().linearVelocity.sqrMagnitude,Is.LessThan(.001f));
        }

        [UnityTest] public IEnumerator InventoryButtonsOpenCloseAndModalBlocksGameplayInput()
        {
            var panel=Object.FindFirstObjectByType<InventoryPanel>();
            Assert.That(panel.View.activeSelf,Is.False);
            yield return driver.Inventory(hud);
            Assert.That(panel.View.activeSelf,Is.True);Assert.That(Time.timeScale,Is.EqualTo(1));
            var position=player.transform.position;
            yield return Click(hud.Attack);
            Assert.That(player.Combat.IsAttacking,Is.False);
            Assert.That(Vector2.Distance(position,player.transform.position),Is.LessThan(.005f));
            Assert.That(player.GetComponent<Rigidbody2D>().linearVelocity.sqrMagnitude,Is.LessThan(.001f));
            yield return Click(panel.Slots[0].GetComponent<Button>());
            Assert.That(panel.Slots[0].Selection.enabled,Is.True);
            yield return Click(panel.View.transform.Find("Inventory Panel/Close").GetComponent<Button>());
            Assert.That(panel.View.activeSelf,Is.False);Assert.That(Time.timeScale,Is.EqualTo(1));
            yield return driver.Inventory(hud);yield return Click(hud.InventoryButton);
            Assert.That(panel.View.activeSelf,Is.True);yield return driver.Inventory(hud);Assert.That(panel.View.activeSelf,Is.False);
        }

        private IEnumerator PunchOnce()
        {
            var left=player.Avatar.Facing.LeftHand.localPosition;var right=player.Avatar.Facing.RightHand.localPosition;
            leftPunchTravel=rightPunchTravel=0;
            var point=ScreenPoint(hud.Attack);
            Pointer(point,true);yield return null;
            var end=Time.time+settings.PunchDuration;
            while(Time.time<end)
            {
                leftPunchTravel=Mathf.Max(leftPunchTravel,Vector3.Distance(left,player.Avatar.Facing.LeftHand.localPosition));
                rightPunchTravel=Mathf.Max(rightPunchTravel,Vector3.Distance(right,player.Avatar.Facing.RightHand.localPosition));
                yield return null;
            }
            Pointer(point,false);yield return null;
        }

        private IEnumerator DestroyNode(ResourceNode node)
        {
            var point=ScreenPoint(hud.Attack);
            Pointer(point,true);yield return null;
            var timeout=Time.time+8;
            while(node!=null && Time.time<timeout) yield return null;
            Pointer(point,false);yield return null;yield return null;
            Assert.That(node==null,Is.True,"Attack input must actually destroy the node.");
        }

        private IEnumerator Collect(string id,int expected)
        {
            yield return new WaitForSeconds(settings.DropPickupDelay+.1f);
            var pickup=Object.FindFirstObjectByType<WorldPickup>();
            Assert.That(pickup,Is.Not.Null);Assert.That(pickup.Item.Id,Is.EqualTo(id));
            var delta=(Vector2)pickup.transform.position-(Vector2)player.transform.position;
            yield return Walk(delta,Mathf.Max(.08f,(delta.magnitude-.5f)/settings.MoveSpeed));
            yield return new WaitForSeconds(.25f);
            Assert.That(Total(id),Is.EqualTo(expected));
            Assert.That(Object.FindFirstObjectByType<WorldPickup>(),Is.Null);
        }

        [UnityTest] public IEnumerator PunchesAlternateAndResourcesDropBeforeInventoryPickupAndStack()
        {
            var tree=WorldTestScene.MatureTree();
            WorldTestScene.MoveResource(tree,new Vector2(-3,1.8f));PositionPlayer(new Vector2(-3,-.2f));yield return new WaitForFixedUpdate();yield return null;
            var bar=tree.GetComponentInChildren<WorldHealthBar>();
            Assert.That(bar.View.activeSelf,Is.False);
            yield return PunchOnce();
            Assert.That(tree.Health.Current,Is.EqualTo(tree.Health.Maximum-settings.PunchDamage));
            Assert.That(bar.View.activeSelf,Is.True);
            Assert.That(leftPunchTravel,Is.GreaterThan(.2f));
            Assert.That(rightPunchTravel,Is.LessThan(.01f));
            yield return new WaitForSeconds(settings.PunchCooldown);
            yield return PunchOnce();
            Assert.That(rightPunchTravel,Is.GreaterThan(.2f));
            yield return new WaitForSeconds(settings.PunchCooldown);
            yield return DestroyNode(tree);
            Assert.That(Total("wood"),Is.Zero);
            Assert.That(Object.FindFirstObjectByType<WorldPickup>(),Is.Not.Null);
            yield return Collect("wood",6);
            var stone=Resource("stone",-6.5f,1.5f);
            PositionPlayer(new Vector2(-6.5f,-.4f));yield return new WaitForFixedUpdate();yield return null;
            var stoneBar=stone.GetComponentInChildren<WorldHealthBar>();
            Assert.That(stoneBar.View.activeSelf,Is.False);
            yield return PunchOnce();Assert.That(stoneBar.View.activeSelf,Is.True);
            yield return new WaitForSeconds(settings.PunchCooldown);
            yield return DestroyNode(stone);Assert.That(Total("stone"),Is.Zero);
            yield return Collect("stone",3);
            stone=Resource("stone",-13.5f,-2.1f);
            PositionPlayer(new Vector2(-13.5f,-4.1f));yield return new WaitForFixedUpdate();yield return null;
            yield return DestroyNode(stone);yield return Collect("stone",6);
            Assert.That(inventory.Model.GetSlot(3).Count,Is.EqualTo(6));
            var panel=Object.FindFirstObjectByType<InventoryPanel>();
            yield return driver.Inventory(hud);
            Assert.That(panel.Slots[2].Count.text,Is.EqualTo("6"));Assert.That(panel.Slots[3].Count.text,Is.EqualTo("6"));
            yield return Click(panel.Slots[3].GetComponent<Button>());
            Assert.That(panel.SelectedName.text,Is.EqualTo("Stone"));
            Assert.That(panel.Slots[3].Selection.enabled,Is.True);
            yield return driver.Inventory(hud);
        }

        [UnityTest] public IEnumerator EnemyTelegraphsDamagesWithoutPhysicsPushAndDiesToAttackInput()
        {
            var enemy=WorldTestScene.SpawnZombie();yield return new WaitForSeconds(1.3f);
            var bar=enemy.GetComponentInChildren<WorldHealthBar>();
            Assert.That(bar.View.activeSelf,Is.False);
            PositionPlayer((Vector2)enemy.transform.position+Vector2.left*1.9f);yield return null;
            var position=player.GetComponent<Rigidbody2D>().position;
            var timeout=Time.time+5;
            while(enemy.State!=EnemyState.Windup && Time.time<timeout) yield return null;
            Assert.That(enemy.State,Is.EqualTo(EnemyState.Windup));
            var own=player.GetComponent<Health>();var hp=own.Current;
            while(own.Current==hp && Time.time<timeout) yield return null;
            Assert.That(own.Current,Is.LessThan(hp));
            Assert.That(Vector2.Distance(position,player.GetComponent<Rigidbody2D>().position),Is.LessThan(.015f));
            Assert.That(Vector2.Distance(player.transform.position,enemy.transform.position),Is.GreaterThanOrEqualTo(settings.EnemySeparation-.02f));
            player.Avatar.Facing.Face((Vector2)enemy.transform.position-(Vector2)player.transform.position);
            yield return PunchOnce();Assert.That(bar.View.activeSelf,Is.True);
            Assert.That(enemy.OwnHealth.Current,Is.LessThan(enemy.OwnHealth.Maximum));
            yield return new WaitForSeconds(settings.PunchCooldown);
            var point=ScreenPoint(hud.Attack);Pointer(point,true);yield return null;
            timeout=Time.time+5;
            while(enemy!=null && enemy.gameObject.activeInHierarchy && Time.time<timeout) yield return null;
            Pointer(point,false);yield return null;
            Assert.That(enemy==null || !enemy.gameObject.activeInHierarchy,Is.True);
            Assert.That(Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None),Is.Empty);
            Assert.That(own.IsAlive,Is.True);
        }

        [UnityTest] public IEnumerator BossHudBindsRealHealthAndUnbindsWithoutLeavingFakeBoss()
        {
            var hud=Object.FindFirstObjectByType<BossHud>();Assert.That(hud.View.activeSelf,Is.False);
            var fixture=new GameObject("Boss fixture");fixture.SetActive(false);
            var health=fixture.AddComponent<Health>();health.Definition=Object.FindFirstObjectByType<ZombieSpawner>().Prefab.OwnHealth.Definition;
            var boss=fixture.AddComponent<BossIdentity>();boss.DisplayName="Guardian";
            fixture.SetActive(true);yield return null;
            Assert.That(hud.View.activeSelf,Is.True);Assert.That(hud.Name.text,Is.EqualTo("Guardian"));
            health.ApplyDamage(settings.PunchDamage);yield return null;
            Assert.That(hud.Fill.fillAmount,Is.EqualTo(health.Ratio).Within(.001f));
            Object.Destroy(fixture);yield return null;
            Assert.That(hud.View.activeSelf,Is.False);Assert.That(BossIdentity.Active,Is.Null);
        }

        [UnityTest] public IEnumerator DefeatCanRestartThroughActualUIButton()
        {
            player.GetComponent<Health>().ApplyDamage(player.GetComponent<Health>().Maximum);yield return null;
            Assert.That(hud.DefeatPanel.activeSelf,Is.True);
            var button=hud.DefeatPanel.transform.Find("Defeat Panel/Restart").GetComponent<Button>();
            yield return Click(button);yield return null;yield return null;
            var session=Object.FindFirstObjectByType<GameplaySession>();
            Assert.That(session.State,Is.EqualTo(SessionState.Running));
            Assert.That(session.PlayerHealth.Current,Is.EqualTo(session.PlayerHealth.Maximum));
            Assert.That(Time.timeScale,Is.EqualTo(1));
            var restarted=Object.FindFirstObjectByType<PlayerEquipment>();
            for(var i=0;i<restarted.Inventory.Model.Capacity;i++) Assert.That(restarted.Inventory.Model.GetSlot(i).IsEmpty,Is.True);
            Assert.That(restarted.ActiveTool,Is.Null);
        }

        [UnityTest] public IEnumerator FacingMovesEyesAndHandsWithoutRotatingBodyAndKeepsLastDirection()
        {
            var facing=player.Avatar.Facing;
            var rotation=player.Avatar.Body.localRotation;
            foreach(var direction in new[]{Vector2.up,Vector2.right,Vector2.down,Vector2.left,new Vector2(-1,-1).normalized})
            {
                facing.Face(direction);yield return new WaitForSeconds(.22f);
                var eyes=(facing.LeftEye.localPosition+facing.RightEye.localPosition)*.5f;
                var hands=(facing.LeftHand.localPosition+facing.RightHand.localPosition)*.5f;
                Assert.That(Vector2.Dot(((Vector2)eyes).normalized,direction),Is.GreaterThan(.99f));
                Assert.That(Vector2.Dot(((Vector2)hands).normalized,direction),Is.GreaterThan(.99f));
                Assert.That(player.Avatar.Body.localRotation,Is.EqualTo(rotation));
                Assert.That(Vector2.Distance(facing.LeftHand.localPosition,facing.RightHand.localPosition),Is.EqualTo(facing.HandSeparation*2).Within(.01f));
            }
            var last=facing.Direction;facing.Face(Vector2.zero);yield return null;
            Assert.That(facing.Direction,Is.EqualTo(last));
        }

        private IEnumerator UseToolOnce()
        {
            var tool=player.GetComponent<PlayerEquipment>().ActiveTool;
            var point=ScreenPoint(hud.Attack);
            var rest=player.Avatar.Facing.RightHand.localRotation;
            var maxArc=0f;
            Pointer(point,true);yield return null;
            var end=Time.time+tool.Duration+.03f;
            while(Time.time<end)
            {
                maxArc=Mathf.Max(maxArc,Quaternion.Angle(rest,player.Avatar.Facing.RightHand.localRotation));
                yield return null;
            }
            Pointer(point,false);yield return null;
            Assert.That(maxArc,Is.GreaterThan(20));
            yield return new WaitForSeconds(tool.Cooldown);
        }

        [UnityTest] public IEnumerator HotbarInventoryAndToolsShareRealSlotsAndApplyResourceEfficiencies()
        {
            var bar=Object.FindFirstObjectByType<HotbarView>();
            var equipment=player.GetComponent<PlayerEquipment>();
            var panel=Object.FindFirstObjectByType<InventoryPanel>();
            Assert.That(bar.Owner,Is.SameAs(inventory));Assert.That(panel.Owner,Is.SameAs(inventory));
            Assert.That(Total("axe"),Is.EqualTo(1));Assert.That(Total("pickaxe"),Is.EqualTo(1));
            yield return Click(bar.Slots[0].GetComponent<Button>());
            Assert.That(inventory.SelectedIndex,Is.Zero);Assert.That(bar.Slots[0].Selection.enabled,Is.True);
            Assert.That(equipment.HeldVisual.enabled,Is.True);Assert.That(equipment.HeldVisual.sprite,Is.SameAs(equipment.ActiveTool.HeldSprite));
            var tree=WorldTestScene.MatureTree();WorldTestScene.MoveResource(tree,new Vector2(-3,1.8f));PositionPlayer(new Vector2(-3,-.2f));
            player.Avatar.Facing.Face(Vector2.up);yield return new WaitForSeconds(.3f);
            yield return UseToolOnce();Assert.That(tree.Health.Current,Is.EqualTo(120));
            for(var hit=1;hit<5;hit++)yield return UseToolOnce();Assert.That(tree==null,Is.True,"Wood Axe must harvest a mature tree in five uses.");
            yield return Collect("wood",6);
            Assert.That(bar.Slots[2].Count.text,Is.EqualTo("6"));
            Assert.That(bar.Slots[2].Icon.sprite,Is.SameAs(inventory.Model.GetSlot(2).Item.Icon));
            var stone=Resource("stone",-6.5f,1.5f);PositionPlayer(new Vector2(-6.5f,-.4f));
            player.Avatar.Facing.Face(Vector2.up);yield return new WaitForSeconds(.3f);
            yield return UseToolOnce();Assert.That(stone.Health.Current,Is.EqualTo(45),"Axe is inefficient on stone.");
            yield return driver.Inventory(hud);yield return Click(panel.Slots[1].GetComponent<Button>());
            Assert.That(inventory.SelectedIndex,Is.EqualTo(1));Assert.That(bar.Slots[1].Selection.enabled,Is.True);
            Assert.That(panel.SelectedName.text,Is.EqualTo("Wood Pickaxe"));Assert.That(panel.SelectedCategory.text,Is.EqualTo("Tool"));
            yield return driver.Inventory(hud);
            Assert.That(equipment.HeldVisual.sprite,Is.SameAs(inventory.Model.GetSlot(1).Item.Tool.HeldSprite));
            yield return UseToolOnce();Assert.That(stone.Health.Current,Is.EqualTo(20));
            yield return UseToolOnce();Assert.That(stone==null,Is.True);yield return Collect("stone",3);
            yield return Click(bar.Slots[2].GetComponent<Button>());
            Assert.That(equipment.HeldVisual.enabled,Is.False,"Selecting a material restores bare hands.");
            yield return Click(bar.Slots[8].GetComponent<Button>());
            Assert.That(equipment.ActiveTool,Is.Null);Assert.That(bar.Slots[8].Selection.enabled,Is.True);
            yield return Click(bar.Slots[1].GetComponent<Button>());
            tree=Resource("wood",-10.1f,3.5f);PositionPlayer((Vector2)tree.transform.position+Vector2.down*2);
            player.Avatar.Facing.Face(Vector2.up);yield return new WaitForSeconds(.3f);
            var before=tree.Health.Current;yield return UseToolOnce();Assert.That(tree.Health.Current,Is.EqualTo(before-5),"Pickaxe is inefficient on wood.");
            Assert.That(Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None),Is.Empty);
        }
    }
}
