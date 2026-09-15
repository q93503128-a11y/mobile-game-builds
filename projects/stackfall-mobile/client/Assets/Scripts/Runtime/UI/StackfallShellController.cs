using StackfallMobile.Runtime.App;
using StackfallMobile.Runtime.Rendering;
using StackfallMobile.Runtime.Stage;
using UnityEngine;
using UnityEngine.UIElements;

namespace StackfallMobile.Runtime.UI
{
    public sealed partial class StackfallShellController : MonoBehaviour
    {
        private static readonly Color Background = new(0.012f, 0.022f, 0.052f, 1f);
        private static readonly Color Panel = new(0.038f, 0.07f, 0.125f, 0.98f);
        private static readonly Color PanelSoft = new(0.055f, 0.105f, 0.175f, 0.98f);
        private static readonly Color PanelBright = new(0.07f, 0.145f, 0.22f, 0.98f);
        private static readonly Color Accent = new(0.16f, 0.82f, 1f, 1f);
        private static readonly Color AccentSoft = new(0.08f, 0.35f, 0.48f, 1f);
        private static readonly Color Gold = new(1f, 0.72f, 0.2f, 1f);
        private static readonly Color Success = new(0.35f, 0.94f, 0.65f, 1f);
        private static readonly Color Danger = new(1f, 0.38f, 0.46f, 1f);
        private static readonly Color Muted = new(0.62f, 0.7f, 0.8f, 1f);
        private static readonly Color MutedDark = new(0.4f, 0.48f, 0.6f, 1f);

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
            var body = AddBody(root, true);

            var heading = Row();
            heading.style.justifyContent = Justify.SpaceBetween;
            heading.style.alignItems = Align.Center;
            var title = new VisualElement();
            title.Add(Label("격납고", 42, FontStyle.Bold));
            title.Add(Label("ORBITAL HANGAR · 출격 준비 구역", 16, FontStyle.Bold, Muted));
            heading.Add(title);
            heading.Add(Chip($"전투력 {_app.CombatPower:N0}", Accent));
            body.Add(heading);

            var hangar = Card(new Color(0.035f, 0.085f, 0.145f, 1f));
            hangar.style.minHeight = 430;
            hangar.style.marginTop = 18;
            hangar.style.overflow = Overflow.Hidden;
            Pad(hangar, 26, 24);
            AddHangarDecor(hangar);
            var hangarHeader = Row();
            hangarHeader.style.justifyContent = Justify.SpaceBetween;
            hangarHeader.style.alignItems = Align.Center;
            var hangarTitle = new VisualElement();
            hangarTitle.Add(Label("현재 조립 기체", 18, FontStyle.Bold, Muted));
            hangarTitle.Add(Label("PULSAR A1", 30, FontStyle.Bold));
            hangarHeader.Add(hangarTitle);
            hangarHeader.Add(Chip("6 / 6 ONLINE", Success));
            hangar.Add(hangarHeader);

            var shipWrap = new VisualElement();
            shipWrap.style.height = 280;
            shipWrap.style.alignItems = Align.Center;
            shipWrap.style.justifyContent = Justify.Center;
            shipWrap.Add(ShipVisualFactory.BuildUiShip(290f));
            hangar.Add(shipWrap);
            hangar.Add(Label("CORE · FRAME · DRIVE · IMPACTOR · ORBITER · REACTOR", 13, FontStyle.Bold, Muted));
            body.Add(hangar);

            var quickHeader = Row();
            quickHeader.style.justifyContent = Justify.SpaceBetween;
            quickHeader.style.alignItems = Align.Center;
            quickHeader.style.marginTop = 20;
            quickHeader.Add(Label("함선 운영", 24, FontStyle.Bold));
            quickHeader.Add(Label("오늘의 정비 상태 정상", 15, FontStyle.Bold, Success));
            body.Add(quickHeader);

            var quickGrid = Row();
            quickGrid.style.marginTop = 10;
            quickGrid.Add(FeatureTile("gift", "출석", "7일 보급", _app.ShowAttendance, "!"));
            quickGrid.Add(FeatureTile("trophy", "임무", "일일 2/5", _app.ShowMissions, "3"));
            quickGrid.Add(FeatureTile("gift", "우편", "보상 도착", _app.ShowMailbox, "3"));
            body.Add(quickGrid);
            var quickGrid2 = Row();
            quickGrid2.style.marginTop = 10;
            quickGrid2.Add(FeatureTile("market", "상점", "일일 상품", _app.ShowStore, null));
            quickGrid2.Add(FeatureTile("pouch", "회수", "부품 신호", _app.ShowRecovery, null));
            quickGrid2.Add(FeatureTile("news", "이벤트", "심우주 작전", _app.ShowEvents, null));
            body.Add(quickGrid2);

