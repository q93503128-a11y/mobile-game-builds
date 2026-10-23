using UnityEngine;
namespace Wildfolk
{
    public sealed class RoyalTomb:MonoBehaviour,IPlayerInteractable
    {
        public BossDefinition Definition;
        public SpriteRenderer Lid,Ember;
        public PoiSite Site {get;private set;}
        public BossWorld World {get;private set;}
        public bool Unlocked=>World!=null && Site!=null && (World.Authority?World.World.Chunks.FeatureCleared(Site.GuardId):Site.ReplicaTombUnlocked);
        public Transform Location=>transform;
        public Sprite Icon=>Definition.Offering.Icon;
        public void Initialize(BossWorld world,PoiSite site,BossDefinition definition){World=world;Site=site;Definition=definition;}
        public bool CanInteract(PlayerInteraction p)=>Site!=null && p.GetComponent<Health>().IsAlive;
        public bool Use(PlayerInteraction p)
        {var net=p.GetComponent<NetworkFacilityInteractor>();if(net!=null)return net.SummonBoss(this);return World.Summon(this,p.GetComponent<PlayerInventory>(),World.Clock.Model.Day);}
        private void Update()
        {
            if(World==null)return;BossEncounter active=null;foreach(var e in World.Encounters)if(e!=null && e.Record!=null && e.Record.Tomb==Site.Id){active=e;break;}
            var age=active!=null && active.State==BossState.Awakening?active.Age:active!=null?4:0;
            var progress=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.7f,1.7f,age));
            Lid.transform.localPosition=new Vector3(progress*.5f,progress*.5f,0);Lid.transform.localRotation=Quaternion.Euler(0,0,-progress*7);
            Ember.color=new Color(.42f,.65f,.57f,active==null?0:.23f+.08f*Mathf.Sin(Time.time*4));
        }
    }
}
