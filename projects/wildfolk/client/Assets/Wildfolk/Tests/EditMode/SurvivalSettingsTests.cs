using NUnit.Framework;
using UnityEngine;
namespace Wildfolk.Tests
{
    public sealed class SurvivalSettingsTests
    {
        [Test] public void EmptyMetersDamageAtIntervalsAndFoodClampsWithoutWastingAtMax()
        {
            var definition=ScriptableObject.CreateInstance<SurvivalDefinition>();
            var food=ScriptableObject.CreateInstance<ConsumableDefinition>();
            try
            {
                definition.MaximumHunger=100;definition.MaximumThirst=100;definition.HungerPerSecond=1;definition.ThirstPerSecond=0;
                var model=new SurvivalModel(definition);food.HungerRestore=35;
                Assert.That(model.CanRestore(food),Is.False);
                Assert.That(model.Advance(100),Is.Zero);Assert.That(model.Hunger,Is.Zero);
                Assert.That(model.Advance(4.9f),Is.Zero);Assert.That(model.Advance(.2f),Is.EqualTo(2));
                model.Restore(food);Assert.That(model.Hunger,Is.EqualTo(35));Assert.That(model.Advance(.1f),Is.Zero);
                model.Restore(food);model.Restore(food);Assert.That(model.Hunger,Is.EqualTo(100));
                Assert.That(model.CanRestore(food),Is.False);
            }
            finally {Object.DestroyImmediate(definition);Object.DestroyImmediate(food);}
        }
        [Test] public void BothEmptyMetersDealIndependentPeriodicDamage()
        {
            var definition=ScriptableObject.CreateInstance<SurvivalDefinition>();
            try
            {
                definition.HungerPerSecond=100;definition.ThirstPerSecond=100;
                var model=new SurvivalModel(definition);Assert.That(model.Advance(1),Is.Zero);
                Assert.That(model.Advance(10),Is.EqualTo(8));Assert.That(model.Hunger,Is.Zero);Assert.That(model.Thirst,Is.Zero);
            }
            finally {Object.DestroyImmediate(definition);}
        }
        [Test] public void LocalPreferencesClampAndRoundTripThroughSeparateKeys()
        {
            const string prefix="wildfolk.automated.preferences.";
            try
            {
                var settings=new LocalSettings();Assert.That(settings.ControlSize,Is.EqualTo(1));
                settings.SetControl(9);settings.SetMaster(-1);settings.SetMusic(.34f);settings.SetSfx(.76f);settings.Store(prefix);PlayerPrefs.Save();
                var loaded=LocalSettings.Load(prefix);Assert.That(loaded.ControlSize,Is.EqualTo(1.4f));Assert.That(loaded.Master,Is.Zero);
                Assert.That(loaded.Music,Is.EqualTo(.34f).Within(.001f));Assert.That(loaded.Sfx,Is.EqualTo(.76f).Within(.001f));
                loaded.SetControl(.1f);Assert.That(loaded.ControlSize,Is.EqualTo(.8f));
            }
            finally {foreach(var key in new[]{"controls","master","music","sfx"}) PlayerPrefs.DeleteKey(prefix+key);PlayerPrefs.Save();}
        }
        [Test] public void CookingReservationRejectsPartialCostWithoutChangingInventory()
        {
            var wood=ScriptableObject.CreateInstance<ItemDefinition>();var stone=ScriptableObject.CreateInstance<ItemDefinition>();
            try
            {
                wood.Id="wood";wood.MaxStack=50;stone.Id="stone";stone.MaxStack=50;
                var model=new InventoryModel(2);model.Add(wood,4);var changes=0;model.Changed+=()=>changes++;
                Assert.That(model.TryConsumeCosts(new[]{new ItemAmount{Item=wood,Count=4},new ItemAmount{Item=stone,Count=1}}),Is.False);
                Assert.That(model.CountItem(wood),Is.EqualTo(4));Assert.That(changes,Is.Zero);
                Assert.That(model.TryConsumeCosts(new[]{new ItemAmount{Item=wood,Count=4}}),Is.True);Assert.That(changes,Is.EqualTo(1));
            }
            finally {Object.DestroyImmediate(wood);Object.DestroyImmediate(stone);}
        }
    }
}
