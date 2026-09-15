using UnityEngine;
using UnityEngine.UIElements;

namespace StackfallMobile.Runtime.UI
{
    public sealed partial class StackfallShellController
    {
        public void ShowMailbox()
        {
            var root = BeginScreen();
            AddCompactHeader(root, "우편함", _app.ShowHome, "gift");
            var body = AddBody(root, true);

            var remaining = 3 - _state.ClaimedMail.Count;
            var summary = Card(PanelBright);
            Pad(summary, 22, 18);
            summary.style.flexDirection = FlexDirection.Row;
            summary.style.alignItems = Align.Center;
            summary.Add(Icon("gift", 62));
            var text = new VisualElement();
            text.style.marginLeft = 18;
            text.style.flexGrow = 1;
            text.Add(Label($"도착한 우편 {remaining}", 24, FontStyle.Bold));
            text.Add(Label("보상 우편은 수령 후 목록에서 정리됩니다.", 15, FontStyle.Normal, Muted));
            summary.Add(text);
            if (remaining > 0)
            {
                var all = Button("전체 수령", () =>
                {
                    _state.ClaimedMail.Add("welcome");
                    _state.ClaimedMail.Add("maintenance");
                    _state.ClaimedMail.Add("event");
                    ShowModal(root, "우편 보상 수령", "도착한 보급품을 모두 수령했습니다.", "확인", ShowMailbox, "gift", Success);
                }, true);
                all.style.width = 190;
                all.style.height = 60;
                summary.Add(all);
            }
            body.Add(summary);

            AddMail(root, body, "welcome", "작전본부", "신규 파일럿 지원 보급", "크레딧 2,000 · 크리스탈 100", "6일 23시간");
            AddMail(root, body, "maintenance", "정비국", "격납고 정기 점검 보상", "크레딧 1,000 · 합금 5", "13일 04시간");
            AddMail(root, body, "event", "탐사국", "심우주 회수 작전 개시", "공명 키 1 · 합금 10", "29일 23시간");
        }

        public void ShowMissions()
        {
            ShowMissionsTab(_state.MissionTab);
        }

        private void ShowMissionsTab(int selected)
        {
            _state.MissionTab = Mathf.Clamp(selected, 0, 2);
            var root = BeginScreen();
            AddCompactHeader(root, "임무", _app.ShowHome, "trophy");
            var body = AddBody(root, true);

            var tabs = Row();
            tabs.Add(SelectableTab("일일", _state.MissionTab == 0, () => ShowMissionsTab(0)));
            tabs.Add(SelectableTab("주간", _state.MissionTab == 1, () => ShowMissionsTab(1)));
            tabs.Add(SelectableTab("업적", _state.MissionTab == 2, () => ShowMissionsTab(2)));
            body.Add(tabs);

            if (_state.MissionTab == 0)
            {
                AddDailyMissions(root, body);
            }
            else if (_state.MissionTab == 1)
            {
                AddWeeklyMissions(root, body);
            }
            else
            {
                AddAchievements(root, body);
            }
        }

        private void AddDailyMissions(VisualElement root, VisualElement body)
        {
            var meter = Card(PanelBright);
            meter.style.marginTop = 16;
            Pad(meter, 22, 18);
            var meterHead = Row();
            meterHead.style.justifyContent = Justify.SpaceBetween;
            meterHead.Add(Label("오늘의 작전 점수", 22, FontStyle.Bold));
            meterHead.Add(Label("45 / 100", 18, FontStyle.Bold, Gold));
            meter.Add(meterHead);
            meter.Add(ProgressBar(45f, Gold));
            meter.Add(Label("20 · 40 · 60 · 80 · 100 점 보급 상자", 14, FontStyle.Normal, Muted));
            body.Add(meter);

            AddMission(root, body, "daily_sortie", "메인 작전 1회 출격", 1, 1, "크리스탈 20", true);
            AddMission(root, body, "daily_kill", "적 120기 격파", 84, 120, "크리스탈 20", false);
            AddMission(root, body, "daily_upgrade", "전투 중 강화 5회 획득", 3, 5, "크리스탈 15", false);
            AddMission(root, body, "daily_hangar", "격납고 확인", 1, 1, "크리스탈 15", true);
        }

        private void AddWeeklyMissions(VisualElement root, VisualElement body)
        {
            var meter = Card(PanelBright);
            meter.style.marginTop = 16;
            Pad(meter, 22, 18);
            meter.Add(Label("주간 작전 진척", 22, FontStyle.Bold));
            meter.Add(ProgressBar(32f, Accent));
            meter.Add(Label("주간 임무는 월요일에 갱신됩니다.", 14, FontStyle.Normal, Muted));
            body.Add(meter);

            AddMission(root, body, "weekly_sortie", "메인 작전 누적 출격", 2, 5, "크리스탈 80", false);
            AddMission(root, body, "weekly_boss", "보스 격파 기록", 1, 3, "공명 키 1", false);
            AddMission(root, body, "weekly_growth", "기체 성장 행동", 4, 8, "합금 20", false);
            AddMission(root, body, "weekly_login", "주간 접속", 3, 7, "크리스탈 100", false);
        }

