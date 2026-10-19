using UnityEngine;
using UnityEngine.Rendering.Universal;
namespace Wildfolk
{
    // Shared by carried and placed sources. No particles, shadows, or AI rules.
    public sealed class LocalFirelight : MonoBehaviour
    {
        public GameplayLightDefinition Definition;
        public Light2D Light;
        public bool VisibilityFloor;
        private float intensity;private GameplayLightDefinition configured;private bool configuredFloor,hasConfiguration;
        private WorldGenerator world;private WorldClock clock;
        private void Start(){world=FindFirstObjectByType<WorldGenerator>();clock=world==null ? null : world.GetComponent<WorldClock>();}
        private void LateUpdate()
        {
            var ambient=1f;if(clock!=null)clock.Model.Lighting(out _,out ambient);ambient=WorldLighting.Ambient(world,transform.position,ambient);
            var strength=VisibilityFloor ? .28f : Definition==null ? 0 : Definition.Strength;
            var target=strength*(1-ambient);
            intensity=Mathf.MoveTowards(intensity,target,Time.unscaledDeltaTime*7);
            if(!Mathf.Approximately(Light.intensity,intensity))Light.intensity=intensity;
            if(hasConfiguration && configured==Definition && configuredFloor==VisibilityFloor)return;
            configured=Definition;configuredFloor=VisibilityFloor;hasConfiguration=true;
            if(Definition==null && !VisibilityFloor)return;
            var radius=VisibilityFloor ? 2.8f : Definition.Radius;
            Light.color=VisibilityFloor ? new Color(.62f,.7f,.8f) : Definition.Color;
            Light.overlapOperation=Light2D.OverlapOperation.Additive;
            Light.pointLightOuterRadius=radius;
            Light.pointLightInnerRadius=VisibilityFloor ? .75f : radius*.22f;
            Light.pointLightInnerAngle=Light.pointLightOuterAngle=360;
        }
        private void OnDisable(){intensity=0;if(Light!=null)Light.intensity=0;}
    }
}
