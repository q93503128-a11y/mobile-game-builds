using UnityEngine;
namespace Wildfolk
{
    public sealed class WorldGenerator : MonoBehaviour
    {
        public WorldGenerationDefinition Definition;
        public WorldTerrainRenderer Terrain;
        public WorldSpawnRegion Region;
        public Transform Environment;
        public PlayerInventory Player;
        public GameplaySession Session;
        public WorldPickup Pickup;
        public GameplaySettings Settings;
        public WaterSourceDefinition Water;
        public Material SpriteMaterial;
        public Sprite Grass,Fern;
        public ResourceRegeneration Renewal;
        public AnimalPopulation Animals;
        public WorldMap Map { get; private set; }
        public int Seed=>Map.Seed;
        public float GenerationMilliseconds { get; private set; }
        public static int? SelectedSeed { get; set; }
        public static int RandomSeed()=>System.Guid.NewGuid().GetHashCode();
        private void Awake()=>Generate(SelectedSeed ?? Definition.DefaultSeed);
        public void Generate(int seed)
        {
            var regenerate=Map!=null;
            var started=System.Diagnostics.Stopwatch.StartNew();
            for(var i=Environment.childCount-1;i>=0;i--){var child=Environment.GetChild(i).gameObject;child.SetActive(false);Destroy(child);}
            Map=new WorldMap(Definition,seed);Renewal.ResetWorld();Region.Size=Definition.Size-Vector2.one*3;Terrain.Build(Map);
            Player.GetComponent<Rigidbody2D>().position=Map.Spawn;Player.transform.position=Map.Spawn;
            for(var index=0;index<Map.Resources.Count;index++)
            {var placement=Map.Resources[index];CreateResource("r"+index,placement.Rule,placement.Position,placement.Scale);}
            WaterGeometry();Decorations(seed);Animals.Generate();
            if(regenerate)GetComponent<GoblinSpawner>().GenerateCamps();
            started.Stop();GenerationMilliseconds=(float)started.Elapsed.TotalMilliseconds;
        }
        public ResourceNode CreateResource(string id,int rule,Vector2 position,float scale)
        {
            var resource=Instantiate(Definition.Resources[rule].Prefab,position,Quaternion.identity,Environment);
            resource.transform.localScale=Vector3.one*scale;resource.Player=Player;resource.PickupPrefab=Pickup;resource.Settings=Settings;
            var number=int.Parse(id.Substring(1));
            if(Definition.Resources[rule].AlternateSprite!=null && number%3==0)resource.Visual.sprite=Definition.Resources[rule].AlternateSprite;
            Renewal.Register(resource,id,rule);return resource;
        }
        private GameObject Child(string name){var child=new GameObject(name);child.transform.SetParent(Environment,false);return child;}
        private void Polygon(string name,Vector2[] points){Child(name).AddComponent<PolygonCollider2D>().points=points;}
        private void RiverPart(float from,float to)
        {
            var segments=Mathf.CeilToInt(to-from);var points=new Vector2[(segments+1)*2];
            for(var i=0;i<=segments;i++){var y=Mathf.Lerp(from,to,(float)i/segments);var x=Map.RiverAt(y);points[i]=new Vector2(x-Definition.RiverWidth,y);points[points.Length-1-i]=new Vector2(x+Definition.RiverWidth,y);}
            Polygon("Deep River",points);
        }
        private void Lake(string name,Vector2 center,Vector2 radius)
        {
            var points=new Vector2[64];for(var i=0;i<points.Length;i++){var angle=i*Mathf.PI*2/points.Length;points[i]=Map.ShorePoint(center,radius,angle);}
            Polygon(name,points);
            for(var i=0;i<12;i++){var angle=i*Mathf.PI*2/12;Source(Map.ShorePoint(center,radius,angle)+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*.8f);}
        }
        private void WaterGeometry()
        {
            RiverPart(-Definition.Size.y*.5f,Map.FordY-WorldMap.FordHalfWidth);RiverPart(Map.FordY+WorldMap.FordHalfWidth,Definition.Size.y*.5f);
            Lake("Deep Lake",Map.LakeCenter,Map.LakeRadius);Lake("Deep Pond",Map.PondCenter,Map.PondRadius);
            for(var y=-Definition.Size.y*.5f+6;y<Definition.Size.y*.5f-5;y+=5)
            {var x=Map.RiverAt(y);Source(new Vector2(x-Definition.RiverWidth-.8f,y));Source(new Vector2(x+Definition.RiverWidth+.8f,y));}
            var half=Definition.Size*.5f;
            Boundary("North",new Vector2(0,half.y),new Vector2(Definition.Size.x+2,1));Boundary("South",new Vector2(0,-half.y),new Vector2(Definition.Size.x+2,1));
            Boundary("West",new Vector2(-half.x,0),new Vector2(1,Definition.Size.y));Boundary("East",new Vector2(half.x,0),new Vector2(1,Definition.Size.y));
        }
        private void Source(Vector2 position)
        {
            if(!Map.Bounds.Contains(position))return;var child=Child("Water Source");child.transform.position=position;
            var source=child.AddComponent<WaterSource>();source.Definition=Water;var collider=child.AddComponent<CircleCollider2D>();collider.radius=.4f;collider.isTrigger=true;
        }
        private void Boundary(string name,Vector2 position,Vector2 size){var child=Child(name);child.transform.position=position;child.AddComponent<BoxCollider2D>().size=size;}
        private void Decorations(int seed)
        {
            var random=new System.Random(seed^13713);
            for(var i=0;i<Definition.DecorationCount;i++)
            {
                var near=i<40;
                var p=new Vector2(((float)random.NextDouble()-.5f)*(near ? 32 : Definition.Size.x),((float)random.NextDouble()-.5f)*(near ? 20 : Definition.Size.y));
                if(Map.IsWater(p,.8f) || Map.IsRoad(p))continue;
                var child=Child("Ground Foliage");child.transform.position=p;child.transform.localScale=Vector3.one*(.4f+(float)random.NextDouble()*.3f);
                var visual=child.AddComponent<SpriteRenderer>();visual.sprite=i%4==0 ? Fern : Grass;visual.sharedMaterial=SpriteMaterial;visual.sortingOrder=WorldSorting.GroundDecoration;
            }
        }
    }
}
