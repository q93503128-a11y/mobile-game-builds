using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    public sealed class SoftSeparation : MonoBehaviour
    {
        public CreatureMember Member;
        public Transform Player;
        private static readonly List<SoftSeparation> actors=new List<SoftSeparation>();
        public float Radius=>Member.Definition.SeparationRadius;
        private void OnEnable()=>actors.Add(this);
        private void OnDisable()=>actors.Remove(this);
        private Vector2 Away(Vector2 other,float radius,int otherId)
        {
            var delta=(Vector2)transform.position-other;var distance=delta.magnitude;
            if(distance>=radius)return Vector2.zero;
            if(distance<.001f){var pair=Mathf.Min(GetInstanceID(),otherId);var angle=(pair%89)*Mathf.PI*2/89;delta=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*(GetInstanceID()<otherId ? 1 : -1);}
            return delta.normalized*Mathf.Clamp01(1-distance/radius);
        }
        public Vector2 Correction(float seconds)
        {
            if(Member==null || Member.Definition==null)return Vector2.zero;
            var force=Vector2.zero;
            if(Player!=null)force+=Away(Player.position,Radius+.55f,Player.GetInstanceID())*2.8f;
            foreach(var actor in actors)if(actor!=this && actor.Member!=null && actor.Member.Definition!=null)force+=Away(actor.transform.position,Radius+actor.Radius,actor.GetInstanceID())*1.8f;
            return Vector2.ClampMagnitude(force,3)*seconds;
        }
    }
}
