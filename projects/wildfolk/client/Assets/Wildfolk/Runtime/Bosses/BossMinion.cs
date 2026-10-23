using System.Collections;
using UnityEngine;
namespace Wildfolk
{
    public sealed class BossMinion:MonoBehaviour,IPoolReset
    {
        public Health Health {get;private set;}
        private EnemyController enemy;private BossEncounter owner;private bool retiring;
        private void Awake(){Health=GetComponent<Health>();enemy=GetComponent<EnemyController>();}
        private void OnEnable(){Health.Died+=Died;}
        private void OnDisable(){Health.Died-=Died;owner=null;}
        public void Initialize(BossEncounter encounter,EnemyController actor){owner=encounter;enemy=actor;retiring=false;Target(owner.Marked);}
        public void Target(Health marked){if(owner==null)return;enemy.PreferredTarget=marked;enemy.MovementMultiplier=owner.Definition.MinionSpeed;}
        public void ResetForSpawn(){owner=null;retiring=false;GetComponent<LootOnDeath>().enabled=true;enemy.enabled=true;if(enemy.Lifecycle!=null)enemy.Lifecycle.enabled=true;enemy.PreferredTarget=null;enemy.MovementMultiplier=1;}
        private void Died()
        {if(owner==null || retiring)return;var network=GetComponent<NetworkCreature>();if(network!=null){retiring=true;enemy.PreferredTarget=null;enemy.MovementMultiplier=1;network.ResolveDeath(owner.Definition.MinionRewards);return;}Retire();}
        public void Retire(){if(retiring || owner==null)return;retiring=true;enemy.PreferredTarget=null;enemy.MovementMultiplier=1;enemy.enabled=false;GetComponent<LootOnDeath>().enabled=false;StartCoroutine(Sink());}
        private IEnumerator Sink(){var rest=enemy.AttackPose.localScale;for(var t=0f;t<.5f;t+=Time.deltaTime){enemy.AttackPose.localScale=rest*(1-t/.5f);yield return null;}PrefabPool.Release(gameObject);}
    }
}
