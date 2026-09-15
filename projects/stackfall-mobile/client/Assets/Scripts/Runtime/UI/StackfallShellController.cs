using System;
using System.Collections.Generic;
using StackfallMobile.Runtime.App;
using StackfallMobile.Runtime.Rendering;
using StackfallMobile.Runtime.Stage;
using UnityEngine;
using UnityEngine.UIElements;

namespace StackfallMobile.Runtime.UI
{
    public sealed class StackfallShellController : MonoBehaviour
    {
        private static readonly Color Background = new(0.018f, 0.03f, 0.065f, 1f);
        private static readonly Color Panel = new(0.055f, 0.085f, 0.15f, 0.98f);
        private static readonly Color PanelSoft = new(0.075f, 0.11f, 0.19f, 0.96f);
        private static readonly Color Accent = new(0.2f, 0.82f, 1f, 1f);
        private static readonly Color Gold = new(1f, 0.73f, 0.24f, 1f);
        private static readonly Color Muted = new(0.6f, 0.68f, 0.78f, 1f);

        private StackfallAppController _app;
        private PanelSettings _panelSettings;
        private UIDocument _document;

        public void Initialize(StackfallAppController app)
        {
            _app = app;
            _panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
            _panelSettings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            _panelSettings.referenceResolution = new Vector2Int(1080, 1920);
            _document = gameObject.AddComponent<UIDocument>();
            _document.panelSettings = _panelSettings;
            _document.sortingOrder = 50;
        }

        private void OnDestroy()
        {
            if (_panelSettings != null)
            {
                Destroy(_panelSettings);
            }
        }

        public void Hide()
        {
            if (_document != null)
            {
                _document.rootVisualElement.style.display = DisplayStyle.None;
            }
        }

        public void ShowHome(int stage)
        {
            var root = BeginScreen();
            AddTopStatus(root);
            var body = AddBody(root, false);

            var heading = Row();
            heading.style.justifyContent = Justify.SpaceBetween;
            heading.style.alignItems = Align.Center;
            var title = new VisualElement();
            title.Add(Label("격납고", 42, FontStyle.Bold));
            title.Add(Label("출격 준비 구역", 20, FontStyle.Normal, Muted));
            heading.Add(title);
            heading.Add(Chip($"전투력 {_app.CombatPower:N0}", Accent));
            body.Add(heading);

            var hangar = Card();
            hangar.style.flexGrow = 1;
            hangar.style.minHeight = 480;
            hangar.style.marginTop = 18;
            hangar.style.marginBottom = 16;
            hangar.style.alignItems = Align.Center;
            hangar.style.justifyContent = Justify.Center;
            hangar.style.overflow = Overflow.Hidden;
            hangar.Add(Label("현재 조립 기체", 18, FontStyle.Bold, Muted));
            hangar.Add(ShipVisualFactory.BuildUiShip(320f));
            hangar.Add(Label("펄서 프레임 · A1", 26, FontStyle.Bold));
            hangar.Add(Label("CORE · FRAME · DRIVE · IMPACTOR · ORBITER · REACTOR", 14, FontStyle.Normal, Muted));
            body.Add(hangar);

            var mission = Card();
            Pad(mission, 26, 22);
            var missionTop = Row();
            missionTop.style.justifyContent = Justify.SpaceBetween;
            missionTop.style.alignItems = Align.Center;
            var missionName = new VisualElement();
            missionName.Add(Label(StackfallContentCatalog.ChapterName(stage), 24, FontStyle.Bold));
            missionName.Add(Label($"Stage {stage}", 32, FontStyle.Bold, Accent));
            missionTop.Add(missionName);
            missionTop.Add(Chip("메인 작전", Gold));
            mission.Add(missionTop);
            mission.Add(Label("주요 위협 · 돌진형 / 광역 펄스 / 최종 보스", 18, FontStyle.Normal, Muted));

            if (_app.HighestClearedStage < StackfallAppController.PlayableStageCap &&
                StackfallContentCatalog.TryGetNextUnlock(_app.HighestClearedStage, out var nextUnlock))
            {
                mission.Add(Label($"다음 해금 · Stage {nextUnlock.Stage} {nextUnlock.Name}", 18, FontStyle.Bold, Accent));
            }

            var sortie = Button("출격", () => _app.OpenLoadout(stage), true);
            sortie.style.height = 94;
            sortie.style.fontSize = 34;
            sortie.style.marginTop = 18;
            mission.Add(sortie);
            body.Add(mission);
            AddBottomNavigation(root, "전투");
        }

