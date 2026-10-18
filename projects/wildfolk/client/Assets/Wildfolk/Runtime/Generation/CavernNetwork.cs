using System;
using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    public enum CavernRoomKind { Entry,Chamber,LargeCavern,OrePocket,WaterChamber,Nest,DeepRuin }
    public sealed class CavernRoom
    {public Vector2 Center,Radius;public CavernRoomKind Kind;public float Depth;public readonly List<int> Neighbors=new List<int>();}
    public sealed class CavernNetwork
    {
        public int Id {get;}
        public Vector2 Origin {get;}
        public readonly List<CavernRoom> Rooms=new List<CavernRoom>();
        public readonly List<Vector2> SurfaceEntrances=new List<Vector2>();
        public const float CorridorWidth=3.8f;
        public CavernNetwork(int seed,int id,Vector2 origin,Vector2 entrance,bool second)
        {
            Id=id;Origin=origin;SurfaceEntrances.Add(entrance);if(second)SurfaceEntrances.Add(entrance+new Vector2(110,45));
            var rng=new System.Random(WorldSeeds.Derive(seed,SeedDomain.Cave,id));
            // A connected loop plus two deep branches; room width and composition change with graph depth.
            for(var i=0;i<12;i++)
            {
                var angle=i%8*Mathf.PI*.25f;var center=i<8 ? new Vector2(Mathf.Cos(angle)*65,Mathf.Sin(angle)*48) : new Vector2((i-9.5f)*40,-100-(i%2)*45);
                center+=new Vector2((float)rng.NextDouble()*8,(float)rng.NextDouble()*8);
                Rooms.Add(new CavernRoom{Center=origin+center,Radius=new Vector2(i>=8 ? 15 : 11+ i%3*3,9+i%3*2),Kind=i==0 ? CavernRoomKind.Entry : i>=10 ? CavernRoomKind.DeepRuin : (CavernRoomKind)(1+i%5)});
            }
            for(var i=0;i<8;i++)Connect(i,(i+1)%8);Connect(5,8);Connect(8,10);Connect(6,9);Connect(9,11);ComputeDepth();
        }
        private void Connect(int a,int b){Rooms[a].Neighbors.Add(b);Rooms[b].Neighbors.Add(a);}
        private void ComputeDepth()
        {
            var distance=new float[Rooms.Count];for(var i=0;i<distance.Length;i++)distance[i]=float.MaxValue;distance[0]=0;if(SurfaceEntrances.Count>1)distance[2]=0;
            var used=new bool[distance.Length];for(var pass=0;pass<distance.Length;pass++){var at=-1;for(var i=0;i<distance.Length;i++)if(!used[i] && (at<0 || distance[i]<distance[at]))at=i;if(at<0)break;used[at]=true;foreach(var next in Rooms[at].Neighbors)distance[next]=Mathf.Min(distance[next],distance[at]+Vector2.Distance(Rooms[at].Center,Rooms[next].Center));}
            var max=0f;foreach(var d in distance)max=Mathf.Max(max,d);for(var i=0;i<Rooms.Count;i++)Rooms[i].Depth=distance[i]/max;
        }
        public float FloorDistance(Vector2 p)
        {
            var best=float.MaxValue;for(var i=0;i<Rooms.Count;i++){var r=Rooms[i];var n=p-r.Center;best=Mathf.Min(best,(new Vector2(n.x/r.Radius.x,n.y/r.Radius.y).magnitude-1)*Mathf.Min(r.Radius.x,r.Radius.y));foreach(var j in r.Neighbors)if(j>i)best=Mathf.Min(best,WorldGeometry.SegmentDistance(p,r.Center,Rooms[j].Center)-CorridorWidth);}return best;
        }
        public CavernRoom Nearest(Vector2 p){CavernRoom found=null;var best=float.MaxValue;foreach(var r in Rooms){var d=(r.Center-p).sqrMagnitude;if(d<best){best=d;found=r;}}return found;}
        public Vector2 Entry(int index)=>Rooms[index==0 ? 0 : 2].Center;
        public float Ambient(Vector2 p)=>Mathf.Lerp(.64f,.3f,Nearest(p).Depth);
        public bool Water(Vector2 p)
        {
            var r=Nearest(p);if(r.Kind!=CavernRoomKind.WaterChamber || Vector2.Distance(p,r.Center+Vector2.right*r.Radius.x*.65f)>=3)return false;
            foreach(var neighbor in r.Neighbors)if(WorldGeometry.SegmentDistance(p,r.Center,Rooms[neighbor].Center)<1.4f)return false;
            return true;
        }
    }
    public static class WorldGeometry
    {
        public static float SegmentDistance(Vector2 p,Vector2 a,Vector2 b){var d=b-a;return Vector2.Distance(p,a+d*Mathf.Clamp01(Vector2.Dot(p-a,d)/Mathf.Max(.0001f,d.sqrMagnitude)));}
    }
}
