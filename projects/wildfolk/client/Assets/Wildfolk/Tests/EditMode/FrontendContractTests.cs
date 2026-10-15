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
            foreach(var key in new[]{"brand.name","home.camp","home.continue","home.single","home.multi","home.confirm_body","home.saved_day"})Assert.That(L.Has(key),Is.True,key);
            var brand=L.Catalog.Entries.Single(e=>e.Key=="brand.name");Assert.That(brand.Values,Is.EqualTo(new[]{"Lowfire","Lowfire"}));
            var entries=L.Catalog.Entries.Where(e=>e.Key.StartsWith("home.")).ToArray();foreach(var e in entries){Assert.That(e.Values.Length,Is.EqualTo(2));Assert.That(e.Values.All(v=>!string.IsNullOrWhiteSpace(v)),Is.True);}
        }
    }
}
