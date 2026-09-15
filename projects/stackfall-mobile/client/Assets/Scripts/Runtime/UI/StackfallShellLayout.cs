using System;
using System.Collections.Generic;
using StackfallMobile.Runtime.Stage;
using UnityEngine;
using UnityEngine.UIElements;

namespace StackfallMobile.Runtime.UI
{
    public sealed partial class StackfallShellController
    {
        private static bool IsCompactDevice => Screen.width <= 390 || Screen.height <= 700;

        private static string BadgeText(int count)
        {
            return count > 0 ? count.ToString() : null;
        }

        private VisualElement BeginScreen(bool addAmbient = true)
        {
            var documentRoot = _document.rootVisualElement;
            documentRoot.Clear();
            documentRoot.style.display = DisplayStyle.Flex;
            documentRoot.style.position = Position.Absolute;
            documentRoot.style.left = 0;
            documentRoot.style.right = 0;
            documentRoot.style.top = 0;
            documentRoot.style.bottom = 0;
            documentRoot.style.backgroundColor = Background;

            var safe = new VisualElement();
            safe.style.flexGrow = 1;
            safe.style.flexDirection = FlexDirection.Column;
            safe.style.position = Position.Relative;
            ApplySafeArea(safe);
            if (addAmbient)
            {
                AddAmbientGlow(safe);
            }
            documentRoot.Add(safe);
            return safe;
        }

        private static VisualElement AddBody(VisualElement root, bool scroll)
        {
            VisualElement body = scroll ? new ScrollView(ScrollViewMode.Vertical) : new VisualElement();
            body.style.flexGrow = 1;
            var horizontalPadding = IsCompactDevice ? 22f : 32f;
            body.style.paddingLeft = horizontalPadding;
            body.style.paddingRight = horizontalPadding;
            body.style.paddingTop = IsCompactDevice ? 12 : 18;
            body.style.paddingBottom = IsCompactDevice ? 12 : 18;
            root.Add(body);
            return body;
        }

        private static void ApplySafeArea(VisualElement root)
        {
            var safeArea = Screen.safeArea;
            var width = Mathf.Max(1f, Screen.width);
            var height = Mathf.Max(1f, Screen.height);
            var panelScale = 1080f / width;
            root.style.paddingLeft = 12f + safeArea.xMin * panelScale;
            root.style.paddingRight = 12f + (width - safeArea.xMax) * panelScale;
            root.style.paddingBottom = 12f + safeArea.yMin * panelScale;
            root.style.paddingTop = 12f + (height - safeArea.yMax) * panelScale;
        }

        private void AddTopStatus(VisualElement root)
        {
            var top = Row();
            top.style.alignItems = IsCompactDevice ? Align.Stretch : Align.Center;
            top.style.justifyContent = Justify.SpaceBetween;
            top.style.flexDirection = IsCompactDevice ? FlexDirection.Column : FlexDirection.Row;
            Pad(top, IsCompactDevice ? 18 : 24, IsCompactDevice ? 10 : 14);
            top.style.backgroundColor = new Color(0.018f, 0.042f, 0.078f, 0.98f);

            var profile = new Button(_app.ShowProfile);
            profile.style.flexDirection = FlexDirection.Row;
            profile.style.alignItems = Align.Center;
            profile.style.backgroundColor = Color.clear;
            profile.style.borderTopWidth = 0;
            profile.style.borderBottomWidth = 0;
            profile.style.borderLeftWidth = 0;
            profile.style.borderRightWidth = 0;
            profile.style.paddingLeft = 0;
            profile.style.paddingRight = 6;
            var avatar = new VisualElement();
            avatar.style.width = 52;
            avatar.style.height = 52;
            avatar.style.backgroundColor = AccentSoft;
            SetRadius(avatar, 26);
            var core = new VisualElement();
            core.style.width = 22;
            core.style.height = 22;
            core.style.marginLeft = 15;
            core.style.marginTop = 15;
            core.style.backgroundColor = Accent;
            SetRadius(core, 11);
            avatar.Add(core);
            profile.Add(avatar);
            var name = new VisualElement();
            name.style.marginLeft = 12;
            name.Add(Label("파일럿", 20, FontStyle.Bold));
            name.Add(Label("Lv.1", 13, FontStyle.Bold, Muted));
            profile.Add(name);
            top.Add(profile);

            var resources = Row();
            resources.style.alignItems = Align.Center;
            resources.style.justifyContent = IsCompactDevice ? Justify.SpaceBetween : Justify.FlexEnd;
            if (IsCompactDevice) resources.style.marginTop = 8;
            resources.Add(Resource("크레딧", "2,450"));
            resources.Add(Resource("크리스탈", "120"));
            resources.Add(IconButton("gift", _app.ShowMailbox, IsCompactDevice ? 44 : 48, BadgeText(_state.UnclaimedMailCount)));
            resources.Add(IconButton("gear", _app.ShowSettings, IsCompactDevice ? 44 : 48));
            top.Add(resources);
            root.Add(top);
        }

