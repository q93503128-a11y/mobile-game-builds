using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Wildfolk.Tests
{
    public sealed class GameplayPlayTests
    {
        private Mouse mouse;
        private PlayerMotor player;
        private PlayerInventory inventory;
        private GameplayHud hud;
        private GameplaySettings settings;
        private float leftPunchTravel;
        private float rightPunchTravel;
        private InputSettings.BackgroundBehavior originalBackground;
#if UNITY_EDITOR
        private InputSettings.EditorInputBehaviorInPlayMode originalEditorInput;
#endif

        [UnitySetUp] public IEnumerator LoadFreshGame()
        {
            Time.timeScale=1;
            originalBackground=InputSystem.settings.backgroundBehavior;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
#if UNITY_EDITOR
            originalEditorInput=InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
            yield return SceneManager.LoadSceneAsync("Wildfolk");
            yield return null;
            mouse=InputSystem.AddDevice<Mouse>();
            player=Object.FindFirstObjectByType<PlayerMotor>();
            inventory=player.GetComponent<PlayerInventory>();
            hud=Object.FindFirstObjectByType<GameplayHud>();
            settings=player.Settings;
            yield return null;
        }

        [UnityTearDown] public IEnumerator ReleaseInput()
        {
            if(mouse!=null) InputSystem.RemoveDevice(mouse);
            InputSystem.settings.backgroundBehavior=originalBackground;
#if UNITY_EDITOR
            InputSystem.settings.editorInputBehaviorInPlayMode=originalEditorInput;
#endif
            Time.timeScale=1;
            yield return null;
        }

        private static Vector2 ScreenPoint(Component component) => RectTransformUtility.WorldToScreenPoint(null,component.transform.position);
        private void Pointer(Vector2 position,bool pressed)
        {
            var state=new MouseState {position=position};
            if(pressed) state=state.WithButton(MouseButton.Left);
            InputSystem.QueueStateEvent(mouse,state);
        }

        private IEnumerator Click(Component component)
        {
            var point=ScreenPoint(component);
            Pointer(point,true);yield return null;
            Pointer(point,false);yield return null;yield return null;
        }

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
            return selected;
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
            yield return Click(hud.InventoryButton);
            Assert.That(panel.View.activeSelf,Is.True);Assert.That(Time.timeScale,Is.Zero);
            var position=player.transform.position;
            yield return Click(hud.Attack);
            Assert.That(player.Combat.IsAttacking,Is.False);
            Assert.That(player.transform.position,Is.EqualTo(position));
            yield return Click(panel.Slots[0].GetComponent<Button>());
            Assert.That(panel.Slots[0].Selection.enabled,Is.True);
            yield return Click(panel.View.transform.Find("Inventory Panel/Close").GetComponent<Button>());
            Assert.That(panel.View.activeSelf,Is.False);Assert.That(Time.timeScale,Is.EqualTo(1));
            yield return Click(hud.InventoryButton);yield return Click(hud.InventoryButton);
            Assert.That(panel.View.activeSelf,Is.False);
        }

        private IEnumerator PunchOnce()
        {
            var left=player.Avatar.LeftHand.localPosition;var right=player.Avatar.RightHand.localPosition;
            leftPunchTravel=rightPunchTravel=0;
            var point=ScreenPoint(hud.Attack);
            Pointer(point,true);yield return null;
            var end=Time.time+settings.PunchDuration;
            while(Time.time<end)
            {
                leftPunchTravel=Mathf.Max(leftPunchTravel,Vector3.Distance(left,player.Avatar.LeftHand.localPosition));
                rightPunchTravel=Mathf.Max(rightPunchTravel,Vector3.Distance(right,player.Avatar.RightHand.localPosition));
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
            var tree=Resource("wood",-3,1.8f);
            PositionPlayer(new Vector2(-3,-.2f));yield return new WaitForFixedUpdate();yield return null;
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
            yield return Collect("wood",3);
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
            Assert.That(inventory.Model.GetSlot(1).Count,Is.EqualTo(6));
            var panel=Object.FindFirstObjectByType<InventoryPanel>();
            yield return Click(hud.InventoryButton);
            Assert.That(panel.Slots[0].Count.text,Is.EqualTo("3"));Assert.That(panel.Slots[1].Count.text,Is.EqualTo("6"));
            yield return Click(panel.Slots[1].GetComponent<Button>());
            Assert.That(panel.SelectedName.text,Is.EqualTo("Stone"));
            Assert.That(panel.Slots[1].Selection.enabled,Is.True);
            yield return Click(hud.InventoryButton);
        }

        [UnityTest] public IEnumerator EnemyTelegraphsDamagesWithoutPhysicsPushAndDiesToAttackInput()
        {
            var enemy=Object.FindFirstObjectByType<EnemyController>();
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
            yield return PunchOnce();Assert.That(bar.View.activeSelf,Is.True);
            Assert.That(enemy.OwnHealth.Current,Is.LessThan(enemy.OwnHealth.Maximum));
            yield return new WaitForSeconds(settings.PunchCooldown);
            var point=ScreenPoint(hud.Attack);Pointer(point,true);yield return null;
            timeout=Time.time+5;
            while(enemy!=null && Time.time<timeout) yield return null;
            Pointer(point,false);yield return null;
            Assert.That(enemy==null,Is.True);
            Assert.That(Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None),Is.Empty);
            Assert.That(own.IsAlive,Is.True);
        }

        [UnityTest] public IEnumerator BossHudBindsRealHealthAndUnbindsWithoutLeavingFakeBoss()
        {
            var hud=Object.FindFirstObjectByType<BossHud>();Assert.That(hud.View.activeSelf,Is.False);
            var fixture=new GameObject("Boss fixture");fixture.SetActive(false);
            var health=fixture.AddComponent<Health>();health.Definition=Object.FindFirstObjectByType<EnemyController>().OwnHealth.Definition;
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
        }
    }
}
