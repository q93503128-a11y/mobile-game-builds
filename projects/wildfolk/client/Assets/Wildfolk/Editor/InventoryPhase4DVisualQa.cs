using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Wildfolk.Editor
{
    [InitializeOnLoad]
    public static class InventoryPhase4DVisualQa
    {
        private const string RunKey = "Lowfire.Phase4D.Run";
        private const string RootKey = "Lowfire.Phase4D.Root";
        private const string PreviousProfileKey = "Lowfire.Phase4D.PreviousProfile";
        private const string PreviousRepositoryKey = "Lowfire.Phase4D.PreviousRepository";
        private const string PreviousSeedKey = "Lowfire.Phase4D.PreviousSeed";
        public static string Phase { get; private set; } = "idle";
        private static int BoundaryFailures;
        private static string Root => UnityEditor.SessionState.GetString(RootKey, "");

        static InventoryPhase4DVisualQa()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state != PlayModeStateChange.EnteredPlayMode || !UnityEditor.SessionState.GetBool(RunKey, false)) return;
                UnityEditor.SessionState.SetBool(RunKey, false);
                var runner = new GameObject("Phase 4D isolated visual QA").AddComponent<DevelopmentCoroutineHost>();
                Object.DontDestroyOnLoad(runner.gameObject);
                runner.StartCoroutine(Guard(Flow()));
            };
        }

        public static void Prepare(string captureRoot)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Prepare the isolated profile before Play Mode.");
            if (string.IsNullOrWhiteSpace(captureRoot)) throw new ArgumentException("Capture output path is required.", nameof(captureRoot));
            Directory.CreateDirectory(captureRoot);
            BoundaryFailures = 0;
            UnityEditor.SessionState.SetString(RootKey, Path.GetFullPath(captureRoot));
            UnityEditor.SessionState.SetString(PreviousProfileKey, UnityEditor.SessionState.GetString("Wildfolk.QA.Directory", ""));
            UnityEditor.SessionState.SetString(PreviousRepositoryKey, SceneFlow.Repository.Path);
            UnityEditor.SessionState.SetString(PreviousSeedKey, WorldGenerator.SelectedSeed.HasValue ? WorldGenerator.SelectedSeed.Value.ToString() : "");
            UnityEditor.SessionState.SetString("Wildfolk.QA.Directory", Path.Combine(Path.GetTempPath(), "Lowfire-Phase4D-QA-" + Guid.NewGuid().ToString("N")));
            UnityEditor.SessionState.SetBool(RunKey, true);
            WorldGenerator.SelectedSeed = 9472026;
            SceneFlow.Repository = SaveRepository.Default();
        }

        private static void Log(string text)
        {
            if (!string.IsNullOrEmpty(Root))
            {
                Directory.CreateDirectory(Root);
                File.AppendAllText(Path.Combine(Root, "qa-live.tsv"), DateTime.UtcNow.ToString("O") + "\t" + Phase + "\t" + text + "\n");
            }
            Debug.Log("[Phase4D QA] " + Phase + " " + text);
        }

        private static void Check(bool ok, string text)
        {
            Log((ok ? "PASS " : "FAIL ") + text);
            if (!ok) throw new InvalidOperationException(text);
        }

        private static void CheckBoundary(bool ok, string text)
        {
            Log((ok ? "PASS " : "FAIL ") + text);
            if (!ok) BoundaryFailures++;
        }

        private static IEnumerator Guard(IEnumerator flow)
        {
            var stack = new Stack<IEnumerator>();
            stack.Push(flow);
            while (stack.Count > 0)
            {
                bool more;
                object value;
                try { more = stack.Peek().MoveNext(); value = more ? stack.Peek().Current : null; }
                catch (Exception error) { Phase = "failed"; Log(error.ToString()); RestoreEditorEnvironment(); yield break; }
                if (!more) { stack.Pop(); continue; }
                if (value is IEnumerator nested) stack.Push(nested); else yield return value;
            }
            RestoreEditorEnvironment();
            Phase = "complete";
            Log("complete");
        }

        private static void RestoreEditorEnvironment()
        {
            var previousProfile = UnityEditor.SessionState.GetString(PreviousProfileKey, "");
            var previousRepository = UnityEditor.SessionState.GetString(PreviousRepositoryKey, "");
            var previousSeed = UnityEditor.SessionState.GetString(PreviousSeedKey, "");
            UnityEditor.SessionState.SetString("Wildfolk.QA.Directory", previousProfile);
            SceneFlow.Repository = string.IsNullOrEmpty(previousRepository) ? SaveRepository.Default() : new SaveRepository(previousRepository);
            int parsedSeed;
            WorldGenerator.SelectedSeed = int.TryParse(previousSeed, out parsedSeed) ? (int?)parsedSeed : null;
            UnityEditor.SessionState.EraseString(PreviousProfileKey);
            UnityEditor.SessionState.EraseString(PreviousRepositoryKey);
            UnityEditor.SessionState.EraseString(PreviousSeedKey);
        }

        private static IEnumerator Flow()
        {
            Phase = "loading";
            Check(DevelopmentProfile.Isolated, "QA profile isolated from user save");
            Check(SceneFlow.Repository != null && SceneFlow.Repository.Path.StartsWith(DevelopmentProfile.Directory, StringComparison.OrdinalIgnoreCase), "repository targets unique temporary QA directory");
            yield return SceneManager.LoadSceneAsync(SceneFlow.GameplayScene);
            WorldSaveService save = null;
            var deadline = Time.realtimeSinceStartup + 60;
            while (Time.realtimeSinceStartup < deadline)
            {
                save = Object.FindFirstObjectByType<WorldSaveService>();
                if (save != null && save.World != null && save.World.Map != null) break;
                yield return null;
            }
            Check(save != null && save.World.Map != null, "gameplay world ready");
            save.AutoSaveEnabled = false;
            save.Clock.enabled = false;
            save.World.Animals.enabled = false;
            save.World.Renewal.enabled = false;
            save.GetComponent<ZombieSpawner>().enabled = false;
            save.GetComponent<GoblinSpawner>().enabled = false;
            var player = save.Player;
            player.GetComponent<PlayerInputAdapter>().enabled = false;
            var inventory = player.GetComponent<PlayerInventory>();
            var equipment = player.GetComponent<PlayerEquipment>();
            equipment.RestoreArmor(null, null);
            equipment.RestoreUtility(null);
            equipment.RestoreAccessory(null);

            foreach (var id in new[] { "leather_head", "leather_body", "lantern", "undead_kings_crown" })
            {
                var item = save.Catalog.Item(id);
                Check(inventory.Model.Add(item, 1) == 1 && equipment.TryEquip(item), "fixture equips " + id);
            }
            var states = new SavedStack[inventory.Model.Capacity];
            for (var i = 0; i < states.Length; i++) states[i] = new SavedStack();
            Put(states, 0, save.Catalog.Item("iron_head"), 1);
            Put(states, 1, save.Catalog.Item("iron_body"), 1);
            Put(states, 2, save.Catalog.Item("crystal_lantern"), 1);
            Put(states, 3, save.Catalog.Item("undead_kings_crown"), 1);
            var wood = save.Catalog.Item("wood");
            Check(wood.MaxStack >= 30, "wood fixture supports split quantity");
            Put(states, 8, wood, 30);
            var used = new HashSet<string>(states.Where(x => x.Count > 0).Select(x => x.Item), StringComparer.Ordinal);
            var candidates = save.Catalog.Items.Where(x => x != null && x.WaterCapacity == 0 && !used.Contains(x.Id)).ToArray();
            var next = 4;
            foreach (var item in candidates)
            {
                while (next < states.Length && states[next].Count > 0) next++;
                if (next >= states.Length) break;
                Put(states, next++, item, 1);
                used.Add(item.Id);
            }
            Check(states.All(x => x.Count > 0), "all 27 storage and 9 hotbar slots populated");
            inventory.Model.Restore(states, save.Catalog);
            var baseItem = save.Catalog.Item("iron_head");
            Check(inventory.Model.ConsumeSlot(0, baseItem, 1),
                "empty the selected hotbar slot for the unselected base inventory capture");
            inventory.SelectSlot(0);
            var panel = Object.FindFirstObjectByType<InventoryPanel>();
            var equipmentUi = Object.FindFirstObjectByType<InventoryEquipment>();
            var drag = Object.FindFirstObjectByType<InventoryDragController>();
            Check(panel != null && equipmentUi != null && drag != null, "canonical inventory UI is present");
            player.GetComponent<PlayerInputCommands>().Session.OpenInventory();
            yield return null;
            panel.RefreshSelection();
            equipmentUi.Refresh();
            Check(equipment.HeadArmor != null && equipment.BodyArmor != null && equipment.Utility != null && equipment.Accessory != null, "four equipment categories contain real catalog icons");
            Check(equipmentUi.SlotStatusLabels.All(x => x != null && string.IsNullOrEmpty(x.text)), "base equipment cells do not repeat equipped status text");
            Check(!drag.SplitButton.interactable, "Split is disabled while no item is selected");
            var splitColors = drag.SplitButton.colors;
            Check(splitColors.disabledColor.grayscale < splitColors.normalColor.grayscale - .15f,
                "disabled Split has a visibly darker tint than its active state");
            var view = panel.View.transform;
            CheckTextBounds(view);

            Phase = "resolution-localization-captures";
            foreach (var size in new[] { new Vector2Int(844, 390), new Vector2Int(1280, 720), new Vector2Int(1920, 1080) })
            foreach (var language in new[] { "ko", "en" })
            {
                SelectResolution(size.x, size.y);
                L.SetLanguage(language);
                ClearEquipmentFilter(equipmentUi);
                equipmentUi.Refresh();
                panel.RefreshSelection();
                yield return Delay(.4f);
                yield return Shot("inventory-" + language + "-" + size.x + "x" + size.y + "-base");
                Check(Screen.width == size.x && Screen.height == size.y, "Game View rendered at requested " + size.x + "x" + size.y + " " + language);
                CheckTextBounds(view);
            }

            Check(inventory.Model.Add(baseItem, 1) == 1, "restore the equipment candidate after the unselected base captures");
            inventory.SelectSlot(0);
            panel.RefreshSelection();
            equipmentUi.Refresh();
            yield return SetResolution(844, 390);
            L.SetLanguage("ko");
            ClearEquipmentFilter(equipmentUi);
            var ordinaryItemIndex = Enumerable.Range(0, inventory.Model.Capacity).First(i => inventory.Model.GetSlot(i).Item == wood);
            panel.Select(ordinaryItemIndex);
            drag.Select(ordinaryItemIndex, false);
            equipmentUi.Refresh();
            Check(panel.SelectedName.text == wood.LocalizedName && string.IsNullOrEmpty(equipmentUi.SelectionState.text),
                "ordinary item selection shows item details without equipment state");
            Check(drag.SplitButton.interactable, "Split is enabled for a selected stack with more than one item");
            yield return CaptureState("item-selected", panel, equipmentUi, drag);

            var reserveIndex = inventory.Model.Capacity - 1;
            var reserveStack = inventory.Model.GetSlot(reserveIndex);
            Check(!reserveStack.IsEmpty && inventory.Model.ConsumeSlot(reserveIndex, reserveStack.Item, reserveStack.Count),
                "visual interaction fixture reserves one bag slot for an unequip action after full-capacity base captures");

            Phase = "replacement-and-equipment-states";
            yield return SetResolution(844, 390);
            L.SetLanguage("ko");
            ClearEquipmentFilter(equipmentUi);
            equipmentUi.Refresh();
            panel.RefreshSelection();
            var woodIndex = Enumerable.Range(0, inventory.Model.Capacity).First(i => inventory.Model.GetSlot(i).Item == wood);
            panel.Select(woodIndex);
            drag.Select(woodIndex, false);
            equipmentUi.SelectSlot((int)ArmorSlot.Head);
            Canvas.ForceUpdateCanvases();
            Check(equipmentUi.SelectionState.text == L.Text("ui.inventory.equipped") && equipmentUi.UnequipButton.gameObject.activeSelf,
                "equipped head can be selected directly without starting a drag");
            yield return CaptureState("equipment-selected", panel, equipmentUi, drag);

            ClearEquipmentFilter(equipmentUi);
            var ironHeadIndex = Enumerable.Range(0, inventory.Model.Capacity).First(i => inventory.Model.GetSlot(i).Item.Id == "iron_head");
            panel.Select(ironHeadIndex);
            drag.Select(ironHeadIndex, false);
            equipmentUi.SelectSlot((int)ArmorSlot.Head);
            Canvas.ForceUpdateCanvases();
            Check(equipmentUi.EquipButton.gameObject.activeSelf && equipmentUi.EquipLabel.text == L.Text("ui.action.replace"),
                "compatible head candidate exposes the primary Replace action");
            Check(equipmentUi.SelectionState.text.Contains(equipment.HeadArmor.LocalizedName) && equipmentUi.Comparison.text.Contains("↑"),
                "replacement candidate names current gear and shows an upward defense delta");
            yield return CaptureState("replacement-candidate", panel, equipmentUi, drag);
            equipmentUi.EquipButton.onClick.Invoke();
            Check(equipment.HeadArmor == save.Catalog.Item("iron_head"), "head replacement applies through existing PlayerEquipment");
            Check(equipmentUi.SelectionState.text == L.Text("ui.inventory.equipped") && equipmentUi.UnequipButton.gameObject.activeSelf &&
                  !equipmentUi.EquipButton.gameObject.activeSelf && equipmentUi.SlotStatusLabels[0].text == string.Empty,
                "replacement completion focuses the newly equipped item and clears the reverse-candidate state");
            yield return CaptureState("replacement-applied", panel, equipmentUi, drag);

            var leatherHeadIndex = Enumerable.Range(0, inventory.Model.Capacity).First(i => inventory.Model.GetSlot(i).Item.Id == "leather_head");
            panel.Select(leatherHeadIndex);
            drag.Select(leatherHeadIndex, false);
            equipmentUi.SelectSlot((int)ArmorSlot.Head);
            Canvas.ForceUpdateCanvases();
            Check(equipmentUi.EquipLabel.text == L.Text("ui.action.replace") && equipmentUi.Comparison.text.Contains("↓"),
                "weaker replacement candidate shows a downward defense delta");
            yield return CaptureState("replacement-downgrade-candidate", panel, equipmentUi, drag);

            Phase = "incompatible-state";
            ClearEquipmentFilter(equipmentUi);
            var ironBodyIndex = Enumerable.Range(0, inventory.Model.Capacity).First(i => inventory.Model.GetSlot(i).Item.Id == "iron_body");
            panel.Select(ironBodyIndex);
            drag.Select(ironBodyIndex, false);
            equipmentUi.SelectSlot((int)ArmorSlot.Accessory);
            panel.Select(ironBodyIndex);
            Canvas.ForceUpdateCanvases();
            Check(!equipmentUi.EquipButton.gameObject.activeSelf && equipmentUi.SelectionState.text.Contains(L.Text("ui.slot." + ArmorSlot.Accessory)),
                "incompatible item is clearly marked and cannot be equipped into Accessory");
            yield return CaptureState("incompatible-candidate", panel, equipmentUi, drag);

            Phase = "unequip-and-empty-equipment-states";
            ClearEquipmentFilter(equipmentUi);
            panel.Select(woodIndex);
            drag.Select(woodIndex, false);
            equipmentUi.SelectSlot((int)ArmorSlot.Head);
            Check(equipmentUi.UnequipButton.gameObject.activeSelf, "current head selection exposes Unequip");
            equipmentUi.UnequipButton.onClick.Invoke();
            Check(equipment.HeadArmor == null, "Unequip clears the selected equipment slot through the existing model");
            Check(string.IsNullOrEmpty(equipmentUi.SelectionState.text) && equipmentUi.SlotStatusLabels[0].text == L.Text("ui.inventory.empty"),
                "empty equipment is visible in its slot without duplicating the detail state");
            yield return CaptureState("equipment-unequipped", panel, equipmentUi, drag);

            Phase = "split-and-long-name-states";
            ClearEquipmentFilter(equipmentUi);
            panel.Select(woodIndex);
            drag.Select(woodIndex, false);
            drag.OpenSplit();
            Check(drag.SplitPanel.View.activeSelf, "existing split modal opens for selected wood stack");
            CheckTextBounds(drag.SplitPanel.View.transform, "Split Panel");
            yield return CaptureState("split-modal", panel, equipmentUi, drag);
            drag.SplitPanel.Cancel();
            var crownIndex = Enumerable.Range(0, inventory.Model.Capacity).First(i => inventory.Model.GetSlot(i).Item.Id == "undead_kings_crown");
            L.SetLanguage("en");
            ClearEquipmentFilter(equipmentUi);
            panel.Select(crownIndex);
            equipmentUi.Refresh();
            yield return CaptureState("long-item-name", panel, equipmentUi, drag);

            Phase = "empty-equipment-state";
            L.SetLanguage("ko");
            equipment.RestoreArmor(null, null);
            equipment.RestoreUtility(null);
            equipment.RestoreAccessory(null);
            equipmentUi.SelectSlot(-1);
            equipmentUi.Refresh();
            yield return Delay(.25f);
            Check(equipment.HeadArmor == null && equipment.BodyArmor == null && equipment.Utility == null && equipment.Accessory == null,
                "empty equipment fixture clears all four slots without changing inventory data");
            CheckTextBounds(view);
            yield return CaptureState("empty-equipment", panel, equipmentUi, drag);

            Phase = "final-isolation-check";
            Check(save.Repository.Path.StartsWith(DevelopmentProfile.Directory, StringComparison.OrdinalIgnoreCase), "final screenshot session still uses isolated profile");
            Check(!save.AutoSaveEnabled, "autosave remains disabled");
            var boundaryFindings = BoundaryFailures;
            Log((boundaryFindings == 0 ? "PASS " : "FAIL ") + "automated text-boundary report; findings=" + boundaryFindings);
            var boundaryStatus = boundaryFindings == 0 ? "PASS" : "PARTIAL - inspect qa-live.tsv for automated text-boundary findings";
            File.WriteAllText(Path.Combine(Root, "qa-result.txt"), boundaryStatus + "\n" + string.Join("\n", new[] { "Play Mode Game View screenshots captured directly by ScreenCapture.CaptureScreenshot", "Autosave disabled", "QA profile=" + DevelopmentProfile.Directory, "World save was not written", "Automated text-boundary findings=" + boundaryFindings }));
            player.GetComponent<PlayerInputCommands>().Session.CloseInventory();
            yield return null;
        }

        private static void Put(SavedStack[] states, int index, ItemDefinition item, int count)
        {
            states[index] = new SavedStack { Item = item.Id, Count = count };
        }

        private static void ClearEquipmentFilter(InventoryEquipment equipmentUi)
        {
            if (equipmentUi.SlotFilter >= 0) equipmentUi.SelectSlot(equipmentUi.SlotFilter);
        }

        private static IEnumerator CaptureState(string name, InventoryPanel panel, InventoryEquipment equipmentUi, InventoryDragController drag)
        {
            foreach (var size in new[] { new Vector2Int(844, 390), new Vector2Int(1280, 720) })
            foreach (var language in new[] { "ko", "en" })
            {
                yield return SetResolution(size.x, size.y);
                L.SetLanguage(language);
                panel.RefreshSelection();
                equipmentUi.Refresh();
                Canvas.ForceUpdateCanvases();
                CheckTextBounds(panel.View.transform);
                if (drag.SplitPanel.View.activeSelf) CheckTextBounds(drag.SplitPanel.View.transform, "Split Panel");
                yield return Shot("inventory-" + language + "-" + size.x + "x" + size.y + "-" + name);
            }
        }

        private static void CheckTextBounds(Transform view, string surfaceName = "Inventory Panel")
        {
            Canvas.ForceUpdateCanvases();
            var panel = view.Find(surfaceName);
            CheckBoundary(panel != null, "text boundary surface exists: " + surfaceName);
            if (panel == null) return;
            var bounds = (RectTransform)panel;
            var panelCorners = new Vector3[4];
            bounds.GetWorldCorners(panelCorners);
            var min = bounds.InverseTransformPoint(panelCorners[0]);
            var max = bounds.InverseTransformPoint(panelCorners[2]);
            foreach (var text in view.GetComponentsInChildren<Text>(false))
            {
                if (!text.isActiveAndEnabled || string.IsNullOrEmpty(text.text)) continue;
                CheckBoundary(text.preferredHeight <= text.rectTransform.rect.height + 2,
                    "text height fits " + text.name + " actual=" + text.rectTransform.rect.height + " preferred=" + text.preferredHeight);
                var corners = new Vector3[4];
                text.rectTransform.GetWorldCorners(corners);
                foreach (var corner in corners)
                {
                    var local = bounds.InverseTransformPoint(corner);
                    CheckBoundary(local.x >= min.x - 2 && local.x <= max.x + 2 && local.y >= min.y - 2 && local.y <= max.y + 2,
                        "text stays inside inventory panel " + text.name + " at " + local);
                }
            }
        }

        private static IEnumerator Delay(float seconds)
        {
            var until = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < until) yield return null;
        }

        private static IEnumerator SetResolution(int width, int height)
        {
            SelectResolution(width, height);
            yield return Delay(.4f);
            Check(Screen.width == width && Screen.height == height,
                "Game View rendered at requested " + width + "x" + height);
            Canvas.ForceUpdateCanvases();
        }

        private const System.Reflection.BindingFlags GameViewFlags = System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static;

        private static void SelectResolution(int width, int height)
        {
            var assembly = typeof(UnityEditor.Editor).Assembly;
            var sizesType = assembly.GetType("UnityEditor.GameViewSizes", true);
            var sizeType = assembly.GetType("UnityEditor.GameViewSize", true);
            var singleton = typeof(ScriptableSingleton<>).MakeGenericType(sizesType).GetProperty("instance").GetValue(null);
            var group = sizesType.GetMethod("GetGroup", GameViewFlags).Invoke(singleton,
                new[] { Enum.ToObject(assembly.GetType("UnityEditor.GameViewSizeGroupType", true), 0) });
            var groupType = group.GetType();
            var labels = (string[])groupType.GetMethod("GetDisplayTexts", GameViewFlags).Invoke(group, null);
            var label = $"Lowfire Phase4D {width}x{height}";
            var index = Array.FindIndex(labels, text => text.StartsWith(label, StringComparison.Ordinal));
            if (index < 0)
            {
                var size = Activator.CreateInstance(sizeType, new[]
                {
                    Enum.ToObject(assembly.GetType("UnityEditor.GameViewSizeType", true), 1), width, height, (object)label
                });
                groupType.GetMethod("AddCustomSize", GameViewFlags).Invoke(group, new[] { size });
                index = (int)groupType.GetMethod("GetTotalCount", GameViewFlags).Invoke(group, null) - 1;
            }
            var gameViewType = assembly.GetType("UnityEditor.GameView", true);
            var windows = UnityEngine.Resources.FindObjectsOfTypeAll(gameViewType);
            if (windows.Length == 0) throw new InvalidOperationException("The existing Game View tab is required for capture.");
            var window = (EditorWindow)windows[0];
            var playBehavior = gameViewType.GetProperty("enterPlayModeBehavior", GameViewFlags);
            if (playBehavior != null) playBehavior.SetValue(window, Enum.Parse(playBehavior.PropertyType, "PlayUnfocused"));
            gameViewType.GetProperty("selectedSizeIndex", GameViewFlags).SetValue(window, index);
            window.Repaint();
        }

        private static IEnumerator Shot(string name)
        {
            yield return null;
            yield return new WaitForEndOfFrame();
            var path = Path.Combine(Root, name + ".png");
            Directory.CreateDirectory(Root);
            ScreenCapture.CaptureScreenshot(path);
            yield return new WaitForEndOfFrame();
            yield return null;
            Log("capture " + Path.GetFileName(path) + " " + Screen.width + "x" + Screen.height);
        }
    }
}
