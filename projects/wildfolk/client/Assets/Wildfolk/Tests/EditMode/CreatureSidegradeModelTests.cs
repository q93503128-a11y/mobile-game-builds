using System;
using System.Linq;
using NUnit.Framework;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
namespace Wildfolk.Tests
{
    public sealed class CreatureSidegradeModelTests
    {
        private ContentCatalog C=>Resources.Load<SaveGameRules>("SaveGameRules").Catalog;
        [Test]public void HunterTradesDamageSpeedAndArcForReachWithoutSkippingIron()
        {
            var h=C.Item("hunters_spear").Tool;var iron=C.Item("iron_spear").Tool;var steel=C.Item("steel_spear").Tool;var diamond=C.Item("diamond_spear").Tool;
            Assert.That(h.Kind,Is.EqualTo(ToolKind.Spear));Assert.That(h.Damage,Is.LessThan(iron.Damage));Assert.That(h.Cooldown,Is.GreaterThan(iron.Cooldown));Assert.That(h.Attack.Range,Is.GreaterThan(steel.Attack.Range));Assert.That(h.Attack.ArcAngle,Is.LessThan(iron.Attack.ArcAngle));Assert.That(h.Damage/h.Cooldown,Is.LessThan(steel.Damage/steel.Cooldown));Assert.That(h.Damage/h.Cooldown,Is.LessThan(diamond.Damage/diamond.Cooldown));Assert.That(h.Attack.MaxTargets,Is.EqualTo(1));Assert.That(h.Attack.PierceCount,Is.Zero);Assert.That(h.Attack.AoERadius,Is.Zero);
            var r=C.Recipe("hunters_spear");Assert.That(r.RequiredWorkbenchTier,Is.EqualTo(2));Assert.That(r.RequiredStation,Is.EqualTo(StationKind.Workbench));Assert.That(r.Ingredients.Select(x=>x.Item.Id+":"+x.Count),Is.EquivalentTo(new[]{"iron_spear:1","bone:6","leather:2"}));Assert.That(C.Recipe("steel_spear").Ingredients.Any(x=>x.Item.Id=="iron_spear"),Is.True);Assert.That(C.Recipe("diamond_spear").Ingredients.Any(x=>x.Item.Id=="steel_spear"),Is.True);
        }
        [Test]public void AppendedItemRoundTripsExistingStackCodecAndRecipesKeepRepeatSinks()
        {
            Assert.That(C.Items.Length,Is.EqualTo(136));Assert.That(C.Recipes.Length,Is.EqualTo(111));Assert.That(C.Items[126].Id,Is.EqualTo("hunters_spear"));var b=new InventoryModel(36);b.Add(C.Item("hunters_spear"),1);b.Add(C.Item("bone"),7);b.Add(C.Item("chitin"),5);var restored=new InventoryModel(36);restored.Restore(b.Capture(),C);Assert.That(restored.GetSlot(0).Item,Is.SameAs(C.Item("hunters_spear")));Assert.That(restored.CountItem(C.Item("bone")),Is.EqualTo(7));Assert.That(restored.CountItem(C.Item("chitin")),Is.EqualTo(5));
            Assert.That(C.Recipes.Count(r=>r.Ingredients.Any(i=>i.Item.Id=="bone")),Is.EqualTo(5));Assert.That(C.Recipes.Count(r=>r.Ingredients.Any(i=>i.Item.Id=="chitin")),Is.EqualTo(3));Assert.That(C.Items.Any(i=>i.Id=="carapace_head"),Is.False);Assert.That(C.Item("carapace_vest").Armor.Defense,Is.EqualTo(3));Assert.That(C.Item("carapace_vest").Armor.StaminaRecoveryBonus,Is.EqualTo(.15f));
        }
        [Test]public void ExistingNetworkToolAndHeldIndicesCarrySidegradeWithoutNewFields()
        {
            var index=checked((short)Array.IndexOf(C.Items,C.Item("hunters_spear")));var a=new PlayerAttackState{Sequence=37,Hand=1,Tool=index,Started=123.5};using(var w=new FastBufferWriter(128,Allocator.Temp)){w.WriteNetworkSerializable(a);using(var r=new FastBufferReader(w,Allocator.Temp)){r.ReadNetworkSerializable(out PlayerAttackState b);Assert.That(b.Equals(a),Is.True);Assert.That(C.Items[b.Tool].Tool,Is.SameAs(C.Item("hunters_spear").Tool));}}
            var status=new PlayerStatusState{HP=83,MaximumHP=100,Held=index,Head=-1,Body=-1,Utility=-1,MealItem=-1};using(var w=new FastBufferWriter(128,Allocator.Temp)){w.WriteNetworkSerializable(status);using(var r=new FastBufferReader(w,Allocator.Temp)){r.ReadNetworkSerializable(out PlayerStatusState b);Assert.That(b.Equals(status),Is.True);Assert.That(C.Items[b.Held].HeldSprite,Is.Not.Null);}}Assert.That(NetworkProtocol.Current,Is.EqualTo(23));
        }
        [Test]public void BothLanguagesHaveNameAndDistinctHeldArt()
        {
            var old=L.Language;try{foreach(var language in new[]{"en","ko"}){L.SetLanguage(language);Assert.That(C.Item("hunters_spear").LocalizedName,Does.Not.Contain("item."));}}finally{L.SetLanguage(old);}
            var i=C.Item("hunters_spear");Assert.That(i.Icon,Is.Not.SameAs(i.HeldSprite));Assert.That(i.HeldSprite.bounds.size.y,Is.GreaterThan(3));Assert.That(i.Tool.HandExtension,Is.EqualTo(1.52f));
        }
    }
}
