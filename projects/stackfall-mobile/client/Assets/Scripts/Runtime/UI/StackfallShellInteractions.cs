using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace StackfallMobile.Runtime.UI
{
    public sealed partial class StackfallShellController
    {
        private readonly StackfallShellInteractionState _state = new();

        private static Button SelectableTab(string text, bool active, Action click)
        {
            var tab = Button(text, click, false);
            tab.style.flexGrow = 1;
            tab.style.height = 58;
            tab.style.marginRight = 8;
            tab.style.fontSize = 17;
            tab.style.color = active ? Accent : Muted;
            tab.style.backgroundColor = active
                ? new Color(0.07f, 0.22f, 0.3f, 1f)
                : new Color(0.03f, 0.055f, 0.09f, 1f);
            return tab;
        }

        private static void ShowToast(VisualElement root, string message, Color color)
        {
            var toast = Card(new Color(0.015f, 0.035f, 0.06f, 0.98f));
            toast.style.position = Position.Absolute;
            toast.style.left = IsCompactDevice ? 42 : 72;
            toast.style.right = IsCompactDevice ? 42 : 72;
            toast.style.bottom = IsCompactDevice ? 116 : 138;
            toast.style.minHeight = 72;
            toast.style.alignItems = Align.Center;
            toast.style.justifyContent = Justify.Center;
            toast.style.borderTopWidth = 2;
            toast.style.borderBottomWidth = 2;
            toast.style.borderLeftWidth = 2;
            toast.style.borderRightWidth = 2;
            toast.style.borderTopColor = color;
            toast.style.borderBottomColor = color;
            toast.style.borderLeftColor = color;
            toast.style.borderRightColor = color;
            Pad(toast, 22, 14);
            toast.Add(Label(message, 17, FontStyle.Bold));
            root.Add(toast);
            toast.BringToFront();
            toast.schedule.Execute(() => toast.RemoveFromHierarchy()).StartingIn(1600);
        }

        private static void ShowModal(
            VisualElement root,
            string title,
            string message,
            string actionText,
            Action action,
            string iconName = null,
            Color? accent = null)
        {
            var overlay = new VisualElement();
            overlay.style.position = Position.Absolute;
            overlay.style.left = 0;
            overlay.style.right = 0;
            overlay.style.top = 0;
            overlay.style.bottom = 0;
            overlay.style.backgroundColor = new Color(0f, 0f, 0f, 0.72f);
            overlay.style.alignItems = Align.Center;
            overlay.style.justifyContent = Justify.Center;

            var modal = Card(new Color(0.028f, 0.06f, 0.11f, 1f));
            modal.style.width = Length.Percent(84f);
            modal.style.maxWidth = 780;
            modal.style.alignItems = Align.Center;
            Pad(modal, 34, 30);

            if (!string.IsNullOrEmpty(iconName))
            {
                modal.Add(Icon(iconName, 70));
            }

            var titleLabel = Label(title, 28, FontStyle.Bold, accent ?? Accent);
            titleLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            modal.Add(titleLabel);

            var messageLabel = Label(message, 17, FontStyle.Normal, Muted);
            messageLabel.style.whiteSpace = WhiteSpace.Normal;
            messageLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            messageLabel.style.marginTop = 12;
            modal.Add(messageLabel);

            var buttons = Row();
            buttons.style.width = Length.Percent(100f);
            buttons.style.marginTop = 24;
            var close = Button("닫기", () => overlay.RemoveFromHierarchy(), false);
            close.style.flexGrow = 1;
            close.style.height = 68;
            buttons.Add(close);

            if (!string.IsNullOrEmpty(actionText))
            {
                var confirm = Button(actionText, () =>
                {
                    overlay.RemoveFromHierarchy();
                    action?.Invoke();
                }, true);
                confirm.style.flexGrow = 1;
                confirm.style.height = 68;
                confirm.style.marginLeft = 10;
                buttons.Add(confirm);
            }

            modal.Add(buttons);
            overlay.Add(modal);
            root.Add(overlay);
            overlay.BringToFront();
        }

        private static VisualElement ClickableSettingRow(string title, string detail, Action click)
        {
            var row = SettingRow(title, detail);
            row.AddManipulator(new Clickable(click ?? (() => { })));
            return row;
        }


        private void SelectShipPreset(int preset)
        {
            _state.ActivePreset = Mathf.Clamp(preset, 0, 2);
            ShowShip();
        }

        private static string PresetName(int preset)
        {
            return preset switch
            {
                1 => "보스",
                2 => "아레나",
                _ => "메인"
            };
        }

        private static string JoinEquippedParts(string[] equipped)
        {
            return $"{equipped[0]} · {equipped[1]} · {equipped[2]}\n{equipped[3]} · {equipped[4]} · {equipped[5]}";
        }
    }
}
