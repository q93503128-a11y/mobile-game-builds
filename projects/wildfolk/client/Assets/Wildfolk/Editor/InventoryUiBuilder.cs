using System;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Wildfolk.Editor.GameplayUiFactory;

namespace Wildfolk.Editor
{
    public static class InventoryUiBuilder
    {
        private static readonly ArmorSlot[] EquipmentSlots = { ArmorSlot.Head, ArmorSlot.Body, ArmorSlot.Utility, ArmorSlot.Accessory };
        private static readonly string[] EquipmentNames = { "Head", "Body", "Utility", "Accessory" };
        private const float PanelWidth = 2050f;
        private const float PanelHeight = 840f;
        private const float SlotSizeValue = 140f;
        private const float GridWidth = 1338f;
        private const float GridX = -312f;
        private const float EquipmentX = 686f;
        private const float EquipmentWidth = 590f;

        internal static void Build(GameObject root, Transform safe, GameplaySession session, PlayerInventory owner, PlayerCrafting crafting, int count)
        {
            var panel = root.AddComponent<InventoryPanel>();
            panel.Session = session;
            panel.Owner = owner;
            panel.Storage = owner.GetComponent<PlayerStorage>();
            panel.Crafting = crafting;

            var drag = root.AddComponent<InventoryDragController>();
            drag.Owner = owner;
            drag.Session = session;
            drag.Storage = panel.Storage;
            drag.CanvasRect = (RectTransform)safe;

            var overlay = Scrim("Inventory Overlay", safe);
            panel.View = overlay.gameObject;
            var surface = Image("Inventory Panel", overlay.transform, "inventory_panel", new Vector2(.5f, .5f), Vector2.zero, new Vector2(PanelWidth, PanelHeight), true);
            QualityUiBuilder.Matte(surface.transform, new Vector2(PanelWidth, PanelHeight));
            surface.transform.Find("Matte Surface").GetComponent<Image>().color = new Color32(48, 36, 29, 244);
            var fit = surface.gameObject.AddComponent<ModalLayout>();
            fit.SafeArea = (RectTransform)safe;
            fit.HorizontalReserve = 24;
            fit.TopReserve = fit.BottomReserve = 28;
            fit.ReferenceSize = new Vector2(PanelWidth, PanelHeight);
            CreateRegions(surface.transform);

            Text("Title", surface.transform, "ui.inventory.title", new Vector2(-740, 358), new Vector2(420, 72), 36, TextAnchor.MiddleLeft);
            var close = Image("Close", surface.transform, "inventory_button", new Vector2(.5f, .5f), new Vector2(960, 358), new Vector2(104, 96), true);
            Text("Close Symbol", close.transform, "×", Vector2.zero, new Vector2(90, 84), 42);
            UnityEventTools.AddPersistentListener(Button(close).onClick, session.CloseInventory);

            Text("Storage Label", surface.transform, "ui.inventory.storage", new Vector2(-700, 270), new Vector2(500, 46), 31, TextAnchor.MiddleLeft);
            Text("Storage Count", surface.transform, "27", new Vector2(322, 270), new Vector2(66, 42), 28, TextAnchor.MiddleRight);
            var storage = Grid("Storage Slots", surface.transform, new Vector2(GridX, 25), new Vector2(GridWidth, 424));
            Text("Hotbar Label", surface.transform, "ui.inventory.hotbar", new Vector2(-700, -211), new Vector2(500, 40), 29, TextAnchor.MiddleLeft);
            Text("Hotbar Count", surface.transform, "9", new Vector2(322, -211), new Vector2(66, 38), 27, TextAnchor.MiddleRight);
            var quick = Grid("Hotbar Slots", surface.transform, new Vector2(GridX, -310), new Vector2(GridWidth, 136));

            panel.Slots = new InventorySlotView[count];
            for (var i = 0; i < count; i++)
            {
                var slot = Slot(i < PlayerInventory.HotbarSize ? quick : storage, i, SlotSizeValue, i < PlayerInventory.HotbarSize);
                var view = slot.GetComponent<InventorySlotView>();
                panel.Slots[i] = view;
                UnityEventTools.AddIntPersistentListener(Button(slot).onClick, panel.Select, i);
                var handler = slot.gameObject.AddComponent<InventorySlotDrag>();
                handler.Controller = drag;
                handler.View = view;
                handler.Index = i;
            }

            panel.SelectedIcon = Image("Selected Icon", surface.transform, "white", new Vector2(.5f, .5f), new Vector2(470, -200), new Vector2(72, 68));
            panel.SelectedIcon.preserveAspect = true;
            panel.SelectedName = Text("Selected Name", surface.transform, string.Empty, new Vector2(708, -185), new Vector2(424, 40), 31, TextAnchor.MiddleLeft);
            panel.SelectedCategory = Text("Selected Category", surface.transform, string.Empty, new Vector2(708, -219), new Vector2(424, 26), 24, TextAnchor.MiddleLeft);

            BuildEquipment(root, surface.transform, owner, panel);
            EnsureEquipmentControls(root.GetComponent<InventoryEquipment>());
            if (crafting != null)
            {
                panel.CraftingButton = QualityUiBuilder.Action(surface.transform, "Crafting", "ui.crafting.title", new Vector2(184, 358), new Vector2(242, 86), crafting.OpenHandCrafting, out _);
                CraftingUiBuilder.Build(root, safe, crafting);
            }

            overlay.gameObject.SetActive(false);
            var split = Image("Split", surface.transform, "inventory_button", new Vector2(.5f, .5f), new Vector2(820, -360), new Vector2(244, 92), true);
            Text("Label", split.transform, "ui.action.split", Vector2.zero, new Vector2(220, 78), 27);
            drag.SplitButton = Button(split);
            UnityEventTools.AddPersistentListener(drag.SplitButton.onClick, drag.OpenSplit);
            drag.ItemDrop = owner.GetComponent<PlayerItemDrop>();
            if (panel.Storage != null) ChestUiBuilder.Build(root, safe, owner, panel.Storage, drag);

            var dropZone = Image("Drop Zone", safe, "inventory_button", new Vector2(.5f, 1), new Vector2(0, -128), new Vector2(320, 88), true);
            Text("Label", dropZone.transform, "ui.inventory.drop", Vector2.zero, new Vector2(300, 68), 27);
            dropZone.gameObject.AddComponent<InventoryDropZone>().Controller = drag;
            drag.DropZone = dropZone.gameObject;
            drag.DropZone.SetActive(false);

            var ghost = Rect("Dragged Item", safe, new Vector2(.5f, .5f), Vector2.zero, new Vector2(94, 94));
            var group = ghost.gameObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            group.interactable = false;
            Image("Frame", ghost, "inventory_slot", new Vector2(.5f, .5f), Vector2.zero, new Vector2(94, 94));
            drag.Ghost = ghost;
            drag.GhostIcon = Image("Icon", ghost, "white", new Vector2(.5f, .5f), Vector2.zero, new Vector2(78, 78));
            drag.GhostIcon.preserveAspect = true;
            drag.GhostCount = Text("Stack Count", ghost, string.Empty, new Vector2(25, -32), new Vector2(50, 28), 23, TextAnchor.MiddleRight);
            ghost.gameObject.SetActive(false);
            StackSplitUiBuilder.Build(safe, drag);
            Arrange(panel);
        }

