using System;
using UnityEngine;

namespace Wildfolk
{
    public sealed class Health : MonoBehaviour
    {
        public VitalityDefinition Definition;
        public int Current { get; private set; }
        public int Maximum => Definition.MaximumHealth;
        public bool IsAlive => Current > 0;
        public float Ratio => Maximum > 0 ? (float)Current / Maximum : 0;
        public event Action Changed;
        public event Action Damaged;
        public event Action Died;

        private void Awake()
        {
            if (Definition == null || Maximum <= 0) throw new InvalidOperationException("A valid vitality definition is required.");
            Current = Maximum;
        }

        public int ApplyDamage(int amount)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
            if (!IsAlive) return 0;
            var applied = Math.Min(Current, amount);
            Current -= applied;
            Changed?.Invoke();
            Damaged?.Invoke();
            if (!IsAlive) Died?.Invoke();
            return applied;
        }
    }
}
