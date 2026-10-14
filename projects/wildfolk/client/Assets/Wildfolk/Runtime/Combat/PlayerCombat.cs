using UnityEngine;

namespace Wildfolk
{
    public sealed class PlayerCombat : MonoBehaviour
    {
        public GameplaySettings Settings;
        public GameplaySession Session;
        public PlayerAvatar Avatar;
        public PlayerEquipment Equipment;
        public Health OwnHealth;
        public bool IsAttacking { get; private set; }
        private readonly Collider2D[] candidates = new Collider2D[32];
        private Health target;
        private Collider2D targetCollider;
        private ToolDefinition attackTool;
        private float started;
        private float nextAttack;
        private float duration;
        private float reach;
        private bool hitApplied;
        private int handIndex = 1;

        public bool TryAttack()
        {
            if (!Session.IsRunning || !OwnHealth.IsAlive || IsAttacking || Time.time < nextAttack) return false;
            attackTool = Equipment.ActiveTool;
            duration = attackTool == null ? Settings.PunchDuration : attackTool.Duration;
            reach = attackTool == null ? Settings.PunchReach : attackTool.Reach;
            FindTarget();
            if (target != null) Avatar.Facing.Face((Vector2)target.transform.position - (Vector2)transform.position);
            handIndex = attackTool == null ? 1 - handIndex : 1;
            started = Time.time;
            nextAttack = started + (attackTool == null ? Settings.PunchCooldown : attackTool.Cooldown);
            hitApplied = false;
            IsAttacking = true;
            OwnHealth.MarkCombat();
            return true;
        }

        private void FindTarget()
        {
            target = null;
            targetCollider = null;
            var filter = new ContactFilter2D { useTriggers = true };
            var count = Physics2D.OverlapCircle(transform.position, reach, filter, candidates);
            var best = float.MaxValue;
            for (var i = 0; i < count; i++)
            {
                var health = candidates[i].GetComponentInParent<Health>();
                if (health == null || health == OwnHealth || !health.IsAlive) continue;
                if(health.GetComponent<WorldPlaceable>()!=null)continue;
                var distance = ((Vector2)transform.position - candidates[i].ClosestPoint(transform.position)).sqrMagnitude;
                if (distance >= best) continue;
                best = distance;
                target = health;
                targetCollider = candidates[i];
            }
        }

        private void Update()
        {
            if (!IsAttacking) return;
            if (!Session.IsRunning) { CancelAttack(); return; }
            var progress = Mathf.Clamp01((Time.time - started) / duration);
            var extension = Mathf.Sin(progress * Mathf.PI);
            if (attackTool == null)
            {
                var towardCenter = (handIndex == 0 ? 1 : -1) * Avatar.Facing.HandSeparation * .55f * extension;
                Avatar.Facing.SetHandPose(handIndex, new Vector2(towardCenter, extension * Settings.PunchExtension));
            }
            else
            {
                var arc = -Mathf.Sin(progress * Mathf.PI * 2) * attackTool.SwingArc;
                Avatar.Facing.SetHandPose(1, new Vector2(-.2f * extension, attackTool.HandExtension * extension), arc);
            }
            if (!hitApplied && progress >= .5f)
            {
                hitApplied = true;
                if (target != null && target.IsAlive && targetCollider != null &&
                    Vector2.Distance(transform.position, targetCollider.ClosestPoint(transform.position)) <= reach)
                {
                    var damage = attackTool == null ? Settings.PunchDamage : attackTool.Damage;
                    var node = target.GetComponent<ResourceNode>();
                    if (attackTool != null && node != null) damage = attackTool.DamageFor(node.Definition.Kind);
                    if(node==null || node.Definition.CanHarvest(attackTool)) target.ApplyDamage(damage,false,Equipment.Inventory);
                }
            }
            if (progress >= 1) CancelAttack();
        }

        public void CancelAttack()
        {
            IsAttacking = false;
            target = null;
            targetCollider = null;
            if (Avatar != null && Avatar.Facing != null) Avatar.Facing.ResetPose();
        }
        private void OnDisable() => CancelAttack();
    }
}
