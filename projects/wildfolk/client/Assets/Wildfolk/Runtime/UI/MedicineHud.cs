using UnityEngine;
using UnityEngine.UI;
namespace Wildfolk
{
    public sealed class MedicineHud : MonoBehaviour
    {
        public PlayerSurvival Player;
        public Text Label;
        private int previous=-1;
        private float next;
        private void OnEnable()=>L.Changed+=RefreshLanguage;
        private void OnDisable()=>L.Changed-=RefreshLanguage;
        private void RefreshLanguage(){previous=-1;next=0;}
        private void Update()
        {
            if(Time.unscaledTime<next)return;next=Time.unscaledTime+.2f;
            var seconds=Player==null ? 0 : Mathf.CeilToInt(Player.TreatmentRemaining);
            if(seconds==previous)return;previous=seconds;Label.enabled=seconds>0;
            if(seconds>0)Label.text=L.Format("ui.medicine.active",seconds);
        }
    }
}
