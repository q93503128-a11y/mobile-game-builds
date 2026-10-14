using UnityEngine;
namespace Wildfolk
{
    [CreateAssetMenu(menuName="Wildfolk/Biome")]
    public sealed class BiomeDefinition : ScriptableObject
    {
        public string Id="temperate",DisplayName="Temperate";
        public Color Grass=new Color(.39f,.46f,.26f),Forest=new Color(.29f,.38f,.23f),Dirt=new Color(.52f,.46f,.32f),Water=new Color(.28f,.47f,.46f),Shallows=new Color(.45f,.56f,.43f);
        [Min(.001f)]public float DensityFrequency=.035f;
    }
}