        [MenuItem("Wildfolk/Refresh Inventory Pilot Scenes")]
        public static void RefreshPilotScenes()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode before arranging the inventory pilot.");
            if (SceneManager.GetActiveScene().isDirty) throw new InvalidOperationException("Save the current scene before arranging the inventory pilot.");
            VisualArtImporter.Import();
            var activePath = SceneManager.GetActiveScene().path;
            foreach (var path in new[] { GameplaySceneBuilder.ScenePath, MultiplayerSceneBuilder.ScenePath })
            {
                var absolutePath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", path));
                if (!File.Exists(absolutePath)) continue;
                var scene = EditorSceneManager.OpenScene(path);
                foreach (var panel in UnityEngine.Object.FindObjectsByType<InventoryPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    var equipment = panel.GetComponent<InventoryEquipment>();
                    if (equipment == null) continue;
                    EnsureEquipmentControls(equipment);
                    Arrange(panel);
                }
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();
            if (!string.IsNullOrEmpty(activePath) && File.Exists(activePath)) EditorSceneManager.OpenScene(activePath);
        }

        internal static RectTransform Grid(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var grid = Rect(name, parent, new Vector2(.5f, .5f), position, size);
            var layout = grid.gameObject.AddComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(92, 92);
            layout.spacing = new Vector2(8, 8);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 9;
            layout.childAlignment = TextAnchor.UpperLeft;
            return grid;
        }

