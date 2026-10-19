using System;
using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    // Bounded terrain flood for the current early-progression excursion. It never materializes objects.
    public static class ProgressionReachability
    {
        private const int Step=16,Limit=80;
        private static readonly WorldChunk[] Directions={new WorldChunk(1,0),new WorldChunk(-1,0),new WorldChunk(0,1),new WorldChunk(0,-1)};
        private static Vector2 Point(WorldChunk p)=>new Vector2(p.X*Step,p.Y*Step);
        private static bool Segment(ContinentalWorldLayout world,Vector2 a,Vector2 b,float resolution)
        {var count=Mathf.CeilToInt(Vector2.Distance(a,b)/resolution);for(var i=0;i<=count;i++)if(!world.Walkable(Vector2.Lerp(a,b,count==0 ? 0 : (float)i/count),.35f))return false;return true;}
        public static void Validate(ContinentalWorldLayout world)
        {
            var reached=new HashSet<WorldChunk>{new WorldChunk(0,0)};var rejected=new HashSet<WorldChunk>();var queue=new Queue<WorldChunk>();queue.Enqueue(new WorldChunk(0,0));
            while(queue.Count>0){var at=queue.Dequeue();foreach(var d in Directions){var next=new WorldChunk(at.X+d.X,at.Y+d.Y);if(Mathf.Abs(next.X)>Limit || Mathf.Abs(next.Y)>Limit || reached.Contains(next) || rejected.Contains(next))continue;if(!world.Walkable(Point(next),.35f)){rejected.Add(next);continue;}if(!Segment(world,Point(at),Point(next),2))continue;reached.Add(next);queue.Enqueue(next);}}
            Check(world,world.ForestAnchor,reached);foreach(var p in world.GuaranteedResources)if(!world.Underground(p.Position))Check(world,p.Position,reached);
            foreach(var entrance in world.Caves[0].SurfaceEntrances)Check(world,entrance,reached);
            if(world.GenerationVersion>=3)ValidateDeepRoutes(world,reached);
        }
        private static void ValidateDeepRoutes(ContinentalWorldLayout world,HashSet<WorldChunk> reached)
        {
            var terrain=new Dictionary<WorldChunk,bool>();
            foreach(var cave in world.Caves)
            {
                if(!SurfacePath(world,cave.SurfaceEntrances[0],terrain,reached))throw new InvalidOperationException("Deep cavern entrance has no validated surface route");
                var rooms=new HashSet<int>{0};var pending=new Queue<int>();pending.Enqueue(0);
                while(pending.Count>0){var at=pending.Dequeue();foreach(var next in cave.Rooms[at].Neighbors)if(!rooms.Contains(next) && Segment(world,cave.Rooms[at].Center,cave.Rooms[next].Center,1)){rooms.Add(next);pending.Enqueue(next);}}
                foreach(var p in world.GuaranteedResources)if(world.Underground(p.Position) && world.CaveAt(p.Position)==cave)
                {var room=cave.Nearest(p.Position);if(!rooms.Contains(cave.Rooms.IndexOf(room)) || !Segment(world,room.Center,p.Position,.5f))throw new InvalidOperationException("Deep source is disconnected from its entrance");}
            }
        }
        // A bounded A* follows surface terrain to remote entrances without expanding simulation chunks.
        private static bool SurfacePath(ContinentalWorldLayout world,Vector2 destination,Dictionary<WorldChunk,bool> terrain,HashSet<WorldChunk> reached)
        {
            const int stride=48;var start=new WorldChunk(0,0);var nearest=destination.sqrMagnitude;foreach(var known in reached){if(known.X%3!=0 || known.Y%3!=0)continue;var distance=(Point(known)-destination).sqrMagnitude;if(distance<nearest){nearest=distance;start=new WorldChunk(known.X/3,known.Y/3);}}var open=new SortedSet<(float Cost,int X,int Y)>();var costs=new Dictionary<WorldChunk,float>{{start,0}};var closed=new HashSet<WorldChunk>();open.Add((Mathf.Sqrt(nearest),start.X,start.Y));
            for(var budget=0;budget<150000 && open.Count>0;budget++)
            {
                var first=open.Min;open.Remove(first);var at=new WorldChunk(first.X,first.Y);if(!closed.Add(at))continue;var point=new Vector2(at.X*stride,at.Y*stride);
                if(Vector2.Distance(point,destination)<stride*2 && Segment(world,point,destination,1))return true;
                foreach(var dir in Directions)
                {
                    var next=new WorldChunk(at.X+dir.X,at.Y+dir.Y);if(closed.Contains(next))continue;var p=new Vector2(next.X*stride,next.Y*stride);if(!terrain.TryGetValue(next,out var walkable))terrain[next]=walkable=world.Walkable(p,.35f);if(!walkable || !Segment(world,point,p,2))continue;
                    var cost=costs[at]+stride;if(costs.TryGetValue(next,out var old) && old<=cost)continue;costs[next]=cost;open.Add((cost+Vector2.Distance(p,destination),next.X,next.Y));
                }
            }
            return false;
        }
        private static void Check(ContinentalWorldLayout world,Vector2 destination,HashSet<WorldChunk> reached)
        {
            var x=Mathf.RoundToInt(destination.x/Step);var y=Mathf.RoundToInt(destination.y/Step);
            for(var dy=-2;dy<=2;dy++)for(var dx=-2;dx<=2;dx++){var key=new WorldChunk(x+dx,y+dy);if(reached.Contains(key) && Segment(world,Point(key),destination,1))return;}
            throw new InvalidOperationException("Required progression source has no validated terrain path from spawn");
        }
    }
}