        private void AddAchievements(VisualElement root, VisualElement body)
        {
            var summary = Card(PanelBright);
            summary.style.marginTop = 16;
            Pad(summary, 22, 18);
            summary.Add(Label("파일럿 업적", 22, FontStyle.Bold));
            summary.Add(Label("영구 기록은 시즌이 바뀌어도 유지됩니다.", 14, FontStyle.Normal, Muted));
            body.Add(summary);

            AddMission(root, body, "achievement_first_sortie", "첫 항해", 1, 1, "프로필 장식", true);
            AddMission(root, body, "achievement_first_boss", "첫 보스 격파", 0, 1, "크리스탈 100", false);
            AddMission(root, body, "achievement_first_s", "첫 S 프로토타입 확보", 0, 1, "공명 키 1", false);
            AddMission(root, body, "achievement_chapter", "챕터 완주", 0, 8, "전용 프로필 장식", false);
        }

        public void ShowStore()
        {
            ShowStoreTab(_state.StoreTab);
        }

        private void ShowStoreTab(int selected)
        {
            _state.StoreTab = Mathf.Clamp(selected, 0, 2);
            var root = BeginScreen();
            AddCompactHeader(root, "상점", _app.ShowHome, "market");
            var body = AddBody(root, true);

            var tabs = Row();
            tabs.Add(SelectableTab("추천", _state.StoreTab == 0, () => ShowStoreTab(0)));
            tabs.Add(SelectableTab("일일", _state.StoreTab == 1, () => ShowStoreTab(1)));
            tabs.Add(SelectableTab("재화", _state.StoreTab == 2, () => ShowStoreTab(2)));
            body.Add(tabs);

            if (_state.StoreTab == 0)
            {
                AddRecommendedStore(root, body);
            }
            else if (_state.StoreTab == 1)
            {
                AddDailyStore(root, body);
            }
            else
            {
                AddCurrencyStore(root, body);
            }
        }

        private void AddRecommendedStore(VisualElement root, VisualElement body)
        {
            var featured = Card(new Color(0.09f, 0.085f, 0.18f, 1f));
            featured.style.marginTop = 16;
            Pad(featured, 24, 24);
            var featuredTop = Row();
            featuredTop.style.alignItems = Align.Center;
            featuredTop.Add(Icon("pouch", 74));
            var featuredText = new VisualElement();
            featuredText.style.marginLeft = 20;
            featuredText.style.flexGrow = 1;
            featuredText.Add(Label("초기 항해 보급 패키지", 25, FontStyle.Bold));
            featuredText.Add(Label("공명 키 · 크리스탈 · 성장 재료 구성", 16, FontStyle.Normal, Muted));
            featuredTop.Add(featuredText);
            featured.Add(featuredTop);
            featured.Add(StoreAction("상품 상세", "1회 한정", () => ShowProductInfo(root, "초기 항해 보급 패키지", "공명 키와 성장 재료를 묶은 초반 항해 지원 구성입니다.")));
            body.Add(featured);

            var row = Row();
            row.style.marginTop = 14;
            row.Add(StoreCard("gift", "일일 보급", "매일 한 번 확인 가능한 무료 보급", _state.FreeDailySupplyClaimed ? "수령 완료" : "무료", () => ClaimFreeSupply(root), true, !_state.FreeDailySupplyClaimed));
            row.Add(StoreCard("market", "정비 재료", "합금 · 설계도", "상품 보기", () => ShowProductInfo(root, "정비 재료", "기체 슬롯 성장과 승급에 사용하는 재료 묶음입니다.")));
            body.Add(row);

            var row2 = Row();
            row2.style.marginTop = 12;
            row2.Add(StoreCard("pouch", "회수 키", "표준 회수 전용", "상품 보기", () => ShowProductInfo(root, "회수 키", "표준 회수에 사용하는 전용 키 상품입니다.")));
            row2.Add(StoreCard("news", "공명 키", "프로토타입 공명 전용", "상품 보기", () => ShowProductInfo(root, "공명 키", "프로토타입 공명에 사용하는 전용 키 상품입니다.")));
            body.Add(row2);
        }

