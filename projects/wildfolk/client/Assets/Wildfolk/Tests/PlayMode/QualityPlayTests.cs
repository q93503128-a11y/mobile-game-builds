using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace Wildfolk.Tests
{
    public sealed class QualityPlayTests
    {
        private WorldSaveService save;private PlayerInventory player;
        private sealed class MoveSource:IPlayerInputSource {public Vector2 Move=>Vector2.right;public bool Primary=>false;public bool Interact=>false;public bool Inventory=>false;public bool Pause=>false;}
        [UnitySetUp]public IEnumerator Setup()
        {Time.timeScale=1;WorldGenerator.SelectedGenerationVersion=1;WorldGenerator.SelectedSeed=9472026;yield return SceneManager.LoadSceneAsync("Wildfolk");yield return null;yield return null;save=Object.FindFirstObjectByType<WorldSaveService>();save.AutoSaveEnabled=false;player=save.Player;save.Clock.enabled=false;save.GetComponent<ZombieSpawner>().enabled=false;save.GetComponent<GoblinSpawner>().enabled=false;}
        [UnityTest]public IEnumerator ArmorAndHandsShareActualWalkingMotionRoot()
        {
            var equipment=player.GetComponent<PlayerEquipment>();foreach(var id in new[]{"iron_head","iron_body"}){var item=save.Catalog.Item(id);player.Model.Add(item,1);equipment.TryEquip(item);}var avatar=player.GetComponent<PlayerAvatar>();Assert.That(equipment.HeadVisual.transform.IsChildOf(avatar.Body),Is.True);Assert.That(equipment.ArmorVisual.transform.IsChildOf(avatar.Body),Is.True);Assert.That(equipment.RightHandGear.transform.IsChildOf(avatar.Facing.RightHand),Is.True);
            player.GetComponent<PlayerInputAdapter>().Source=new MoveSource();yield return new WaitForSeconds(.25f);Assert.That(Mathf.Abs(avatar.Body.localPosition.y),Is.GreaterThan(.001f));var relative=equipment.HeadVisual.transform.InverseTransformPoint(equipment.ArmorVisual.transform.position);yield return new WaitForSeconds(.1f);Assert.That(equipment.HeadVisual.transform.InverseTransformPoint(equipment.ArmorVisual.transform.position),Is.EqualTo(relative));player.GetComponent<PlayerInputAdapter>().Source=null;
        }
        [UnityTest]public IEnumerator FurnaceQueueButtonsReserveReturnCollectAndPersist()
        {
            var station=Object.Instantiate(save.Catalog.Item("furnace").Placement.Prefab,player.transform.position+Vector3.right*2,Quaternion.identity).GetComponent<CraftingStation>();yield return null;var recipe=save.Catalog.Recipe("iron_ingot");player.Model.Add(recipe.Ingredients[0].Item,10);player.GetComponent<PlayerCrafting>().OpenStation(station);yield return null;var panel=Object.FindFirstObjectByType<CraftingPanel>();panel.EnqueueOne();panel.EnqueueAll();var job=station.GetComponent<StationProcessing>();Assert.That(job.Queue.WaitingCount,Is.EqualTo(9));Assert.That(player.Model.CountItem(recipe.Ingredients[0].Item),Is.Zero);panel.WithdrawOne();Assert.That(job.Queue.WaitingCount,Is.EqualTo(8));job.Advance(2.1f);Assert.That(job.OutputCount,Is.EqualTo(1));var snapshot=save.Capture();save.Apply(SaveRepository.Decode(SaveRepository.Encode(snapshot)));yield return null;var restored=Object.FindFirstObjectByType<StationProcessing>();Assert.That(restored.Queue.WaitingCount,Is.EqualTo(7));Assert.That(restored.OutputCount,Is.EqualTo(1));restored.Collect(player);Assert.That(player.Model.CountItem(recipe.Result),Is.EqualTo(1));Assert.That(player.Model.CountItem(recipe.Ingredients[0].Item),Is.EqualTo(1));
        }
        [UnityTest]public IEnumerator SettingsSwitchRefreshesExistingLabelsAndUsesKoreanGlyphs()
        {
            var preferences=save.GetComponent<UserPreferences>();var previous=preferences.Values.Language;try{preferences.SetLanguage(1);yield return null;Assert.That(L.Language,Is.EqualTo("ko"));var inventory=Object.FindFirstObjectByType<InventoryPanel>();save.World.Session.OpenInventory();yield return null;var title=inventory.View.GetComponentsInChildren<LocalizedLabel>(true).Single(x=>x.Key=="ui.inventory.title");Assert.That(title.GetComponent<Text>().text,Is.EqualTo("인벤토리"));Assert.That(title.GetComponent<Text>().font.HasCharacter('인'),Is.True);preferences.SetLanguage(0);yield return null;Assert.That(title.GetComponent<Text>().text,Is.EqualTo("Inventory"));Assert.That(save.Capture().Version,Is.EqualTo(7));}finally{preferences.SetLanguage(System.Array.FindIndex(L.Catalog.Languages,x=>x.Id==previous));preferences.Flush();}
        }
        [UnityTest]public IEnumerator RecipeFirstScreenGroupsAllAndColorsEachIngredient()
        {
            var station=Object.Instantiate(save.Catalog.Item("workbench_ii").Placement.Prefab,player.transform.position+Vector3.right*2,Quaternion.identity).GetComponent<CraftingStation>();yield return null;player.Model.Add(save.Catalog.Item("wood"),12);player.Model.Add(save.Catalog.Item("stone"),1);player.GetComponent<PlayerCrafting>().OpenStation(station);yield return null;var panel=Object.FindFirstObjectByType<CraftingPanel>();Assert.That(Object.FindFirstObjectByType<InventoryPanel>().View.activeSelf,Is.False);Assert.That(panel.View.activeSelf,Is.True);Assert.That(panel.Sections.Count(x=>x!=null && x.activeSelf),Is.GreaterThan(2));var card=panel.Cards.Single(c=>c.Recipe.Id=="stone_wall");Assert.That(card.Materials.text,Does.Contain("<color=#C5CE9B>"));Assert.That(card.Materials.text,Does.Contain("<color=#D99078>"));Assert.That(card.Materials.text,Does.Contain("1 / 3"));var before=panel.Cards.Select(c=>c.Recipe.Id).ToArray();player.GetComponent<PlayerCrafting>().MarkSeen(card.Recipe);Assert.That(panel.Cards.Select(c=>c.Recipe.Id),Is.EqualTo(before));
        }
        [UnityTest]public IEnumerator NearStaminaTracksPlayerAndFadesAtFull()
        {
            var hud=Object.FindFirstObjectByType<PlayerStaminaHud>();var move=player.GetComponent<PlayerLocomotion>();move.Restore(50,1);yield return null;yield return null;Assert.That(hud.Group.alpha,Is.GreaterThan(0));Assert.That(hud.Arc.GetComponent<CanvasRenderer>(),Is.Not.Null);Assert.That(hud.Arc.canvasRenderer.GetMesh().vertexCount,Is.GreaterThan(0));var projected=Camera.main.WorldToScreenPoint(player.transform.position);Assert.That(Vector2.Distance(hud.Anchor.position,projected),Is.LessThan(2));move.Restore(move.Stamina.Maximum,0);yield return new WaitForSeconds(1.2f);Assert.That(hud.Group.alpha,Is.Zero);Assert.That(Object.FindFirstObjectByType<ProfileHud>().GetType().GetField("Stamina"),Is.Null);
        }
        [UnityTest]public IEnumerator V2MigrationKeepsEquipmentLearnedSkillsDestroyedResourcesAndReservedBatch()
        {
            var equipment=player.GetComponent<PlayerEquipment>();var armor=save.Catalog.Item("iron_head");player.Model.Add(armor,1);equipment.TryEquip(armor);player.GetComponent<PlayerProgression>().SetValues(90,11);
            var node=Object.FindFirstObjectByType<ResourceIdentity>();var id=node.Id;node.Node.Health.ApplyDamage(node.Node.Health.Maximum);yield return null;
            Object.Instantiate(save.Catalog.Item("furnace").Placement.Prefab,player.transform.position+Vector3.right*2,Quaternion.identity);yield return null;
            var old=save.Capture();old.Version=2;old.Recovery=null;old.Animals=new[]{new AnimalSnapshot{Id="a0_0",Rule=0,HP=10,Position=new Vector2(-12,-5),Home=new Vector2(-12,-5)},new AnimalSnapshot{Id="a1_0",Rule=1,HP=20,Position=new Vector2(14,-6),Home=new Vector2(14,-6)}};old.Player.Growth.Skills[2].Rank=1;old.Player.Growth.Points--;old.Resources.Single(x=>x.Id==id).RespawnAt=old.Elapsed+2400;
            old.Placeables.Single(x=>x.Item=="furnace").Processing=new ProcessingSnapshot{Recipe="iron_ingot",Batch=3,Remaining=4};
            save.Apply(SaveRepository.Decode(SaveRepository.Encode(old)));yield return null;Assert.That(equipment.HeadArmor.Id,Is.EqualTo("iron_head"));Assert.That(player.GetComponent<PlayerProgression>().Rank(2),Is.EqualTo(1));Assert.That(player.GetComponent<PlayerProgression>().Gold,Is.EqualTo(11));Assert.That(save.World.Renewal.Capture().Single(x=>x.Id==id).Destroyed,Is.True);Assert.That(save.World.Renewal.Population.Harvests.Count,Is.EqualTo(1));
            var process=Object.FindFirstObjectByType<StationProcessing>();Assert.That(process.Queue.WaitingCount,Is.EqualTo(1));Assert.That(process.OutputCount,Is.EqualTo(1));Assert.That(save.Capture().Version,Is.EqualTo(7));save.Validate(save.Capture());
        }
    }
}
