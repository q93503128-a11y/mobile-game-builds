using UnityEngine;
namespace Wildfolk
{
    [CreateAssetMenu(menuName="Wildfolk/Surface Environment")]
    public sealed class SurfaceEnvironmentDefinition : ScriptableObject
    {
        public SurfaceBiome Biome;
        public Color Ground,DenseGround,Detail;
        public Vector2 PreferredMoisture,PreferredForestation,PreferredRockiness,PreferredHeight;
        public float MoistureWeight=1,ForestWeight=1,RockWeight=1,HeightWeight=1;
        public float Score(EnvironmentSample e)=>(Fit(e.Moisture,PreferredMoisture)*MoistureWeight+Fit(e.Forestation,PreferredForestation)*ForestWeight+Fit(e.Rockiness,PreferredRockiness)*RockWeight+Fit(e.Height,PreferredHeight)*HeightWeight)/(MoistureWeight+ForestWeight+RockWeight+HeightWeight);
        private static float Fit(float n,Vector2 range)=>n<range.x ? 1-(range.x-n)*2 : n>range.y ? 1-(n-range.y)*2 : 1;
    }
}
