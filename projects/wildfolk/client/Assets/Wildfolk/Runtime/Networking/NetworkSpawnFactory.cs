using UnityEngine;
namespace Wildfolk
{
    public static class NetworkSpawnFactory
    {
        public static T Spawn<T>(T prefab,Vector3 position,Quaternion rotation,Transform parent=null) where T:Component
        {
            var world=NetworkCreatureWorld.Active;
            if(world==null)return Object.Instantiate(prefab,position,rotation,parent);
            return world.Create(prefab,position,rotation,parent);
        }
    }
}
