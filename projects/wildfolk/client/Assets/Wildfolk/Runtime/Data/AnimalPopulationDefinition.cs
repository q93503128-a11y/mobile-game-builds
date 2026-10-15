using System;
using UnityEngine;
namespace Wildfolk
{
    [Serializable]public struct AnimalPopulationRule
    {public NeutralAnimal Prefab;[Min(1)]public int Population;[Min(1)]public float RespawnSeconds;}
    [CreateAssetMenu(menuName="Wildfolk/Animal Population")]
    public sealed class AnimalPopulationDefinition : ScriptableObject
    {public AnimalPopulationRule[] Rules=Array.Empty<AnimalPopulationRule>();[Min(1)]public int HardCap=80;[Min(1)]public float PlayerDistance=14,BaseDistance=8;public bool RegionalConfigured;public int[] LegacyPopulation={1,1};}
}
