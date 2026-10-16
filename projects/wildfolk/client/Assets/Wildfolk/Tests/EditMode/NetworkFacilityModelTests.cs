using System.Linq;
using NUnit.Framework;
using UnityEditor;
using Unity.Netcode;
using UnityEngine;
namespace Wildfolk.Tests
{
    public sealed class NetworkFacilityModelTests
    {
        private ContentCatalog Catalog=>AssetDatabase.LoadAssetAtPath<ContentCatalog>("Assets/Wildfolk/Data/ContentCatalog.asset");
        [Test]public void FacilityPrefabsAndInteractionComponentsAreRegisteredBeforeSessionStarts()
        {
            var world=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/NetworkEcology.prefab").GetComponent<NetworkCreatureWorld>();
            var facilities=world.Prefabs.Where(e=>e.Source.GetComponent<WorldPlaceable>()!=null).ToArray();Assert.That(facilities.Length,Is.EqualTo(5));
            foreach(var entry in facilities){Assert.That(entry.NetworkPrefab.GetComponent<NetworkFacility>(),Is.Not.Null);Assert.That(entry.NetworkPrefab.GetComponent<NetworkObject>(),Is.Not.Null);}
            var player=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/NetworkPlayer.prefab");Assert.That(player.GetComponent<PlayerStorage>(),Is.Not.Null);Assert.That(player.GetComponent<PlayerCrafting>(),Is.Not.Null);Assert.That(player.GetComponent<PlayerInteraction>(),Is.Not.Null);Assert.That(player.GetComponent<NetworkFacilityInteractor>(),Is.Not.Null);Assert.That(player.GetComponent<PlayerPlacement>(),Is.Null);
            Assert.That(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/NetworkSession.prefab").GetComponent<NetworkManager>().NetworkConfig.ProtocolVersion,Is.EqualTo(4));
        }
        [Test]public void QueueVersionOnlyChangesAtCompletionAndBlockedCargoIsConserved()
        {
            var recipe=Catalog.Recipe("iron_ingot");var bag=new InventoryModel(2);bag.Add(recipe.Ingredients[0].Item,3);var queue=new ProcessingQueue();Assert.That(queue.Add(recipe,3,bag,50,1),Is.True);var version=queue.Version;
            queue.Advance(.1f,1,1);Assert.That(queue.Version,Is.EqualTo(version));queue.Advance(100,1,1);Assert.That(queue.Version,Is.EqualTo(version+1));Assert.That(queue.OutputCount,Is.EqualTo(1));Assert.That(queue.Remaining,Is.Zero);Assert.That(queue.WaitingCount,Is.EqualTo(1));Assert.That(queue.Release().Sum(a=>a.Count),Is.EqualTo(3));
        }
        [Test]public void CraftFailureKeepsAllIngredientsAndSameIngredientsOnlyCraftOnce()
        {
            var recipe=Catalog.Recipe("empty_cup");var bag=new InventoryModel(2);bag.Add(recipe.Ingredients[0].Item,recipe.Ingredients[0].Count+1);bag.Add(Catalog.Item("stone"),50);var before=bag.CountItem(recipe.Ingredients[0].Item);
            Assert.That(bag.TryExchange(recipe.Ingredients,recipe.Result,recipe.ResultCount),Is.EqualTo(ExchangeResult.NoSpace));Assert.That(bag.CountItem(recipe.Ingredients[0].Item),Is.EqualTo(before));Assert.That(bag.CountItem(recipe.Result),Is.Zero);
            bag.ConsumeSlot(0,recipe.Ingredients[0].Item,1);Assert.That(bag.TryExchange(recipe.Ingredients,recipe.Result,recipe.ResultCount),Is.EqualTo(ExchangeResult.Success));Assert.That(bag.TryExchange(recipe.Ingredients,recipe.Result,recipe.ResultCount),Is.EqualTo(ExchangeResult.MissingMaterials));Assert.That(bag.CountItem(recipe.Result),Is.EqualTo(recipe.ResultCount));
        }
    }
}
