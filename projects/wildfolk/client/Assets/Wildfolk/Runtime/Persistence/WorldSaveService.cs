using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Wildfolk
{
    public sealed class WorldSaveService : MonoBehaviour
    {
        public ContentCatalog Catalog;
        public WorldGenerator World;
        public WorldClock Clock;
        public PlayerInventory Player;
        public bool AutoSaveEnabled=true;
        public SaveRepository Repository {get;set;}
        public string LastError {get;private set;}
        public SaveGameRules Rules;
        private void Awake()
        {Repository=SceneFlow.Repository;if(Rules==null)Rules=Resources.Load<SaveGameRules>("SaveGameRules");}
        private void Start(){var game=SceneFlow.TakePending();if(game!=null)Apply(game);}
        public void Validate(SavedGame game)=>Rules.Validate(game);
        public SavedGame Capture()=>WorldSnapshotCodec.Capture(World,Clock,Player);
        public bool Save()
        {
            if(World.Map==null || !Player.GetComponent<Health>().IsAlive)return false;
            try{Repository.Write(Capture(),Validate);LastError=null;return true;}
            catch(Exception error)when(error is InvalidDataException || error is IOException || error is ArgumentException || error is InvalidOperationException){LastError=error.Message;return false;}
        }
        public void SaveFromMenu()=>PlayerFeedback.ShowKey(Save() ? "feedback.saved" : "feedback.save_failed");
        public bool Load()
        {
            SavedGame game;
            try{game=Repository.Read(Validate);LastError=null;}
            catch(Exception error)when(error is InvalidDataException || error is IOException || error is ArgumentException || error is InvalidOperationException){LastError=error.Message;PlayerFeedback.ShowKey("feedback.load_failed");return false;}
            SceneFlow.Continue(game,Repository);return true;
        }
        public void SaveAndHome(){if(!Save()){PlayerFeedback.ShowKey("feedback.save_failed");return;}GetComponent<UserPreferences>().Flush();AutoSaveEnabled=false;SceneFlow.Home(Repository);}
        public void Apply(SavedGame game)
        {
            Validate(game);World.Session.Resume();World.Session.CloseInventory();World.Session.CloseCharacter();World.Session.CloseSkillTree();
            WorldSnapshotCodec.Apply(World,Clock,Catalog,game,Player);PlayerSnapshotCodec.Apply(Player,game.Player,Catalog);Physics2D.SyncTransforms();
        }
        private void OnApplicationPause(bool paused){if(paused && AutoSaveEnabled && Application.isPlaying)Save();}
        private void OnApplicationQuit(){if(AutoSaveEnabled && World.Map!=null)Save();}
    }
}