        private void AddDailyStore(VisualElement root, VisualElement body)
        {
            var header = Card(PanelBright);
            header.style.marginTop = 16;
            Pad(header, 22, 18);
            header.Add(Label("오늘의 정비 보급", 23, FontStyle.Bold));
            header.Add(Label("일일 상품은 다음 갱신 시 목록이 교체됩니다.", 14, FontStyle.Normal, Muted));
            body.Add(header);

            var row = Row();
            row.style.marginTop = 14;
            row.Add(StoreCard("gift", "무료 보급", "크레딧 · 합금", _state.FreeDailySupplyClaimed ? "수령 완료" : "무료", () => ClaimFreeSupply(root), true, !_state.FreeDailySupplyClaimed));
            row.Add(StoreCard("market", "합금 상자", "슬롯 강화 재료", "상품 보기", () => ShowProductInfo(root, "합금 상자", "슬롯 강화에 필요한 합금이 포함됩니다.")));
            body.Add(row);

            var row2 = Row();
            row2.style.marginTop = 12;
            row2.Add(StoreCard("pouch", "회수 보급", "표준 회수 키", "상품 보기", () => ShowProductInfo(root, "회수 보급", "표준 회수에 사용하는 키가 포함됩니다.")));
            row2.Add(StoreCard("news", "공명 보급", "프로토타입 공명 키", "상품 보기", () => ShowProductInfo(root, "공명 보급", "프로토타입 공명에 사용하는 키가 포함됩니다.")));
            body.Add(row2);
        }

        private void AddCurrencyStore(VisualElement root, VisualElement body)
        {
            var header = Card(PanelBright);
            header.style.marginTop = 16;
            Pad(header, 22, 18);
            header.Add(Label("크리스탈", 23, FontStyle.Bold));
            header.Add(Label("상점 상품을 통해 크리스탈을 충전할 수 있습니다.", 14, FontStyle.Normal, Muted));
            body.Add(header);

            var row = Row();
            row.style.marginTop = 14;
            row.Add(StoreCard("news", "크리스탈 팩 A", "소형 크리스탈 묶음", "상품 보기", () => ShowProductInfo(root, "크리스탈 팩 A", "소형 크리스탈 충전 상품입니다.")));
            row.Add(StoreCard("news", "크리스탈 팩 B", "중형 크리스탈 묶음", "상품 보기", () => ShowProductInfo(root, "크리스탈 팩 B", "중형 크리스탈 충전 상품입니다.")));
            body.Add(row);

            var row2 = Row();
            row2.style.marginTop = 12;
            row2.Add(StoreCard("pouch", "크리스탈 팩 C", "대형 크리스탈 묶음", "상품 보기", () => ShowProductInfo(root, "크리스탈 팩 C", "대형 크리스탈 충전 상품입니다.")));
            row2.Add(StoreCard("gift", "월간 보급", "매일 크리스탈 수령", "상품 보기", () => ShowProductInfo(root, "월간 보급", "구매 후 일정 기간 매일 보급을 수령하는 상품입니다.")));
            body.Add(row2);
        }

        private void ClaimFreeSupply(VisualElement root)
        {
            if (_state.FreeDailySupplyClaimed)
            {
                return;
            }

            _state.FreeDailySupplyClaimed = true;
            ShowModal(root, "무료 보급 수령", "오늘의 무료 정비 보급을 수령했습니다.", "확인", ShowStore, "gift", Success);
        }

        private static void ShowProductInfo(VisualElement root, string title, string message)
        {
            ShowModal(root, title, message, null, null, "market", Accent);
        }

