using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    public readonly struct BeaconPoint
    {public readonly string Id;public readonly int Number;public readonly Vector2 Position;public BeaconPoint(string id,int number,Vector2 position){Id=id;Number=number;Position=position;}}
    public static class BeaconDirectory
    {
        private static readonly Dictionary<string,BeaconPoint> points=new Dictionary<string,BeaconPoint>();
        public static IReadOnlyDictionary<string,BeaconPoint> Points=>points;
        public static uint Revision {get;private set;}public static int Serial {get;private set;}
        public static int Next()=>++Serial;
        public static void Clear(){points.Clear();Serial=0;Revision++;}
        public static void Put(string id,int number,Vector2 p){if(string.IsNullOrEmpty(id))return;Serial=Mathf.Max(Serial,number);points[id]=new BeaconPoint(id,number,p);Revision++;}
        public static void Remove(string id){if(id!=null && points.Remove(id))Revision++;}
    }
    public sealed class SignalBeacon : MonoBehaviour,IPlayerInteractable
    {
        public int Number {get;private set;}
        public Transform Location=>transform;public Sprite Icon=>GetComponent<WorldPlaceable>().Definition.Item.Icon;
        private void OnEnable(){if(Number==0)Number=BeaconDirectory.Next();Publish();}
        public void Publish()=>BeaconDirectory.Put(GetComponent<WorldPlaceable>().SaveId,Number,transform.position);
        public void Restore(int number){Number=number>0 ? number : BeaconDirectory.Next();Publish();}
        public bool CanInteract(PlayerInteraction actor)=>actor.Session.IsRunning && GetComponent<WorldPlaceable>().Health.IsAlive;
        public bool Use(PlayerInteraction actor){TierThreeHud.OpenBeacon(this,actor.GetComponent<PlayerInventory>());return true;}
        public bool Recover(PlayerInventory player)
        {var p=GetComponent<WorldPlaceable>();if(!SimulationAuthority.Allows(this) || !p.Health.IsAlive || player==null || Vector2.Distance(player.transform.position,transform.position)>player.Settings.StationInteractionRange || player.Model.Add(p.Definition.Item,1)!=1)return false;BeaconDirectory.Remove(p.SaveId);PrefabPool.Release(gameObject);return true;}
    }
}
