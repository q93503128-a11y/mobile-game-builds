using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    public interface IPoolReset {void ResetForSpawn();}
    public sealed class PrefabPool : MonoBehaviour
    {
        private static PrefabPool current;
        private readonly Dictionary<GameObject,Stack<GameObject>> available=new Dictionary<GameObject,Stack<GameObject>>();
        private void Awake()=>current=this;
        private void OnDestroy(){if(current==this)current=null;}
        public static T Spawn<T>(T prefab,Vector3 position,Quaternion rotation) where T:Component
        {
            if(NetworkCreatureWorld.Active!=null && (prefab.GetComponent<CreatureMember>()!=null || prefab.GetComponent<StoneProjectile>()!=null || prefab.GetComponent<WorldPickup>()!=null))return NetworkSpawnFactory.Spawn(prefab,position,rotation);
            if(current==null)return Object.Instantiate(prefab,position,rotation);
            var key=prefab.gameObject;
            if(!current.available.TryGetValue(key,out var stack)){stack=new Stack<GameObject>();current.available.Add(key,stack);}
            GameObject go=null;while(stack.Count>0 && go==null)go=stack.Pop();
            if(go==null){go=Object.Instantiate(key,position,rotation);var member=go.AddComponent<PoolMember>();member.Owner=current;member.Prefab=key;}
            else
            {
                go.transform.SetParent(null,false);go.transform.SetPositionAndRotation(position,rotation);
                var body=go.GetComponent<Rigidbody2D>();if(body!=null){body.position=position;body.linearVelocity=Vector2.zero;}
                var health=go.GetComponent<Health>();if(health!=null)health.ResetForSpawn();
                foreach(var reset in go.GetComponentsInChildren<IPoolReset>(true))reset.ResetForSpawn();
                go.SetActive(true);
            }
            return go.GetComponent<T>();
        }
        public static void Release(GameObject go)
        {
            var network=go.GetComponent<Unity.Netcode.NetworkObject>();
            if(network!=null){if(network.IsSpawned && network.NetworkManager.IsServer)network.Despawn();else if(!network.IsSpawned && NetworkCreatureWorld.Active!=null)NetworkCreatureWorld.Active.CancelPending(go);return;}
            var member=go.GetComponent<PoolMember>();
            if(member==null || member.Owner==null){Object.Destroy(go);return;}
            if(!go.activeSelf)return;go.SetActive(false);go.transform.SetParent(member.Owner.transform,false);
            var stack=member.Owner.available[member.Prefab];
            if(stack.Count<32)stack.Push(go);else Object.Destroy(go);
        }
    }
}