        internal static void Position(Component component, Vector2 position, Vector2 size)
        {
            var rect = (RectTransform)component.transform;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        internal static void Caption(Text text, Vector2 position, Vector2 size, int font = 30)
        {
            Position(text, position, size);
            text.fontSize = font;
        }

        internal static void ActionSize(Button button, Vector2 position, Vector2 size)
        {
            if (button == null) return;
            Position(button, position, size);
            var label = button.GetComponentInChildren<Text>(true);
            if (label != null) Caption(label, Vector2.zero, size - new Vector2(20, 12), 30);
        }

        internal static void SlotSize(InventorySlotView slot, float size = 128)
        {
            ((RectTransform)slot.transform).sizeDelta = Vector2.one * size;
            foreach (var child in new[] { "Selection", "Drop Target" })
            {
                var rect = slot.transform.Find(child) as RectTransform;
                if (rect != null) rect.sizeDelta = Vector2.one * size;
            }
            var inset = slot.transform.Find("Selection/Inset") as RectTransform;
            if (inset != null) inset.sizeDelta = Vector2.one * (size - 8);
            Position(slot.Icon, new Vector2(0, 3), new Vector2(size * .68f, size * .72f));
            Caption(slot.Count, new Vector2(size * .24f, -size * .31f), new Vector2(60, 44), 28);
            var quick = slot.transform.Find("Quick Slot");
            if (quick != null) Caption(quick.GetComponent<Text>(), new Vector2(-size * .3f, size * .3f), new Vector2(32, 36), 22);
        }

        // Canonical layout authoring for the shared SP/MP inventory presentation. All item and drag bindings remain on their existing controllers.
        internal static void Arrange(InventoryPanel panel)
        {
            if (panel == null || panel.View == null) return;
            var surface = panel.View.transform.Find("Inventory Panel");
            if (surface == null) return;
            var equipment = panel.GetComponent<InventoryEquipment>();
            if (equipment == null) return;
            EnsureEquipmentControls(equipment);

            var size = new Vector2(PanelWidth, PanelHeight);
            ((RectTransform)surface).sizeDelta = size;
            var fit = surface.GetComponent<ModalLayout>();
            if (fit != null)
            {
                fit.ReferenceSize = size;
                fit.HorizontalReserve = 24;
                fit.TopReserve = fit.BottomReserve = 28;
            }
            var background = surface.Find("Matte Surface");
            if (background != null)
            {
                ((RectTransform)background).sizeDelta = size - Vector2.one * 6;
                background.GetComponent<Image>().color = new Color32(48, 36, 29, 244);
            }
            SetNineSlice(surface.GetComponent<Image>(), "inventory_panel");
            CreateRegions(surface);

            Caption(surface.Find("Title").GetComponent<Text>(), new Vector2(-740, 358), new Vector2(420, 72), 36);
            var close = surface.Find("Close").GetComponent<Button>();
            SetButtonArt(close, "inventory_button");
            ActionSize(close, new Vector2(960, 358), new Vector2(104, 96));
            var closeSymbol = surface.Find("Close/Close Symbol");
            if (closeSymbol != null) Caption(closeSymbol.GetComponent<Text>(), Vector2.zero, new Vector2(90, 84), 42);

            Caption(surface.Find("Storage Label").GetComponent<Text>(), new Vector2(-700, 270), new Vector2(500, 46), 31);
            var storageCount = FindText(surface, "Storage Count") ?? Text("Storage Count", surface, "27", new Vector2(322, 270), new Vector2(66, 42), 28, TextAnchor.MiddleRight);
            var hotbarCount = FindText(surface, "Hotbar Count") ?? Text("Hotbar Count", surface, "9", new Vector2(322, -211), new Vector2(66, 38), 27, TextAnchor.MiddleRight);
            Caption(storageCount, new Vector2(322, 270), new Vector2(66, 42), 28);
            Caption(surface.Find("Hotbar Label").GetComponent<Text>(), new Vector2(-700, -211), new Vector2(500, 40), 29);
            Caption(hotbarCount, new Vector2(322, -211), new Vector2(66, 38), 27);
            Position(surface.Find("Storage Slots"), new Vector2(GridX, 25), new Vector2(GridWidth, 424));
            Position(surface.Find("Hotbar Slots"), new Vector2(GridX, -310), new Vector2(GridWidth, 136));
            foreach (var name in new[] { "Storage Slots", "Hotbar Slots" })
            {
                var grid = surface.Find(name).GetComponent<GridLayoutGroup>();
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = 9;
                grid.cellSize = Vector2.one * SlotSizeValue;
                grid.spacing = Vector2.one * 8;
                grid.padding = new RectOffset();
                grid.childAlignment = TextAnchor.UpperLeft;
            }
            if (panel.Slots != null)
                foreach (var slot in panel.Slots) ApplyInventorySlotStyle(slot);

            if (panel.SelectedIcon != null)
            {
                Position(panel.SelectedIcon, new Vector2(470, -200), new Vector2(72, 68));
                panel.SelectedIcon.preserveAspect = true;
            }
            if (panel.SelectedName != null) Caption(panel.SelectedName, new Vector2(708, -185), new Vector2(424, 40), 31);
            if (panel.SelectedCategory != null) Caption(panel.SelectedCategory, new Vector2(708, -219), new Vector2(424, 26), 24);

            ArrangeEquipment(surface, equipment);
            if (panel.CraftingButton != null)
            {
                SetButtonArt(panel.CraftingButton, "inventory_button");
                ActionSize(panel.CraftingButton, new Vector2(184, 358), new Vector2(242, 86));
                SetButtonColors(panel.CraftingButton);
            }
            var drag = panel.GetComponent<InventoryDragController>();
            if (drag != null)
            {
                if (drag.SplitButton != null)
                {
                    SetButtonArt(drag.SplitButton, "inventory_button");
                    ActionSize(drag.SplitButton, new Vector2(820, -360), new Vector2(244, 92));
                    SetButtonColors(drag.SplitButton);
                }
                if (drag.DropZone != null)
                {
                    var drop = drag.DropZone.GetComponent<Image>();
                    if (drop != null) SetNineSlice(drop, "inventory_button");
                }
                if (drag.Ghost != null)
                {
                    var frame = drag.Ghost.Find("Frame");
                    if (frame != null) SetNineSlice(frame.GetComponent<Image>(), "inventory_slot");
                }
                ArrangeSplitArt(drag.SplitPanel);
            }
            var hint = surface.Find("Slot Hint");
            if (hint != null) hint.gameObject.SetActive(false);
            var portrait = surface.Find("Survivor Preview");
            if (portrait != null) UnityEngine.Object.DestroyImmediate(portrait.gameObject);

            foreach (var line in surface.GetComponentsInChildren<Image>(true))
                if (line.transform.parent == surface && line.name == "Connection") UnityEngine.Object.DestroyImmediate(line.gameObject);
        }

        private static void BuildEquipment(GameObject root, Transform surface, PlayerInventory player, InventoryPanel panel)
        {
            var equipment = root.AddComponent<InventoryEquipment>();
            equipment.Player = player;
            equipment.Panel = panel;
            equipment.Equipment = player.GetComponent<PlayerEquipment>();
            Text("Equipment Title", surface, "ui.inventory.equipment", new Vector2(EquipmentX, 342), new Vector2(EquipmentWidth, 42), 31, TextAnchor.MiddleLeft);

            for (var i = 0; i < 2; i++)
            {
                var slotType = i == 0 ? ArmorSlot.Head : ArmorSlot.Body;
                var name = i == 0 ? "Head" : "Body";
                var slot = Image(name + " Equipment", surface, "inventory_rail", new Vector2(.5f, .5f), Vector2.zero, new Vector2(EquipmentWidth, 108), true);
                var button = Button(slot);
                var drag = slot.gameObject.AddComponent<EquipmentSlotDrag>();
                drag.Controller = root.GetComponent<InventoryDragController>();
                drag.Slot = slotType;
                UnityEventTools.AddIntPersistentListener(button.onClick, equipment.SelectSlot, (int)slotType);
                var icon = Image("Equipped Item", slot.transform, "white", new Vector2(.5f, .5f), new Vector2(-220, 0), new Vector2(64, 68));
                icon.preserveAspect = true;
                if (i == 0) { equipment.HeadButton = button; equipment.HeadIcon = icon; }
                else { equipment.BodyButton = button; equipment.BodyIcon = icon; }
            }
            equipment.Details = Text("Armor Details", surface, string.Empty, new Vector2(EquipmentX, -266), new Vector2(520, 50), 23, TextAnchor.MiddleLeft);
            equipment.Comparison = Text("Comparison", surface, string.Empty, new Vector2(EquipmentX, -309), new Vector2(520, 28), 23, TextAnchor.MiddleLeft);
            equipment.EquipButton = QualityUiBuilder.Action(surface, "Equip", "ui.action.equip", new Vector2(534, -368), new Vector2(244, 82), equipment.Equip, out equipment.EquipLabel);
            equipment.UnequipButton = QualityUiBuilder.Action(surface, "Unequip", "ui.action.unequip", new Vector2(534, -368), new Vector2(244, 82), equipment.Unequip, out _);
            for (var i = 0; i < 2; i++)
            {
                var slotType = i == 0 ? ArmorSlot.Head : ArmorSlot.Body;
                var key = "ui.slot." + slotType;
                EnsureEquipmentText(equipment.HeadButton.transform.parent, key);
            }
        }

        internal static void EnsureEquipmentControls(InventoryEquipment equipment)
        {
            if (equipment == null || equipment.HeadButton == null || equipment.BodyButton == null) return;
            if (equipment.UtilityButton == null) equipment.UtilityButton = CloneEquipmentButton(equipment, "Utility", ArmorSlot.Utility, out equipment.UtilityIcon);
            if (equipment.AccessoryButton == null) equipment.AccessoryButton = CloneEquipmentButton(equipment, "Accessory", ArmorSlot.Accessory, out equipment.AccessoryIcon);
            var parent = equipment.HeadButton.transform.parent;
            equipment.SlotItemNames = new Text[4];
            equipment.SlotStatusLabels = new Text[4];
            equipment.SlotSelectionImages = new Image[4];
            var buttons = new[] { equipment.HeadButton, equipment.BodyButton, equipment.UtilityButton, equipment.AccessoryButton };
            var icons = new[] { equipment.HeadIcon, equipment.BodyIcon, equipment.UtilityIcon, equipment.AccessoryIcon };
            for (var i = 0; i < buttons.Length; i++)
            {
                var button = buttons[i];
                if (button == null) continue;
                button.name = EquipmentNames[i] + " Equipment";
                var image = button.GetComponent<Image>();
                SetNineSlice(image, "inventory_rail");
                SetButtonColors(button);
                var icon = icons[i];
                if (icon != null)
                {
                    icon.preserveAspect = true;
                    Position(icon, new Vector2(-220, 0), new Vector2(64, 68));
                }
                var key = "ui.slot." + EquipmentNames[i];
                var category = EnsureEquipmentText(parent, key);
                category.transform.SetParent(button.transform, false);
                Position(category, new Vector2(-48, 22), new Vector2(256, 34));
                category.fontSize = 27;
                category.alignment = TextAnchor.MiddleLeft;

                var itemName = FindText(button.transform, "Equipped Item Name") ?? Text("Equipped Item Name", button.transform, "—", new Vector2(0, -20), new Vector2(290, 36), 27, TextAnchor.MiddleLeft);
                Position(itemName, new Vector2(-48, -20), new Vector2(256, 36));
                itemName.fontSize = 27;
                itemName.resizeTextForBestFit = true;
                itemName.resizeTextMinSize = 20;
                itemName.resizeTextMaxSize = 27;
                itemName.horizontalOverflow = HorizontalWrapMode.Overflow;
                itemName.color = new Color32(237, 221, 194, 255);
                itemName.raycastTarget = false;
                equipment.SlotItemNames[i] = itemName;

                var status = FindText(button.transform, "Equipment Status") ?? Text("Equipment Status", button.transform, string.Empty, new Vector2(189, 0), new Vector2(112, 42), 22, TextAnchor.MiddleCenter);
                Position(status, new Vector2(189, 0), new Vector2(112, 42));
                status.fontSize = 22;
                status.resizeTextForBestFit = true;
                status.resizeTextMinSize = 18;
                status.resizeTextMaxSize = 22;
                status.color = new Color32(222, 174, 120, 255);
                status.raycastTarget = false;
                equipment.SlotStatusLabels[i] = status;

                var selected = FindImage(button.transform, "Equipment Selection") ?? Image("Equipment Selection", button.transform, "inventory_selected_slot", new Vector2(.5f, .5f), Vector2.zero, new Vector2(EquipmentWidth + 8, 116));
                Position(selected, Vector2.zero, new Vector2(EquipmentWidth + 8, 116));
                SetNineSlice(selected, "inventory_selected_slot");
                selected.raycastTarget = false;
                equipment.SlotSelectionImages[i] = selected;
            }
        }

        private static Button CloneEquipmentButton(InventoryEquipment equipment, string name, ArmorSlot slot, out Image icon)
        {
            var clone = UnityEngine.Object.Instantiate(equipment.HeadButton.gameObject, equipment.HeadButton.transform.parent);
            clone.name = name + " Equipment";
            var button = clone.GetComponent<Button>();
            button.onClick = new Button.ButtonClickedEvent();
            UnityEventTools.AddIntPersistentListener(button.onClick, equipment.SelectSlot, (int)slot);
            var drag = clone.GetComponent<EquipmentSlotDrag>();
            if (drag == null) drag = clone.AddComponent<EquipmentSlotDrag>();
            drag.Controller = equipment.GetComponent<InventoryDragController>();
            drag.Slot = slot;
            var iconPath = AnimationUtility.CalculateTransformPath(equipment.HeadIcon.transform, equipment.HeadButton.transform);
            icon = clone.transform.Find(iconPath).GetComponent<Image>();
            icon.sprite = null;
            icon.enabled = false;
            return button;
        }

        private static void ArrangeEquipment(Transform surface, InventoryEquipment equipment)
        {
            Caption(surface.Find("Equipment Title").GetComponent<Text>(), new Vector2(EquipmentX, 342), new Vector2(EquipmentWidth, 42), 31);
            var buttons = new[] { equipment.HeadButton, equipment.BodyButton, equipment.UtilityButton, equipment.AccessoryButton };
            for (var i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] == null) continue;
                Position(buttons[i], new Vector2(EquipmentX, 250 - i * 112), new Vector2(EquipmentWidth, 100));
            }
            Position(equipment.Details, new Vector2(EquipmentX, -266), new Vector2(520, 50));
            Position(equipment.Comparison, new Vector2(EquipmentX, -309), new Vector2(520, 28));
            equipment.Details.fontSize = 23;
            equipment.Comparison.fontSize = 23;
            equipment.Details.resizeTextForBestFit = true;
            equipment.Details.resizeTextMinSize = 19;
            equipment.Details.resizeTextMaxSize = 23;
            equipment.Comparison.resizeTextForBestFit = true;
            equipment.Comparison.resizeTextMinSize = 19;
            equipment.Comparison.resizeTextMaxSize = 23;
            SetButtonArt(equipment.EquipButton, "inventory_button");
            SetButtonArt(equipment.UnequipButton, "inventory_button");
            ActionSize(equipment.EquipButton, new Vector2(534, -368), new Vector2(244, 82));
            ActionSize(equipment.UnequipButton, new Vector2(534, -368), new Vector2(244, 82));
            SetButtonColors(equipment.EquipButton);
            SetButtonColors(equipment.UnequipButton);
            equipment.EquipLabel.fontSize = 27;
            var split = surface.Find("Split");
            if (split != null)
            {
                SetButtonArt(split.GetComponent<Button>(), "inventory_button");
                ActionSize(split.GetComponent<Button>(), new Vector2(820, -368), new Vector2(244, 82));
            }
        }

