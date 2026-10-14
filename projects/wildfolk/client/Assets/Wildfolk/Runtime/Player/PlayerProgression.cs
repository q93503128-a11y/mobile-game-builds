using UnityEngine;
namespace Wildfolk
{
    public sealed class PlayerProgression : MonoBehaviour
    {
        public long Experience { get; private set; }
        public long Gold { get; private set; }
        public void SetValues(long experience,long gold)
        {if(experience<0 || gold<0)throw new System.ArgumentOutOfRangeException();Experience=experience;Gold=gold;}
        public void Reward(int experience,int gold)
        {
            if(experience<0 || gold<0)throw new System.ArgumentOutOfRangeException();
            Experience=checked(Experience+experience);Gold=checked(Gold+gold);
        }
    }
}
