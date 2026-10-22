using UnityEngine;
namespace Wildfolk
{
    // World-space marker reuses project art; state-driven and visible to every peer.
    public sealed class PlayerRescueMarker:MonoBehaviour
    {
        public GameObject View;
        private PlayerDeath death;
        private void Awake(){death=GetComponent<PlayerDeath>();}
        private void OnEnable(){death.Changed+=Refresh;}
        private void OnDisable(){death.Changed-=Refresh;}
        private void Start()=>Refresh();
        private void Refresh(){View.SetActive(death.State==PlayerLife.Downed || death.State==PlayerLife.Reviving);}
    }
}
