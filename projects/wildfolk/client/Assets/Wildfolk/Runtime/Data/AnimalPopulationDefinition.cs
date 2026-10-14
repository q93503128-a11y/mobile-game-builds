using System;
using UnityEngine;
namespace Wildfolk
{
    [Serializable]public struct AnimalPopulationRule
    {public NeutralAnimal Prefab;public Vector2 Home;[Min(1)]public int Population;[Min(1)]public float RespawnSeconds;}
    [CreateAssetMenu(menuName="Wildfolk/Animal Population")]
    public sealed class AnimalPopulationDefinition : ScriptableObject
    {public AnimalPopulationRule[] Rules=Array.Empty<AnimalPopulationRule>();}
}
