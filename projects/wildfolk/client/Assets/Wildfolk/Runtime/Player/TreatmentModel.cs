using System;
namespace Wildfolk
{
    // One transient treatment. Cumulative delivery avoids frame-rate rounding and catch-up duplication.
    public sealed class TreatmentModel
    {
        private int total,delivered;
        private float duration,elapsed;
        public float Remaining=>Math.Max(0,duration-elapsed);
        public bool Active=>Remaining>0;
        public bool Begin(int health,float seconds)
        {
            if(health<=0 || seconds<=0 || float.IsNaN(seconds) || float.IsInfinity(seconds))throw new ArgumentOutOfRangeException();
            if(Active)return false;total=health;duration=seconds;elapsed=0;delivered=0;return true;
        }
        public int Advance(float seconds)
        {
            if(seconds<0 || float.IsNaN(seconds) || float.IsInfinity(seconds))throw new ArgumentOutOfRangeException(nameof(seconds));
            if(!Active)return 0;elapsed=Math.Min(duration,elapsed+seconds);
            var due=elapsed>=duration ? total : (int)Math.Floor(total*(double)elapsed/duration);
            var amount=due-delivered;delivered=due;return amount;
        }
        public void Clear(){total=delivered=0;duration=elapsed=0;}
    }
}
