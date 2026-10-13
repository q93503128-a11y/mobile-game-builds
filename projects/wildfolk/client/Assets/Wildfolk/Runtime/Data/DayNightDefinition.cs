using UnityEngine;
namespace Wildfolk
{
    [CreateAssetMenu(menuName="Wildfolk/Day Night")]
    public sealed class DayNightDefinition : ScriptableObject
    {
        [Min(1)] public float DaySeconds=240;
        [Min(1)] public float EveningSeconds=60;
        [Min(1)] public float NightSeconds=150;
        [Min(1)] public float DawnSeconds=30;
        public Color DayColor=Color.white;
        public Color EveningColor=new Color(1,.88f,.72f);
        public Color NightColor=new Color(.78f,.83f,.76f);
        [Range(.1f,1)] public float EveningIntensity=.9f;
        [Range(.1f,1)] public float NightIntensity=.48f;
        public Sprite SunIcon,MoonIcon;
        public float CycleSeconds => DaySeconds+EveningSeconds+NightSeconds+DawnSeconds;
    }
}
