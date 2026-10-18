using UnityEngine;
using UnityEngine.Rendering.Universal;
namespace Wildfolk
{
    public sealed class WorldLighting : MonoBehaviour
    {
        public WorldClock Clock;
        public Light2D Global;
        public WorldGenerator World;
        public Transform Player;
        public static float Ambient(WorldGenerator world,Vector2 point,float surface)
        {return world!=null && world.Map!=null && world.Map.Streamed && world.Map.Continental.Underground(point) ? world.Map.Continental.CaveAt(point).Ambient(point) : surface;}
        private void LateUpdate()
        {
            if(Clock==null)return;Clock.Model.Lighting(out var color,out var intensity);
            var local=Ambient(World,Player==null ? Vector2.zero : (Vector2)Player.position,intensity);
            var underground=World!=null && World.Map!=null && World.Map.Streamed && Player!=null && World.Map.Continental.Underground(Player.position);
            Global.color=underground ? new Color(.68f,.74f,.78f) : color;
            Global.intensity=Mathf.MoveTowards(Global.intensity,local,Time.unscaledDeltaTime*2);
        }
    }
}
