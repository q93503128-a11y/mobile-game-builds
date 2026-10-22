using UnityEngine;
namespace Wildfolk
{
    public sealed class ClassWorldFeedback:MonoBehaviour
    {
        public Sprite Icon;private SpriteRenderer marker;private PlayerClass role;private float next;
        private void Awake(){role=GetComponent<PlayerClass>();var go=new GameObject("Quarry Mark");marker=go.AddComponent<SpriteRenderer>();marker.sprite=Icon;marker.sortingOrder=WorldSorting.Overlay;marker.color=new Color32(233,181,103,255);go.transform.SetParent(transform,false);go.transform.localScale=Vector3.one*.35f;go.SetActive(false);}
        private void OnDisable(){if(marker!=null)marker.gameObject.SetActive(false);}
        private void Update()
        {
            if(Time.unscaledTime<next)return;next=Time.unscaledTime+.1f;var net=GetComponent<NetworkClassActions>();Health target=role.Quarry;
            if(net!=null && net.IsSpawned && !net.IsServer){target=null;var id=net.State.Value.VisibleQuarry;if(net.NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(id,out var obj))target=obj.GetComponent<Health>();}
            var visible=target!=null && target.IsAlive && role.Has(ClassTrait.Quarry) && ClassActionRules.Clear(GetComponent<Health>(),target);marker.gameObject.SetActive(visible);if(visible)marker.transform.position=target.transform.position+Vector3.up*1.4f;
        }
    }
}
