using UnityEngine;

namespace Wildfolk
{
    public sealed class FistCombat : MonoBehaviour
    {
        public GameplaySettings Settings;
        public GameplaySession Session;
        public PlayerAvatar Avatar;
        public Health OwnHealth;
        public bool IsAttacking { get; private set; }
        private readonly Collider2D[] candidates = new Collider2D[32];
        private Transform[] hands;
        private Vector3[] rest;
        private Health target;
        private Collider2D targetCollider;
        private float started;
        private float nextAttack;
        private bool hitApplied;
        private int handIndex = 1;

        private void Awake()
        {
            hands = new[] { Avatar.LeftHand, Avatar.RightHand };
            rest = new[] { hands[0].localPosition, hands[1].localPosition };
        }

        public bool TryAttack()
        {
            if (!Session.IsRunning || !OwnHealth.IsAlive || IsAttacking || Time.time < nextAttack) return false;
            FindTarget();
            if (target != null) Avatar.Face((Vector2)target.transform.position - (Vector2)transform.position);
            handIndex = 1 - handIndex;
            started = Time.time;
            nextAttack = started + Settings.PunchCooldown;
            hitApplied = false;
            IsAttacking = true;
            return true;
        }

        private void FindTarget()
        {
            target = null;
            targetCollider = null;
            var filter = new ContactFilter2D { useTriggers = true };
            var count = Physics2D.OverlapCircle(transform.position, Settings.PunchReach, filter, candidates);
            var best = float.MaxValue;
            for (var i = 0; i < count; i++)
            {
                var health = candidates[i].GetComponentInParent<Health>();
                if (health == null || health == OwnHealth || !health.IsAlive) continue;
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
            var progress = Mathf.Clamp01((Time.time - started) / Settings.PunchDuration);
            var extension = Mathf.Sin(progress * Mathf.PI);
            var pose = rest[handIndex];
            hands[handIndex].localPosition = new Vector3(pose.x * (1 - extension * .55f), pose.y - extension * Settings.PunchExtension, pose.z);
            if (!hitApplied && progress >= .5f)
            {
                hitApplied = true;
                if (target != null && target.IsAlive && targetCollider != null &&
                    Vector2.Distance(transform.position, targetCollider.ClosestPoint(transform.position)) <= Settings.PunchReach)
                    target.ApplyDamage(Settings.PunchDamage);
            }
            if (progress < 1) return;
            hands[handIndex].localPosition = rest[handIndex];
            IsAttacking = false;
            target = null;
            targetCollider = null;
        }
    }
}
