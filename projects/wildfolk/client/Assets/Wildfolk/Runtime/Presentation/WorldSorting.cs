using UnityEngine;
namespace Wildfolk
{
    public static class WorldSorting
    {
        public const int Ground=-12000,GroundDecoration=-11000,Shadow=-10000,Drop=-9000,Overlay=12000;
        public static float OriginY {get;set;}
        public static int At(float baseY)=>Mathf.Clamp(-Mathf.RoundToInt((baseY-OriginY)*20),-8000,8000);
    }
}
