using UnityEngine;
using UnityEngine.Rendering.Universal;
namespace Wildfolk
{
    public sealed class WorldLighting : MonoBehaviour
    {
        public WorldClock Clock;
        public Light2D Global;
        private void LateUpdate()
        {
            Clock.Model.Lighting(out var color,out var intensity);
            Global.color=color;Global.intensity=intensity;
        }
    }
}