        public void ShowLoadout(int stage)
        {
            var root = BeginScreen();
            AddCompactHeader(root, "출격 편성", _app.ShowHome);
            var body = AddBody(root, true);

            var mission = Card();
            Pad(mission, 24, 20);
            mission.Add(Label($"{StackfallContentCatalog.ChapterName(stage)} · Stage {stage}", 28, FontStyle.Bold));
            mission.Add(Label("자동 추천 편성 · 액티브 최대 8 / 지원 최대 8", 18, FontStyle.Normal, Muted));
            mission.Add(Label("적 특성 · 근접 압박 / 돌진 / 광역 예고", 17, FontStyle.Bold, Accent));
            body.Add(mission);

            AddDeckSection(body, "액티브 후보 덱", "전투 중 최대 6종 획득", StackfallContentCatalog.ActiveDeck);
            AddDeckSection(body, "지원 후보 덱", "전투 중 최대 4종 획득", StackfallContentCatalog.SupportDeck);

            var parts = Card();
            parts.style.marginTop = 18;
            Pad(parts, 22, 18);
            parts.Add(Label("기체 부품", 22, FontStyle.Bold));
            var partsText = Label("펄서 코어 · 바스티온 프레임 · 벡터 드라이브\n절단 엣지 · 정찰 오비터 · 가속 리액터", 17, FontStyle.Normal, Muted);
            partsText.style.whiteSpace = WhiteSpace.Normal;
            parts.Add(partsText);
            body.Add(parts);

            var footer = new VisualElement();
            Pad(footer, 30, 18);
            footer.style.backgroundColor = new Color(0.012f, 0.02f, 0.045f, 0.98f);
            var start = Button("전투 시작", () => _app.BeginCombat(stage), true);
            start.style.height = 96;
            start.style.fontSize = 32;
            footer.Add(start);
            root.Add(footer);
        }

        public void ShowShip()
        {
            var root = BeginScreen();
            AddTopStatus(root);
            var body = AddBody(root, true);
            body.Add(Label("기체", 42, FontStyle.Bold));
            body.Add(Label("현재 조립 상태", 19, FontStyle.Normal, Muted));

            var bay = Card();
            bay.style.marginTop = 18;
            Pad(bay, 24, 24);
            bay.style.alignItems = Align.Center;
            bay.Add(ShipVisualFactory.BuildUiShip(300f));
            bay.Add(Label("펄서 프레임 · A1", 24, FontStyle.Bold));
            body.Add(bay);

            AddPart(body, "CORE", "펄서 코어");
            AddPart(body, "FRAME", "바스티온 프레임");
            AddPart(body, "DRIVE", "벡터 드라이브");
            AddPart(body, "IMPACTOR", "절단 엣지");
            AddPart(body, "ORBITER", "정찰 오비터");
            AddPart(body, "REACTOR", "가속 리액터");
            AddBottomNavigation(root, "기체");
        }

        public void ShowLockedSection(string section, int unlockStage)
        {
            var root = BeginScreen();
            AddTopStatus(root);
            var body = AddBody(root, false);
            body.style.alignItems = Align.Center;
            body.style.justifyContent = Justify.Center;

            var panel = Card();
            panel.style.width = Length.Percent(100f);
            panel.style.maxWidth = 780;
            panel.style.alignItems = Align.Center;
            Pad(panel, 36, 46);
            panel.Add(Label(section, 42, FontStyle.Bold));
            panel.Add(Label($"Stage {unlockStage} 클리어 후 개방", 25, FontStyle.Bold, Accent));
            var guide = Label("메인 스테이지를 진행해 새로운 기능을 개방하세요.", 19, FontStyle.Normal, Muted);
            guide.style.whiteSpace = WhiteSpace.Normal;
            guide.style.unityTextAlign = TextAnchor.MiddleCenter;
            panel.Add(guide);
            body.Add(panel);
            AddBottomNavigation(root, section);
        }