        public void ShowSummon()
        {
            var root = BeginScreen();
            AddCompactHeader(root, "소환", _app.ShowHome, "pouch");
            var body = AddBody(root, true);

            var banner = Card(new Color(0.075f, 0.055f, 0.17f, 1f));
            banner.style.minHeight = 360;
            Pad(banner, 28, 26);
            banner.Add(Label("SIGNAL ACQUISITION", 13, FontStyle.Bold, Gold));
            banner.Add(Label("프로토타입 공명", 34, FontStyle.Bold));
            banner.Add(Label("S 등급 부품 획득 시 다음 S 등급까지의 누적 횟수가 초기화됩니다.", 16, FontStyle.Normal, Muted));
            var orb = new VisualElement();
            orb.style.width = 150;
            orb.style.height = 150;
            orb.style.alignSelf = Align.Center;
            orb.style.marginTop = 20;
            orb.style.backgroundColor = new Color(0.12f, 0.65f, 0.95f, 0.28f);
            SetRadius(orb, 75);
            var inner = new VisualElement();
            inner.style.width = 82;
            inner.style.height = 82;
            inner.style.alignSelf = Align.Center;
            inner.style.marginTop = 34;
            inner.style.backgroundColor = Accent;
            SetRadius(inner, 41);
            orb.Add(inner);
            banner.Add(orb);
            var pity = Row();
            pity.style.justifyContent = Justify.SpaceBetween;
            pity.style.marginTop = 16;
            pity.Add(Label("S 등급 확정", 16, FontStyle.Bold, Muted));
            pity.Add(Label("0 / 50", 17, FontStyle.Bold, Gold));
            banner.Add(pity);
            banner.Add(ProgressBar(4f, Gold));
            body.Add(banner);

            var buttons = Row();
            buttons.style.marginTop = 14;
            var single = Button("1회 소환\n크리스탈 300", () => ShowInsufficientCrystal(root), false);
            single.style.flexGrow = 1;
            single.style.height = 92;
            var multi = Button("10회 소환\n크리스탈 2,700", () => ShowInsufficientCrystal(root), true);
            multi.style.flexGrow = 1;
            multi.style.height = 92;
            multi.style.marginLeft = 10;
            buttons.Add(single);
            buttons.Add(multi);
            body.Add(buttons);

            var info = Card();
            info.style.marginTop = 14;
            Pad(info, 20, 18);
            info.Add(Label("등급 확률", 20, FontStyle.Bold));
            info.Add(Label("S 1.50% · 영웅 6.00% · 희귀 22.50% · 고급 35.00% · 일반 35.00%", 15, FontStyle.Normal, Muted));
            info.Add(Label("50회 S 확정 · S 등장 시 픽업 세트 60% · 픽업 실패 후 다음 S 픽업 확정", 14, FontStyle.Normal, Muted));
            body.Add(info);
            AddBottomNavigation(root, "회수");
        }

        private void ShowInsufficientCrystal(VisualElement root)
        {
            ShowModal(root, "크리스탈 부족", "소환에 필요한 크리스탈이 부족합니다.", "상점 보기", _app.ShowStore, "pouch", Gold);
        }

        public void ShowSettings()
        {
            var root = BeginScreen();
            AddCompactHeader(root, "설정", _app.ShowHome, "gear");
            var body = AddBody(root, true);

            var account = Card(PanelBright);
            Pad(account, 22, 18);
            account.Add(Label("파일럿 계정", 21, FontStyle.Bold));
            account.Add(Label("PILOT-0001 · 파일럿 계정", 15, FontStyle.Normal, Muted));
            body.Add(account);

            body.Add(SettingToggle("배경 음악", _state.BgmEnabled, value => _state.BgmEnabled = value));
            body.Add(SettingToggle("효과음", _state.SfxEnabled, value => _state.SfxEnabled = value));
            body.Add(SettingToggle("진동", _state.VibrationEnabled, value => _state.VibrationEnabled = value));
            body.Add(SettingToggle("절전 모드", _state.PowerSaveEnabled, value => _state.PowerSaveEnabled = value));

            var graphics = Card();
            graphics.style.marginTop = 12;
            Pad(graphics, 22, 18);
            graphics.Add(Label("그래픽 품질", 20, FontStyle.Bold));
            var quality = Row();
            quality.style.marginTop = 10;
            quality.Add(SelectableTab("낮음", _state.GraphicsQuality == 0, () => SetGraphicsQuality(0)));
            quality.Add(SelectableTab("중간", _state.GraphicsQuality == 1, () => SetGraphicsQuality(1)));
            quality.Add(SelectableTab("높음", _state.GraphicsQuality == 2, () => SetGraphicsQuality(2)));
            graphics.Add(quality);
            body.Add(graphics);

            var language = ClickableSettingRow("언어", "한국어", () => ShowModal(root, "언어", "현재 표시 언어는 한국어입니다.", null, null, "gear"));
            language.style.marginTop = 12;
            body.Add(language);
            body.Add(ClickableSettingRow("알림", "이벤트 · 우편 · 보상", () => ShowModal(root, "알림", "이벤트, 우편, 보상 알림을 한 곳에서 관리합니다.", null, null, "news")));
            body.Add(ClickableSettingRow("고객지원", "문의 및 FAQ", () => ShowModal(root, "고객지원", "계정과 결제, 게임 이용 관련 도움말을 확인할 수 있습니다.", null, null, "chat")));
            body.Add(ClickableSettingRow("이용약관", "서비스 정책", () => ShowModal(root, "이용약관", "서비스 이용 정책과 운영 기준을 확인할 수 있습니다.", null, null, "news")));
            body.Add(ClickableSettingRow("개인정보처리방침", "개인정보 보호", () => ShowModal(root, "개인정보처리방침", "개인정보 보호 및 처리 기준을 확인할 수 있습니다.", null, null, "lock")));
        }

        private void SetGraphicsQuality(int quality)
        {
            _state.GraphicsQuality = Mathf.Clamp(quality, 0, 2);
            ShowSettings();
        }
    }
}
