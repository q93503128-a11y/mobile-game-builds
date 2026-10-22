using UnityEngine;
namespace Wildfolk
{
    public enum ExplorationToolCategory { None,Optic,Survey,Signal }
    public interface IAimableExplorationTool {bool Observing{get;}Vector2 ObservedPoint{get;}float EffectiveRange{get;}}
    public sealed class PlayerTeamMarker:MonoBehaviour
    {
        public Vector2 Point {get;private set;}
        public double Until {get;private set;}
        public byte Meaning {get;private set;}
        public string ObservedType {get;private set;}
        public event System.Action Changed;
        private PlayerClass role;private PlayerInventory bag;private PlayerInputCommands commands;private WorldGenerator world;
        private double ready;private readonly RaycastHit2D[] barriers=new RaycastHit2D[64];
        private void Awake(){role=GetComponent<PlayerClass>();bag=GetComponent<PlayerInventory>();commands=GetComponent<PlayerInputCommands>();}
        public bool Available=>role.CanMark && GetComponent<IAimableExplorationTool>()?.Observing==true && bag.SelectedStack.Item?.ExplorationCategory==ExplorationToolCategory.Optic;
        public bool Place(Vector2 point,byte meaning)
        {
            if(!SimulationAuthority.Allows(this) || !role.CanMark || !commands.Session.IsRunning || GetComponent<PlayerDeath>()?.CanAct!=true || !Available || !float.IsFinite(point.x) || !float.IsFinite(point.y) || role.Now<ready || meaning>2 || meaning!=0 && role.Value(ClassEffect.SignalMeaning)<=0)return false;
            if(world==null)world=FindFirstObjectByType<WorldGenerator>();var tool=GetComponent<IAimableExplorationTool>();var from=(Vector2)transform.position;var delta=point-from;
            if(world?.Map==null || !world.Map.Contains(point) || !PlayerExplorationUtility.SameRegion(world.Map,from,point) || delta.magnitude>tool.EffectiveRange+.5f || delta.magnitude<tool.EffectiveRange-.5f)return false;
            // Reticle point may be on visible land or water, but cannot reveal through cave walls or barriers.
            for(var d=1f;d<delta.magnitude;d+=1)if(!world.Map.IsWalkable(from+delta.normalized*d,0))return false;
            var n=Physics2D.Raycast(from,delta.normalized,new ContactFilter2D{useTriggers=false},barriers,delta.magnitude);if(n==barriers.Length)return false;
            for(var i=0;i<n;i++)if(barriers[i].collider.GetComponentInParent<Health>()!=GetComponent<Health>())return false;
            Point=point;Until=role.Now+60+role.Value(ClassEffect.MarkerDuration);Meaning=meaning;ready=role.Now+1;ReadObservedType(point);Changed?.Invoke();GetComponent<NetworkClassActions>()?.Publish();return true;
        }
        public void ReadObservedType(Vector2 point)
        {
            ObservedType=null;if(role.Value(ClassEffect.PoiReading)<=0)return;
            // Only existing static presentation/active landmark objects in the current camera footprint.
            var camera=Camera.main;if(camera==null)return;
            foreach(var landmark in FindObjectsByType<ObservedLandmark>(FindObjectsSortMode.None))
            {var p=landmark.transform.position;var viewport=camera.WorldToViewportPoint(p);if(viewport.z>0 && viewport.x>=0 && viewport.x<=1 && viewport.y>=0 && viewport.y<=1 && Vector2.Distance(p,point)<12){ObservedType=L.Text("class.poi."+landmark.Kind);break;}}
        }
        public void Clear(){Point=default;Until=ready=0;ObservedType=null;Changed?.Invoke();}
        private void Update(){if(!SimulationAuthority.Allows(this) || Until<=0 || role.Now<Until)return;Until=0;Changed?.Invoke();GetComponent<NetworkClassActions>()?.Publish();}
        private void OnDisable()=>Clear();
    }
}
