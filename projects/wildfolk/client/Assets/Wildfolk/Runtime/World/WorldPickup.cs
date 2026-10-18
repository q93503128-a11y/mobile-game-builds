using UnityEngine;

namespace Wildfolk
{
    public sealed class WorldPickup : MonoBehaviour,IPoolReset
    {
        public static readonly System.Collections.Generic.List<WorldPickup> Active=new System.Collections.Generic.List<WorldPickup>();
        private void OnEnable(){if(!Active.Contains(this))Active.Add(this);}
        private void OnDisable()=>Active.Remove(this);
        public SpriteRenderer Icon;
        public Transform FloatingVisual;
        public ItemDefinition Item { get; private set; }
        public int Count { get; private set; }
        public int Charges {get;private set;}
        public LiquidState Quality {get;private set;}
        private PlayerInventory player;
        private GameplaySettings settings;
        private Vector3 origin;
        private Vector3 destination;
        private Vector3 restVisual;
        private float age;
        private float nextPickupAttempt;
        private bool waitForExit;
        private void Awake()=>restVisual=FloatingVisual.localPosition;
        public void ResetForSpawn(){Item=null;Count=0;player=null;age=nextPickupAttempt=0;waitForExit=false;FloatingVisual.localPosition=restVisual;}

        public void Initialize(ItemDefinition item, int count, PlayerInventory owner, GameplaySettings configuration,bool playerDropped=false,int charges=0,LiquidState quality=LiquidState.Empty)
        {
            var network=GetComponent<NetworkGroundItem>();if(network!=null){network.Configure(item,count,owner,configuration,playerDropped,charges,quality);return;}
            Item = item;
            Count = count;Charges=charges;Quality=quality;
            player = owner;
            settings = configuration;
            Icon.sprite = item.Icon;
            origin = transform.position;
            var direction = Random.insideUnitCircle.normalized;
            if (direction.sqrMagnitude < .1f) direction = Vector2.right;
            destination = playerDropped ? origin : origin + (Vector3)(direction * settings.DropScatter);
            FloatingVisual.localPosition=restVisual;nextPickupAttempt=0;
            age = 0;
            waitForExit=playerDropped;
        }

        public void ApplyNetwork(ItemDefinition item,int count,int charges=0,LiquidState quality=LiquidState.Empty){Item=item;Count=count;Charges=charges;Quality=quality;Icon.sprite=item.Icon;}
        public void PresentNetwork(Vector2 from,Vector2 to,float elapsed,GameplaySettings config)
        {
            var t=Mathf.Clamp01(elapsed/config.DropTravelTime);transform.position=Vector2.Lerp(from,to,1-(1-t)*(1-t));
            FloatingVisual.localPosition=restVisual+Vector3.up*(t<1 ? Mathf.Sin(t*Mathf.PI)*.4f : Mathf.Sin(elapsed*3)*.035f);
        }
        private void Update()
        {
            if (Item == null || player == null || Time.timeScale == 0) return;
            age += Time.deltaTime;
            var t = Mathf.Clamp01(age / settings.DropTravelTime);
            transform.position = Vector3.Lerp(origin, destination, 1 - (1-t)*(1-t));
            var height = t < 1 ? Mathf.Sin(t * Mathf.PI) * .4f : Mathf.Sin(age * 3) * .035f;
            FloatingVisual.localPosition = restVisual + Vector3.up * height;
            if(waitForExit){if(Vector2.Distance(transform.position,player.transform.position)>settings.PickupRadius+.5f)waitForExit=false;return;}
            if (age < settings.DropPickupDelay || Time.time < nextPickupAttempt) return;
            if (Vector2.Distance(transform.position, player.transform.position) > settings.PickupRadius) return;
            nextPickupAttempt = Time.time + .15f;
            var accepted = player.Model.Add(Item, Count,Charges,Quality);
            if(accepted>0)PlayerFeedback.ShowPickup(Item,accepted);
            Count -= accepted;
            if (Count == 0) PrefabPool.Release(gameObject);
        }
    }
}
