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
            Check(world,world.ForestAnchor,reached);foreach(var p in world.GuaranteedResources)Check(world,p.Position,reached);
            foreach(var entrance in world.Caves[0].SurfaceEntrances)Check(world,entrance,reached);
        }
        private static void Check(ContinentalWorldLayout world,Vector2 destination,HashSet<WorldChunk> reached)
        {
            var x=Mathf.RoundToInt(destination.x/Step);var y=Mathf.RoundToInt(destination.y/Step);
            for(var dy=-2;dy<=2;dy++)for(var dx=-2;dx<=2;dx++){var key=new WorldChunk(x+dx,y+dy);if(reached.Contains(key) && Segment(world,Point(key),destination,1))return;}
            throw new InvalidOperationException("Required progression source has no validated terrain path from spawn");
        }
    }
}
