using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace Wildfolk.Tests
{
    public sealed class FrontendContractTests
    {
        [Test]public void AppStartsAtHomeAndGameplayRemainsEnabled()
        {var scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).ToArray();Assert.That(scenes[0].path,Is.EqualTo("Assets/Wildfolk/Scenes/MainMenu.unity"));Assert.That(scenes.Count(s=>s.path=="Assets/Wildfolk/Scenes/Wildfolk.unity"),Is.EqualTo(1));}
        [Test]public void SaveRulesReferenceGameplayContentAndHomeKeysHaveBothLanguages()
        {
            var rules=Resources.Load<SaveGameRules>("SaveGameRules");Assert.That(rules,Is.Not.Null);Assert.That(rules.Catalog,Is.SameAs(AssetDatabase.LoadAssetAtPath<ContentCatalog>("Assets/Wildfolk/Data/ContentCatalog.asset")));Assert.That(rules.Player.Settings.InventorySlots,Is.EqualTo(36));Assert.That(rules.NightPrefabs.Length,Is.EqualTo(2));
            var entries=L.Catalog.Entries.Where(e=>e.Key.StartsWith("home.")).ToArray();Assert.That(entries.Length,Is.GreaterThanOrEqualTo(15));foreach(var e in entries){Assert.That(e.Values.Length,Is.EqualTo(2));Assert.That(e.Values.All(v=>!string.IsNullOrWhiteSpace(v)),Is.True);}
        }
    }
}