        private void AddCompactHeader(VisualElement root, string title, Action back, string iconName = null)
        {
            var header = Row();
            header.style.alignItems = Align.Center;
            Pad(header, IsCompactDevice ? 18 : 24, IsCompactDevice ? 12 : 18);
            header.style.backgroundColor = new Color(0.018f, 0.042f, 0.078f, 0.98f);
            var backButton = Button("‹", back, false);
            backButton.style.width = IsCompactDevice ? 62 : 72;
            backButton.style.height = IsCompactDevice ? 54 : 62;
            backButton.style.fontSize = IsCompactDevice ? 32 : 38;
            header.Add(backButton);
            if (!string.IsNullOrEmpty(iconName))
            {
                var icon = Icon(iconName, IsCompactDevice ? 36 : 42);
                icon.style.marginLeft = IsCompactDevice ? 12 : 18;
                header.Add(icon);
            }
            var label = Label(title, IsCompactDevice ? 27 : 32, FontStyle.Bold);
            label.style.marginLeft = 14;
            header.Add(label);
            root.Add(header);
        }

        private void AddDeckSection(VisualElement parent, string title, string subtitle, IReadOnlyList<AbilityPreview> deck)
        {
            var header = new VisualElement();
            header.style.marginTop = 22;
            header.Add(Label(title, 25, FontStyle.Bold));
            header.Add(Label(subtitle, 16, FontStyle.Normal, Muted));
            parent.Add(header);

            for (var rowIndex = 0; rowIndex < 2; rowIndex++)
            {
                var row = Row();
                row.style.marginTop = 8;
                parent.Add(row);
                for (var column = 0; column < 4; column++)
                {
                    var ability = deck[rowIndex * 4 + column];
                    var unlocked = StackfallContentCatalog.IsUnlocked(ability, _app.HighestClearedStage);
                    var card = Card(unlocked ? PanelSoft : new Color(0.03f, 0.05f, 0.08f, 1f));
                    card.style.width = Length.Percent(24f);
                    card.style.height = IsCompactDevice ? 132 : 144;
                    card.style.marginLeft = column == 0 ? 0 : 5;
                    card.style.marginRight = column == 3 ? 0 : 5;
                    card.style.alignItems = Align.Center;
                    card.style.justifyContent = Justify.Center;
                    Pad(card, 8, 10);
                    if (!unlocked)
                    {
                        card.Add(Icon("lock", 28));
                    }
                    var name = Label(ability.Name, IsCompactDevice ? 14 : 16, FontStyle.Bold, unlocked ? Color.white : Muted);
                    name.style.whiteSpace = WhiteSpace.Normal;
                    name.style.unityTextAlign = TextAnchor.MiddleCenter;
                    card.Add(name);
                    card.Add(Label(ability.Role, 12, FontStyle.Normal, Muted));
                    card.Add(Label(unlocked ? "편성" : $"Stage {ability.UnlockStage}", 12, FontStyle.Bold, unlocked ? Accent : Gold));
                    row.Add(card);
                }
            }
        }

        private void AddBottomNavigation(VisualElement root, string active)
        {
            var nav = Row();
            nav.style.height = IsCompactDevice ? 96 : 112;
            nav.style.flexShrink = 0;
            nav.style.alignItems = Align.Stretch;
            nav.style.backgroundColor = new Color(0.018f, 0.04f, 0.075f, 1f);
            Pad(nav, 8, 8);
            AddNavButton(nav, "전투", active == "전투", _app.ShowHome);
            AddNavButton(nav, "기체", active == "기체", _app.ShowShip);
            AddNavButton(nav, "회수", active == "회수", _app.ShowRecovery);
            AddNavButton(nav, "도전", active == "도전", _app.ShowChallenges);
            AddNavButton(nav, "길드", active == "길드", _app.ShowGuild);
            root.Add(nav);
        }

        private static void AddNavButton(VisualElement parent, string text, bool active, Action click)
        {
            var button = Button(text, click, false);
            button.style.flexGrow = 1;
            button.style.marginLeft = 4;
            button.style.marginRight = 4;
            button.style.fontSize = IsCompactDevice ? 16 : 18;
            button.style.color = active ? Accent : new Color(0.7f, 0.76f, 0.84f, 1f);
            button.style.backgroundColor = active ? new Color(0.055f, 0.18f, 0.25f, 1f) : new Color(0.025f, 0.052f, 0.09f, 1f);
            parent.Add(button);
        }

        private static void AddPart(VisualElement parent, string slot, string partName, string level)
        {
            var card = Card(PanelSoft);
            card.style.marginTop = 10;
            card.style.flexDirection = FlexDirection.Row;
            card.style.justifyContent = Justify.SpaceBetween;
            card.style.alignItems = Align.Center;
            Pad(card, 20, 16);
            var slotText = new VisualElement();
            slotText.Add(Label(slot, 14, FontStyle.Bold, Accent));
            slotText.Add(Label(partName, 20, FontStyle.Bold));
            card.Add(slotText);
            card.Add(Chip(level, Muted));
            parent.Add(card);
        }
    }
}
