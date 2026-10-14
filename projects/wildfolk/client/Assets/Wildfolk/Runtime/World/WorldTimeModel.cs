using System;
using UnityEngine;
namespace Wildfolk
{
    public enum DayPhase { Day, Evening, Night, Dawn }
    public sealed class WorldTimeModel
    {
        public DayNightDefinition Definition { get; }
        public double Elapsed { get; private set; }
        public int Day => (int)Math.Floor(Elapsed/Definition.CycleSeconds)+1;
        public float CycleTime => (float)(Elapsed%Definition.CycleSeconds);
        public DayPhase Phase
        {
            get
            {
                var t=CycleTime;
                if(t<Definition.DaySeconds)return DayPhase.Day;t-=Definition.DaySeconds;
                if(t<Definition.EveningSeconds)return DayPhase.Evening;t-=Definition.EveningSeconds;
                return t<Definition.NightSeconds ? DayPhase.Night : DayPhase.Dawn;
            }
        }
        public WorldTimeModel(DayNightDefinition definition) => Definition=definition;
        public void SetElapsed(double seconds){if(double.IsNaN(seconds) || double.IsInfinity(seconds) || seconds<0)throw new ArgumentOutOfRangeException(nameof(seconds));Elapsed=seconds;}
        public void Advance(float seconds) {if(seconds<0)throw new ArgumentOutOfRangeException(nameof(seconds));Elapsed+=seconds;}
        public void Lighting(out Color color,out float intensity)
        {
            var t=CycleTime;
            if(Phase==DayPhase.Day){color=Definition.DayColor;intensity=1;return;}
            if(Phase==DayPhase.Night){color=Definition.NightColor;intensity=Definition.NightIntensity;return;}
            if(Phase==DayPhase.Dawn)
            {
                var progress=(t-Definition.DaySeconds-Definition.EveningSeconds-Definition.NightSeconds)/Definition.DawnSeconds;
                progress=Mathf.SmoothStep(0,1,progress);
                color=Color.Lerp(Definition.NightColor,Definition.DayColor,progress);intensity=Mathf.Lerp(Definition.NightIntensity,1,progress);return;
            }
            var evening=(t-Definition.DaySeconds)/Definition.EveningSeconds;
            if(evening<.5f)
            {
                evening=Mathf.SmoothStep(0,1,evening*2);
                color=Color.Lerp(Definition.DayColor,Definition.EveningColor,evening);intensity=Mathf.Lerp(1,Definition.EveningIntensity,evening);
            }
            else
            {
                evening=Mathf.SmoothStep(0,1,(evening-.5f)*2);
                color=Color.Lerp(Definition.EveningColor,Definition.NightColor,evening);intensity=Mathf.Lerp(Definition.EveningIntensity,Definition.NightIntensity,evening);
            }
        }
    }
}
