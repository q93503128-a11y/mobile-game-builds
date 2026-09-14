using System;
using StackfallMobile.Runtime.Enemies;
using StackfallMobile.Runtime.Player;
using StackfallMobile.Runtime.Progression;
using StackfallMobile.Runtime.Stage;
using UnityEngine;
using UnityEngine.UIElements;

namespace StackfallMobile.Runtime.UI
{
    public sealed class PlayerHudController : MonoBehaviour
    {
        private PlayerHealth _health;
        private PlayerProgression _progression;
        private EnemySpawner _spawner;
        private StageUpgradeDirector _upgrades;
        private StageSessionController _session;
        private ProgressBar _healthBar;
        private ProgressBar _experienceBar;
        private Label _levelLabel;
        private Label _timerLabel;
        private VisualElement _choiceOverlay;
        private VisualElement _resultOverlay;
        private Label _resultLabel;
        private Button[] _choiceButtons;
        private PanelSettings _panelSettings;

        public void Initialize(
            PlayerHealth health,
            PlayerProgression progression,
            EnemySpawner spawner,
            StageUpgradeDirector upgrades,
            StageSessionController session)
        {
            _health = health;
            _progression = progression;
            _spawner = spawner;
            _upgrades = upgrades;
            _session = session;

            BuildUi();
            _health.HealthChanged += OnHealthChanged;
            _progression.ExperienceChanged += OnExperienceChanged;
            _upgrades.ChoicesOffered += OnChoicesOffered;
            _session.StateChanged += OnSessionStateChanged;

            OnHealthChanged(_health.CurrentHealth, _health.MaxHealth);
            OnExperienceChanged(_progression.Level, _progression.Experience, _progression.ExperienceToNext);
        }

        private void OnDestroy()
        {
            if (_health != null) _health.HealthChanged -= OnHealthChanged;
            if (_progression != null) _progression.ExperienceChanged -= OnExperienceChanged;
            if (_upgrades != null) _upgrades.ChoicesOffered -= OnChoicesOffered;
            if (_session != null) _session.StateChanged -= OnSessionStateChanged;
            if (_panelSettings != null) Destroy(_panelSettings);
        }

        private void Update()
        {
            if (_timerLabel == null || _spawner == null)
            {
                return;
            }

            if (_spawner.BossSpawned)
            {
                _timerLabel.text = "보스 전투";
                return;
            }

            var remaining = Mathf.Max(0f, _spawner.BossSpawnTime - _spawner.Elapsed);
            var minutes = Mathf.FloorToInt(remaining / 60f);
            var seconds = Mathf.FloorToInt(remaining % 60f);
            _timerLabel.text = $"보스 {minutes:00}:{seconds:00}";
        }

        private void BuildUi()
        {
            _panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
            _panelSettings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            _panelSettings.referenceResolution = new Vector2Int(1080, 1920);

            var document = gameObject.AddComponent<UIDocument>();
            document.panelSettings = _panelSettings;
            document.sortingOrder = 100;

            var root = document.rootVisualElement;
            root.style.position = Position.Absolute;
            root.style.left = 0;
            root.style.right = 0;
            root.style.top = 0;
            root.style.bottom = 0;
            root.style.paddingLeft = 26;
            root.style.paddingRight = 26;
            root.style.paddingTop = 34;
            root.style.paddingBottom = 34;

            var top = new VisualElement();
            top.style.flexDirection = FlexDirection.Column;
            top.style.gap = 8;
            root.Add(top);

            var statusRow = new VisualElement();
            statusRow.style.flexDirection = FlexDirection.Row;
            statusRow.style.justifyContent = Justify.SpaceBetween;
            top.Add(statusRow);

            _levelLabel = MakeLabel("Lv.1", 28, FontStyle.Bold);
            _timerLabel = MakeLabel("보스 05:00", 28, FontStyle.Bold);
            statusRow.Add(_levelLabel);
            statusRow.Add(_timerLabel);

            _healthBar = MakeProgressBar("선체", 100f);
            _experienceBar = MakeProgressBar("동력", 100f);
            top.Add(_healthBar);
            top.Add(_experienceBar);

            BuildChoiceOverlay(root);
            BuildResultOverlay(root);
        }

