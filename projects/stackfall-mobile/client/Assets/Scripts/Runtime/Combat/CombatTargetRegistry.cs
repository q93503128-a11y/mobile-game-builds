using System.Collections.Generic;
using StackfallMobile.Runtime.Enemies;
using UnityEngine;

namespace StackfallMobile.Runtime.Combat
{
    public static class CombatTargetRegistry
    {
        private static readonly List<EnemyActor> Targets = new(128);

        public static IReadOnlyList<EnemyActor> ActiveTargets => Targets;

        public static void Register(EnemyActor enemy)
        {
            if (enemy != null && !Targets.Contains(enemy))
            {
                Targets.Add(enemy);
            }
        }

        public static void Unregister(EnemyActor enemy)
        {
            if (enemy != null)
            {
                Targets.Remove(enemy);
            }
        }

        public static EnemyActor FindNearest(Vector2 origin, float maxRange)
        {
            EnemyActor best = null;
            var bestSqr = maxRange * maxRange;

            for (var i = Targets.Count - 1; i >= 0; i--)
            {
                var candidate = Targets[i];
                if (candidate == null || !candidate.IsAlive || !candidate.gameObject.activeInHierarchy)
                {
                    if (candidate == null)
                    {
                        Targets.RemoveAt(i);
                    }
                    continue;
                }

                var sqr = ((Vector2)candidate.transform.position - origin).sqrMagnitude;
                if (sqr >= bestSqr)
                {
                    continue;
                }

                bestSqr = sqr;
                best = candidate;
            }

            return best;
        }

        public static int CountAliveInRadius(Vector2 origin, float radius)
        {
            var count = 0;
            var radiusSqr = radius * radius;

            for (var i = 0; i < Targets.Count; i++)
            {
                var candidate = Targets[i];
                if (candidate == null || !candidate.IsAlive || !candidate.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (((Vector2)candidate.transform.position - origin).sqrMagnitude <= radiusSqr)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
