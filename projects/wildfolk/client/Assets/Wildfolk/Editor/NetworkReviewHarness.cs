using System;
using System.IO;
using System.Linq;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Wildfolk.Editor
{
    // Editor-only, bounded command/telemetry bridge for two real NGO processes.
    [InitializeOnLoad]
    public static class NetworkReviewHarness
    {
        [Serializable]public sealed class Command
        {public int Sequence;public float X,Y;public bool Dash,Leave,Quit,Fixture;public long Actor=-1;public int HP=-1;public string Head,Body,Held;}
        [Serializable]public sealed class Actor
        {public ulong Owner;public bool Local,Server;public Vector2 Position,Facing;public byte Motion;public int HP,MaxHP;public string Head,Body,Held,HeadSprite,BodySprite,HeldSprite;}
        [Serializable]public sealed class Snapshot
        {public string Role,Scene,Session,Message,Failure,RoomCode;public int Sequence,Frame;public float PlayTime;public Actor[] Actors;}
        private sealed class ReviewInput : IPlayerInputSource
        {public Vector2 Direction;public Vector2 Move=>Direction;public bool Primary=>false;public bool Interact=>false;public bool Inventory=>false;public bool Pause=>false;}
        private static readonly ReviewInput input=new ReviewInput();
        private static double next;
        private static int sequence;
        private static bool started;
        static NetworkReviewHarness(){if(!string.IsNullOrEmpty(UnityEditor.SessionState.GetString("Lowfire.ReviewRole","")))EditorApplication.update+=Tick;}
        public static void Monitor(string directory,string role)
        {
            UnityEditor.SessionState.SetString("Lowfire.ReviewDir",directory);UnityEditor.SessionState.SetString("Lowfire.ReviewRole",role);
            Directory.CreateDirectory(directory);File.WriteAllText(Path.Combine(directory,role+"-command.json"),"{\"Sequence\":0}");sequence=0;input.Direction=Vector2.zero;started=false;next=0;EditorApplication.update-=Tick;EditorApplication.update+=Tick;
        }
        public static void Client()
        {
            var args=Environment.GetCommandLineArgs();string Arg(string key){var index=Array.IndexOf(args,key);return index>=0 && index+1<args.Length ? args[index+1] : string.Empty;}
            var directory=Arg("-lowfire-review-dir");if(string.IsNullOrEmpty(directory))throw new ArgumentException("Review directory required.");
            UnityEditor.SessionState.SetString("Lowfire.ReviewDestination",Arg("-lowfire-room"));
            UnityEditor.SessionState.SetBool("Lowfire.ReviewLocal",args.Contains("-lowfire-local"));
            Monitor(directory,"client");EditorSceneManager.OpenScene(FrontendSceneBuilder.ScenePath);EditorApplication.isPlaying=true;
        }
        public static void Stop()
        {
            foreach(var player in UnityEngine.Object.FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None))if(player.IsOwner){player.GetComponent<PlayerInputAdapter>().Source=null;player.Commands.SetMove(Vector2.zero);}
            EditorApplication.update-=Tick;UnityEditor.SessionState.EraseString("Lowfire.ReviewRole");UnityEditor.SessionState.EraseString("Lowfire.ReviewDir");
        }
        private static void Tick()
        {
            if(!EditorApplication.isPlaying || EditorApplication.timeSinceStartup<next)return;next=EditorApplication.timeSinceStartup+.1;
            var role=UnityEditor.SessionState.GetString("Lowfire.ReviewRole","");if(string.IsNullOrEmpty(role))return;
            if(role=="client"){EditorApplication.isPaused=false;EditorApplication.QueuePlayerLoopUpdate();}
            var directory=UnityEditor.SessionState.GetString("Lowfire.ReviewDir","");
            if(role=="client" && !started)
            {
                var home=UnityEngine.Object.FindFirstObjectByType<HomeScreen>();if(home==null)return;
                started=true;RelayRoomConnection.AuthenticationProfile="lowfirereviewclient";home.OpenMultiplayer();
                var menu=home.Multiplayer.GetComponent<MultiplayerMenu>();
                var route=UnityEditor.SessionState.GetBool("Lowfire.ReviewLocal",false) ? ConnectionRoute.LocalNetwork : ConnectionRoute.Internet;
                _=menu.Session.Join(route,UnityEditor.SessionState.GetString("Lowfire.ReviewDestination",""));
            }
            var local=UnityEngine.Object.FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None).FirstOrDefault(p=>p.IsSpawned && p.IsOwner);
            if(local!=null)local.GetComponent<PlayerInputAdapter>().Source=input;
            var commandPath=Path.Combine(directory,role+"-command.json");
            try
            {
                if(File.Exists(commandPath))
                {
                    var command=JsonUtility.FromJson<Command>(File.ReadAllText(commandPath));
                    if(command!=null && command.Sequence>sequence)
                    {
                        sequence=command.Sequence;input.Direction=NetworkInputRules.Sanitize(new Vector2(command.X,command.Y));
                        if(local!=null){local.Commands.SetMove(input.Direction);if(command.Dash)local.Commands.Dash();}
                        var session=NetworkSession.Instance;
                        if(command.Fixture && command.Actor>=0 && session!=null && session.Manager.IsServer)ApplyFixture(command);
                        if(command.Leave)session?.Leave();
                        if(command.Quit && role=="client"){EditorApplication.Exit(0);return;}
                    }
                }
                Write(directory,role);
            }
            catch(IOException){ }
        }
        private static void ApplyFixture(Command command)
        {
            var actor=UnityEngine.Object.FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None).FirstOrDefault(p=>p.OwnerClientId==(ulong)command.Actor);if(actor==null)return;
            if(command.HP>=0)actor.Health.RestoreHealth(command.HP);
            var bag=actor.GetComponent<PlayerInventory>();
            foreach(var id in new[]{command.Head,command.Body})if(!string.IsNullOrEmpty(id)){var item=actor.Catalog.Item(id);bag.Model.Add(item,1);actor.Equipment.TryEquip(item);}
            if(!string.IsNullOrEmpty(command.Held)){var item=actor.Catalog.Item(command.Held);bag.Model.Add(item,1);for(var i=0;i<bag.Model.Capacity;i++)if(bag.Model.GetSlot(i).Item==item){bag.SelectSlot(i);break;}}
        }
        private static void Write(string directory,string role)
        {
            var session=NetworkSession.Instance;
            var snapshot=new Snapshot{Role=role,Scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,Sequence=sequence,Frame=Time.frameCount,PlayTime=Time.time,Session=session==null ? "none" : session.State.ToString(),Message=session?.MessageKey,Failure=session?.LastFailure,RoomCode=session?.RoomCode,
                Actors=UnityEngine.Object.FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None).Where(p=>p.IsSpawned).Select(p=>new Actor{Owner=p.OwnerClientId,Local=p.IsOwner,Server=p.IsServer,Position=p.transform.position,Facing=p.Motor.Avatar.Facing.Direction,Motion=p.Motion.Value.Flags,HP=p.Health.Current,MaxHP=p.Health.Maximum,Head=p.Equipment.HeadArmor?.Id,Body=p.Equipment.BodyArmor?.Id,Held=p.Equipment.SelectedItem?.Id,HeadSprite=p.Equipment.HeadVisual.sprite?.name,BodySprite=p.Equipment.ArmorVisual.sprite?.name,HeldSprite=p.Equipment.HeldVisual.enabled ? p.Equipment.HeldVisual.sprite?.name : null}).ToArray()};
            var path=Path.Combine(directory,role+"-state.json");var next=path+".next";
            File.WriteAllText(next,JsonUtility.ToJson(snapshot,true));if(File.Exists(path))File.Replace(next,path,null);else File.Move(next,path);
        }
    }
}
