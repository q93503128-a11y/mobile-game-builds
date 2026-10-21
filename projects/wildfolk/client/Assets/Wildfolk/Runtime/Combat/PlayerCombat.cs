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
        public bool CreatureTargetsOnly;
        public bool NetworkTargetsOnly;
        public int HandIndex=>handIndex;
        public event System.Action AttackStarted;
        public bool IsAttacking { get; private set; }
        private readonly Collider2D[] candidates = new Collider2D[32];
        private readonly RaycastHit2D[] obstacles=new RaycastHit2D[32];
        private readonly Health[] targets=new Health[32];
        private readonly Collider2D[] targetColliders=new Collider2D[32];
        private readonly float[] scores=new float[32];
        private int targetCount;
        private ResourceNode denied;private float deniedScore;
        public event System.Action<ResourceDefinition> HarvestDenied;
        private AttackProfile profile;
        private Vector2 strikeFacing;
        public int LastHitCount {get;private set;}
        public int LastDamage {get;private set;}
        private ToolDefinition attackTool;
        private float started;
        private float nextAttack;
        private float duration;
        private float reach;
        private bool hitApplied;
        private int handIndex = 1;
        private Collider2D ownCollider;
        private void Awake()=>ownCollider=GetComponent<Collider2D>();

        public bool TryAttack()
        {
            if (!SimulationAuthority.Allows(this) || !Session.IsRunning || !OwnHealth.IsAlive || IsAttacking || Time.time < nextAttack) return false;
            if(Equipment.SelectedItem!=null && (Equipment.SelectedItem.ExplorationUtility!=null || Equipment.SelectedItem.Ranged!=null || Equipment.SelectedItem.FishingRod) || GetComponent<PlayerRanged>()!=null && GetComponent<PlayerRanged>().IsBusy)return false;
            attackTool = Equipment.ActiveTool;
            duration = attackTool == null ? Settings.PunchDuration : attackTool.Duration;
            profile=attackTool==null ? Settings.PunchAttack : attackTool.Attack;
            reach=profile.Range;strikeFacing=Avatar.Facing.Direction;LastHitCount=LastDamage=0;
            FindTarget();
            if(targetCount==0 && denied!=null){HarvestDenied?.Invoke(denied.Definition);if(GetComponent<NetworkPlayer>()==null)PlayerFeedback.ShowRequirement(denied.Definition);}
            if(targetCount>0)Avatar.Facing.Face((Vector2)targets[0].transform.position-(Vector2)transform.position);
            handIndex = attackTool == null ? 1 - handIndex : 1;
            started = Time.time;
            nextAttack = started + (attackTool == null ? Settings.PunchCooldown : attackTool.Cooldown);
            hitApplied = false;
            IsAttacking = true;
            AttackStarted?.Invoke();
            OwnHealth.MarkCombat();
            return true;
        }

        private bool InSector(Collider2D collider,out float distance,out float alignment)
        {
            var origin=(Vector2)transform.position;
            var delta=(Vector2)collider.bounds.center-origin;
            distance=Vector2.Distance(origin,collider.ClosestPoint(origin));
            alignment=delta.sqrMagnitude<.0001f ? 1 : Vector2.Dot(strikeFacing,delta.normalized);
            var contact=ownCollider!=null && ownCollider.Distance(collider).distance<=.12f;
            return distance<=reach+.001f && (contact || alignment>=Mathf.Cos(profile.ArcAngle*.5f*Mathf.Deg2Rad)) && ClearPath(collider);
        }
        private bool ClearPath(Collider2D target)
        {
            var origin=(Vector2)transform.position;
            var delta=target.ClosestPoint(origin)-origin;
            if(delta.sqrMagnitude<.000001f)return true;
            var count=Physics2D.Raycast(origin,delta.normalized,new ContactFilter2D{useTriggers=false},obstacles,delta.magnitude);
            if(count==obstacles.Length)return false;
            var health=target.GetComponentInParent<Health>();
            for(var i=0;i<count;i++)
            {
                var owner=obstacles[i].collider.GetComponentInParent<Health>();
                if(owner!=OwnHealth && owner!=health)return false;
            }
            return true;
        }
        private void FindTarget()
        {
            targetCount=0;denied=null;deniedScore=float.MaxValue;
            var count=Physics2D.OverlapCircle(transform.position,reach,new ContactFilter2D{useTriggers=true},candidates);
            for(var i=0;i<count;i++)
            {
                var health=candidates[i].GetComponentInParent<Health>();
                if(NetworkTargetsOnly && (health==null || health.GetComponent<NetworkCreature>()==null && health.GetComponent<NetworkResource>()==null))continue;
                if(CreatureTargetsOnly && (health==null || health.GetComponent<NetworkCreature>()==null))continue;
                if(!CombatAffiliation.CanDamage(OwnHealth,health) || !InSector(candidates[i],out var distance,out var alignment))continue;
                var node=health.GetComponent<ResourceNode>();
                if(attackTool!=null && attackTool.HarvestMethod!=HarvestMethod.Strike && node==null)continue;
                if(node!=null && !node.Definition.CanHarvest(attackTool)){var rejected=distance/reach+(1-alignment)*.75f;if(rejected<deniedScore){denied=node;deniedScore=rejected;}continue;}
                var duplicate=false;for(var j=0;j<targetCount;j++)if(targets[j]==health)duplicate=true;
                if(duplicate)continue;
                var score=distance/reach+(profile.SelectionRule==TargetSelectionRule.Nearest ? 0 : (1-alignment)*.75f);
                var at=0;while(at<targetCount && scores[at]<=score)at++;
                var limit=Mathf.Min(profile.MaxTargets,targets.Length);if(at>=limit)continue;
                var last=Mathf.Min(targetCount,limit-1);
                for(var j=last;j>at;j--){targets[j]=targets[j-1];targetColliders[j]=targetColliders[j-1];scores[j]=scores[j-1];}
                targets[at]=health;targetColliders[at]=candidates[i];scores[at]=score;targetCount=Mathf.Min(targetCount+1,limit);
            }
        }

        private void Update()
        {
            if (!IsAttacking) return;
            if (!Session.IsRunning) { CancelAttack(); return; }
            var progress = Mathf.Clamp01((Time.time - started) / duration);
            Present(progress,attackTool,handIndex);
            if (!hitApplied && progress >= .5f)
            {
                hitApplied = true;
                for(var i=0;i<targetCount;i++)
                {
                    var target=targets[i];var collider=targetColliders[i];
                    if(collider==null || !collider.enabled || !CombatAffiliation.CanDamage(OwnHealth,target) || !InSector(collider,out _,out _))continue;
                    var node=target.GetComponent<ResourceNode>();
                    var damage=attackTool==null ? Settings.PunchDamage : node==null ? attackTool.Damage : attackTool.DamageFor(node.Definition.Kind);
                    var applied=node!=null ? node.Harvest(attackTool,damage,Equipment.Inventory) : target.ApplyDamage(damage,false,Equipment.Inventory);
                    LastDamage+=applied;if(applied>0)LastHitCount++;
                }
            }
            if (progress >= 1) CancelAttack();
        }

        public void Present(float progress,ToolDefinition tool,int hand)
        {
            var extension = Mathf.Sin(progress * Mathf.PI);
            if(tool!=null && tool.HarvestMethod==HarvestMethod.Panning)
            {
                var sway=Mathf.Sin(progress*Mathf.PI*6)*.1f*extension;
                Avatar.Facing.SetHandPose(1,new Vector2(sway,.32f*extension),Mathf.Sin(progress*Mathf.PI*6)*8);return;
            }
            if (tool == null)
            {
                var towardCenter = (hand == 0 ? 1 : -1) * Avatar.Facing.HandSeparation * .55f * extension;
                Avatar.Facing.SetHandPose(hand, new Vector2(towardCenter, extension * Settings.PunchExtension));
            }
            else
            {
                var arc = tool.Kind==ToolKind.Spear ? 0 : -Mathf.Sin(progress * Mathf.PI * 2) * tool.SwingArc;
                var inward=tool.Kind==ToolKind.Spear ? Avatar.Facing.HandSeparation : .2f;
                Avatar.Facing.SetHandPose(1, new Vector2(-inward * extension, tool.HandExtension * extension), arc);
            }
        }

        public void CancelAttack()
        {
            IsAttacking = false;
            targetCount=0;
            if (Avatar != null && Avatar.Facing != null) Avatar.Facing.ResetPose();
        }
        private void OnDisable() => CancelAttack();
    }
}
