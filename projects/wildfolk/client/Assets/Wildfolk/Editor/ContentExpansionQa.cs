using System;
using System.Linq;
using UnityEngine;
namespace Wildfolk.Editor
{
    // Small reusable Editor fixture. No player-facing controls or original save/profile access.
    public static class ContentExpansionQa
    {
        public static WorldSaveService S=>DeveloperHarness.Context();
        public static PlayerInventory Bag=>S.Player;
        public static WorldGenerator World=>S.World;
        public static CraftingStation Bench;
        public static Health Target;
        public static string SavedBuildingId;
        public static int HealthBefore;
        public static void Prepare()
        {
            if(!DevelopmentProfile.Isolated || !S.Repository.Path.StartsWith(DevelopmentProfile.Directory,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Isolated QA save required");
            S.AutoSaveEnabled=false;S.Clock.enabled=false;Bag.GetComponent<PlayerMotor>().enabled=false;Bag.GetComponent<PlayerInputAdapter>().enabled=false;
            World.GetComponent<ZombieSpawner>().enabled=World.GetComponent<GoblinSpawner>().enabled=false;World.Chunks.DrainPending();
        }
        public static void Move(Vector2 position)
        {World.Session.CloseInventory();var body=Bag.GetComponent<Rigidbody2D>();body.position=position;body.linearVelocity=Vector2.zero;Bag.transform.position=position;Physics2D.SyncTransforms();World.Chunks.Reconcile();World.Chunks.DrainPending();}
        public static int Select(string id,bool give=true)
        {var item=S.Catalog.Item(id);if(give && Bag.Model.CountItem(item)==0)Bag.Model.Add(item,1);for(var i=0;i<Bag.Model.Capacity;i++)if(Bag.Model.GetSlot(i).Item==item){if(i>=9){var empty=Enumerable.Range(0,9).FirstOrDefault(j=>Bag.Model.GetSlot(j).IsEmpty);Bag.Model.MoveStack(i,empty);i=empty;}Bag.SelectSlot(i);return i;}throw new InvalidOperationException("Missing item: "+id);}
        public static void Clear(Vector2 p,float radius=7)
        {foreach(var r in UnityEngine.Object.FindObjectsByType<ResourceNode>(FindObjectsSortMode.None))if(Vector2.Distance(p,r.transform.position)<radius)r.gameObject.SetActive(false);Physics2D.SyncTransforms();}
        public static WorldPlaceable Place(string id,Vector2 p)
        {return UnityEngine.Object.Instantiate(S.Catalog.Item(id).Placement.Prefab,p,Quaternion.identity);}
        public static void OpenBench(int tier=1)
        {if(Bench==null){Clear((Vector2)Bag.transform.position+Vector2.right*2,3);Bench=Place("workbench",(Vector2)Bag.transform.position+Vector2.right*2).GetComponent<CraftingStation>();}Move((Vector2)Bench.transform.position+Vector2.left*2);var craft=Bag.GetComponent<PlayerCrafting>();craft.OpenStation(Bench);if(tier==2 && Bench.Definition.WorkbenchTier==1){foreach(var cost in Bench.Definition.UpgradeCosts)Bag.Model.Add(cost.Item,cost.Count);if(!craft.UpgradeWorkbench())throw new InvalidOperationException("Workbench upgrade failed");Bench=WorldPlaceable.Active.First(p=>p.Definition.Station==StationKind.Workbench && p.Definition.WorkbenchTier==2).GetComponent<CraftingStation>();craft.OpenStation(Bench);}}
        public static int Craft(string id)
        {var recipe=S.Catalog.Recipe(id);foreach(var cost in recipe.Ingredients){var missing=cost.Count-(int)Bag.Model.CountAmount(cost);if(missing>0)Bag.Model.TryAddAmounts(new[]{new ItemAmount{Item=cost.Item,Count=missing,Charges=cost.Charges,Quality=cost.Quality}});}var before=Bag.Model.CountItem(recipe.Result);var panel=UnityEngine.Object.FindFirstObjectByType<CraftingPanel>();var card=panel.Cards.Single(c=>c.Recipe==recipe);card.CraftButton.onClick.Invoke();return checked((int)(Bag.Model.CountItem(recipe.Result)-before));}
        public static void TargetAt(float distance)
        {var candidate=UnityEngine.Object.FindObjectsByType<NeutralAnimal>(FindObjectsSortMode.None).OrderByDescending(a=>a.GetComponent<Health>().Maximum).First();candidate.enabled=false;candidate.GetComponent<Rigidbody2D>().bodyType=RigidbodyType2D.Kinematic;candidate.transform.position=Bag.transform.position+Vector3.right*distance;Target=candidate.GetComponent<Health>();Target.RestoreHealth(Target.Maximum);HealthBefore=Target.Current;Physics2D.SyncTransforms();}
        public static void Capture(string name)
        {System.IO.Directory.CreateDirectory("Captures/content-expansion");ScreenCapture.CaptureScreenshot("Captures/content-expansion/"+name+".png");}
    }
}
