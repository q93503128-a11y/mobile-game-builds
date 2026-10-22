using UnityEngine;
using UnityEngine.UI;
namespace Wildfolk
{
    public sealed class DeathHud : MonoBehaviour
    {
        public PlayerDeath Player;
        public GameObject DeadView,DownView;
        public Text Title,Message,DownLabel,GemLabel;
        public Button Ad,Gem,Home;
        private float nextRefresh;private string lastDown;
        private void OnEnable(){L.Changed+=Refresh;}
        private void Start(){var p=Player;Player=null;if(p!=null)Bind(p);else Refresh();Ad.onClick.AddListener(Reward);Gem.onClick.AddListener(Pay);Home.onClick.AddListener(Respawn);}
        private void OnDestroy(){L.Changed-=Refresh;if(Player!=null)Player.Changed-=Refresh;Ad.onClick.RemoveListener(Reward);Gem.onClick.RemoveListener(Pay);Home.onClick.RemoveListener(Respawn);}
        public void Bind(PlayerDeath player){if(Player!=null)Player.Changed-=Refresh;Player=player;if(Player!=null)Player.Changed+=Refresh;Refresh();}
        public void Reward(){Player?.RewardedRevive();Refresh();}
        public void Pay(){Player?.GemRevive();Refresh();}
        public void Respawn(){if(Player==null)return;var net=Player.GetComponent<NetworkPlayer>();if(net!=null)net.RequestHome();else Player.HomeRespawn();Refresh();}
        private void Refresh()
        {
            var dead=Player!=null && (Player.State==PlayerLife.Dead || Player.State==PlayerLife.Respawning);
            DeadView.SetActive(dead);var multi=Player!=null && Player.GetComponent<PlayerInputCommands>().Session.Mode==SessionMode.Multiplayer;
            Ad.gameObject.SetActive(!multi);Gem.gameObject.SetActive(!multi);
            if(Player==null){DownView.SetActive(false);return;}
            Title.text=L.Text("death.title");Ad.interactable=Player.CanAd;Gem.interactable=Player.CanGem;Home.interactable=Player.State==PlayerLife.Dead;
            GemLabel.text=Player.Definition.ReviveGemCost>0 ? L.Format("death.gem_cost",Player.Definition.ReviveGemCost) : L.Text("death.gem");
            Message.text=L.Text(Player.MessageKey??(!Player.Durable ? "death.save_failed" : multi ? "death.home_hint" : Player.PaymentPending ? "death.wait" : "death.choices"));
            if(!multi && !Player.PaymentPending && !Player.AdUsed && !ReviveServices.Rewarded.Available)Message.text+="\n"+L.Text("death.ad_unavailable");
            if(!multi && Player.AdUsed)Message.text+="\n"+L.Text("death.ad_used");
            if(!multi && (Player.Definition.ReviveGemCost<=0 || !ReviveServices.Wallet.Available))Message.text+="\n"+L.Text("death.gem_unavailable");
            else if(!multi && ReviveServices.Wallet.Balance<Player.Definition.ReviveGemCost)Message.text+="\n"+L.Text("death.gem_short");
            RefreshClock();
        }
        private void Update(){if(Player==null || Time.unscaledTime<nextRefresh)return;nextRefresh=Time.unscaledTime+.1f;RefreshClock();}
        private void RefreshClock()
        {
            var down=Player.State==PlayerLife.Downed || Player.State==PlayerLife.Reviving;
            var target=down ? Player : Player.RescueTarget??Player.NearbyDowned();
            var show=down || target!=null;DownView.SetActive(show);if(!show)return;
            string text;
            if(down)text=L.Format("death.downed",Mathf.CeilToInt((float)System.Math.Max(0,Player.DownUntil-Player.Now)));
            else text=L.Text("death.hold");
            if(target!=null && target.HoldUntil>target.HoldStarted)text+="\n"+L.Format("death.progress",Mathf.RoundToInt(Mathf.Clamp01((float)((target.Now-target.HoldStarted)/(target.HoldUntil-target.HoldStarted)))*100));
            if(text==lastDown)return;lastDown=text;DownLabel.text=text;
        }
    }
}
