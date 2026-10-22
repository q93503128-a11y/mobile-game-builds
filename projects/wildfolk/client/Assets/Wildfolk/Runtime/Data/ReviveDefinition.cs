using UnityEngine;
namespace Wildfolk
{
    [CreateAssetMenu(menuName="Wildfolk/Revive")]
    public sealed class ReviveDefinition : ScriptableObject
    {
        [Min(1)]public float DownedSeconds=24,HoldSeconds=4,Range=2.6f;
        [Range(.01f,1)]public float RevivedHealth=.4f;
        [Min(0)]public float ProtectionSeconds=2;
        // Zero means no production price has been approved, hence unavailable.
        [Min(0)]public int ReviveGemCost;
    }
    public interface ICooperativeReviveModifiers
    {float ReviveProtectionBonus{get;}float DownedDrainMultiplier{get;}float GoldenTimeBonus{get;}}
    public interface IReviveModifiers
    {
        float DownedDurationMultiplier {get;}
        float ReviveSpeedMultiplier {get;}
        float RevivedHealthMultiplier {get;}
    }
}
