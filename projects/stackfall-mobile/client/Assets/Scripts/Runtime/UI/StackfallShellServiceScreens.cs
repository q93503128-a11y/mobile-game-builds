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

            var summary = Card(PanelBright);
            Pad(summary, 22, 18);
            summary.style.flexDirection = FlexDirection.Row;
            summary.style.alignItems = Align.Center;
            summary.Add(Icon("gift", 62));
            var text = new VisualElement();
            text.style.marginLeft = 18;
            text.style.flexGrow = 1;
            text.Add(Label("도착한 우편 3", 24, FontStyle.Bold));
            text.Add(Label("보상 우편은 수령 후 목록에서 정리됩니다.", 15, FontStyle.Normal, Muted));
            summary.Add(text);
            body.Add(summary);

            AddMail(body, "작전본부", "신규 파일럿 지원 보급", "크레딧 2,000 · 크리스탈 100", "6일 23시간");
            AddMail(body, "정비국", "격납고 정기 점검 보상", "크레딧 1,000 · 합금 5", "13일 04시간");
            AddMail(body, "탐사국", "심우주 회수 작전 개시", "공명 키 1 · 합금 10", "29일 23시간");
        }

        public void ShowMissions()
        {
            var root = BeginScreen();
            AddCompactHeader(root, "임무", _app.ShowHome, "trophy");
            var body = AddBody(root, true);

            var tabs = Row();
            tabs.Add(TabChip("일일", true));
            tabs.Add(TabChip("주간", false));
            tabs.Add(TabChip("업적", false));
            body.Add(tabs);

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

            AddMission(body, "메인 작전 1회 출격", 1, 1, "크리스탈 20", true);
            AddMission(body, "적 120기 격파", 84, 120, "크리스탈 20", false);
            AddMission(body, "전투 중 강화 5회 획득", 3, 5, "크리스탈 15", false);
            AddMission(body, "격납고 확인", 1, 1, "크리스탈 15", true);
        }

        public void ShowStore()
        {
            var root = BeginScreen();
            AddCompactHeader(root, "상점", _app.ShowHome, "market");
            var body = AddBody(root, true);

            var tabs = Row();
            tabs.Add(TabChip("추천", true));
            tabs.Add(TabChip("일일", false));
            tabs.Add(TabChip("재화", false));
            body.Add(tabs);

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
            featured.Add(StoreAction("상품 상세", "1회 한정"));
            body.Add(featured);

            var row = Row();
            row.style.marginTop = 14;
            row.Add(StoreCard("gift", "일일 보급", "크레딧 · 합금", "무료"));
            row.Add(StoreCard("market", "정비 재료", "합금 · 설계도", "상품 보기"));
            body.Add(row);

            var row2 = Row();
            row2.style.marginTop = 12;
            row2.Add(StoreCard("pouch", "회수 키", "표준 회수 전용", "상품 보기"));
            row2.Add(StoreCard("news", "공명 키", "프로토타입 공명 전용", "상품 보기"));
            body.Add(row2);
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
            var single = Button("1회 소환\n크리스탈 300", () => { }, false);
            single.style.flexGrow = 1;
            single.style.height = 92;
            var multi = Button("10회 소환\n크리스탈 2,700", () => { }, true);
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

            body.Add(SettingToggle("배경 음악", true));
            body.Add(SettingToggle("효과음", true));
            body.Add(SettingToggle("진동", true));
            body.Add(SettingToggle("절전 모드", false));

            var graphics = Card();
            graphics.style.marginTop = 12;
            Pad(graphics, 22, 18);
            graphics.Add(Label("그래픽 품질", 20, FontStyle.Bold));
            var quality = Row();
            quality.style.marginTop = 10;
            quality.Add(TabChip("낮음", false));
            quality.Add(TabChip("중간", false));
            quality.Add(TabChip("높음", true));
            graphics.Add(quality);
            body.Add(graphics);

            var language = SettingRow("언어", "한국어");
            language.style.marginTop = 12;
            body.Add(language);
            body.Add(SettingRow("알림", "이벤트 · 우편 · 보상"));
            body.Add(SettingRow("고객지원", "문의 및 FAQ"));
            body.Add(SettingRow("이용약관", "서비스 정책"));
            body.Add(SettingRow("개인정보처리방침", "개인정보 보호"));
        }
    }
}
