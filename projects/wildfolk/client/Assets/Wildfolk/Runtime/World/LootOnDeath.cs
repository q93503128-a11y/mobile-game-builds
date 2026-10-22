using System.Collections;
using UnityEngine;
namespace Wildfolk
{
    public sealed class LootOnDeath : MonoBehaviour,IPoolReset
    {
        public Health Health;
        public LootTable Table;
        public PlayerInventory Player;
        public WorldPickup Pickup;
        public GameplaySettings Settings;
        private string rewardReceipt=System.Guid.NewGuid().ToString("N");
        private Collider2D[] colliders;private bool[] colliderStates;
        private Behaviour[] simulation;private bool[] simulationStates;private bool deathPresented;
        private void Awake(){colliders=GetComponents<Collider2D>();colliderStates=new bool[colliders.Length];for(var i=0;i<colliders.Length;i++)colliderStates[i]=colliders[i].enabled;simulation=new Behaviour[]{GetComponent<NeutralAnimal>(),GetComponent<EnemyController>(),GetComponent<GoblinController>(),GetComponent<SlingshotAttack>(),GetComponent<ZombieLifecycle>(),GetComponent<WildlifeActionBrain>()};simulationStates=new bool[simulation.Length];}
        public void ResetForSpawn(){rewardReceipt=System.Guid.NewGuid().ToString("N");for(var i=0;i<colliders.Length;i++)colliders[i].enabled=colliderStates[i];if(deathPresented)for(var i=0;i<simulation.Length;i++)if(simulation[i]!=null)simulation[i].enabled=simulationStates[i];deathPresented=false;}
        private void OnEnable()=>Health.Died+=Drop;
        private void OnDisable()=>Health.Died-=Drop;
        private void Drop()
        {
            if(deathPresented)return;
            var network=GetComponent<NetworkCreature>();if(network!=null){network.ResolveDeath(Table);return;}
            foreach(var entry in Table.Roll())
            {var drop=PrefabPool.Spawn(Pickup,transform.position,Quaternion.identity);drop.Initialize(entry.Item,entry.Count,Player,Settings);}
            foreach(var collider in colliders)collider.enabled=false;
            deathPresented=true;for(var i=0;i<simulation.Length;i++)if(simulation[i]!=null){simulationStates[i]=simulation[i].enabled;simulation[i].enabled=false;}
            var body=GetComponent<Rigidbody2D>();if(body!=null)body.linearVelocity=Vector2.zero;
            GetComponent<HitFeedback>()?.BeginDeath();
            if(Health.LastAttacker!=null){var reward=Health.LastAttacker.GetComponent<PlayerProgression>();if(reward!=null)reward.GameplayReward(Table.Experience,Table.Gold,rewardReceipt);}
            StartCoroutine(RemoveDead());
        }
        private IEnumerator RemoveDead(){yield return new WaitForSeconds(HitFeedback.DeathDuration);PrefabPool.Release(gameObject);}
    }
}
