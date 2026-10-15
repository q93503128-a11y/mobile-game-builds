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
            }
            foreach(var guid in AssetDatabase.FindAssets("t:AnimalPopulationDefinition",new[]{"Assets/Wildfolk/Data"}))
            {var d=AssetDatabase.LoadAssetAtPath<AnimalPopulationDefinition>(AssetDatabase.GUIDToAssetPath(guid));var total=0;var species=new HashSet<string>();foreach(var r in d.Rules){total+=r.Population;if(r.Prefab==null || r.Prefab.Member==null || r.Prefab.Member.Definition==null || !species.Add(r.Prefab.Member.Definition.Id) || r.Population<1 || r.RespawnSeconds<=0)errors.Add("Invalid species population");}if(total>d.HardCap || !d.RegionalConfigured)errors.Add("Invalid animal cap/policy");}
        }
    }
}
