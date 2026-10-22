using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using Unity.Netcode;
using UnityEngine;
namespace Wildfolk.Tests
{
    public sealed class NetworkPersistenceModelTests
    {
        private SaveGameRules Rules=>Resources.Load<SaveGameRules>("SaveGameRules");
        private SavedGame ValidGame()
        {
            var rules=Rules;var map=new WorldMap(rules.World,rules.World.DefaultSeed);var player=rules.Player;var slots=new SavedStack[player.Settings.InventorySlots];for(var i=0;i<slots.Length;i++)slots[i]=new SavedStack();var growth=player.GetComponent<PlayerProgression>().Definition;
            return new SavedGame{Version=11,Seed=map.Seed,Player=new PlayerSnapshot{Position=map.Spawn,HP=player.GetComponent<Health>().Definition.MaximumHealth,Inventory=slots,Survival=new SurvivalModel(player.GetComponent<PlayerSurvival>().Definition).Capture(),Growth=GrowthValidation.FromLegacy(0,growth),Stamina=player.GetComponent<PlayerLocomotion>().Definition.MaximumStamina},Recovery=new ResourcePopulation(map).Capture(),AnimalEcology=new AnimalEcologySnapshot(),GoblinEcology=new GoblinEcologySnapshot()};
        }
        [Test]public void LegacyV4AndV11MigrateTo12PreservingCorePlayerPayload()
        {
            var game=ValidGame();game.Version=4;var before=JsonUtility.ToJson(game.Player);Rules.Validate(game);Assert.That(game.Version,Is.EqualTo(12));var death=game.Player.Death;Assert.That(death.Home,Is.EqualTo(Vector2.zero));game.Player.Death=null;Assert.That(JsonUtility.ToJson(game.Player),Is.EqualTo(before));game.Player.Death=death;Assert.That(game.Multiplayer,Is.Null);var restored=SaveRepository.Decode(SaveRepository.Encode(game));Rules.Validate(restored);Assert.That(restored.Seed,Is.EqualTo(game.Seed));Assert.That(restored.Player.Inventory.Length,Is.EqualTo(36));
        }
        [Test]public void ParticipantMetadataRejectsDuplicatesInvalidIdentityAndMismatchedHost()
        {
            var game=ValidGame();var id=Guid.NewGuid().ToString("N");game.IsMultiplayer=true;game.Multiplayer=new MultiplayerSnapshot{WorldId=Guid.NewGuid().ToString("N"),HostId=id,Players=new[]{new SavedParticipant{Id=id,State=game.Player}}};Rules.Validate(game);
            game.Multiplayer.Players=new[]{game.Multiplayer.Players[0],game.Multiplayer.Players[0]};Assert.Throws<InvalidDataException>(()=>Rules.Validate(game));game.Multiplayer.Players=game.Multiplayer.Players.Take(1).ToArray();game.Multiplayer.Players[0].Id="account-like-label";Assert.Throws<InvalidDataException>(()=>Rules.Validate(game));game.Multiplayer.Players[0].Id=id;game.Multiplayer.HostId=Guid.NewGuid().ToString("N");Assert.Throws<InvalidDataException>(()=>Rules.Validate(game));
        }
        [Test]public void SeparateRepositoriesAndDeadParticipantPolicyPreserveSingleplayerRules()
        {
            Assert.That(SaveRepository.MultiplayerDefault().Path,Is.Not.EqualTo(SaveRepository.Default().Path));var game=ValidGame();game.Player.HP=0;Assert.Throws<InvalidDataException>(()=>Rules.Validate(game));var id=Guid.NewGuid().ToString("N");game.IsMultiplayer=true;game.Multiplayer=new MultiplayerSnapshot{WorldId=Guid.NewGuid().ToString("N"),HostId=id,Players=new[]{new SavedParticipant{Id=id,State=game.Player}}};Assert.DoesNotThrow(()=>Rules.Validate(game));
            var single=ValidGame();Rules.Validate(single);single.Player.HP=0;Assert.Throws<InvalidDataException>(()=>Rules.Validate(single));single.Player.Death.Dead=true;Assert.DoesNotThrow(()=>Rules.Validate(single));Assert.That(single.Player.Death.Home,Is.EqualTo(Vector2.zero));
        }
        [Test]public void EveryExistingPlaceableUsesOneSourceDefinitionAndServerBuildingState()
        {
            var world=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/NetworkEcology.prefab").GetComponent<NetworkCreatureWorld>();var items=Rules.Catalog.Items.Where(i=>i.Placement!=null).ToArray();Assert.That(items.Length,Is.EqualTo(31));foreach(var item in items){var entry=world.Prefabs.Single(e=>e.Source==item.Placement.Prefab.gameObject);Assert.That(entry.NetworkPrefab.GetComponent<NetworkBuilding>(),Is.Not.Null);Assert.That(entry.NetworkPrefab.GetComponent<WorldPlaceable>().Definition,Is.SameAs(item.Placement));}
            Assert.That(world.GetComponent<NetworkWorldState>(),Is.Not.Null);Assert.That(world.GetComponent<NetworkWorldPersistence>().Rules,Is.SameAs(Rules));Assert.That(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/NetworkSession.prefab").GetComponent<NetworkManager>().NetworkConfig.ProtocolVersion,Is.EqualTo(NetworkProtocol.Current));
        }
    }
}
