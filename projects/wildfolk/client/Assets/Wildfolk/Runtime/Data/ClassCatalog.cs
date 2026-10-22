using System;
using UnityEngine;
namespace Wildfolk
{
    public enum ClassTrait { None,Survival,Guard,Quarry,Medicine,Exploration,Engineering }
    public enum ClassEffect { SurvivalHealth, SurvivalStamina, SurvivalCooldown, LowRecovery, LowMedicine, LowPenalty, Nutrition, MealDuration, HomeDrain, GuardDrain, GuardMove, GuardHeavy, GuardInterval, CounterDamage, CounterStamina, GuardForward, GuardRecovery, GuardAngle, QuarryDuration, QuarryDetail, QuarrySprint, BowRange, SteadyShot, FinishShot, SpearReach, ChaseSpear, LargeSpear, HealingOut, MedicineCooldown, HotSpeed, ReviveSpeed, ReviveHealth, ReviveProtection, Stabilize, GoldenTime, EmergencyHeal, OpticRange, PoiReading, MarkerDuration, SurveyRange, SurveyDetail, SurveyCooldown, SignalRange, SignalDuration, SignalMeaning, RepairSpeed, Overrepair, RepairLock, OverclockDuration, OverclockCooldown, OverclockPower, BarrierBuffer, TrapService, TurretService }
    [Serializable]public sealed class ClassNode
    {
        public string Id,Prerequisite,NameKO,NameEN,DescriptionKO,DescriptionEN,BranchKO,BranchEN;
        public ClassEffect Effect;public float Amount;
        public string Name=>L.Language=="ko"?NameKO:NameEN;
        public string Description=>L.Language=="ko"?DescriptionKO:DescriptionEN;
        public string Branch=>L.Language=="ko"?BranchKO:BranchEN;
    }
    [Serializable]public sealed class ClassDefinition
    {
        public string Id,NameKO,NameEN,RoleKO,RoleEN,DescriptionKO,DescriptionEN,Currency;
        public int Price;public ClassTrait Trait;public ClassNode[] Nodes;
        public string Name=>L.Language=="ko"?NameKO:NameEN;
        public string Role=>L.Language=="ko"?RoleKO:RoleEN;
        public string Description=>L.Language=="ko"?DescriptionKO:DescriptionEN;
        public bool Active=>Trait==ClassTrait.Guard || Trait==ClassTrait.Engineering;
    }
    [Serializable]public sealed class ClassCatalog
    {
        public long[] Milestones;public ClassDefinition[] Classes;
        private static ClassCatalog current;
        public static ClassCatalog Current=>current??(current=JsonUtility.FromJson<ClassCatalog>(Resources.Load<TextAsset>("Classes").text));
        public ClassDefinition Find(string id)=>Array.Find(Classes,c=>c.Id==id);
        public int Earned(long xp){var n=0;foreach(var milestone in Milestones)if(xp>=milestone)n++;return n;}
    }
}