            var eventBanner = Card(new Color(0.055f, 0.075f, 0.16f, 1f));
            eventBanner.style.marginTop = 16;
            eventBanner.style.flexDirection = FlexDirection.Row;
            eventBanner.style.alignItems = Align.Center;
            Pad(eventBanner, 22, 18);
            eventBanner.Add(Icon("news", 56));
            var eventText = new VisualElement();
            eventText.style.flexGrow = 1;
            eventText.style.marginLeft = 18;
            eventText.Add(Label("심우주 회수 작전", 21, FontStyle.Bold));
            eventText.Add(Label("메인 작전 클리어로 회수 신호를 추적하세요.", 15, FontStyle.Normal, Muted));
            eventBanner.Add(eventText);
            eventBanner.Add(Chip("7일", Gold));
            eventBanner.AddManipulator(new Clickable(_app.ShowEvents));
            body.Add(eventBanner);

            var mission = Card();
            mission.style.marginTop = 16;
            Pad(mission, 26, 22);
            var missionTop = Row();
            missionTop.style.justifyContent = Justify.SpaceBetween;
            missionTop.style.alignItems = Align.Center;
            var missionName = new VisualElement();
            missionName.Add(Label(StackfallContentCatalog.ChapterName(stage), 22, FontStyle.Bold));
            missionName.Add(Label($"Stage {stage}", 34, FontStyle.Bold, Accent));
            missionTop.Add(missionName);
            missionTop.Add(Chip("메인 작전", Gold));
            mission.Add(missionTop);
            mission.Add(Label("주요 위협 · 돌진형 / 광역 펄스 / 최종 보스", 17, FontStyle.Normal, Muted));

            if (_app.HighestClearedStage < StackfallAppController.PlayableStageCap &&
                StackfallContentCatalog.TryGetNextUnlock(_app.HighestClearedStage, out var nextUnlock))
            {
                mission.Add(Label($"다음 해금 · Stage {nextUnlock.Stage} {nextUnlock.Name}", 17, FontStyle.Bold, Accent));
            }

            var missionActions = Row();
            missionActions.style.marginTop = 18;
            var chapters = Button("챕터 선택", _app.ShowChapters, false);
            chapters.style.width = 260;
            chapters.style.height = 94;
            missionActions.Add(chapters);
            var sortie = Button("출격", () => _app.OpenLoadout(stage), true);
            sortie.style.flexGrow = 1;
            sortie.style.height = 94;
            sortie.style.fontSize = 34;
            sortie.style.marginLeft = 10;
            missionActions.Add(sortie);
            mission.Add(missionActions);
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
            var missionTop = Row();
            missionTop.style.justifyContent = Justify.SpaceBetween;
            missionTop.style.alignItems = Align.Center;
            var left = new VisualElement();
            left.Add(Label($"{StackfallContentCatalog.ChapterName(stage)} · Stage {stage}", 28, FontStyle.Bold));
            left.Add(Label("추천 편성 · 전투 기록 기반", 16, FontStyle.Normal, Muted));
            missionTop.Add(left);
            missionTop.Add(Chip("위험도 NORMAL", Success));
            mission.Add(missionTop);
            mission.Add(Label("적 특성 · 근접 압박 / 돌진 / 광역 예고", 17, FontStyle.Bold, Accent));
            body.Add(mission);

            AddDeckSection(body, "액티브 후보 덱", "전투 중 최대 6종 획득", StackfallContentCatalog.ActiveDeck);
            AddDeckSection(body, "지원 후보 덱", "전투 중 최대 4종 획득", StackfallContentCatalog.SupportDeck);

            var parts = Card();
            parts.style.marginTop = 18;
            Pad(parts, 22, 18);
            var partsHeader = Row();
            partsHeader.style.justifyContent = Justify.SpaceBetween;
            partsHeader.Add(Label("기체 부품", 22, FontStyle.Bold));
            partsHeader.Add(Label("6 / 6", 18, FontStyle.Bold, Accent));
            parts.Add(partsHeader);
            var partsText = Label(JoinEquippedParts(_state.EquippedParts), 17, FontStyle.Normal, Muted);
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

