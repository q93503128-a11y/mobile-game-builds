using UnityEngine;
using UnityEngine.UI;

namespace Wildfolk
{
    public sealed class MultiplayerArena : MonoBehaviour
    {
        public CameraFollow Follow;
        public NetworkInventoryUi InventoryUi;
        public VirtualJoystick Joystick;
        public Text HealthLabel,CampLabel;
        public Button DashButton;
        public NetworkPlayer LocalPlayer {get;private set;}
        private void Start(){SceneFlow.MultiplayerReady();if(NetworkSession.Instance!=null){NetworkSession.Instance.ArenaReady();NetworkSession.Instance.Changed+=RefreshCamp;}RefreshCamp();}
        private void OnEnable()=>L.Changed+=RefreshText;
        private void OnDisable()=>L.Changed-=RefreshText;
        private void OnDestroy(){if(LocalPlayer!=null)LocalPlayer.Health.Changed-=RefreshHealth;if(NetworkSession.Instance!=null)NetworkSession.Instance.Changed-=RefreshCamp;}
        public void BindLocal(NetworkPlayer player)
        {
            LocalPlayer=player;Follow.Target=player.transform;Joystick.Session=player.Commands.Session;player.GetComponent<PlayerInputAdapter>().Joystick=Joystick;Joystick.gameObject.SetActive(true);DashButton.interactable=true;
            var light=FindFirstObjectByType<WorldLighting>();if(light!=null)light.Player=player.transform;
            FindFirstObjectByType<DeathHud>()?.Bind(player.GetComponent<PlayerDeath>());InventoryUi.Bind(player);player.Health.Changed+=RefreshHealth;RefreshText();
            var medicine=FindFirstObjectByType<MedicineHud>();if(medicine!=null)medicine.Player=player.GetComponent<PlayerSurvival>();
            var meal=FindFirstObjectByType<MealHud>();if(meal!=null)meal.Player=player.GetComponent<PlayerSurvival>();
        }
        public void Dash(){if(LocalPlayer!=null)LocalPlayer.Commands.Dash();}
        public void Leave(){if(NetworkSession.Instance!=null){NetworkSession.Instance.Leave();}else SceneFlow.Home(SceneFlow.Repository);}
        private void RefreshText(){RefreshHealth();RefreshCamp();}
        private void RefreshHealth(){HealthLabel.text=LocalPlayer==null ? string.Empty : L.Format("net.health",LocalPlayer.Health.Current,LocalPlayer.Health.Maximum);}
        private void RefreshCamp()=>CampLabel.text=L.Text(NetworkSession.Instance!=null && NetworkSession.Instance.MessageKey=="net.save_failed" ? "net.save_failed" : "net.camp");
    }
}