        public void ShowResult(StageSessionState state, int stage, int combatLevel, bool firstClear)
        {
            var root = BeginScreen();
            var body = AddBody(root, false);
            body.style.alignItems = Align.Center;
            body.style.justifyContent = Justify.Center;

            var card = Card();
            card.style.width = Length.Percent(100f);
            card.style.maxWidth = 840;
            card.style.alignItems = Align.Center;
            Pad(card, 34, 38);
            var cleared = state == StageSessionState.Cleared;
            card.Add(Label(cleared ? "스테이지 클리어" : "기체 파괴", 52, FontStyle.Bold, cleared ? Accent : new Color(1f, 0.42f, 0.48f, 1f)));
            card.Add(Label($"{StackfallContentCatalog.ChapterName(stage)} · Stage {stage}", 24, FontStyle.Bold));
            card.Add(Label($"도달 레벨 · Lv.{combatLevel}", 20, FontStyle.Normal, Muted));

            if (cleared)
            {
                AddResultUnlock(card, stage, firstClear);
                var action = _app.CanAdvanceFrom(stage)
                    ? Button("다음 스테이지", () => _app.OpenLoadout(stage + 1), true)
                    : Button("다시 출격", () => _app.OpenLoadout(stage), true);
                action.style.width = Length.Percent(100f);
                action.style.height = 88;
                action.style.marginTop = 28;
                card.Add(action);
            }
            else
            {
                var guide = Label("이동 경로와 강화 선택을 조정해 다시 도전해 보세요.", 19, FontStyle.Normal, Muted);
                guide.style.whiteSpace = WhiteSpace.Normal;
                guide.style.unityTextAlign = TextAnchor.MiddleCenter;
                card.Add(guide);
                var retry = Button("다시 출격", () => _app.OpenLoadout(stage), true);
                retry.style.width = Length.Percent(100f);
                retry.style.height = 88;
                retry.style.marginTop = 24;
                card.Add(retry);
            }

            var home = Button("격납고", _app.ShowHome, false);
            home.style.width = Length.Percent(100f);
            home.style.height = 76;
            home.style.marginTop = 12;
            card.Add(home);
            body.Add(card);
        }

        private void AddResultUnlock(VisualElement card, int stage, bool firstClear)
        {
            if (firstClear && StackfallContentCatalog.TryGetUnlockAtStage(stage, out var unlocked))
            {
                var unlockCard = Card(new Color(0.07f, 0.16f, 0.22f, 1f));
                unlockCard.style.width = Length.Percent(100f);
                unlockCard.style.marginTop = 24;
                unlockCard.style.alignItems = Align.Center;
                Pad(unlockCard, 20, 20);
                unlockCard.Add(Label("신규 해금", 18, FontStyle.Bold, Gold));
                unlockCard.Add(Label(unlocked.Name, 30, FontStyle.Bold, Accent));
                unlockCard.Add(Label(unlocked.Category, 17, FontStyle.Normal, Muted));
                card.Add(unlockCard);
                return;
            }

            if (_app.CanAdvanceFrom(stage) && StackfallContentCatalog.TryGetNextUnlock(stage, out var next))
            {
                var label = Label($"다음 해금 · Stage {next.Stage} {next.Name}", 19, FontStyle.Bold, Accent);
                label.style.marginTop = 20;
                card.Add(label);
            }
        }

        private VisualElement BeginScreen()
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
            ApplySafeArea(safe);
            documentRoot.Add(safe);
            return safe;
        }

        private static VisualElement AddBody(VisualElement root, bool scroll)
        {
            VisualElement body = scroll ? new ScrollView() : new VisualElement();
            body.style.flexGrow = 1;
            body.style.paddingLeft = 32;
            body.style.paddingRight = 32;
            body.style.paddingTop = 18;
            body.style.paddingBottom = 18;
            root.Add(body);
            return body;
        }

        private static void ApplySafeArea(VisualElement root)
        {
            var safeArea = Screen.safeArea;
            var width = Mathf.Max(1f, Screen.width);
            var height = Mathf.Max(1f, Screen.height);
            var scaleX = 1080f / width;
            var scaleY = 1920f / height;
            root.style.paddingLeft = 12f + safeArea.xMin * scaleX;
            root.style.paddingRight = 12f + (width - safeArea.xMax) * scaleX;
            root.style.paddingBottom = 12f + safeArea.yMin * scaleY;
            root.style.paddingTop = 12f + (height - safeArea.yMax) * scaleY;
        }

        private void AddTopStatus(VisualElement root)
        {
            var top = Row();
            top.style.alignItems = Align.Center;
            top.style.justifyContent = Justify.SpaceBetween;
            Pad(top, 28, 16);
            top.style.backgroundColor = new Color(0.025f, 0.045f, 0.085f, 1f);

            var profile = Row();
            profile.style.alignItems = Align.Center;
            var avatar = new VisualElement();
            avatar.style.width = 50;
            avatar.style.height = 50;
            avatar.style.backgroundColor = Accent;
            SetRadius(avatar, 25);
            profile.Add(avatar);
            var name = Label("파일럿", 21, FontStyle.Bold);
            name.style.marginLeft = 12;
            profile.Add(name);
            top.Add(profile);

            var resources = Row();
            resources.Add(Resource("크레딧", "0"));
            resources.Add(Resource("크리스탈", "0"));
            top.Add(resources);
            root.Add(top);
        }