        public void ShowLoading(int stage)
        {
            var root = BeginScreen(false);
            root.style.backgroundColor = new Color(0.006f, 0.012f, 0.035f, 1f);
            AddSpaceField(root);

            var center = new VisualElement();
            center.style.flexGrow = 1;
            center.style.alignItems = Align.Center;
            center.style.justifyContent = Justify.Center;
            Pad(center, 60, 40);
            root.Add(center);

            var emblem = Card(new Color(0.02f, 0.11f, 0.18f, 0.88f));
            emblem.style.width = 180;
            emblem.style.height = 180;
            emblem.style.alignItems = Align.Center;
            emblem.style.justifyContent = Justify.Center;
            SetRadius(emblem, 90);
            emblem.Add(ShipVisualFactory.BuildUiShip(120f));
            center.Add(emblem);

            var title = Label("항로 동기화 중", 35, FontStyle.Bold);
            title.style.marginTop = 34;
            center.Add(title);
            center.Add(Label($"{StackfallContentCatalog.ChapterName(stage)} · Stage {stage}", 20, FontStyle.Bold, Accent));

            var progress = Card(new Color(0.02f, 0.04f, 0.07f, 0.95f));
            progress.style.width = Length.Percent(82f);
            progress.style.height = 22;
            progress.style.marginTop = 30;
            progress.style.overflow = Overflow.Hidden;
            SetRadius(progress, 11);
            var fill = new VisualElement();
            fill.style.height = Length.Percent(100f);
            fill.style.width = Length.Percent(12f);
            fill.style.backgroundColor = Accent;
            SetRadius(fill, 11);
            progress.Add(fill);
            center.Add(progress);

            var step = 0;
            fill.schedule.Execute(() =>
            {
                step++;
                fill.style.width = Length.Percent(Mathf.Min(96f, 12f + step * 10f));
            }).Every(45);

            var tip = Card(new Color(0.025f, 0.045f, 0.085f, 0.9f));
            tip.style.width = Length.Percent(90f);
            tip.style.marginTop = 42;
            Pad(tip, 26, 20);
            tip.Add(Label("TACTICAL NOTE", 13, FontStyle.Bold, Gold));
            var tipText = Label("돌진 경고선이 나타나면 정면보다 측면 공간을 먼저 확보하세요.", 18, FontStyle.Normal, Muted);
            tipText.style.whiteSpace = WhiteSpace.Normal;
            tipText.style.unityTextAlign = TextAnchor.MiddleCenter;
            tip.Add(tipText);
            center.Add(tip);
        }

        public void ShowShip()
        {
            var root = BeginScreen();
            AddTopStatus(root);
            var body = AddBody(root, true);
            var header = Row();
            header.style.justifyContent = Justify.SpaceBetween;
            header.style.alignItems = Align.Center;
            var title = new VisualElement();
            title.Add(Label("기체", 42, FontStyle.Bold));
            title.Add(Label("ASSEMBLY BAY", 15, FontStyle.Bold, Muted));
            header.Add(title);
            header.Add(Chip("A1", Accent));
            body.Add(header);

            var bay = Card(new Color(0.035f, 0.085f, 0.145f, 1f));
            bay.style.marginTop = 18;
            Pad(bay, 24, 24);
            bay.style.alignItems = Align.Center;
            bay.Add(ShipVisualFactory.BuildUiShip(300f));
            bay.Add(Label("펄서 프레임 · A1", 25, FontStyle.Bold));
            bay.Add(Label($"전투력 {_app.CombatPower:N0}", 18, FontStyle.Bold, Accent));
            body.Add(bay);

            var presets = Row();
            presets.style.marginTop = 14;
            presets.Add(TabChip("메인", true));
            presets.Add(TabChip("보스", false));
            presets.Add(TabChip("아레나", false));
            body.Add(presets);

            var partsHeader = Row();
            partsHeader.style.marginTop = 18;
            partsHeader.style.justifyContent = Justify.SpaceBetween;
            partsHeader.style.alignItems = Align.Center;
            partsHeader.Add(Label("장착 부품", 24, FontStyle.Bold));
            var inventory = Button("부품 보관함", _app.ShowPartInventory, false);
            inventory.style.width = 230;
            inventory.style.height = 58;
            partsHeader.Add(inventory);
            body.Add(partsHeader);

            AddPart(body, "CORE", _state.EquippedParts[0], "Lv.1");
            AddPart(body, "FRAME", _state.EquippedParts[1], "Lv.1");
            AddPart(body, "DRIVE", _state.EquippedParts[2], "Lv.1");
            AddPart(body, "IMPACTOR", _state.EquippedParts[3], "Lv.1");
            AddPart(body, "ORBITER", _state.EquippedParts[4], "Lv.1");
            AddPart(body, "REACTOR", _state.EquippedParts[5], "Lv.1");

            var setSummary = Card(PanelBright);
            setSummary.style.marginTop = 14;
            Pad(setSummary, 20, 18);
            setSummary.Add(Label("현재 구성", 18, FontStyle.Bold, Accent));
            setSummary.Add(Label("6슬롯 혼합 구성 · 슬롯 강화는 부품 교체 후에도 유지", 15, FontStyle.Normal, Muted));
            body.Add(setSummary);
            AddBottomNavigation(root, "기체");
        }
    }
}
