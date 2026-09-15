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
                var startIndex = rowIndex == 0 ? 0 : 4;
                var endIndex = rowIndex == 0 ? 4 : 7;
                for (var i = startIndex; i < endIndex; i++)
                {
                    row.Add(AttendanceCard(rewards[i].Item1, rewards[i].Item2, rewards[i].Item3, i == 0, i == 0 && _state.AttendanceClaimed));
                }
                body.Add(row);
            }

            var claim = Button(_state.AttendanceClaimed ? "오늘 보급 수령 완료" : "오늘 보급 수령", () =>
            {
                if (_state.AttendanceClaimed)
                {
                    return;
                }
                _state.AttendanceClaimed = true;
                ShowModal(root, "출석 보급 수령", "DAY 1 항해 보급을 수령했습니다.", "확인", ShowAttendance, "gift", Success);
            }, !_state.AttendanceClaimed);
            claim.style.height = 72;
            claim.style.marginTop = 16;
            claim.SetEnabled(!_state.AttendanceClaimed);
            body.Add(claim);

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
            var action = Button("작전 현황", () => ShowModal(root, "심우주 회수 작전", "회수 신호 추적률 28% · 메인 작전 진행과 함께 이벤트 진척도가 상승합니다.", null, null, "news", Accent), true);
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

        public void ShowRecovery()
        {
            var root = BeginScreen();
            AddTopStatus(root);
            var body = AddBody(root, true);

            var heading = Row();
            heading.style.justifyContent = Justify.SpaceBetween;
            heading.style.alignItems = Align.Center;
            var title = new VisualElement();
            title.Add(Label("회수", 42, FontStyle.Bold));
            title.Add(Label("SALVAGE NETWORK · 부품 신호 탐지", 15, FontStyle.Bold, Muted));
            heading.Add(title);
            heading.Add(Chip("S 1.50%", Gold));
            body.Add(heading);

            var standard = Card(new Color(0.035f, 0.085f, 0.135f, 1f));
            standard.style.marginTop = 18;
            Pad(standard, 24, 22);
            standard.Add(Label("STANDARD SALVAGE", 13, FontStyle.Bold, Accent));
            standard.Add(Label("표준 회수", 28, FontStyle.Bold));
            standard.Add(Label("상시 일반~영웅 부품과 상시 S 프로토타입을 폭넓게 회수합니다.", 15, FontStyle.Normal, Muted));
            var standardInfo = Row();
            standardInfo.style.marginTop = 12;
            standardInfo.Add(Chip("S 1.5%", Gold));
            standardInfo.Add(Chip("50회 S 확정", Accent));
            standard.Add(standardInfo);
            var standardButton = Button("표준 회수", () => ShowModal(root, "회수 키 부족", "표준 회수에 필요한 회수 키가 없습니다.", "상점 보기", _app.ShowStore, "pouch", Gold), false);
            standardButton.style.height = 68;
            standardButton.style.marginTop = 16;
            standard.Add(standardButton);
            body.Add(standard);

            var resonance = Card(new Color(0.075f, 0.055f, 0.17f, 1f));
            resonance.style.marginTop = 14;
            Pad(resonance, 24, 22);
            resonance.Add(Label("PROTOTYPE RESONANCE", 13, FontStyle.Bold, Gold));
            resonance.Add(Label("프로토타입 공명", 28, FontStyle.Bold));
            resonance.Add(Label("대표 S 세트 60% · 픽업 실패 후 다음 S 대표 세트 확정", 15, FontStyle.Normal, Muted));
            var resonanceButton = Button("공명 배너 보기", _app.ShowSummon, true);
            resonanceButton.style.height = 70;
            resonanceButton.style.marginTop = 16;
            resonance.Add(resonanceButton);
            body.Add(resonance);

            var tuning = Card();
            tuning.style.marginTop = 14;
            Pad(tuning, 22, 20);
            var tuningHead = Row();
            tuningHead.style.justifyContent = Justify.SpaceBetween;
            var tuningText = new VisualElement();
            tuningText.Add(Label("목표 부품 조율", 23, FontStyle.Bold));
            tuningText.Add(Label("대표 세트의 6부품 중 목표 부품을 지정", 14, FontStyle.Normal, Muted));
            tuningHead.Add(tuningText);
            tuningHead.Add(Chip("조율 데이터 0", Accent));
            tuning.Add(tuningHead);
            tuning.Add(Label("대표 세트 S 등장 시 목표 부품 50% · 목표 실패 S 1회마다 조율 데이터 1", 14, FontStyle.Normal, Muted));
            var tuneButton = Button("조율 규칙", () => ShowModal(root, "목표 부품 조율", "대표 세트의 목표 부품을 정하고, 목표 부품을 얻지 못한 S 획득마다 조율 데이터를 모읍니다. 조율 데이터 3개로 현재 대표 세트의 목표 부품을 확정 교환할 수 있습니다.", null, null, "news", Accent), false);
            tuneButton.style.height = 60;
            tuneButton.style.marginTop = 14;
            tuning.Add(tuneButton);
            body.Add(tuning);

            var pity = Card(PanelBright);
            pity.style.marginTop = 14;
            Pad(pity, 22, 18);
            var pityHead = Row();
            pityHead.style.justifyContent = Justify.SpaceBetween;
            pityHead.Add(Label("프로토타입 공명 S 천장", 18, FontStyle.Bold));
            pityHead.Add(Label("0 / 50", 18, FontStyle.Bold, Gold));
            pity.Add(pityHead);
            pity.Add(ProgressBar(0f, Gold));
            pity.Add(Label("공명 계열 배너가 교체되어도 S 천장과 대표 세트 보장 상태가 이어집니다.", 14, FontStyle.Normal, Muted));
            body.Add(pity);

            AddBottomNavigation(root, "회수");
        }

        public void ShowPartInventory()
        {
            ShowPartInventoryTab(_state.PartTab, _state.SelectedPartIndex);
        }

        private void ShowPartInventoryTab(int selectedTab, int selectedPart)
        {
            _state.PartTab = Mathf.Clamp(selectedTab, 0, 5);
            var options = PartOptions(_state.PartTab);
            _state.SelectedPartIndex = Mathf.Clamp(selectedPart, 0, options.Length - 1);

            var root = BeginScreen();
            AddCompactHeader(root, "부품 보관함", _app.ShowShip);
            var body = AddBody(root, true);

            var tabs = Row();
            tabs.Add(SelectableTab("CORE", _state.PartTab == 0, () => SelectPartTab(0)));
            tabs.Add(SelectableTab("FRAME", _state.PartTab == 1, () => SelectPartTab(1)));
            tabs.Add(SelectableTab("DRIVE", _state.PartTab == 2, () => SelectPartTab(2)));
            body.Add(tabs);
            var tabs2 = Row();
            tabs2.style.marginTop = 8;
            tabs2.Add(SelectableTab("IMPACTOR", _state.PartTab == 3, () => SelectPartTab(3)));
            tabs2.Add(SelectableTab("ORBITER", _state.PartTab == 4, () => SelectPartTab(4)));
            tabs2.Add(SelectableTab("REACTOR", _state.PartTab == 5, () => SelectPartTab(5)));
            body.Add(tabs2);

            var equipped = Card(PanelBright);
            equipped.style.marginTop = 16;
            Pad(equipped, 22, 18);
            equipped.Add(Label("현재 장착", 15, FontStyle.Bold, Accent));
            equipped.Add(Label(_state.EquippedParts[_state.PartTab], 25, FontStyle.Bold));
            equipped.Add(Label("슬롯 강화 Lv.1 · 강화 레벨 공유", 15, FontStyle.Normal, Muted));
            body.Add(equipped);

            for (var rowIndex = 0; rowIndex < 2; rowIndex++)
            {
                var row = Row();
                row.style.marginTop = rowIndex == 0 ? 14 : 12;
                var first = rowIndex * 2;
                for (var i = first; i < Mathf.Min(first + 2, options.Length); i++)
                {
                    var index = i;
                    var option = options[i];
                    row.Add(PartInventoryCard(option.Name, option.Role, option.Rarity, option.Color, i == _state.SelectedPartIndex, () =>
                    {
                        _state.SelectedPartIndex = index;
                        ShowPartInventory();
                    }));
                }
                body.Add(row);
            }

            var selected = options[_state.SelectedPartIndex];
            var detail = Card(new Color(0.04f, 0.075f, 0.13f, 1f));
            detail.style.marginTop = 16;
            Pad(detail, 22, 20);
            var detailHead = Row();
            detailHead.style.justifyContent = Justify.SpaceBetween;
            var detailText = new VisualElement();
            detailText.Add(Label(selected.Name, 24, FontStyle.Bold));
            detailText.Add(Label(selected.Role, 15, FontStyle.Normal, Muted));
            detailHead.Add(detailText);
            detailHead.Add(Chip(selected.Rarity, selected.Color));
            detail.Add(detailHead);
            var isEquipped = _state.EquippedParts[_state.PartTab] == selected.Name;
            var equip = Button(isEquipped ? "장착 중" : "장착", () => EquipSelectedPart(root, selected.Name), !isEquipped);
            equip.style.height = 68;
            equip.style.marginTop = 14;
            equip.SetEnabled(!isEquipped);
            detail.Add(equip);
            body.Add(detail);

            var note = Card();
            note.style.marginTop = 16;
            Pad(note, 20, 18);
            note.Add(Label("슬롯 강화 공유", 20, FontStyle.Bold));
            note.Add(Label("새 부품으로 교체해도 해당 슬롯의 강화 레벨은 유지됩니다.", 15, FontStyle.Normal, Muted));
            body.Add(note);
        }

        private void SelectPartTab(int tab)
        {
            _state.PartTab = tab;
            _state.SelectedPartIndex = 0;
            ShowPartInventory();
        }

        private void EquipSelectedPart(VisualElement root, string partName)
        {
            _state.EquippedParts[_state.PartTab] = partName;
            ShowModal(root, "부품 장착", $"{partName} 장착이 완료되었습니다.", "확인", ShowPartInventory, "market", Success);
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

        private static VisualElement AttendanceCard(string day, string reward, string icon, bool today, bool claimed)
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
            var rewardLabel = Label(reward, 14, FontStyle.Bold, claimed ? Muted : Color.white);
            rewardLabel.style.whiteSpace = WhiteSpace.Normal;
            rewardLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            card.Add(rewardLabel);
            if (today)
            {
                card.Add(Chip(claimed ? "수령 완료" : "오늘", claimed ? Muted : Success));
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

        private static VisualElement PartInventoryCard(
            string title,
            string role,
            string rarity,
            Color rarityColor,
            bool selected,
            System.Action click)
        {
            var card = Card(selected ? PanelBright : Panel);
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
            card.AddManipulator(new Clickable(click ?? (() => { })));
            return card;
        }

        private static (string Name, string Role, string Rarity, Color Color)[] PartOptions(int slot)
        {
            var s = Gold;
            var epic = new Color(0.75f, 0.45f, 1f, 1f);
            var rare = new Color(0.3f, 0.65f, 1f, 1f);
            switch (slot)
            {
                case 0:
                    return new[]
                    {
                        ("펄서 코어", "기본 출력 계통", "일반", Accent),
                        ("레드라인 코어", "과열 · 연속 공격", "S 프로토타입", s),
                        ("특이점 코어", "중력 · 질량 표식", "S 프로토타입", s),
                        ("이지스 코어", "보호막 · 생존", "S 프로토타입", s)
                    };
                case 1:
                    return new[]
                    {
                        ("바스티온 프레임", "기본 방어 계통", "일반", Accent),
                        ("방열 프레임", "열 축적 제어", "S 프로토타입", s),
                        ("고밀도 프레임", "질량 · 내구", "S 프로토타입", s),
                        ("적층 프레임", "비상 장갑", "S 프로토타입", s)
                    };
                case 2:
                    return new[]
                    {
                        ("벡터 드라이브", "기본 기동 계통", "희귀", rare),
                        ("슬링 드라이브", "중력 기동", "S 프로토타입", s),
                        ("앵커 드라이브", "위치 고정 · 생존", "S 프로토타입", s),
                        ("블링크 드라이브", "위상 이동", "S 프로토타입", s)
                    };
                case 3:
                    return new[]
                    {
                        ("절단 엣지", "기본 충돌 계통", "영웅", epic),
                        ("램제트 임팩터", "고속 충돌", "S 프로토타입", s),
                        ("사건지평 임팩터", "중력 붕괴", "S 프로토타입", s),
                        ("보복 임팩터", "반격 · 생존", "S 프로토타입", s)
                    };
                case 4:
                    return new[]
                    {
                        ("정찰 오비터", "기본 보조 사격", "희귀", rare),
                        ("펄스 오비터", "집중 표적 보조", "S 프로토타입", s),
                        ("조석 오비터", "조석선 · 범위 제어", "S 프로토타입", s),
                        ("인터셉터 오비터", "요격 · 방어", "S 프로토타입", s)
                    };
                default:
                    return new[]
                    {
                        ("가속 리액터", "기본 에너지 계통", "영웅", epic),
                        ("레드라인 리액터", "과열 · 출력", "S 프로토타입", s),
                        ("붕괴 리액터", "중력 반응", "S 프로토타입", s),
                        ("백업 리액터", "비상 회복", "S 프로토타입", s)
                    };
            }
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
