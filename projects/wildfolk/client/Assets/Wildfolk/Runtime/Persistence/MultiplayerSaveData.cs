using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Wildfolk
{
    [Serializable]public sealed class SavedParticipant {public string Id;public PlayerSnapshot State;}
    [Serializable]public sealed class MultiplayerSnapshot
    {public string WorldId,HostId;public SavedParticipant[] Players=Array.Empty<SavedParticipant>();}
    public static class LocalPlayerIdentity
    {
        public static bool Valid(string id)=>id!=null && id.Length==32 && Guid.TryParseExact(id,"N",out _);
        public static string Get()
        {
            if(DevelopmentProfile.Isolated)return "cccccccccccccccccccccccccccccccc";
            const string key="wildfolk.multiplayer.player";var id=PlayerPrefs.GetString(key,"");if(Valid(id))return id;
            id=Guid.NewGuid().ToString("N");PlayerPrefs.SetString(key,id);PlayerPrefs.Save();return id;
        }
    }
    public static class MultiplayerSaveRules
    {
        public static void Validate(SavedGame game,SaveGameRules rules)
        {
            if(!game.IsMultiplayer){game.Multiplayer=null;return;}var multi=game.Multiplayer;
            if(multi==null || game.Version!=14 || !LocalPlayerIdentity.Valid(multi.WorldId) || !LocalPlayerIdentity.Valid(multi.HostId) || multi.Players==null || multi.Players.Length<1 || multi.Players.Length>256)throw new InvalidDataException("Invalid cooperative world metadata.");
            var ids=new HashSet<string>();var host=false;var map=rules.ValidationMap(game);var player=rules.Player;
            foreach(var entry in multi.Players)
            {
                if(entry==null || !LocalPlayerIdentity.Valid(entry.Id) || !ids.Add(entry.Id))throw new InvalidDataException("Duplicate or invalid participant.");
                SaveValidation.ValidatePlayer(entry.State,map,rules.Catalog,player.Settings.InventorySlots,player.GetComponent<Health>().Definition.MaximumHealth,player.GetComponent<PlayerSurvival>().Definition,player.GetComponent<PlayerProgression>().Definition,player.GetComponent<PlayerLocomotion>().Definition,true);
                DeathSaveValidation.Validate(entry.State,map);
                if(entry.Id==multi.HostId){host=true;if(JsonUtility.ToJson(entry.State)!=JsonUtility.ToJson(game.Player))throw new InvalidDataException("Host snapshot mismatch.");}
            }
            if(!host)throw new InvalidDataException("Missing Host participant.");
            foreach(var placed in game.Placeables)if(!string.IsNullOrEmpty(placed.Creator) && !LocalPlayerIdentity.Valid(placed.Creator))throw new InvalidDataException("Invalid creator metadata.");
        }
    }
}
