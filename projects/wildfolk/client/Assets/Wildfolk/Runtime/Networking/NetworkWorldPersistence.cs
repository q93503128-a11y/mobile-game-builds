using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
namespace Wildfolk
{
    public sealed class NetworkWorldPersistence : MonoBehaviour
    {
        public static NetworkWorldPersistence Active {get;private set;}
        public SaveGameRules Rules;
        public string WorldId {get;private set;}
        public bool Ready {get;private set;}
        private readonly Dictionary<string,PlayerSnapshot> participants=new Dictionary<string,PlayerSnapshot>();
        private NetworkCreatureWorld world;
        private void Awake(){Active=this;world=GetComponent<NetworkCreatureWorld>();}
        public void InitializeWorld()
        {
            if(!world.IsAuthority)return;var game=NetworkSession.Instance.PreparedWorld;
            if(game==null){WorldId=Guid.NewGuid().ToString("N");world.World.Generate(world.World.Definition.DefaultSeed,true,NetworkSession.Instance.NewWorldGenerationVersion);world.CreateFacilities();}
            else
            {
                Rules.Validate(game);WorldId=game.Multiplayer.WorldId;
                foreach(var entry in game.Multiplayer.Players)participants.Add(entry.Id,entry.State);
                WorldSnapshotCodec.Apply(world.World,world.Clock,world.Catalog,game,world.World.Player);
            }
            world.FlushPending();Ready=true;
        }
        public void RestorePlayer(NetworkPlayer player)
        {if(!world.IsAuthority)throw new InvalidOperationException("Host required.");if(participants.TryGetValue(player.ProfileId,out var state))PlayerSnapshotCodec.Apply(player.GetComponent<PlayerInventory>(),state,world.Catalog);else player.GetComponent<PlayerDeath>()?.InitializeHome(world.World.Map.Spawn);}
        public void Remember(NetworkPlayer player)
        {if(world.IsAuthority && Ready && LocalPlayerIdentity.Valid(player.ProfileId))participants[player.ProfileId]=PlayerSnapshotCodec.Capture(player.GetComponent<PlayerInventory>());}
        public SavedGame Capture()
        {
            if(!world.IsAuthority || !Ready)throw new InvalidOperationException("World is not ready for capture.");
            world.FlushPending();foreach(var player in NetworkPlayer.Active)Remember(player);
            var hostId=NetworkSession.Instance.LocalProfile;if(!participants.TryGetValue(hostId,out var hostState))throw new InvalidOperationException("Host snapshot is unavailable.");
            var game=WorldSnapshotCodec.Capture(world.World,world.Clock,hostState);var entries=new List<SavedParticipant>();
            foreach(var pair in participants)entries.Add(new SavedParticipant{Id=pair.Key,State=pair.Value});entries.Sort((a,b)=>string.CompareOrdinal(a.Id,b.Id));
            game.IsMultiplayer=true;game.Multiplayer=new MultiplayerSnapshot{WorldId=WorldId,HostId=hostId,Players=entries.ToArray()};Rules.Validate(game);return game;
        }
        public bool Save()
        {
            if(!world.IsAuthority || !Ready)return false;
            try{var game=Capture();NetworkSession.Instance.WorldRepository.Write(game,Rules.Validate);foreach(var player in NetworkPlayer.Active)player.GetComponent<PlayerClass>()?.RecoverBossXP();return true;}
            catch(Exception error)when(error is System.IO.IOException || error is UnauthorizedAccessException || error is ArgumentException || error is InvalidOperationException || error is OverflowException)
            {Debug.LogError("Cooperative world save failed: "+error.GetType().Name);return false;}
        }
        private void OnApplicationPause(bool paused){if(paused && world.IsAuthority && Ready)Save();}
        private void OnDestroy(){if(Active==this)Active=null;}
    }
}
