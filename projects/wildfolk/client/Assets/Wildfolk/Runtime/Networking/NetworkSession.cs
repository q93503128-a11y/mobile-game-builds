using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Wildfolk
{
    public enum NetworkSessionState { Idle,Connecting,Waiting,Loading,Playing,Closing }
    public sealed class NetworkSession : MonoBehaviour
    {
        public const int MaximumPlayers=4;
        public static NetworkSession Instance {get;private set;}
        public NetworkManager Manager;
        public GameObject PlayerPrefab;
        public GameObject EcologyPrefab;
        public NetworkSessionState State {get;private set;}
        public ConnectionRoute Route {get;private set;}
        public string RoomCode {get;private set;}=string.Empty;
        public string MessageKey {get;private set;}
        public string LastFailure {get;private set;}
        public event Action Changed;
        private int operation;
        private bool quitting;
        private UnityTransport Transport=>Manager.GetComponent<UnityTransport>();
        private void Awake()
        {
            if(Instance!=null && Instance!=this){Destroy(gameObject);return;}
            Instance=this;DontDestroyOnLoad(gameObject);
            Manager.AddNetworkPrefab(PlayerPrefab);
            foreach(var entry in EcologyPrefab.GetComponent<NetworkCreatureWorld>().Prefabs)Manager.AddNetworkPrefab(entry.NetworkPrefab);
            Manager.OnClientConnectedCallback+=Connected;Manager.OnClientDisconnectCallback+=Disconnected;
            Manager.OnTransportFailure+=TransportFailed;Manager.ConnectionApprovalCallback=Approve;
        }
        private void OnDestroy()
        {
            if(Instance!=this)return;
            Manager.OnClientConnectedCallback-=Connected;Manager.OnClientDisconnectCallback-=Disconnected;Manager.OnTransportFailure-=TransportFailed;Instance=null;
        }
        private void OnApplicationQuit(){quitting=true;operation++;}
        private IRoomConnection Connection(ConnectionRoute route)=>route==ConnectionRoute.Internet ? (IRoomConnection)new RelayRoomConnection() : new LocalRoomConnection();
        private void Notify()=>Changed?.Invoke();
        public async Task<bool> Host(ConnectionRoute route)
        {
            if(State!=NetworkSessionState.Idle)return false;
            var ticket=++operation;Route=route;State=NetworkSessionState.Connecting;MessageKey="net.connecting";LastFailure=null;Notify();
            try
            {
                var code=await Connection(route).Host(Transport,MaximumPlayers);
                if(ticket!=operation || quitting)return false;
                if(!Manager.StartHost())throw new InvalidOperationException("Host transport did not start.");
                AttachScenes();RoomCode=code;State=NetworkSessionState.Waiting;MessageKey="net.host_ready";Notify();return true;
            }
            catch(Exception error){if(ticket==operation)ConnectionFailed(error,route==ConnectionRoute.Internet ? "net.online_unavailable" : "net.connection_failed");return false;}
        }
        public async Task<bool> Join(ConnectionRoute route,string destination)
        {
            if(State!=NetworkSessionState.Idle)return false;
            destination=(destination??string.Empty).Trim();if(route==ConnectionRoute.Internet)destination=destination.ToUpperInvariant();
            if(route==ConnectionRoute.Internet ? !RelayRoomConnection.ValidCode(destination) : !LocalRoomConnection.TryAddress(destination,out _,out _))
            {MessageKey=route==ConnectionRoute.Internet ? "net.invalid_code" : "net.invalid_address";Notify();return false;}
            var ticket=++operation;Route=route;State=NetworkSessionState.Connecting;MessageKey="net.connecting";LastFailure=null;Notify();
            try
            {
                await Connection(route).Join(Transport,destination);
                if(ticket!=operation || quitting)return false;
                if(!Manager.StartClient())throw new InvalidOperationException("Client transport did not start.");
                RoomCode=route==ConnectionRoute.Internet ? destination : string.Empty;StartCoroutine(JoinTimeout(ticket));return true;
            }
            catch(Exception error){if(ticket==operation)ConnectionFailed(error,"net.connection_failed");return false;}
        }
        private IEnumerator JoinTimeout(int ticket)
        {yield return new WaitForSecondsRealtime(12);if(ticket==operation && State==NetworkSessionState.Connecting){MessageKey="net.connection_failed";Leave(false);}}
        private void ConnectionFailed(Exception error,string key)
        {LastFailure=error.GetType().Name+": "+error.Message;State=NetworkSessionState.Idle;MessageKey=key;RoomCode=string.Empty;Notify();}
        private void Approve(NetworkManager.ConnectionApprovalRequest request,NetworkManager.ConnectionApprovalResponse response)
        {
            response.Approved=Manager.ConnectedClientsIds.Count<MaximumPlayers;response.CreatePlayerObject=false;
            response.Pending=false;if(!response.Approved)response.Reason="net.room_full";
        }
        private void Connected(ulong id)
        {
            if(!Manager.IsServer && id==Manager.LocalClientId){State=SceneManager.GetActiveScene().name==SceneFlow.MultiplayerScene ? NetworkSessionState.Playing : NetworkSessionState.Waiting;MessageKey="net.waiting_host";}
            Notify();
        }
        private void Disconnected(ulong id)
        {
            if(quitting || State==NetworkSessionState.Closing || State==NetworkSessionState.Idle)return;
            if(Manager.IsHost && id!=Manager.LocalClientId){Notify();return;}
            MessageKey=State==NetworkSessionState.Connecting ? (Manager.DisconnectReason=="net.room_full" ? "net.room_full" : "net.connection_failed") : "net.host_left";
            Leave(true);
        }
        private void TransportFailed(){if(State==NetworkSessionState.Closing)return;MessageKey="net.connection_failed";Leave(true);}
        private void AttachScenes()
        {Manager.SceneManager.OnLoadEventCompleted+=Loaded;Manager.SceneManager.OnSynchronizeComplete+=Synchronized;}
        public bool EnterGame()
        {
            if(!Manager.IsHost || State!=NetworkSessionState.Waiting)return false;
            var result=Manager.SceneManager.LoadScene(SceneFlow.MultiplayerScene,LoadSceneMode.Single);
            if(result!=SceneEventProgressStatus.Started){MessageKey="net.connection_failed";Notify();return false;}
            State=NetworkSessionState.Loading;MessageKey="net.entering";Notify();return true;
        }
        private void Loaded(string scene,LoadSceneMode mode,List<ulong> completed,List<ulong> timedOut)
        {if(scene!=SceneFlow.MultiplayerScene || !Manager.IsServer)return;foreach(var id in completed)Spawn(id);}
        private void Synchronized(ulong id){if(Manager.IsServer && SceneManager.GetActiveScene().name==SceneFlow.MultiplayerScene)Spawn(id);}
        private void Spawn(ulong id)
        {
            var ecology=NetworkCreatureWorld.Active;
            if(ecology!=null && ecology.World.Map==null){StartCoroutine(SpawnWhenReady(id));return;}
            if(!Manager.ConnectedClients.TryGetValue(id,out var client)||client.PlayerObject!=null)return;
            var order=0;foreach(var peer in Manager.ConnectedClientsIds){if(peer==id)break;order++;}var position=new Vector3((order%2)*3-1.5f,(order/2)*-3,0);
            var world=NetworkCreatureWorld.Active;if(world!=null)position+=(Vector3)world.World.Map.Spawn;
            var actor=Instantiate(PlayerPrefab,position,Quaternion.identity);actor.GetComponent<NetworkObject>().SpawnAsPlayerObject(id,true);
        }
        private IEnumerator SpawnWhenReady(ulong id)
        {
            while(Manager.IsListening && NetworkCreatureWorld.Active!=null && NetworkCreatureWorld.Active.World.Map==null)yield return null;
            if(Manager.IsServer && Manager.IsListening && NetworkCreatureWorld.Active!=null)Spawn(id);
        }
        public void ArenaReady(){State=NetworkSessionState.Playing;MessageKey=null;SceneFlow.MultiplayerReady();Notify();}
        public void Leave(bool returnHome=true)
        {if(State==NetworkSessionState.Closing)return;operation++;State=NetworkSessionState.Closing;Notify();StartCoroutine(Close(returnHome));}
        private IEnumerator Close(bool returnHome)
        {
            // Leave NGO callbacks before stopping or loading another scene.
            yield return null;
            if(Manager.SceneManager!=null){Manager.SceneManager.OnLoadEventCompleted-=Loaded;Manager.SceneManager.OnSynchronizeComplete-=Synchronized;}
            if(Manager.IsListening)Manager.Shutdown();
            while(Manager.ShutdownInProgress)yield return null;
            RoomCode=string.Empty;State=NetworkSessionState.Idle;Notify();
            if(returnHome && SceneManager.GetActiveScene().name==SceneFlow.MultiplayerScene)
            {SceneFlow.Home(SceneFlow.Repository);yield return null;if(MessageKey=="net.host_left" || MessageKey=="net.connection_failed")FindFirstObjectByType<HomeScreen>()?.OpenMultiplayer();}
        }
    }
}