        private static void CreateRegions(Transform surface)
        {
            EnsureRegion(surface, "Storage Region", "inventory_rail", new Vector2(GridX, 24), new Vector2(GridWidth, 454), 1);
            EnsureRegion(surface, "Hotbar Region", "inventory_rail", new Vector2(GridX, -307), new Vector2(GridWidth, 192), 2);
            EnsureRegion(surface, "Equipment Region", "inventory_rail", new Vector2(EquipmentX, 42), new Vector2(EquipmentWidth, 620), 3);
            EnsureRegion(surface, "Selected Item Region", "inventory_panel", new Vector2(EquipmentX, -287), new Vector2(EquipmentWidth, 250), 4);
        }

        private static void EnsureRegion(Transform parent, string name, string sprite, Vector2 position, Vector2 size, int sibling)
        {
            var image = FindImage(parent, name);
            if (image == null) image = Image(name, parent, sprite, new Vector2(.5f, .5f), position, size);
            Position(image, position, size);
            SetNineSlice(image, sprite);
            image.color = name == "Selected Item Region" ? new Color32(232, 211, 183, 255) : Color.white;
            image.raycastTarget = false;
            image.transform.SetSiblingIndex(Mathf.Min(sibling, parent.childCount - 1));
        }

        private static void ApplyInventorySlotStyle(InventorySlotView slot)
        {
            if (slot == null) return;
            SlotSize(slot, SlotSizeValue);
            var slotImage = slot.GetComponent<Image>();
            SetNineSlice(slotImage, "inventory_slot");
            slotImage.color = Color.white;
            if (slot.Selection != null)
            {
                Position(slot.Selection, Vector2.zero, Vector2.one * (SlotSizeValue + 10));
                SetNineSlice(slot.Selection, "inventory_selected_slot");
                slot.Selection.color = Color.white;
                var inset = slot.Selection.transform.Find("Inset");
                if (inset != null) inset.gameObject.SetActive(false);
            }
            if (slot.Icon != null)
            {
                Position(slot.Icon, new Vector2(0, 8), new Vector2(92, 96));
                slot.Icon.preserveAspect = true;
            }
            if (slot.Count != null)
            {
                Caption(slot.Count, new Vector2(39, -46), new Vector2(58, 34), 29);
                slot.Count.resizeTextForBestFit = true;
                slot.Count.resizeTextMinSize = 22;
                slot.Count.resizeTextMaxSize = 29;
                slot.Count.fontStyle = FontStyle.Bold;
                slot.Count.color = new Color32(255, 238, 206, 255);
            }
            var quick = slot.transform.Find("Quick Slot");
            if (quick != null)
            {
                var label = quick.GetComponent<Text>();
                Caption(label, new Vector2(-44, 44), new Vector2(30, 28), 23);
                label.fontStyle = FontStyle.Bold;
                label.color = new Color32(232, 196, 150, 255);
            }
            SetButtonColors(slot.GetComponent<Button>());
        }

