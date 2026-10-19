using NUnit.Framework;
using UnityEngine;
namespace Wildfolk.Tests
{
    public sealed class GamefeelModelTests
    {
        [TestCase(false)][TestCase(true)]public void CachedCaveRoutesFollowShortestDistanceToEitherExit(bool second)
        {
            for(var seed=1;seed<=8;seed++)
            {
                var cave=new CavernNetwork(seed,0,Vector2.zero,Vector2.zero,second);
                for(var i=0;i<cave.Rooms.Count;i++)
                {
                    var room=cave.Rooms[i];var next=cave.ExitWaypoint(room.Center);
                    if(i==0 || second && i==2){Assert.That(next,Is.EqualTo(room.Center));continue;}
                    var at=cave.Rooms.FindIndex(r=>r.Center==next);Assert.That(room.Neighbors,Does.Contain(at));Assert.That(cave.Rooms[at].Depth,Is.LessThan(room.Depth));
                    Assert.That(cave.ExitWaypoint(room.Center+Vector2.up*3),Is.EqualTo(room.Center));
                }
            }
        }
    }
}
