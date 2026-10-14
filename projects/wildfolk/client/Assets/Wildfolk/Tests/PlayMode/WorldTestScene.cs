using UnityEngine;
namespace Wildfolk.Tests
{
    internal static class WorldTestScene
    {
        internal static EnemyController SpawnZombie()
        {
            foreach(var node in Object.FindObjectsByType<ResourceNode>(FindObjectsSortMode.None))if(Vector2.Distance(node.transform.position,new Vector2(6.6f,-.2f))<7)node.gameObject.SetActive(false);
            var spawner=Object.FindFirstObjectByType<ZombieSpawner>();spawner.enabled=false;Object.FindFirstObjectByType<GoblinSpawner>().enabled=false;
            var clock=spawner.Clock;if(clock.Model.Phase!=DayPhase.Night)clock.Model.Advance(clock.Definition.DaySeconds+clock.Definition.EveningSeconds);
            var enemy=Object.Instantiate(spawner.Prefab,new Vector2(6.6f,-.2f),Quaternion.identity);enemy.Session=spawner.Session;enemy.Clock=clock;enemy.PlayerHealth=spawner.Player;
            var lifecycle=enemy.GetComponent<ZombieLifecycle>();lifecycle.Clock=clock;enemy.GetComponent<LootOnDeath>().Player=spawner.Player.GetComponent<PlayerInventory>();return enemy;
        }
        internal static void MoveResource(ResourceNode node,Vector2 point)
        {
            foreach(var other in Object.FindObjectsByType<ResourceNode>(FindObjectsSortMode.None))if(other!=node && Vector2.Distance(other.transform.position,point)<5)other.gameObject.SetActive(false);
            node.transform.position=point;Physics2D.SyncTransforms();
        }
        internal static ResourceNode MatureTree()
        {
            foreach(var node in Object.FindObjectsByType<ResourceNode>(FindObjectsSortMode.None))if(node.Definition.Kind==ResourceKind.Wood && node.Health.Maximum==150)return node;
            throw new System.InvalidOperationException("Generated mature tree is required.");
        }
    }
}
