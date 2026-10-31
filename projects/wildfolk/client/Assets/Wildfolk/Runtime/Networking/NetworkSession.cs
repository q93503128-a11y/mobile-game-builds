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
    public sealed partial class NetworkSession : MonoBehaviour
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
        public SaveRepository WorldRepository {get;set;}
        public int NewWorldGenerationVersion {get;set;}=WorldGenerationVersion.Current;
        public SavedGame PreparedWorld {get;private set;}
        public string ProfileOverride {get;set;}
        public string LocalProfile {get;private set;}
        private readonly Dictionary<ulong,string> identities=new Dictionary<ulong,string>();
        public IClassAdmissionVerifier ClassVerifier {get;set;}=new SurvivorClassAdmission();
        private readonly ClassAdmissionLocks classLocks=new ClassAdmissionLocks();
        private readonly HashSet<ulong> spawning=new HashSet<ulong>();
        private SaveGameRules SaveRules=>Resources.Load<SaveGameRules>("SaveGameRules");
        public event Action Changed;
        private int operation;
        private bool quitting;
        private UnityTransport Transport=>Manager.GetComponent<UnityTransport>();
        private void Awake()
        {
            if(Instance!=null && Instance!=this){Destroy(gameObject);return;}
            Instance=this;DontDestroyOnLoad(gameObject);WorldRepository=SaveRepository.MultiplayerDefault();Application.wantsToQuit+=CanQuit;
            Manager.NetworkConfig.ProtocolVersion=NetworkProtocol.Current;Manager.AddNetworkPrefab(PlayerPrefab);
            foreach(var entry in EcologyPrefab.GetComponent<NetworkCreatureWorld>().Prefabs)Manager.AddNetworkPrefab(entry.NetworkPrefab);
            Manager.OnClientConnectedCallback+=Connected;Manager.OnClientDisconnectCallback+=Disconnected;
            Manager.OnTransportFailure+=TransportFailed;Manager.ConnectionApprovalCallback=Approve;
        }
        private void OnDestroy()
        {
            if(Instance!=this)return;
            UnregisterPremiumMessages();Application.wantsToQuit-=CanQuit;Manager.OnClientConnectedCallback-=Connected;Manager.OnClientDisconnectCallback-=Disconnected;Manager.OnTransportFailure-=TransportFailed;Instance=null;
        }
        private bool CanQuit(){return !Manager.IsHost || NetworkWorldPersistence.Active==null || !NetworkWorldPersistence.Active.Ready || TrySaveWorld();}
        private void OnApplicationQuit(){quitting=true;operation++;}
        public bool TrySaveWorld()
        {
            if(!Manager.IsHost || NetworkWorldPersistence.Active==null || !NetworkWorldPersistence.Active.Ready)return true;
            if(NetworkWorldPersistence.Active.Save())return true;
            MessageKey="net.save_failed";Notify();return false;
        }
        private void ConfigureIdentity()
        {LocalProfile=ProfileOverride??LocalPlayerIdentity.Get();if(!LocalPlayerIdentity.Valid(LocalProfile))throw new InvalidOperationException("Invalid local participant.");Manager.NetworkConfig.ConnectionData=System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(ClassAdmission.Local(LocalProfile)));}
        private IRoomConnection Connection(ConnectionRoute route)=>route==ConnectionRoute.Internet ? (IRoomConnection)new RelayRoomConnection() : new LocalRoomConnection();
        private void Notify()=>Changed?.Invoke();
        public async Task<bool> Host(ConnectionRoute route,bool continueWorld=true)
        {
            if(State!=NetworkSessionState.Idle)return false;
            var ticket=++operation;Route=route;State=NetworkSessionState.Connecting;MessageKey="net.connecting";LastFailure=null;Notify();
            try
            {
                ConfigureIdentity();PreparedWorld=null;identities.Clear();spawning.Clear();classLocks.Clear();BeginPremiumSession();await PrepareHostPremium();
                if(ticket!=operation||quitting)return false;
                if(continueWorld && WorldRepository.Exists)
                {
                    try{PreparedWorld=WorldRepository.Read(SaveRules.Validate);if(!PreparedWorld.IsMultiplayer || PreparedWorld.Multiplayer==null || PreparedWorld.Multiplayer.HostId!=LocalProfile)throw new InvalidOperationException("World belongs to a different local Host.");}
                    catch(Exception error){ConnectionFailed(error,"net.load_failed");return false;}
                }
                var code=await Connection(route).Host(Transport,MaximumPlayers);
                if(ticket!=operation || quitting)return false;
                if(!Manager.StartHost())throw new InvalidOperationException("Host transport did not start.");
                RegisterPremiumMessages();AttachScenes();RoomCode=code;State=NetworkSessionState.Waiting;MessageKey=PremiumMessageKey??"net.host_ready";Notify();return true;
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
                ConfigureIdentity();PremiumMessageKey=null;lastClientPremiumAdmission=null;await Connection(route).Join(Transport,destination);
                if(ticket!=operation || quitting)return false;
                if(!Manager.StartClient())throw new InvalidOperationException("Client transport did not start.");
                RegisterPremiumMessages();
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
            ClassAdmission admission=null;try{if(request.Payload?.Length<=2048)admission=JsonUtility.FromJson<ClassAdmission>(System.Text.Encoding.UTF8.GetString(request.Payload));}catch(Exception){}
            var id=admission?.Profile;
            var accepted=CanApprove(id,request.ClientNetworkId);
            if(accepted&&ClassCatalog.Current.Find(admission.Selected)==null)accepted=false;
            if(accepted)accepted=classLocks.Admit(LocalProfile,admission,request.ClientNetworkId==NetworkManager.ServerClientId,ClassVerifier,out _,hostPremiumSnapshot);
            response.Approved=accepted;response.CreatePlayerObject=false;
            response.Pending=false;if(response.Approved){identities[request.ClientNetworkId]=id;QueuePremium(request.ClientNetworkId,admission);}else response.Reason=Manager.ConnectedClientsIds.Count>=MaximumPlayers ? "net.room_full" : LocalPlayerIdentity.Valid(id) ? "net.duplicate_player" : "net.connection_failed";
        }
        public bool CanApprove(string profile,ulong clientId)
        {
            if(!LocalPlayerIdentity.Valid(profile) || Manager.ConnectedClientsIds.Count>=MaximumPlayers)return false;
            foreach(var pair in identities)if(pair.Key!=clientId && pair.Value==profile)return false;
            return true;
        }
        public bool TryRegisterParticipant(string profile,ulong clientId)
        {if(!Manager.IsServer || !CanApprove(profile,clientId))return false;identities[clientId]=profile;return true;}
        private void Connected(ulong id)
        {
            if(Manager.IsServer&&id!=NetworkManager.ServerClientId&&premiumPending.ContainsKey(id))StartCoroutine(StartPremiumChallenge(id));
            if(!Manager.IsServer && id==Manager.LocalClientId){State=SceneManager.GetActiveScene().name==SceneFlow.MultiplayerScene ? NetworkSessionState.Playing : NetworkSessionState.Waiting;MessageKey="net.waiting_host";}
            Notify();
        }
        private void Disconnected(ulong id)
        {
            if(quitting || State==NetworkSessionState.Closing || State==NetworkSessionState.Idle)return;
            if(Manager.IsHost && id!=Manager.LocalClientId){RetireParticipant(id);StartCoroutine(SaveAfterDisconnect());Notify();return;}
            MessageKey=State==NetworkSessionState.Connecting ? (Manager.DisconnectReason=="net.room_full" ? "net.room_full" : Manager.DisconnectReason=="net.duplicate_player" ? "net.duplicate_player" : "net.connection_failed") : "net.host_left";
            Leave(true);
        }
        private IEnumerator SaveAfterDisconnect()
        {yield return null;if(Manager.IsHost && State==NetworkSessionState.Playing)TrySaveWorld();}
        private void TransportFailed(){if(State==NetworkSessionState.Closing)return;MessageKey="net.connection_failed";Leave(true,true);}
        private void AttachScenes()
        {Manager.SceneManager.OnLoadEventCompleted+=Loaded;Manager.SceneManager.OnSynchronizeComplete+=Synchronized;Manager.SceneManager.OnSceneEvent+=PresentSceneTransition;}
        private void PresentSceneTransition(SceneEvent progress)
        {if(!Manager.IsServer&&progress.ClientId==Manager.LocalClientId&&progress.SceneName==SceneFlow.MultiplayerScene&&progress.SceneEventType==SceneEventType.Load)TransitionOverlay.Watch(this,"net.entering");}
        public bool EnterGame()
        {
            if(!Manager.IsHost || State!=NetworkSessionState.Waiting)return false;
            var result=Manager.SceneManager.LoadScene(SceneFlow.MultiplayerScene,LoadSceneMode.Single);
            if(result!=SceneEventProgressStatus.Started){MessageKey="net.connection_failed";Notify();return false;}
            State=NetworkSessionState.Loading;MessageKey="net.entering";Notify();return true;
        }
        private void Loaded(string scene,LoadSceneMode mode,List<ulong> completed,List<ulong> timedOut)
        {if(scene!=SceneFlow.MultiplayerScene || !Manager.IsServer)return;foreach(var id in completed)SpawnParticipant(id);}
        private void Synchronized(ulong id){if(Manager.IsServer && SceneManager.GetActiveScene().name==SceneFlow.MultiplayerScene)SpawnParticipant(id);}
        public void SpawnParticipant(ulong id)
        {
            var ecology=NetworkCreatureWorld.Active;
            if(ecology==null || !ecology.Ready||premiumPending.ContainsKey(id)){if(spawning.Add(id))StartCoroutine(SpawnWhenReady(id));return;}
            if(!Manager.ConnectedClients.TryGetValue(id,out var client)||client.PlayerObject!=null)return;
            var order=0;foreach(var peer in Manager.ConnectedClientsIds){if(peer==id)break;order++;}var position=new Vector3((order%2)*3-1.5f,(order/2)*-3,0);
            var world=NetworkCreatureWorld.Active;if(world!=null)position+=(Vector3)world.World.Map.Spawn;
            if(!identities.TryGetValue(id,out var profile))return;
            var actor=Instantiate(PlayerPrefab,position,Quaternion.identity);var player=actor.GetComponent<NetworkPlayer>();player.ProfileId=profile;if(!classLocks.TryGet(profile,out var locked)){Destroy(actor);return;}player.GetComponent<PlayerClass>().Lock(profile,locked);NetworkWorldPersistence.Active.RestorePlayer(player);actor.GetComponent<NetworkObject>().SpawnAsPlayerObject(id,true);
        }
        private IEnumerator SpawnWhenReady(ulong id)
        {
            while(Manager.IsListening && (NetworkCreatureWorld.Active==null || !NetworkCreatureWorld.Active.Ready||premiumPending.ContainsKey(id)))yield return null;
            spawning.Remove(id);
            if(Manager.IsServer && Manager.IsListening && NetworkCreatureWorld.Active!=null)SpawnParticipant(id);
        }
        public void ArenaReady(){State=NetworkSessionState.Playing;MessageKey=null;SceneFlow.MultiplayerReady();Notify();}
        public void RetireParticipant(ulong id)
        {
            if(Manager.IsServer && Manager.ConnectedClients.TryGetValue(id,out var client) && client.PlayerObject!=null)
            {var player=client.PlayerObject.GetComponent<NetworkPlayer>();NetworkWorldPersistence.Active?.Remember(player);if(client.PlayerObject.IsSpawned)client.PlayerObject.Despawn();client.PlayerObject=null;}
            identities.Remove(id);spawning.Remove(id);premiumPending.Remove(id);
        }
        public void Leave(bool returnHome=true,bool force=false)
        {
            if(State==NetworkSessionState.Closing || State==NetworkSessionState.Idle)return;
            if(Manager.IsHost && !TrySaveWorld() && !force)return;
            operation++;State=NetworkSessionState.Closing;Notify();StartCoroutine(Close(returnHome));
        }
        private IEnumerator Close(bool returnHome)
        {
            // Leave NGO callbacks before stopping or loading another scene.
            yield return null;
            if(Manager.SceneManager!=null){Manager.SceneManager.OnLoadEventCompleted-=Loaded;Manager.SceneManager.OnSynchronizeComplete-=Synchronized;}
            UnregisterPremiumMessages();premiumPending.Clear();if(Manager.IsListening)Manager.Shutdown();
            while(Manager.ShutdownInProgress)yield return null;
            RoomCode=string.Empty;identities.Clear();spawning.Clear();PreparedWorld=null;State=NetworkSessionState.Idle;Notify();
            if(returnHome && SceneManager.GetActiveScene().name==SceneFlow.MultiplayerScene)
            {SceneFlow.Home(SceneFlow.Repository);yield return null;if(MessageKey=="net.host_left" || MessageKey=="net.connection_failed")FindFirstObjectByType<HomeScreen>()?.OpenMultiplayer();}
        }
    }
}
