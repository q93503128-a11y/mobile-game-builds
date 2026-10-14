using System;
using UnityEditor;
using UnityEngine;
namespace Wildfolk.Editor
{
    public static class DeveloperHarness
    {
        public static WorldSaveService Context()
        {if(!EditorApplication.isPlaying)throw new InvalidOperationException("Enter Play Mode first.");var service=UnityEngine.Object.FindFirstObjectByType<WorldSaveService>();if(service==null || service.World.Map==null)throw new InvalidOperationException("The world is not ready.");service.AutoSaveEnabled=false;return service;}
        public static int GiveItem(string id,int amount){var service=Context();return service.Player.Model.Add(service.Catalog.Item(id),amount);}
        public static void GiveAllEquipment(){var service=Context();foreach(var item in service.Catalog.Items)if(item.Tool!=null)service.Player.Model.Add(item,1);}
        public static void SetHealth(int hp){var health=Context().Player.GetComponent<Health>();hp=Mathf.Clamp(hp,0,health.Maximum);if(hp==0 && health.IsAlive)health.ApplyDamage(health.Current,true);else health.RestoreHealth(hp);}
        public static void SetNeeds(float hunger,float thirst)=>Context().Player.GetComponent<PlayerSurvival>().Model.SetMeters(hunger,thirst);
        public static void SetProgression(long xp,long gold)=>Context().Player.GetComponent<PlayerProgression>().SetValues(xp,gold);
        public static void SetTime(int day,float cycleTime){var clock=Context().Clock;clock.Model.SetElapsed((Mathf.Max(1,day)-1)*clock.Definition.CycleSeconds+Mathf.Clamp(cycleTime,0,clock.Definition.CycleSeconds-.001f));}
        public static void Teleport(Vector2 p)
        {var service=Context();if(!service.World.Map.IsWalkable(p,.6f))throw new ArgumentException("Choose traversable terrain.");var body=service.Player.GetComponent<Rigidbody2D>();body.position=p;body.linearVelocity=Vector2.zero;service.Player.transform.position=p;Physics2D.SyncTransforms();}
        public static void GenerateWorld(int seed){var service=Context();foreach(var placed in UnityEngine.Object.FindObjectsByType<WorldPlaceable>(FindObjectsSortMode.None)){placed.gameObject.SetActive(false);UnityEngine.Object.Destroy(placed.gameObject);}ClearEnemies(10000);service.World.Generate(seed);}
        public static void Regenerate()=>GenerateWorld(Context().World.Seed);
        public static void ClearEnemies(float radius=30)
        {var player=Context().Player.transform;foreach(var health in UnityEngine.Object.FindObjectsByType<Health>(FindObjectsSortMode.None))if((health.GetComponent<EnemyController>()!=null || health.GetComponent<GoblinController>()!=null) && Vector2.Distance(health.transform.position,player.position)<=radius)PrefabPool.Release(health.gameObject);}
        public static Component Spawn(string kind,Vector2 p)
        {
            var service=Context();if(!service.World.Map.IsWalkable(p,.7f))throw new ArgumentException("Choose traversable terrain.");
            if(kind=="Zombie")
            {
                var spawner=service.GetComponent<ZombieSpawner>();SetTime(service.Clock.Model.Day,service.Clock.Definition.DaySeconds+service.Clock.Definition.EveningSeconds+1);
                var enemy=PrefabPool.Spawn(spawner.Prefab,p,Quaternion.identity);enemy.Session=service.World.Session;enemy.PlayerHealth=service.Player.GetComponent<Health>();enemy.Clock=service.Clock;enemy.Lifecycle.Clock=service.Clock;enemy.Lifecycle.Definition=spawner.Definition;enemy.GetComponent<LootOnDeath>().Player=service.Player;spawner.Register(enemy);return enemy;
            }
            if(kind=="Goblin")
            {var spawner=service.GetComponent<GoblinSpawner>();var goblin=PrefabPool.Spawn(spawner.Prefab,p,Quaternion.identity);goblin.Session=service.World.Session;goblin.PlayerHealth=service.Player.GetComponent<Health>();goblin.GetComponent<LootOnDeath>().Player=service.Player;spawner.Register(goblin);return goblin;}
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/"+kind+".prefab");if(prefab==null || prefab.GetComponent<NeutralAnimal>()==null)throw new ArgumentException("Choose a current creature.");
            var animal=UnityEngine.Object.Instantiate(prefab,p,Quaternion.identity).GetComponent<NeutralAnimal>();animal.Player=service.Player.transform;animal.Session=service.World.Session;animal.Region=service.World.Region;animal.GetComponent<LootOnDeath>().Player=service.Player;return animal;
        }
        public static void LoadProfile(DeveloperTestProfile profile)
        {
            var service=Context();var empty=new SavedStack[service.Player.Model.Capacity];for(var i=0;i<empty.Length;i++)empty[i]=new SavedStack();service.Player.Model.Restore(empty,service.Catalog);
            foreach(var stack in profile.Items)service.Player.Model.Add(stack.Item,stack.Count);service.Player.SelectSlot(0);SetHealth(service.Player.GetComponent<Health>().Maximum);SetNeeds(profile.Hunger,profile.Thirst);SetProgression(profile.XP,profile.Gold);
        }
        [MenuItem("Wildfolk/Developer/Open Combat Sandbox")]
        public static void OpenCombatSandbox()
        {
            var service=Context();ClearEnemies();service.GetComponent<ZombieSpawner>().enabled=false;service.GetComponent<GoblinSpawner>().enabled=false;service.World.Renewal.enabled=false;service.World.Animals.enabled=false;SetTime(1,30);service.Clock.enabled=false;Teleport(Vector2.zero);
            foreach(var resource in UnityEngine.Object.FindObjectsByType<ResourceNode>(FindObjectsSortMode.None))if(resource.transform.position.sqrMagnitude<144)resource.gameObject.SetActive(false);
            var profiles=AssetDatabase.LoadAssetAtPath<DeveloperTestProfile>("Assets/Wildfolk/Data/DeveloperProfile3.asset");LoadProfile(profiles);
            for(var rule=0;rule<3;rule++)
            {var resource=UnityEngine.Object.Instantiate(service.World.Definition.Resources[rule].Prefab,new Vector2((rule-1)*4,3),Quaternion.identity);resource.Player=service.Player;}
            for(var i=0;i<2;i++)
            {var target=new GameObject("Combat Sandbox Target");target.SetActive(false);var health=target.AddComponent<Health>();health.Definition=service.Player.GetComponent<Health>().Definition;target.AddComponent<CircleCollider2D>().radius=.55f;var visual=target.AddComponent<SpriteRenderer>();visual.sprite=VisualArtImporter.Sprite("player_body");visual.color=new Color(.55f,.45f,.3f);visual.sharedMaterial=service.World.SpriteMaterial;target.transform.position=new Vector2(i*2,-3);target.SetActive(true);}
        }
    }
}
