using UnityEngine;
using UnityEngine.Rendering.Universal;
namespace Wildfolk
{
    public sealed class FacilityGlow : MonoBehaviour
    {
        public Light2D Light;
        private WorldClock clock;
        private void Start() => clock=FindFirstObjectByType<WorldClock>();
        private void LateUpdate()
        {
            if(clock==null){Light.intensity=0;return;}
            clock.Model.Lighting(out _,out var ambient);
            Light.intensity=(1-ambient)*1.2f;
        }
    }
}
