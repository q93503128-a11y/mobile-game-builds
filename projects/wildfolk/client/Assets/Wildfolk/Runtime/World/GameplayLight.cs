using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    public sealed class GameplayLight : MonoBehaviour
    {
        public GameplayLightDefinition Definition;
        private static readonly List<GameplayLight> lights=new List<GameplayLight>();
        private void OnEnable()=>lights.Add(this);
        private void OnDisable()=>lights.Remove(this);
        public static bool IsDim(Vector2 point)
        {foreach(var light in lights)if(light!=null && Vector2.Distance(point,light.transform.position)<=light.Definition.Radius*1.35f)return !IsSafe(point);return false;}
        public static bool IsSafe(Vector2 point)
        {
            foreach(var light in lights)if(light!=null && light.Definition.Strength>=light.Definition.SafeThreshold && Vector2.Distance(point,light.transform.position)<=light.Definition.Radius)return true;
            return false;
        }
        public static float Grace(Vector2 point)
        {
            var grace=float.MaxValue;foreach(var light in lights)if(light!=null && Vector2.Distance(point,light.transform.position)<=light.Definition.Radius)grace=Mathf.Min(grace,light.Definition.PursuitGrace);
            return grace==float.MaxValue ? 0 : grace;
        }
    }
}
