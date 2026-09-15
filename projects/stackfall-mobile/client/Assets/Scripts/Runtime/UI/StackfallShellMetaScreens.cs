using StackfallMobile.Runtime.App;
using StackfallMobile.Runtime.Rendering;
using UnityEngine;
using UnityEngine.UIElements;

namespace StackfallMobile.Runtime.UI
{
    public sealed partial class StackfallShellController
    {
        public void ShowProfile()
        {
            var root = BeginScreen();
            AddCompactHeader(root, "파일럿 프로필", _app.ShowHome);
            var body = AddBody(root, true);

            var hero = Card(new Color(0.035f, 0.09f, 0.15f, 1f));
            hero.style.alignItems = Align.Center;
            Pad(hero, 28, 28);
            var avatar = new VisualElement();
            avatar.style.width = 150;
            avatar.style.height = 150;
            avatar.style.alignItems = Align.Center;
            avatar.style.justifyContent = Justify.Center;
            avatar.style.backgroundColor = new Color(0.06f, 0.22f, 0.31f, 1f);
            SetRadius(avatar, 75);
            avatar.Add(ShipVisualFactory.BuildUiShip(104f));
            hero.Add(avatar);
            hero.Add(Label("파일럿", 32, FontStyle.Bold));
            hero.Add(Label("PILOT-0001", 15, FontStyle.Bold, Muted));
            hero.Add(Chip($"전투력 {_app.CombatPower:N0}", Accent));
            body.Add(hero);

            var stats = Row();
            stats.style.marginTop = 14;
            stats.Add(ProfileStat("진행", $"Stage {_app.CurrentStage}"));
            stats.Add(ProfileStat("최고 클리어", _app.HighestClearedStage > 0 ? $"Stage {_app.HighestClearedStage}" : "기록 없음"));
            stats.Add(ProfileStat("기체", "PULSAR A1"));
            body.Add(stats);

            var record = Card();
            record.style.marginTop = 14;
            Pad(record, 22, 20);
            record.Add(Label("작전 기록", 22, FontStyle.Bold));
            record.Add(ProfileRecord("메인 작전", "폐기 궤도장"));
            record.Add(ProfileRecord("대표 프리셋", "메인"));
            record.Add(ProfileRecord("대표 기체", "PULSAR A1"));
            body.Add(record);

            var medals = Card();
            medals.style.marginTop = 14;
            Pad(medals, 22, 20);
            medals.Add(Label("프로필 장식", 22, FontStyle.Bold));
            var badgeRow = Row();
            badgeRow.style.marginTop = 12;
            badgeRow.Add(ProfileBadge("trophy", "초기 항해"));
            badgeRow.Add(ProfileBadge("news", "심우주 탐사"));
            badgeRow.Add(ProfileBadge("lock", "추가 기록"));
            medals.Add(badgeRow);
            body.Add(medals);
        }

        public void ShowAttendance()
        {
            var root = BeginScreen();
            AddCompactHeader(root, "7일 출석 보급", _app.ShowHome, "gift");
            var body = AddBody(root, true);

            var banner = Card(new Color(0.055f, 0.08f, 0.17f, 1f));
            Pad(banner, 24, 22);
            banner.Add(Label("항해 보급 일정", 30, FontStyle.Bold));
            banner.Add(Label("매일 접속해 항해에 필요한 보급품을 확보하세요.", 16, FontStyle.Normal, Muted));
            body.Add(banner);

            var rewards = new[]
            {
                ("DAY 1", "크레딧 보급", "gift"),
                ("DAY 2", "합금 보급", "market"),
                ("DAY 3", "크리스탈 보급", "news"),
                ("DAY 4", "회수 키", "pouch"),
                ("DAY 5", "정밀부품 보급", "market"),
                ("DAY 6", "크리스탈 보급", "news"),
                ("DAY 7", "공명 키", "pouch")
            };

            for (var rowIndex = 0; rowIndex < 2; rowIndex++)
            {
                var row = Row();
                row.style.marginTop = 14;
                var start = rowIndex == 0 ? 0 : 4;
                var end = rowIndex == 0 ? 4 : 7;
                for (var i = start; i < end; i++)
                {
                    row.Add(AttendanceCard(rewards[i].Item1, rewards[i].Item2, rewards[i].Item3, i == 0));
                }
                body.Add(row);
            }

            var streak = Card(PanelBright);
            streak.style.marginTop = 16;
            Pad(streak, 20, 18);
            streak.Add(Label("주간 누적", 21, FontStyle.Bold));
            streak.Add(Label("7일 보급을 모두 확인하면 다음 주기 보급 일정으로 이어집니다.", 15, FontStyle.Normal, Muted));
            body.Add(streak);
        }

