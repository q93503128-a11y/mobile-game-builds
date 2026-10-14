using UnityEngine;
namespace Wildfolk
{
    public static class WorldSorting
    {
        public const int Ground=-12000,GroundDecoration=-11000,Shadow=-10000,Drop=-9000,Overlay=12000;
        public static int At(float baseY)=>-Mathf.RoundToInt(baseY*20);
    }
}
