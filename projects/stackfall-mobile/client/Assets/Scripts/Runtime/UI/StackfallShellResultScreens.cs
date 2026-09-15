using StackfallMobile.Runtime.Stage;
using UnityEngine;
using UnityEngine.UIElements;

namespace StackfallMobile.Runtime.UI
{
    public sealed partial class StackfallShellController
    {
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
            panel.Add(Icon("lock", 82));
            panel.Add(Label(section, 42, FontStyle.Bold));
            panel.Add(Label($"Stage {unlockStage} 클리어 후 개방", 25, FontStyle.Bold, Accent));
            var guide = Label("메인 스테이지를 진행해 새로운 함선 기능을 개방하세요.", 19, FontStyle.Normal, Muted);
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
            card.Add(Label(cleared ? "스테이지 클리어" : "기체 파괴", 52, FontStyle.Bold, cleared ? Accent : Danger));
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
            home.style.height = 78;
            home.style.marginTop = 12;
            card.Add(home);
            body.Add(card);
        }

        private void AddResultUnlock(VisualElement card, int stage, bool firstClear)
        {
            if (!firstClear || !StackfallContentCatalog.TryGetUnlockAtStage(stage, out var unlock))
            {
                card.Add(Label("작전 보상 · 크레딧 및 성장 데이터 확보", 18, FontStyle.Bold, Gold));
                return;
            }

            var unlockCard = Card(new Color(0.07f, 0.16f, 0.22f, 1f));
            unlockCard.style.width = Length.Percent(100f);
            unlockCard.style.marginTop = 24;
            unlockCard.style.alignItems = Align.Center;
            Pad(unlockCard, 24, 20);
            unlockCard.Add(Label("신규 능력 해금", 17, FontStyle.Bold, Gold));
            unlockCard.Add(Label(unlock.Name, 30, FontStyle.Bold, Accent));
            unlockCard.Add(Label($"{unlock.Category} 후보 덱에 추가되었습니다.", 17, FontStyle.Normal, Muted));
            card.Add(unlockCard);
        }
    }
}