        public void ShowEvents()
        {
            var root = BeginScreen();
            AddCompactHeader(root, "이벤트", _app.ShowHome, "news");
            var body = AddBody(root, true);

            var hero = Card(new Color(0.075f, 0.055f, 0.17f, 1f));
            hero.style.minHeight = 300;
            Pad(hero, 28, 24);
            hero.Add(Label("DEEP SPACE SALVAGE", 13, FontStyle.Bold, Gold));
            hero.Add(Label("심우주 회수 작전", 34, FontStyle.Bold));
            hero.Add(Label("메인 작전을 클리어하고 회수 신호를 추적해 이벤트 보급을 확보하세요.", 17, FontStyle.Normal, Muted));
            hero.Add(ProgressBar(28f, Accent));
            var action = Button("작전 현황", () => { }, true);
            action.style.height = 70;
            action.style.marginTop = 18;
            hero.Add(action);
            body.Add(hero);

            body.Add(EventCard("gift", "7일 출석 보급", "매일 갱신되는 항해 보급 일정", "출석 확인", _app.ShowAttendance));
            body.Add(EventCard("trophy", "주간 파일럿 임무", "일일·주간 임무 진행도를 합산", "임무 확인", _app.ShowMissions));
            body.Add(EventCard("market", "회수 지원 교환소", "이벤트 활동으로 확보한 교환 재화 사용", "교환소", _app.ShowStore));
        }

        public void ShowChapters()
        {
            var root = BeginScreen();
            AddCompactHeader(root, "메인 작전", _app.ShowHome);
            var body = AddBody(root, true);

            body.Add(Label("작전 구역", 28, FontStyle.Bold));
            body.Add(Label("8개 초기 챕터의 위협과 환경 규칙을 미리 확인할 수 있습니다.", 15, FontStyle.Normal, Muted));

            var chapters = new[]
            {
                ("CHAPTER 1", "폐기 궤도장", "이동 · 자동공격 · 엘리트 · 보스", 1),
                ("CHAPTER 2", "채굴 위성", "포격 · 지형 압박 · 우선 처치", 11),
                ("CHAPTER 3", "냉각 시설", "감속 지대 · 원거리 혼합", 21),
                ("CHAPTER 4", "붉은 정제소", "폭발 연쇄 · 돌진 압박", 31),
                ("CHAPTER 5", "중력 연구소", "흡인 · 밀어냄 · 위치 제어", 41),
                ("CHAPTER 6", "파손 함대", "원거리 포화 · 지원기 우선 처치", 51),
                ("CHAPTER 7", "공허 경계", "위상 이동 · 잔상 · 변위", 61),
                ("CHAPTER 8", "성간 묘지", "앞선 위협을 혼합하는 종합 전투", 71)
            };

            foreach (var chapter in chapters)
            {
                var available = chapter.Item4 == 1;
                body.Add(ChapterCard(chapter.Item1, chapter.Item2, chapter.Item3, chapter.Item4, available));
            }
        }

        public void ShowPartInventory()
        {
            var root = BeginScreen();
            AddCompactHeader(root, "부품 보관함", _app.ShowShip);
            var body = AddBody(root, true);

            var tabs = Row();
            tabs.Add(TabChip("CORE", true));
            tabs.Add(TabChip("FRAME", false));
            tabs.Add(TabChip("DRIVE", false));
            body.Add(tabs);
            var tabs2 = Row();
            tabs2.style.marginTop = 8;
            tabs2.Add(TabChip("IMPACTOR", false));
            tabs2.Add(TabChip("ORBITER", false));
            tabs2.Add(TabChip("REACTOR", false));
            body.Add(tabs2);

            var equipped = Card(PanelBright);
            equipped.style.marginTop = 16;
            Pad(equipped, 22, 18);
            equipped.Add(Label("현재 장착", 15, FontStyle.Bold, Accent));
            equipped.Add(Label("펄서 코어", 25, FontStyle.Bold));
            equipped.Add(Label("슬롯 강화 Lv.1 · 기본 출력 계통", 15, FontStyle.Normal, Muted));
            body.Add(equipped);

            var row = Row();
            row.style.marginTop = 14;
            row.Add(PartInventoryCard("펄서 코어", "장착 중", "일반", Accent));
            row.Add(PartInventoryCard("중력핵 코어", "중력 계통", "S 프로토타입", Gold));
            body.Add(row);

            var row2 = Row();
            row2.style.marginTop = 12;
            row2.Add(PartInventoryCard("과부하 코어", "연속 처치 계통", "영웅", new Color(0.75f, 0.45f, 1f, 1f)));
            row2.Add(PartInventoryCard("요새 코어", "보호막 계통", "희귀", new Color(0.3f, 0.65f, 1f, 1f)));
            body.Add(row2);

            var note = Card();
            note.style.marginTop = 16;
            Pad(note, 20, 18);
            note.Add(Label("슬롯 강화 공유", 20, FontStyle.Bold));
            note.Add(Label("새 부품으로 교체해도 해당 슬롯의 강화 레벨은 유지됩니다.", 15, FontStyle.Normal, Muted));
            body.Add(note);
        }

