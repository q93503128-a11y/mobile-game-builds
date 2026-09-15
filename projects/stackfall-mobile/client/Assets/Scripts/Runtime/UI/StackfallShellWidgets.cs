using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace StackfallMobile.Runtime.UI
{
    public sealed partial class StackfallShellController
    {
        private void AddMail(
            VisualElement root,
            VisualElement parent,
            string id,
            string sender,
            string title,
            string reward,
            string expiry)
        {
            var claimed = _state.ClaimedMail.Contains(id);
            var card = Card();
            card.style.marginTop = 12;
            Pad(card, 20, 18);
            var row = Row();
            row.style.alignItems = Align.Center;
            row.Add(Icon("gift", 46));
            var text = new VisualElement();
            text.style.flexGrow = 1;
            text.style.marginLeft = 16;
            text.Add(Label(sender, 13, FontStyle.Bold, Accent));
            text.Add(Label(title, 20, FontStyle.Bold, claimed ? Muted : Color.white));
            text.Add(Label(reward, 15, FontStyle.Normal, Muted));
            row.Add(text);
            row.Add(Label(expiry, 13, FontStyle.Normal, MutedDark));
            card.Add(row);
            var claim = Button(claimed ? "수령 완료" : "수령", null, !claimed);
            claim.style.height = 62;
            claim.style.marginTop = 14;
            claim.SetEnabled(!claimed);
            if (!claimed)
            {
                claim.clicked += () =>
                {
                    _state.ClaimedMail.Add(id);
                    claim.text = "수령 완료";
                    claim.SetEnabled(false);
                    ShowModal(root, "우편 보상 수령", $"{reward}\n보급을 수령했습니다.", "확인", ShowMailbox, "gift", Success);
                };
            }
            card.Add(claim);
            parent.Add(card);
        }

        private void AddMission(
            VisualElement root,
            VisualElement parent,
            string id,
            string title,
            int current,
            int target,
            string reward,
            bool claimable)
        {
            var claimed = _state.ClaimedMissions.Contains(id);
            var card = Card();
            card.style.marginTop = 12;
            Pad(card, 20, 18);
            var top = Row();
            top.style.justifyContent = Justify.SpaceBetween;
            top.Add(Label(title, 19, FontStyle.Bold));
            top.Add(Label($"{current}/{target}", 16, FontStyle.Bold, current >= target ? Success : Accent));
            card.Add(top);
            card.Add(ProgressBar(target <= 0 ? 0 : Mathf.Clamp01((float)current / target) * 100f, current >= target ? Success : Accent));
            var bottom = Row();
            bottom.style.justifyContent = Justify.SpaceBetween;
            bottom.style.alignItems = Align.Center;
            bottom.style.marginTop = 8;
            bottom.Add(Label(reward, 15, FontStyle.Bold, Gold));
            var actionText = claimed ? "완료" : claimable ? "수령" : "진행 중";
            var action = Button(actionText, null, claimable && !claimed);
            action.style.width = 150;
            action.style.height = 54;
            action.SetEnabled(claimable && !claimed);
            if (claimable && !claimed)
            {
                action.clicked += () =>
                {
                    _state.ClaimedMissions.Add(id);
                    action.text = "완료";
                    action.SetEnabled(false);
                    ShowToast(root, $"{reward} 수령", Success);
                };
            }
            bottom.Add(action);
            card.Add(bottom);
            parent.Add(card);
        }

        private static VisualElement StoreAction(string text, string note, Action click, bool primary = true)
        {
            var row = Row();
            row.style.marginTop = 18;
            row.style.justifyContent = Justify.SpaceBetween;
            row.style.alignItems = Align.Center;
            row.Add(Label(note, 15, FontStyle.Bold, Gold));
            var button = Button(text, click, primary);
            button.style.width = 240;
            button.style.height = 62;
            row.Add(button);
            return row;
        }

        private static VisualElement StoreCard(string iconName, string title, string detail, string buttonText, Action click, bool primary = false, bool enabled = true)
        {
            var card = Card();
            card.style.flexGrow = 1;
            card.style.minHeight = 220;
            card.style.marginRight = 6;
            card.style.alignItems = Align.Center;
            Pad(card, 14, 18);
            card.Add(Icon(iconName, 62));
            card.Add(Label(title, 19, FontStyle.Bold));
            var detailLabel = Label(detail, 14, FontStyle.Normal, Muted);
            detailLabel.style.whiteSpace = WhiteSpace.Normal;
            detailLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            card.Add(detailLabel);
            var buy = Button(buttonText, click, primary);
            buy.SetEnabled(enabled);
            buy.style.width = Length.Percent(100f);
            buy.style.height = 54;
            buy.style.marginTop = 12;
            card.Add(buy);
            return card;
        }

        private static VisualElement SettingToggle(string title, bool initial, Action<bool> changed)
        {
            var row = Card();
            row.style.marginTop = 12;
            row.style.flexDirection = FlexDirection.Row;
            row.style.justifyContent = Justify.SpaceBetween;
            row.style.alignItems = Align.Center;
            Pad(row, 22, 16);
            row.Add(Label(title, 19, FontStyle.Bold));
            var toggle = new Toggle { value = initial };
            toggle.style.width = 84;
            toggle.style.height = 42;
            toggle.RegisterValueChangedCallback(evt => changed?.Invoke(evt.newValue));
            row.Add(toggle);
            return row;
        }

        private static VisualElement SettingRow(string title, string detail)
        {
            var row = Card();
            row.style.marginTop = 10;
            row.style.flexDirection = FlexDirection.Row;
            row.style.justifyContent = Justify.SpaceBetween;
            row.style.alignItems = Align.Center;
            Pad(row, 22, 16);
            var left = new VisualElement();
            left.Add(Label(title, 19, FontStyle.Bold));
            left.Add(Label(detail, 14, FontStyle.Normal, Muted));
            row.Add(left);
            row.Add(Label("›", 30, FontStyle.Bold, Accent));
            return row;
        }

        private static VisualElement FeatureTile(string iconName, string title, string subtitle, Action click, string badge)
        {
            var button = new Button(click ?? (() => { }));
            button.style.flexGrow = 1;
            button.style.height = 168;
            button.style.marginLeft = 4;
            button.style.marginRight = 4;
            button.style.backgroundColor = Panel;
            button.style.borderTopWidth = 0;
            button.style.borderBottomWidth = 0;
            button.style.borderLeftWidth = 0;
            button.style.borderRightWidth = 0;
            SetRadius(button, 20);
            button.style.position = Position.Relative;
            button.style.alignItems = Align.Center;
            button.style.justifyContent = Justify.Center;
            button.Add(Icon(iconName, 54));
            button.Add(Label(title, 18, FontStyle.Bold));
            button.Add(Label(subtitle, 12, FontStyle.Normal, Muted));
            if (!string.IsNullOrEmpty(badge))
            {
                var badgeElement = Badge(badge);
                badgeElement.style.position = Position.Absolute;
                badgeElement.style.right = 8;
                badgeElement.style.top = 8;
                button.Add(badgeElement);
            }
            return button;
        }

        private static VisualElement TabChip(string text, bool active)
        {
            var chip = Card(active ? new Color(0.07f, 0.22f, 0.3f, 1f) : new Color(0.03f, 0.055f, 0.09f, 1f));
            chip.style.flexGrow = 1;
            chip.style.marginRight = 8;
            chip.style.height = 58;
            chip.style.alignItems = Align.Center;
            chip.style.justifyContent = Justify.Center;
            chip.Add(Label(text, 17, FontStyle.Bold, active ? Accent : Muted));
            return chip;
        }

        private static VisualElement ProgressBar(float percent, Color color)
        {
            var bg = new VisualElement();
            bg.style.height = 16;
            bg.style.marginTop = 10;
            bg.style.backgroundColor = new Color(0.015f, 0.025f, 0.045f, 1f);
            bg.style.overflow = Overflow.Hidden;
            SetRadius(bg, 8);
            var fill = new VisualElement();
            fill.style.height = Length.Percent(100f);
            fill.style.width = Length.Percent(Mathf.Clamp(percent, 0f, 100f));
            fill.style.backgroundColor = color;
            SetRadius(fill, 8);
            bg.Add(fill);
            return bg;
        }

        private static VisualElement Resource(string name, string value)
        {
            var box = new VisualElement();
            box.style.marginLeft = 12;
            box.style.alignItems = Align.FlexEnd;
            box.Add(Label(name, 11, FontStyle.Normal, Muted));
            box.Add(Label(value, 17, FontStyle.Bold));
            return box;
        }

        private static VisualElement IconButton(string iconName, Action click, float size, string badge = null)
        {
            var button = new Button(click ?? (() => { }));
            button.style.width = size;
            button.style.height = size;
            button.style.marginLeft = 10;
            button.style.paddingLeft = 10;
            button.style.paddingRight = 10;
            button.style.paddingTop = 10;
            button.style.paddingBottom = 10;
            button.style.backgroundColor = new Color(0.045f, 0.085f, 0.14f, 1f);
            button.style.borderTopWidth = 0;
            button.style.borderBottomWidth = 0;
            button.style.borderLeftWidth = 0;
            button.style.borderRightWidth = 0;
            SetRadius(button, 14);
            button.style.position = Position.Relative;
            button.Add(Icon(iconName, size - 20));
            if (!string.IsNullOrEmpty(badge))
            {
                var badgeElement = Badge(badge);
                badgeElement.style.position = Position.Absolute;
                badgeElement.style.right = -4;
                badgeElement.style.top = -4;
                button.Add(badgeElement);
            }
            return button;
        }

        private static VisualElement Badge(string text)
        {
            var badge = new VisualElement();
            badge.style.minWidth = 30;
            badge.style.height = 30;
            badge.style.alignItems = Align.Center;
            badge.style.justifyContent = Justify.Center;
            badge.style.backgroundColor = Danger;
            SetRadius(badge, 15);
            badge.Add(Label(text, 12, FontStyle.Bold));
            return badge;
        }

        private static UnityEngine.UIElements.Image Icon(string name, float size)
        {
            var image = new UnityEngine.UIElements.Image();
            image.style.width = size;
            image.style.height = size;
            image.scaleMode = ScaleMode.ScaleToFit;
            image.image = Resources.Load<Texture2D>($"StackfallExternal/{name}");
            image.pickingMode = PickingMode.Ignore;
            return image;
        }

        private static VisualElement Row()
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            return row;
        }

        private static VisualElement Card()
        {
            return Card(Panel);
        }

        private static VisualElement Card(Color color)
        {
            var panel = new VisualElement();
            panel.style.backgroundColor = color;
            SetRadius(panel, 22);
            return panel;
        }

        private static VisualElement Chip(string text, Color color)
        {
            var chip = new VisualElement();
            Pad(chip, 16, 9);
            chip.style.backgroundColor = new Color(color.r * 0.18f, color.g * 0.18f, color.b * 0.18f, 1f);
            SetRadius(chip, 16);
            chip.Add(Label(text, 14, FontStyle.Bold, color));
            return chip;
        }

        private static Button Button(string text, Action click, bool primary)
        {
            var button = new Button(click ?? (() => { })) { text = text };
            button.style.fontSize = 23;
            button.style.whiteSpace = WhiteSpace.Normal;
            button.style.unityTextAlign = TextAnchor.MiddleCenter;
            button.style.unityFontStyleAndWeight = FontStyle.Bold;
            button.style.color = primary ? new Color(0.01f, 0.045f, 0.065f, 1f) : Color.white;
            button.style.backgroundColor = primary ? Accent : new Color(0.065f, 0.11f, 0.18f, 1f);
            button.style.borderTopWidth = 0;
            button.style.borderBottomWidth = 0;
            button.style.borderLeftWidth = 0;
            button.style.borderRightWidth = 0;
            SetRadius(button, 18);
            return button;
        }

        private static Label Label(string text, int size, FontStyle style, Color? color = null)
        {
            var label = new Label(text);
            label.style.fontSize = size;
            label.style.unityFontStyleAndWeight = style;
            label.style.color = color ?? Color.white;
            label.style.marginTop = 4;
            return label;
        }

        private static void AddAmbientGlow(VisualElement root)
        {
            var glowA = new VisualElement();
            glowA.pickingMode = PickingMode.Ignore;
            glowA.style.position = Position.Absolute;
            glowA.style.width = 420;
            glowA.style.height = 420;
            glowA.style.right = -180;
            glowA.style.top = 120;
            glowA.style.backgroundColor = new Color(0.02f, 0.3f, 0.48f, 0.09f);
            SetRadius(glowA, 210);
            root.Add(glowA);

            var glowB = new VisualElement();
            glowB.pickingMode = PickingMode.Ignore;
            glowB.style.position = Position.Absolute;
            glowB.style.width = 360;
            glowB.style.height = 360;
            glowB.style.left = -170;
            glowB.style.bottom = 240;
            glowB.style.backgroundColor = new Color(0.22f, 0.05f, 0.45f, 0.08f);
            SetRadius(glowB, 180);
            root.Add(glowB);
        }

        private static void AddHangarDecor(VisualElement hangar)
        {
            for (var i = 0; i < 5; i++)
            {
                var line = new VisualElement();
                line.pickingMode = PickingMode.Ignore;
                line.style.position = Position.Absolute;
                line.style.height = 2;
                line.style.width = 120 + i * 55;
                line.style.right = -30;
                line.style.top = 80 + i * 44;
                line.style.backgroundColor = new Color(0.15f, 0.8f, 1f, 0.12f);
                hangar.Add(line);
            }
        }

        private static void AddSpaceField(VisualElement root)
        {
            var rng = new System.Random(8127);
            for (var i = 0; i < 90; i++)
            {
                var star = new VisualElement();
                star.pickingMode = PickingMode.Ignore;
                var size = rng.NextDouble() > 0.88 ? 5 : (rng.NextDouble() > 0.55 ? 3 : 2);
                star.style.position = Position.Absolute;
                star.style.width = size;
                star.style.height = size;
                star.style.left = Length.Percent((float)rng.NextDouble() * 100f);
                star.style.top = Length.Percent((float)rng.NextDouble() * 100f);
                star.style.backgroundColor = new Color(0.7f, 0.9f, 1f, 0.25f + (float)rng.NextDouble() * 0.7f);
                SetRadius(star, size / 2f);
                root.Add(star);
            }

            var planet = new VisualElement();
            planet.pickingMode = PickingMode.Ignore;
            planet.style.position = Position.Absolute;
            planet.style.width = 480;
            planet.style.height = 480;
            planet.style.right = -260;
            planet.style.top = 160;
            planet.style.backgroundColor = new Color(0.03f, 0.22f, 0.38f, 0.55f);
            SetRadius(planet, 240);
            root.Add(planet);

            var nebula = new VisualElement();
            nebula.pickingMode = PickingMode.Ignore;
            nebula.style.position = Position.Absolute;
            nebula.style.width = 720;
            nebula.style.height = 420;
            nebula.style.left = -320;
            nebula.style.bottom = 100;
            nebula.style.backgroundColor = new Color(0.24f, 0.05f, 0.45f, 0.13f);
            SetRadius(nebula, 210);
            root.Add(nebula);
        }

        private static void Pad(VisualElement element, float horizontal, float vertical)
        {
            element.style.paddingLeft = horizontal;
            element.style.paddingRight = horizontal;
            element.style.paddingTop = vertical;
            element.style.paddingBottom = vertical;
        }

        private static void SetRadius(VisualElement element, float radius)
        {
            element.style.borderTopLeftRadius = radius;
            element.style.borderTopRightRadius = radius;
            element.style.borderBottomLeftRadius = radius;
            element.style.borderBottomRightRadius = radius;
        }
    }
}