        private void BuildChoiceOverlay(VisualElement root)
        {
            _choiceOverlay = new VisualElement();
            FillScreen(_choiceOverlay);
            _choiceOverlay.style.backgroundColor = new Color(0.015f, 0.02f, 0.05f, 0.88f);
            _choiceOverlay.style.alignItems = Align.Center;
            _choiceOverlay.style.justifyContent = Justify.Center;
            _choiceOverlay.style.display = DisplayStyle.None;
            root.Add(_choiceOverlay);

            var panel = new VisualElement();
            panel.style.width = Length.Percent(92f);
            panel.style.maxWidth = 860;
            panel.style.paddingLeft = 28;
            panel.style.paddingRight = 28;
            panel.style.paddingTop = 32;
            panel.style.paddingBottom = 32;
            panel.style.backgroundColor = new Color(0.06f, 0.08f, 0.15f, 0.98f);
            panel.style.borderTopLeftRadius = 26;
            panel.style.borderTopRightRadius = 26;
            panel.style.borderBottomLeftRadius = 26;
            panel.style.borderBottomRightRadius = 26;
            _choiceOverlay.Add(panel);

            var title = MakeLabel("강화 선택", 42, FontStyle.Bold);
            title.style.unityTextAlign = TextAnchor.MiddleCenter;
            title.style.marginBottom = 22;
            panel.Add(title);

            _choiceButtons = new Button[3];
            for (var i = 0; i < _choiceButtons.Length; i++)
            {
                var captured = i;
                var button = new Button(() => SelectChoice(captured));
                button.style.height = 150;
                button.style.marginTop = 8;
                button.style.marginBottom = 8;
                button.style.fontSize = 30;
                button.style.whiteSpace = WhiteSpace.Normal;
                button.style.unityTextAlign = TextAnchor.MiddleCenter;
                _choiceButtons[i] = button;
                panel.Add(button);
            }
        }

        private void BuildResultOverlay(VisualElement root)
        {
            _resultOverlay = new VisualElement();
            FillScreen(_resultOverlay);
            _resultOverlay.style.backgroundColor = new Color(0.015f, 0.02f, 0.05f, 0.9f);
            _resultOverlay.style.alignItems = Align.Center;
            _resultOverlay.style.justifyContent = Justify.Center;
            _resultOverlay.style.display = DisplayStyle.None;
            root.Add(_resultOverlay);

            _resultLabel = MakeLabel(string.Empty, 56, FontStyle.Bold);
            _resultLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            _resultOverlay.Add(_resultLabel);
        }

        private void OnHealthChanged(float current, float max)
        {
            _healthBar.highValue = Mathf.Max(1f, max);
            _healthBar.value = current;
            _healthBar.title = $"선체 {Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
        }

        private void OnExperienceChanged(int level, int current, int required)
        {
            _levelLabel.text = $"Lv.{level}";
            _experienceBar.highValue = Mathf.Max(1, required);
            _experienceBar.value = current;
            _experienceBar.title = $"동력 {current} / {required}";
        }

        private void OnChoicesOffered(System.Collections.Generic.IReadOnlyList<StageUpgradeId> choices)
        {
            for (var i = 0; i < _choiceButtons.Length; i++)
            {
                var visible = i < choices.Count;
                _choiceButtons[i].style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
                if (visible)
                {
                    _choiceButtons[i].text = ChoiceText(choices[i]);
                }
            }

            _choiceOverlay.style.display = DisplayStyle.Flex;
        }

        private void SelectChoice(int index)
        {
            if (_upgrades.Choose(index))
            {
                _choiceOverlay.style.display = DisplayStyle.None;
            }
        }

        private void OnSessionStateChanged(StageSessionState state)
        {
            _choiceOverlay.style.display = DisplayStyle.None;
            _resultLabel.text = state == StageSessionState.Cleared ? "스테이지 클리어" : "기체 파괴";
            _resultOverlay.style.display = DisplayStyle.Flex;
        }

        private static string ChoiceText(StageUpgradeId id)
        {
            return id switch
            {
                StageUpgradeId.CorePulse => "코어 펄스\n집중 사격 강화",
                StageUpgradeId.PulseBlade => "펄스 블레이드\n공전 절단 강화",
                StageUpgradeId.GravityWell => "중력 우물\n흡인장 강화",
                _ => throw new ArgumentOutOfRangeException(nameof(id), id, null)
            };
        }

        private static ProgressBar MakeProgressBar(string title, float highValue)
        {
            var bar = new ProgressBar
            {
                title = title,
                lowValue = 0f,
                highValue = highValue,
                value = highValue
            };
            bar.style.height = 30;
            bar.style.fontSize = 18;
            return bar;
        }

        private static Label MakeLabel(string text, int fontSize, FontStyle fontStyle)
        {
            var label = new Label(text);
            label.style.fontSize = fontSize;
            label.style.unityFontStyleAndWeight = fontStyle;
            label.style.color = Color.white;
            return label;
        }

        private static void FillScreen(VisualElement element)
        {
            element.style.position = Position.Absolute;
            element.style.left = -26;
            element.style.right = -26;
            element.style.top = -34;
            element.style.bottom = -34;
        }
    }
}