        private void AddCompactHeader(VisualElement root, string title, Action back)
        {
            var header = Row();
            header.style.alignItems = Align.Center;
            Pad(header, 24, 18);
            header.style.backgroundColor = new Color(0.025f, 0.045f, 0.085f, 1f);
            var backButton = Button("‹", back, false);
            backButton.style.width = 72;
            backButton.style.height = 62;
            backButton.style.fontSize = 38;
            header.Add(backButton);
            var label = Label(title, 32, FontStyle.Bold);
            label.style.marginLeft = 20;
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
                    var card = Card(unlocked ? PanelSoft : new Color(0.04f, 0.055f, 0.085f, 1f));
                    card.style.width = Length.Percent(24f);
                    card.style.height = 132;
                    card.style.marginLeft = column == 0 ? 0 : 5;
                    card.style.marginRight = column == 3 ? 0 : 5;
                    card.style.alignItems = Align.Center;
                    card.style.justifyContent = Justify.Center;
                    Pad(card, 8, 10);
                    var name = Label(ability.Name, 16, FontStyle.Bold, unlocked ? Color.white : Muted);
                    name.style.whiteSpace = WhiteSpace.Normal;
                    name.style.unityTextAlign = TextAnchor.MiddleCenter;
                    card.Add(name);
                    card.Add(Label(ability.Role, 13, FontStyle.Normal, Muted));
                    card.Add(Label(unlocked ? "편성" : $"Stage {ability.UnlockStage}", 13, FontStyle.Bold, unlocked ? Accent : Gold));
                    row.Add(card);
                }
            }
        }

        private void AddBottomNavigation(VisualElement root, string active)
        {
            var nav = Row();
            nav.style.height = 112;
            nav.style.flexShrink = 0;
            nav.style.alignItems = Align.Stretch;
            nav.style.backgroundColor = new Color(0.022f, 0.04f, 0.078f, 1f);
            Pad(nav, 8, 8);
            AddNavButton(nav, "전투", active == "전투", _app.ShowHome);
            AddNavButton(nav, "기체", active == "기체", _app.ShowShip);
            AddNavButton(nav, "회수", active == "회수", () => _app.ShowLockedSection("회수", 3));
            AddNavButton(nav, "도전", active == "도전", () => _app.ShowLockedSection("도전", 5));
            AddNavButton(nav, "길드", active == "길드", () => _app.ShowLockedSection("길드", 25));
            root.Add(nav);
        }

        private static void AddNavButton(VisualElement parent, string text, bool active, Action click)
        {
            var button = Button(text, click, false);
            button.style.flexGrow = 1;
            button.style.marginLeft = 4;
            button.style.marginRight = 4;
            button.style.fontSize = 19;
            button.style.color = active ? Accent : new Color(0.7f, 0.76f, 0.84f, 1f);
            button.style.backgroundColor = active ? new Color(0.08f, 0.18f, 0.25f, 1f) : new Color(0.03f, 0.055f, 0.095f, 1f);
            parent.Add(button);
        }

        private static void AddPart(VisualElement parent, string slot, string partName)
        {
            var card = Card(PanelSoft);
            card.style.marginTop = 10;
            card.style.flexDirection = FlexDirection.Row;
            card.style.justifyContent = Justify.SpaceBetween;
            card.style.alignItems = Align.Center;
            Pad(card, 20, 16);
            card.Add(Label(slot, 16, FontStyle.Bold, Accent));
            card.Add(Label(partName, 21, FontStyle.Bold));
            parent.Add(card);
        }

        private static VisualElement Resource(string name, string value)
        {
            var box = new VisualElement();
            box.style.marginLeft = 12;
            box.style.alignItems = Align.FlexEnd;
            box.Add(Label(name, 12, FontStyle.Normal, Muted));
            box.Add(Label(value, 19, FontStyle.Bold));
            return box;
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
            SetRadius(panel, 24);
            return panel;
        }

        private static VisualElement Chip(string text, Color color)
        {
            var chip = new VisualElement();
            Pad(chip, 18, 10);
            chip.style.backgroundColor = new Color(color.r * 0.18f, color.g * 0.18f, color.b * 0.18f, 1f);
            SetRadius(chip, 18);
            chip.Add(Label(text, 16, FontStyle.Bold, color));
            return chip;
        }

        private static Button Button(string text, Action click, bool primary)
        {
            var button = new Button(click) { text = text };
            button.style.fontSize = 24;
            button.style.unityFontStyleAndWeight = FontStyle.Bold;
            button.style.color = primary ? new Color(0.02f, 0.05f, 0.075f, 1f) : Color.white;
            button.style.backgroundColor = primary ? Accent : new Color(0.08f, 0.12f, 0.2f, 1f);
            button.style.borderTopWidth = 0;
            button.style.borderBottomWidth = 0;
            button.style.borderLeftWidth = 0;
            button.style.borderRightWidth = 0;
            SetRadius(button, 20);
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