        private static void ArrangeSplitArt(StackSplitPanel split)
        {
            if (split == null) return;
            foreach (var button in split.GetComponentsInChildren<Button>(true))
            {
                var image = button.GetComponent<Image>();
                if (image != null) SetNineSlice(image, "inventory_button");
                SetButtonColors(button);
            }
            foreach (var image in split.GetComponentsInChildren<Image>(true))
            {
                if (image.name == "Split Panel" || image.name == "Item Actions Panel") SetNineSlice(image, "inventory_panel");
            }
        }

        private static Text EnsureEquipmentText(Transform parent, string key)
        {
            foreach (var label in parent.GetComponentsInChildren<LocalizedLabel>(true))
                if (label.Key == key) return label.GetComponent<Text>();
            return Text("Equipment Category", parent, key, Vector2.zero, new Vector2(280, 34), 27, TextAnchor.MiddleLeft);
        }

        private static Text FindText(Transform parent, string name)
        {
            foreach (var text in parent.GetComponentsInChildren<Text>(true)) if (text.name == name) return text;
            return null;
        }

        private static Image FindImage(Transform parent, string name)
        {
            foreach (var image in parent.GetComponentsInChildren<Image>(true)) if (image.name == name) return image;
            return null;
        }

        private static void SetNineSlice(Image image, string spriteName)
        {
            if (image == null) return;
            image.sprite = VisualArtImporter.Sprite(spriteName);
            image.type = UnityEngine.UI.Image.Type.Sliced;
            image.color = Color.white;
        }

        private static void SetButtonArt(Button button, string spriteName)
        {
            if (button == null) return;
            SetNineSlice(button.GetComponent<Image>(), spriteName);
            SetButtonColors(button);
        }

        private static void SetButtonColors(Button button)
        {
            if (button == null) return;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color32(255, 226, 183, 255);
            colors.pressedColor = new Color32(191, 131, 79, 255);
            colors.selectedColor = new Color32(244, 205, 147, 255);
            colors.disabledColor = new Color32(123, 104, 87, 210);
            button.colors = colors;
        }
    }
}
