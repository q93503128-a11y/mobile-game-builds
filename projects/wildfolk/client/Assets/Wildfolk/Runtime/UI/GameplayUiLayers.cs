using UnityEngine;
namespace Wildfolk
{
    public sealed class GameplayUiLayers : MonoBehaviour
    {
        public GameplaySession Session;
        public CanvasGroup Gameplay;
        private void OnEnable()=>Session.StateChanged+=Refresh;
        private void OnDisable(){if(Session!=null)Session.StateChanged-=Refresh;}
        private void Start()=>Refresh();
        private void Refresh(){Gameplay.interactable=Gameplay.blocksRaycasts=Session.IsRunning;}
    }
}
