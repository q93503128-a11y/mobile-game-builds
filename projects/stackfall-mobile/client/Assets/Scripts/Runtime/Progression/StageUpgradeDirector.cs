using System;
using System.Collections.Generic;
using StackfallMobile.Runtime.Combat.Weapons;
using UnityEngine;

namespace StackfallMobile.Runtime.Progression
{
    public enum StageUpgradeId
    {
        CorePulse,
        PulseBlade,
        GravityWell
    }

    public sealed class StageUpgradeDirector : MonoBehaviour
    {
        private readonly List<StageUpgradeId> _available = new(3);
        private readonly List<StageUpgradeId> _currentChoices = new(3);

        private PlayerProgression _progression;
        private CorePulseWeapon _corePulse;
        private PulseBladeWeapon _pulseBlade;
        private GravityWellWeapon _gravityWell;
        private bool _automaticChoice;
        private int _pendingLevelGains;
        private int _choiceSequence;

        public IReadOnlyList<StageUpgradeId> CurrentChoices => _currentChoices;
        public bool HasPendingChoice => _currentChoices.Count > 0;

        public event Action<IReadOnlyList<StageUpgradeId>> ChoicesOffered;
        public event Action<StageUpgradeId> UpgradeApplied;

        public void Initialize(
            PlayerProgression progression,
            CorePulseWeapon corePulse,
            PulseBladeWeapon pulseBlade,
            GravityWellWeapon gravityWell,
            bool automaticChoice)
        {
            _progression = progression;
            _corePulse = corePulse;
            _pulseBlade = pulseBlade;
            _gravityWell = gravityWell;
            _automaticChoice = automaticChoice;
            _progression.LevelGained += OnLevelGained;
        }

        private void OnDestroy()
        {
            if (_progression != null)
            {
                _progression.LevelGained -= OnLevelGained;
            }
        }

        public bool Choose(int index)
        {
            if (index < 0 || index >= _currentChoices.Count)
            {
                return false;
            }

            var choice = _currentChoices[index];
            _currentChoices.Clear();
            Apply(choice);

            if (_pendingLevelGains > 0)
            {
                _pendingLevelGains--;
                OfferNextChoice();
            }
            else
            {
                Time.timeScale = 1f;
            }

            return true;
        }

        private void OnLevelGained(int level)
        {
            if (HasPendingChoice)
            {
                _pendingLevelGains++;
                return;
            }

            _choiceSequence = Mathf.Max(_choiceSequence, level - 1);
            OfferNextChoice();
        }

        private void OfferNextChoice()
        {
            BuildChoices(++_choiceSequence);
            if (_currentChoices.Count == 0)
            {
                Time.timeScale = 1f;
                return;
            }

            if (_automaticChoice)
            {
                Choose((_choiceSequence - 1) % _currentChoices.Count);
                return;
            }

            Time.timeScale = 0f;
            ChoicesOffered?.Invoke(_currentChoices);
        }

        private void BuildChoices(int sequence)
        {
            _available.Clear();
            _currentChoices.Clear();

            if (_corePulse.Rank < 6) _available.Add(StageUpgradeId.CorePulse);
            if (_pulseBlade.Rank < 6) _available.Add(StageUpgradeId.PulseBlade);
            if (_gravityWell.Rank < 6) _available.Add(StageUpgradeId.GravityWell);

            if (_available.Count == 0)
            {
                return;
            }

            var offset = Mathf.Abs(sequence * 7) % _available.Count;
            for (var i = 0; i < Mathf.Min(3, _available.Count); i++)
            {
                _currentChoices.Add(_available[(offset + i) % _available.Count]);
            }
        }

        private void Apply(StageUpgradeId id)
        {
            switch (id)
            {
                case StageUpgradeId.CorePulse:
                    _corePulse.IncreaseRank();
                    break;
                case StageUpgradeId.PulseBlade:
                    _pulseBlade.IncreaseRank();
                    break;
                case StageUpgradeId.GravityWell:
                    _gravityWell.IncreaseRank();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(id), id, null);
            }

            UpgradeApplied?.Invoke(id);
        }
    }
}
