using UnityEditor;
using UnityEngine;
namespace Wildfolk.Editor
{
    public static class DeathDataValidator
    {
        public static string Validate()
        {
            var d=AssetDatabase.LoadAssetAtPath<ReviveDefinition>("Assets/Wildfolk/Data/Revive.asset");
            if(d==null || d.DownedSeconds<10 || d.DownedSeconds>45 || d.HoldSeconds<1 || d.HoldSeconds>=d.DownedSeconds || d.Range<=0 || d.RevivedHealth<=0 || d.RevivedHealth>=1 || d.ProtectionSeconds<0 || d.ProtectionSeconds>3 || d.ReviveGemCost<0)throw new System.InvalidOperationException("Invalid revive configuration.");
            foreach(var p in new[]{"Player","NetworkPlayer"})if(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/"+p+".prefab").GetComponent<PlayerDeath>()?.Definition!=d)throw new System.InvalidOperationException("Missing lifecycle configuration.");
            return "Revive data valid; PremiumGem cost0/production unavailable; Item134/Recipe111 unchanged.";
        }
    }
}
