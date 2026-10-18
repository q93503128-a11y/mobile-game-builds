using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    // Macro samples are continuous at render time. Small disconnected labels are merged once.
    public sealed class SurfaceBiomeRegions
    {
        public const int CellSize=128;
        private readonly int width;private readonly Vector4[] weights;private readonly int[] labels;
        public int RepairedFragments {get;private set;}
        public SurfaceBiomeRegions(ContinentalWorldLayout world,WorldGenerationDefinition data)
        {
            width=Mathf.CeilToInt(world.Size.x/CellSize)+2;weights=new Vector4[width*width];labels=new int[weights.Length];
            for(var y=0;y<width;y++)for(var x=0;x<width;x++)
            {var p=new Vector2(x-width/2,y-width/2)*CellSize;var e=world.RawEnvironment(p);var w=Vector4.zero;float sum=0,max=-1;int label=0;foreach(var profile in data.SurfaceEnvironments){var score=Mathf.Exp(profile.Score(e)*12);w[(int)profile.Biome]=score;sum+=score;if(score>max){max=score;label=(int)profile.Biome;}}weights[y*width+x]=w/sum;labels[y*width+x]=label;}
            var protectedCells=new HashSet<int>{Index(Vector2.zero),Index(world.ForestAnchor),Index(world.RockyAnchor),Index(world.SedimentAnchor)};
            var seen=new bool[weights.Length];var group=new List<int>();var queue=new Queue<int>();var votes=new int[4];
            for(var start=0;start<labels.Length;start++)
            {
                if(seen[start])continue;group.Clear();queue.Enqueue(start);seen[start]=true;var label=labels[start];var protect=false;for(var i=0;i<4;i++)votes[i]=0;
                while(queue.Count>0){var at=queue.Dequeue();group.Add(at);protect|=protectedCells.Contains(at);var x=at%width;var y=at/width;foreach(var d in offsets){var nx=x+d.x;var ny=y+d.y;if(nx<0 || ny<0 || nx>=width || ny>=width)continue;var next=ny*width+nx;if(labels[next]!=label){votes[labels[next]]++;continue;}if(!seen[next]){seen[next]=true;queue.Enqueue(next);}}}
                if(protect || group.Count>=4)continue;var best=0;for(var i=1;i<4;i++)if(votes[i]>votes[best])best=i;foreach(var index in group){labels[index]=best;var w=weights[index];w[best]+=1.5f;w[label]*=.1f;weights[index]=w/(w.x+w.y+w.z+w.w);}RepairedFragments++;
            }
        }
        private static readonly Vector2Int[] offsets={Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right};
        private int Index(Vector2 p){var x=Mathf.Clamp(Mathf.RoundToInt(p.x/CellSize)+width/2,0,width-1);var y=Mathf.Clamp(Mathf.RoundToInt(p.y/CellSize)+width/2,0,width-1);return y*width+x;}
        public Vector4 Sample(Vector2 p)
        {var v=p/CellSize+Vector2.one*(width/2);var x=Mathf.Clamp(Mathf.FloorToInt(v.x),0,width-2);var y=Mathf.Clamp(Mathf.FloorToInt(v.y),0,width-2);var tx=Mathf.SmoothStep(0,1,v.x-x);var ty=Mathf.SmoothStep(0,1,v.y-y);return Vector4.Lerp(Vector4.Lerp(weights[y*width+x],weights[y*width+x+1],tx),Vector4.Lerp(weights[(y+1)*width+x],weights[(y+1)*width+x+1],tx),ty);}
    }
}
