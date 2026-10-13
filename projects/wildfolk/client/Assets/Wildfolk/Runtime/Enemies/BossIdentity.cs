using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wildfolk
{
    [RequireComponent(typeof(Health))]
    public sealed class BossIdentity : MonoBehaviour
    {
        public string DisplayName;
        public Health Health { get; private set; }
        private static readonly List<BossIdentity> active=new List<BossIdentity>();
        public static event Action ActiveChanged;
        public static BossIdentity Active
        {
            get
            {
                for(var i=0;i<active.Count;i++) if(active[i]!=null && active[i].Health.IsAlive) return active[i];
                return null;
            }
        }

        private void Awake() => Health=GetComponent<Health>();
        private void OnEnable() { active.Add(this); Health.Died+=Notify; ActiveChanged?.Invoke(); }
        private void OnDisable() { active.Remove(this); Health.Died-=Notify; ActiveChanged?.Invoke(); }
        private void Notify() => ActiveChanged?.Invoke();
    }
}