        public void ShowChallenges()
        {
            var root = BeginScreen();
            AddTopStatus(root);
            var body = AddBody(root, true);
            body.Add(Label("도전", 42, FontStyle.Bold));
            body.Add(Label("반복 콘텐츠마다 다른 성장 재료와 기록 목표를 제공합니다.", 16, FontStyle.Normal, Muted));

            body.Add(ChallengeCard("market", "크레딧 채굴장", "슬롯 강화용 크레딧", "일일"));
            body.Add(ChallengeCard("pouch", "정밀부품 회수전", "10레벨 단위 승급 재료", "일일"));
            body.Add(ChallengeCard("trophy", "보스 격파전", "90초 최대 피해 기록", "기록"));
            body.Add(ChallengeCard("news", "무한 탑", "층별 최초 클리어와 시즌 최고층", "시즌"));
            body.Add(ChallengeCard("lock", "아레나", "비동기 PvP 방어 기체 스냅샷", "경쟁"));
            AddBottomNavigation(root, "도전");
        }

        public void ShowGuild()
        {
            var root = BeginScreen();
            AddTopStatus(root);
            var body = AddBody(root, true);

            var banner = Card(new Color(0.045f, 0.09f, 0.14f, 1f));
            Pad(banner, 26, 24);
            banner.Add(Icon("chat", 62));
            banner.Add(Label("길드 네트워크", 32, FontStyle.Bold));
            banner.Add(Label("Stage 25 클리어 후 길드 가입·출석·기부·연구·보스 기능이 개방됩니다.", 16, FontStyle.Normal, Muted));
            body.Add(banner);

            var grid = Row();
            grid.style.marginTop = 14;
            grid.Add(GuildFeature("gift", "출석", "개인 보상 · 길드 활동도"));
            grid.Add(GuildFeature("market", "기부", "길드 코인 · 연구 포인트"));
            body.Add(grid);
            var grid2 = Row();
            grid2.style.marginTop = 12;
            grid2.Add(GuildFeature("trophy", "길드 보스", "개인 · 길드 누적 피해"));
            grid2.Add(GuildFeature("chat", "채팅", "글로벌 · 국가 · 길드"));
            body.Add(grid2);

            var research = Card();
            research.style.marginTop = 14;
            Pad(research, 20, 18);
            research.Add(Label("길드 연구", 20, FontStyle.Bold));
            research.Add(Label("일반 공격 출력 · 보스 피해 · 최대 HP · 크레딧 획득량", 15, FontStyle.Normal, Muted));
            body.Add(research);
            AddBottomNavigation(root, "길드");
        }

        private static VisualElement ProfileStat(string title, string value)
        {
            var card = Card();
            card.style.flexGrow = 1;
            card.style.marginRight = 6;
            card.style.alignItems = Align.Center;
            Pad(card, 12, 18);
            card.Add(Label(title, 13, FontStyle.Bold, Muted));
            card.Add(Label(value, 18, FontStyle.Bold, Accent));
            return card;
        }

        private static VisualElement ProfileRecord(string title, string value)
        {
            var row = Row();
            row.style.marginTop = 12;
            row.style.justifyContent = Justify.SpaceBetween;
            row.Add(Label(title, 16, FontStyle.Bold, Muted));
            row.Add(Label(value, 16, FontStyle.Bold));
            return row;
        }

        private static VisualElement ProfileBadge(string icon, string title)
        {
            var card = Card(PanelSoft);
            card.style.flexGrow = 1;
            card.style.marginRight = 6;
            card.style.alignItems = Align.Center;
            Pad(card, 12, 16);
            card.Add(Icon(icon, 46));
            card.Add(Label(title, 14, FontStyle.Bold));
            return card;
        }

        private static VisualElement AttendanceCard(string day, string reward, string icon, bool today)
        {
            var card = Card(today ? PanelBright : Panel);
            card.style.flexGrow = 1;
            card.style.marginRight = 6;
            card.style.minHeight = 210;
            card.style.alignItems = Align.Center;
            card.style.justifyContent = Justify.Center;
            Pad(card, 10, 14);
            card.Add(Label(day, 13, FontStyle.Bold, today ? Gold : Muted));
            card.Add(Icon(icon, 50));
            var rewardLabel = Label(reward, 14, FontStyle.Bold);
            rewardLabel.style.whiteSpace = WhiteSpace.Normal;
            rewardLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            card.Add(rewardLabel);
            if (today)
            {
                card.Add(Chip("오늘", Success));
            }
            return card;
        }

