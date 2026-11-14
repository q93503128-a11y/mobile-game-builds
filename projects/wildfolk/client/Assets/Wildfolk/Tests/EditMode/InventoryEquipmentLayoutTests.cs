using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Wildfolk.Tests
{
    public sealed class InventoryEquipmentLayoutTests
    {
        private static readonly string[] ScenePaths =
        {
            "Assets/Wildfolk/Scenes/Wildfolk.unity",
            "Assets/Wildfolk/Scenes/Multiplayer.unity"
        };

        [TestCaseSource(nameof(ScenePaths))]
        public void EquipmentTargetsAndLabelsFitSharedInventoryLayout(string scenePath)
        {
            var scene = EditorSceneManager.OpenPreviewScene(scenePath);
            try
            {
                var panels = new List<InventoryPanel>();
                foreach (var root in scene.GetRootGameObjects())
                    panels.AddRange(root.GetComponentsInChildren<InventoryPanel>(true));
                Assert.That(panels.Count, Is.GreaterThan(0), scenePath + " contains an inventory panel");

                foreach (var panel in panels)
                {
                    Assert.That(panel.Slots, Has.Length.EqualTo(36), scenePath + " preserves 9 hotbar + 27 storage slots");
                    var surface = panel.View.transform.Find("Inventory Panel");
                    var storage = surface.Find("Storage Slots");
                    var hotbar = surface.Find("Hotbar Slots");
                    Assert.That(storage.GetComponent<GridLayoutGroup>().constraintCount, Is.EqualTo(9));
                    Assert.That(hotbar.GetComponent<GridLayoutGroup>().constraintCount, Is.EqualTo(9));
                    for (var i = 0; i < panel.Slots.Length; i++)
                    {
                        Assert.That(panel.Slots[i].transform.parent, Is.SameAs(i < PlayerInventory.HotbarSize ? hotbar : storage), "slot " + i + " remains in its original bank");
                        Assert.That(panel.Slots[i].Count.rectTransform.sizeDelta.y, Is.GreaterThanOrEqualTo(44f), "slot " + i + " quantity has room for the actual type size");
                        if (i < PlayerInventory.HotbarSize)
                            Assert.That(panel.Slots[i].transform.Find("Quick Slot").GetComponent<Text>().rectTransform.sizeDelta.y, Is.GreaterThanOrEqualTo(36f), "hotbar slot " + i + " number fits without font reduction");
                    }

                    var equipment = panel.GetComponent<InventoryEquipment>();
                    var buttons = new[] { equipment.HeadButton, equipment.BodyButton, equipment.UtilityButton, equipment.AccessoryButton };
                    var icons = new[] { equipment.HeadIcon, equipment.BodyIcon, equipment.UtilityIcon, equipment.AccessoryIcon };
                    var region = surface.Find("Equipment Region") as RectTransform;
                    Assert.That(region, Is.Not.Null);
                    var regionRect = LocalRect(region);
                    var targets = new Rect[buttons.Length];

                    for (var i = 0; i < buttons.Length; i++)
                    {
                        Assert.That(buttons[i], Is.Not.Null, "equipment slot " + i + " is bound");
                        var target = buttons[i].transform as RectTransform;
                        targets[i] = LocalRect(target);
                        Assert.That(target.sizeDelta.x, Is.GreaterThanOrEqualTo(250f), "equipment touch width " + i);
                        Assert.That(target.sizeDelta.y, Is.GreaterThanOrEqualTo(180f), "equipment touch height " + i);
                        Assert.That(Contains(regionRect, targets[i]), Is.True, "equipment target stays within its panel " + i);

                        var frame = target.Find("Equipment Slot Surface") as RectTransform;
                        Assert.That(frame, Is.Not.Null, "visible item slot " + i);
                        Assert.That(buttons[i].targetGraphic, Is.SameAs(frame.GetComponent<Image>()), "hover/press feedback targets the visible slot " + i);
                        var targetLocal = CenteredRect(target);
                        Assert.That(Contains(targetLocal, LocalRect(frame)), Is.True, "item frame stays within its hit target " + i);
                        Assert.That(icons[i], Is.Not.Null, "equipment icon binding " + i);
                        Assert.That(Contains(targetLocal, LocalRect(icons[i].rectTransform)), Is.True, "equipment icon stays within its hit target " + i);
                        Assert.That(icons[i].rectTransform.sizeDelta.x, Is.GreaterThanOrEqualTo(80f), "equipment icon stays large within its frame " + i);
                        Assert.That(frame.sizeDelta.x, Is.GreaterThanOrEqualTo(92f), "equipment frame has room around the icon " + i);

                        Assert.That(equipment.SlotStatusLabels, Has.Length.EqualTo(4));
                        Assert.That(equipment.SlotStatusLabels[i], Is.Not.Null, "equipment state label " + i);
                        Assert.That(equipment.SlotStatusLabels[i].rectTransform.sizeDelta.y, Is.GreaterThanOrEqualTo(40f), "equipment state has a separate readable line below the icon frame " + i);
                        Assert.That(equipment.SlotStatusLabels[i].fontSize, Is.GreaterThanOrEqualTo(24), "equipment status keeps a readable fixed font " + i);
                        Assert.That(Contains(targetLocal, LocalRect(equipment.SlotStatusLabels[i].rectTransform)), Is.True, "equipment state label stays in its hit target " + i);
                        Assert.That(equipment.SlotStatusLabels[i].text, Is.Empty, "unselected equipment has no repeated equipped caption " + i);
                        var category = buttons[i].GetComponentInChildren<LocalizedLabel>(true);
                        Assert.That(category, Is.Not.Null, "equipment kind label " + i);
                        Assert.That(category.GetComponent<RectTransform>().sizeDelta.y, Is.GreaterThanOrEqualTo(44f), "equipment kind text has a dedicated title band " + i);
                        Assert.That(Contains(targetLocal, LocalRect(category.GetComponent<RectTransform>()), 0.01f), Is.True, "equipment kind label stays in its hit target " + i);
                        Assert.That(category.GetComponent<Text>().fontSize, Is.GreaterThanOrEqualTo(28), "equipment category stays fixed-size and readable " + i);
                        var frameRect = LocalRect(frame);
                        var categoryRect = LocalRect(category.GetComponent<RectTransform>());
                        var statusRect = LocalRect(equipment.SlotStatusLabels[i].rectTransform);
                        var selectionRect = LocalRect(equipment.SlotSelectionImages[i].rectTransform);
                        Assert.That(categoryRect.yMin - frameRect.yMax, Is.GreaterThanOrEqualTo(8f), "category text band is visibly separated from the icon frame " + i);
                        Assert.That(frameRect.yMin - statusRect.yMax, Is.GreaterThanOrEqualTo(10f), "status text band is visibly separated from the icon frame " + i);
                        Assert.That(categoryRect.yMin - selectionRect.yMax, Is.GreaterThanOrEqualTo(4f), "category text stays outside the selected frame outline " + i);
                        Assert.That(selectionRect.yMin - statusRect.yMax, Is.GreaterThanOrEqualTo(8f), "status text stays outside the selected frame outline " + i);
                        Assert.That(Contains(frameRect, LocalRect(icons[i].rectTransform)), Is.True, "equipment icon stays within its visible frame " + i);

                        var rootImage = buttons[i].GetComponent<Image>();
                        Assert.That(rootImage.raycastTarget, Is.True, "the complete cell remains a pointer target " + i);
                    }

                    for (var a = 0; a < targets.Length; a++)
                        for (var b = a + 1; b < targets.Length; b++)
                            Assert.That(targets[a].Overlaps(targets[b]), Is.False, "equipment touch targets do not overlap " + a + "/" + b);

                    var selectedName = RelativeRect(panel.SelectedName.rectTransform, surface as RectTransform);
                    Assert.That(equipment.SelectionState, Is.Not.Null, "selected item has a dedicated equipment state line");
                    var selectedState = RelativeRect(equipment.SelectionState.rectTransform, surface as RectTransform);
                    Assert.That(panel.SelectedCategory.rectTransform.sizeDelta.y, Is.GreaterThanOrEqualTo(40f), "selected item category has enough height for its actual type size");
                    var selectedCategory = RelativeRect(panel.SelectedCategory.rectTransform, surface as RectTransform);
                    Assert.That(selectedCategory.Overlaps(selectedName), Is.False, "selected item name and category have separate bounds");
                    Assert.That(equipment.SelectionState.fontSize, Is.GreaterThanOrEqualTo(27), "equipment state remains readable without best-fit shrinking");
                    Assert.That(equipment.SelectionState.rectTransform.sizeDelta.y, Is.GreaterThanOrEqualTo(42f), "equipment state allows the full fixed font line height");
                    Assert.That(selectedState.Overlaps(selectedName), Is.False, "equipment state does not collide with selected item name");
                    Assert.That(selectedState.Overlaps(selectedCategory), Is.False, "equipment state does not collide with item category");
                    foreach (var status in equipment.SlotStatusLabels)
                        Assert.That(RelativeRect(status.rectTransform, surface as RectTransform).Overlaps(selectedName), Is.False,
                            "equipment status text does not collide with selected item name");
                    var details = RelativeRect(equipment.Details.rectTransform, surface as RectTransform);
                    var comparison = RelativeRect(equipment.Comparison.rectTransform, surface as RectTransform);
                    Assert.That(selectedCategory.Overlaps(details), Is.False, "selected item category does not collide with item details");
                    Assert.That(selectedState.Overlaps(details), Is.False, "equipment state does not collide with item details");
                    Assert.That(details.Overlaps(comparison), Is.False, "item details and armor comparison have separate bounds");
                    Assert.That(equipment.Details.fontSize, Is.GreaterThanOrEqualTo(29), "item details use a readable fixed type size");
                    Assert.That(equipment.Details.rectTransform.sizeDelta.y, Is.GreaterThanOrEqualTo(84f), "item details retain a multi-line description area");
                    var drag = panel.GetComponent<InventoryDragController>();
                    Assert.That(equipment.EquipButton.GetComponent<RectTransform>().sizeDelta.x,
                        Is.GreaterThan(drag.SplitButton.GetComponent<RectTransform>().sizeDelta.x),
                        "the primary equipment action has more visual weight than Split");
                    foreach (var modalSurface in new[]
                    {
                        drag.SplitPanel.View.transform.Find("Split Panel"),
                        drag.SplitPanel.Actions.transform.Find("Item Actions Panel")
                    })
                    {
                        Assert.That(modalSurface, Is.Not.Null, "item modal surface exists");
                        var matte = modalSurface.Find("Matte Surface");
                        Assert.That(matte, Is.Not.Null, "item modal surface has a backing matte");
                        Assert.That(matte.GetComponent<Image>().color.a, Is.EqualTo(1f).Within(0.001f),
                            "modal backing is opaque so inventory labels cannot bleed through");
                    }
                    Assert.That(comparison.Overlaps(RelativeRect(equipment.EquipButton.transform as RectTransform, surface as RectTransform)), Is.False,
                        "armor comparison does not overlap Equip action");
                    Assert.That(comparison.Overlaps(RelativeRect(drag.SplitButton.transform as RectTransform, surface as RectTransform)), Is.False,
                        "armor comparison does not overlap Split action");
                }
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }

        private static Rect LocalRect(RectTransform rect) => new Rect(rect.anchoredPosition - Vector2.Scale(rect.sizeDelta, rect.pivot), rect.sizeDelta);
        private static Rect CenteredRect(RectTransform rect) => new Rect(-Vector2.Scale(rect.sizeDelta, rect.pivot), rect.sizeDelta);

        private static Rect RelativeRect(RectTransform rect, RectTransform ancestor)
        {
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            var min = ancestor.InverseTransformPoint(corners[0]);
            var max = ancestor.InverseTransformPoint(corners[2]);
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        private static bool Contains(Rect outer, Rect inner, float epsilon = 0.01f) =>
            inner.xMin >= outer.xMin - epsilon && inner.xMax <= outer.xMax + epsilon &&
            inner.yMin >= outer.yMin - epsilon && inner.yMax <= outer.yMax + epsilon;
    }
}
