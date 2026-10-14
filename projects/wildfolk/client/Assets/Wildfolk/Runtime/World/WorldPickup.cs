using UnityEngine;

namespace Wildfolk
{
    public sealed class WorldPickup : MonoBehaviour,IPoolReset
    {
        public SpriteRenderer Icon;
        public Transform FloatingVisual;
        public ItemDefinition Item { get; private set; }
        public int Count { get; private set; }
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

        public void Initialize(ItemDefinition item, int count, PlayerInventory owner, GameplaySettings configuration,bool playerDropped=false)
        {
            Item = item;
            Count = count;
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
            var accepted = player.Model.Add(Item, Count);
            Count -= accepted;
            if (Count == 0) PrefabPool.Release(gameObject);
        }
    }
}
