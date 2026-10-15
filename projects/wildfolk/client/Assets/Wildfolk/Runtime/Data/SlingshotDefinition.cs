using UnityEngine;
namespace Wildfolk
{
    [CreateAssetMenu(menuName="Wildfolk/Slingshot Attack")]
    public sealed class SlingshotDefinition : ScriptableObject
    {
        public float MinimumDistance=2.4f,PreferredDistance=4.8f,MaximumDistance=7,RepositionSeconds=.65f,RepositionCooldown=2.5f;
        public float Ready=.12f,Draw=.42f,Hold=.1f,Release=.08f,Recovery=.28f,ProjectileSpeed=10,ProjectileLifetime=1.4f;
        public int Damage=4;
        public float LaunchAt=>Ready+Draw+Hold;
        public float Duration=>LaunchAt+Release+Recovery;
    }
}
