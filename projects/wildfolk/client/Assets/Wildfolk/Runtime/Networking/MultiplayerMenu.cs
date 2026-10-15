using UnityEngine;
using UnityEngine.UI;

namespace Wildfolk
{
    public sealed class MultiplayerMenu : MonoBehaviour
    {
        public HomeScreen Home;
        public NetworkSession SessionPrefab;
        public Button HostButton,JoinButton,EnterButton,ModeButton,BackButton,SingleButton;
        public InputField Destination;
        public Text RouteLabel,DestinationLabel,CodeLabel,StatusLabel;
        private ConnectionRoute route;
        private NetworkSession session;
        public NetworkSession Session=>session;
        private void OnEnable()
        {
            session=NetworkSession.Instance!=null ? NetworkSession.Instance : Instantiate(SessionPrefab);
            session.Changed+=Refresh;L.Changed+=Refresh;Refresh();
        }
        private void OnDisable(){if(session!=null)session.Changed-=Refresh;L.Changed-=Refresh;}
        public void ToggleRoute(){if(session.State!=NetworkSessionState.Idle)return;route=route==ConnectionRoute.Internet ? ConnectionRoute.LocalNetwork : ConnectionRoute.Internet;Destination.text=string.Empty;Refresh();}
        public async void Host()=>await session.Host(route);
        public async void Join()=>await session.Join(route,Destination.text);
        public void Enter()=>session.EnterGame();
        public void Back(){session.Leave(false);Home.ShowHome();}
        public void Single(){session.Leave(false);Home.SinglePlayer();}
        private void Refresh()
        {
            var idle=session.State==NetworkSessionState.Idle;var waiting=session.State==NetworkSessionState.Waiting;
            HostButton.interactable=JoinButton.interactable=ModeButton.interactable=Destination.interactable=idle;
            EnterButton.gameObject.SetActive(waiting && session.Manager.IsHost);
            RouteLabel.text=L.Text(route==ConnectionRoute.Internet ? "net.internet" : "net.local");
            DestinationLabel.text=L.Text(route==ConnectionRoute.Internet ? "net.room_code" : "net.address");
            var placeholder=Destination.placeholder as Text;if(placeholder!=null)placeholder.text=L.Text(route==ConnectionRoute.Internet ? "net.code_placeholder" : "net.address_placeholder");
            CodeLabel.text=waiting || session.State==NetworkSessionState.Playing ? (session.Route==ConnectionRoute.Internet ? L.Format("net.code_value",session.RoomCode) : L.Text("net.local_ready")) : string.Empty;
            StatusLabel.text=string.IsNullOrEmpty(session.MessageKey) ? L.Text("net.intro") : L.Text(session.MessageKey);
            SingleButton.interactable=idle;BackButton.interactable=session.State!=NetworkSessionState.Closing;
        }
    }
}
