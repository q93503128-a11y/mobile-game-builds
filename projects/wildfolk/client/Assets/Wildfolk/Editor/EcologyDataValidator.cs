using System.Collections.Generic;
using UnityEditor;
namespace Wildfolk.Editor
{
    internal static class EcologyDataValidator
    {
        internal static void Validate(List<string> errors)
        {
            var ids=new HashSet<string>();foreach(var guid in AssetDatabase.FindAssets("t:CreatureDefinition",new[]{"Assets/Wildfolk/Data"}))
            {
                var d=AssetDatabase.LoadAssetAtPath<CreatureDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if(string.IsNullOrEmpty(d.Id) || !ids.Add(d.Id) || !L.Has("creature."+d.Id+".name") || d.SeparationRadius<=0 || d.MinimumGroup<1 || d.MaximumGroup<d.MinimumGroup || d.AlertMaxDistance<d.AlertRadius || d.RoamRadius<=0)errors.Add("Invalid creature/group: "+d.name);
                if(d.Has(CreatureTrait.LightSensitive) && (d.Light.DimDetection<=0 || d.Light.SafeMovement<=0 || d.Light.ChaseGraceMultiplier<0))errors.Add("Invalid light sensitivity: "+d.name);
                if(d.Actions!=null){var a=d.Actions;var invalid=false;foreach(var n in new[]{a.Detection,a.Leash,a.WalkSpeed,a.ChaseSpeed,a.ActionSpeed,a.WarningSeconds,a.WindupSeconds,a.ActionSeconds,a.RecoverySeconds,a.Reach,a.EngageRange,a.FleeSpeed})invalid|=float.IsNaN(n) || float.IsInfinity(n) || n<0;if(invalid || a.Detection<=0 || a.Leash<a.Detection || !System.Enum.IsDefined(typeof(WildlifeMovement),a.Movement) || !System.Enum.IsDefined(typeof(WildlifeAttack),a.Attack) || a.Attack!=WildlifeAttack.None && (a.Damage<=0 || a.WarningSeconds+a.WindupSeconds<.3f || a.ActionSeconds<=0 || a.RecoverySeconds<=0) || d.Has(CreatureTrait.Flying)!=(a.Movement==WildlifeMovement.Flight) || d.Has(CreatureTrait.Camouflage)!=a.Camouflage)errors.Add("Invalid wildlife action: "+d.name);}
            }
            foreach(var guid in AssetDatabase.FindAssets("t:AnimalPopulationDefinition",new[]{"Assets/Wildfolk/Data"}))
            {var d=AssetDatabase.LoadAssetAtPath<AnimalPopulationDefinition>(AssetDatabase.GUIDToAssetPath(guid));var species=new HashSet<string>();foreach(var r in d.Rules){if(r.Prefab==null || r.Prefab.Member==null || r.Prefab.Member.Definition==null || !species.Add(r.Prefab.Member.Definition.Id) || r.Population<1 || r.RespawnSeconds<=0 || float.IsNaN(r.ChunkChance) || r.ChunkChance<0 || r.ChunkChance>1 || r.RegionalTarget<0)errors.Add("Invalid species population");}if(d.HardCap<1 || !d.RegionalConfigured)errors.Add("Invalid animal cap/policy");}
        }
    }
}