        private static VisualElement EventCard(string icon, string title, string detail, string actionText, System.Action action)
        {
            var card = Card();
            card.style.marginTop = 12;
            Pad(card, 20, 18);
            var top = Row();
            top.style.alignItems = Align.Center;
            top.Add(Icon(icon, 52));
            var text = new VisualElement();
            text.style.marginLeft = 16;
            text.style.flexGrow = 1;
            text.Add(Label(title, 20, FontStyle.Bold));
            text.Add(Label(detail, 14, FontStyle.Normal, Muted));
            top.Add(text);
            card.Add(top);
            var button = Button(actionText, action, false);
            button.style.height = 58;
            button.style.marginTop = 12;
            card.Add(button);
            return card;
        }

        private VisualElement ChapterCard(string chapter, string title, string detail, int firstStage, bool available)
        {
            var card = Card(available ? PanelBright : new Color(0.026f, 0.045f, 0.075f, 1f));
            card.style.marginTop = 12;
            Pad(card, 22, 18);
            var top = Row();
            top.style.justifyContent = Justify.SpaceBetween;
            top.style.alignItems = Align.Center;
            var left = new VisualElement();
            left.Add(Label(chapter, 13, FontStyle.Bold, available ? Accent : Muted));
            left.Add(Label(title, 22, FontStyle.Bold, available ? Color.white : Muted));
            left.Add(Label(detail, 14, FontStyle.Normal, Muted));
            top.Add(left);
            if (available)
            {
                var enter = Button("선택", () => _app.OpenLoadout(_app.CurrentStage), true);
                enter.style.width = 160;
                enter.style.height = 60;
                top.Add(enter);
            }
            else
            {
                var lockInfo = new VisualElement();
                lockInfo.style.alignItems = Align.FlexEnd;
                lockInfo.Add(Icon("lock", 32));
                lockInfo.Add(Label($"Stage {firstStage}", 13, FontStyle.Bold, Gold));
                top.Add(lockInfo);
            }
            card.Add(top);
            return card;
        }

        private static VisualElement PartInventoryCard(string title, string role, string rarity, Color rarityColor)
        {
            var card = Card();
            card.style.flexGrow = 1;
            card.style.marginRight = 6;
            card.style.minHeight = 220;
            Pad(card, 18, 18);
            card.Add(Chip(rarity, rarityColor));
            card.Add(Label(title, 21, FontStyle.Bold));
            card.Add(Label(role, 14, FontStyle.Normal, Muted));
            var preview = new VisualElement();
            preview.style.height = 78;
            preview.style.marginTop = 12;
            preview.style.backgroundColor = new Color(rarityColor.r * 0.12f, rarityColor.g * 0.12f, rarityColor.b * 0.12f, 1f);
            SetRadius(preview, 16);
            card.Add(preview);
            return card;
        }

        private static VisualElement ChallengeCard(string icon, string title, string detail, string tag)
        {
            var card = Card();
            card.style.marginTop = 12;
            Pad(card, 20, 18);
            var row = Row();
            row.style.alignItems = Align.Center;
            row.Add(Icon(icon, 56));
            var text = new VisualElement();
            text.style.marginLeft = 16;
            text.style.flexGrow = 1;
            text.Add(Label(title, 21, FontStyle.Bold));
            text.Add(Label(detail, 14, FontStyle.Normal, Muted));
            row.Add(text);
            row.Add(Chip(tag, Accent));
            card.Add(row);
            var lockRow = Row();
            lockRow.style.marginTop = 10;
            lockRow.style.alignItems = Align.Center;
            lockRow.Add(Icon("lock", 22));
            var lockText = Label("메인 작전 진행으로 개방", 13, FontStyle.Bold, Muted);
            lockText.style.marginLeft = 8;
            lockRow.Add(lockText);
            card.Add(lockRow);
            return card;
        }

        private static VisualElement GuildFeature(string icon, string title, string detail)
        {
            var card = Card();
            card.style.flexGrow = 1;
            card.style.marginRight = 6;
            card.style.minHeight = 190;
            card.style.alignItems = Align.Center;
            Pad(card, 16, 18);
            card.Add(Icon(icon, 50));
            card.Add(Label(title, 19, FontStyle.Bold));
            var detailLabel = Label(detail, 13, FontStyle.Normal, Muted);
            detailLabel.style.whiteSpace = WhiteSpace.Normal;
            detailLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            card.Add(detailLabel);
            return card;
        }
    }
}
