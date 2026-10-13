using System;
using UnityEngine;
namespace Wildfolk
{
    public sealed class PlayerStorage : MonoBehaviour
    {
        public GameplaySession Session;
        public GameplaySettings Settings;
        public StorageChest Chest { get; private set; }
        public bool IsOpen => Chest!=null;
        public event Action Changed;
        private void OnEnable() => Session.StateChanged+=StateChanged;
        private void OnDisable() => Session.StateChanged-=StateChanged;
        private void Update()
        {
            if(Session.State==SessionState.Inventory && opened && (Chest==null || !Chest.InRange(transform.position,Settings.StationInteractionRange)))
            {Clear();Session.CloseInventory();}
        }
        private bool opened;
        public bool OpenChest(StorageChest chest)
        {
            if(!Session.IsRunning || chest==null || !chest.InRange(transform.position,Settings.StationInteractionRange)) return false;
            chest.BindVisitor(GetComponent<PlayerInventory>());Chest=chest;opened=true;Changed?.Invoke();Session.OpenInventory();return true;
        }
        private void StateChanged() { if(Session.State!=SessionState.Inventory) Clear(); }
        private void Clear() {if(!opened)return;Chest=null;opened=false;Changed?.Invoke();}
    }
}
