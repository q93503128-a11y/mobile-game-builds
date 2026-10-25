using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Wildfolk
{
    [Serializable]public sealed class ClassXpIntent
    {
        public string Receipt,ClassId,WorldPath,Boss,Tomb;
        public int Seed,Generation,Experience;
        public long Night;
        public void Validate()
        {if(string.IsNullOrEmpty(Receipt) || ClassCatalog.Current.Find(ClassId)==null || string.IsNullOrEmpty(WorldPath) || !Path.IsPathRooted(WorldPath) || string.IsNullOrEmpty(Boss) || string.IsNullOrEmpty(Tomb) || Night<1 || !WorldGenerationVersion.Valid(Generation) || Experience<=0)throw new InvalidDataException("Invalid pending class reward");}
        public bool Same(ClassXpIntent other)=>other!=null && Receipt==other.Receipt && ClassId==other.ClassId && WorldPath==other.WorldPath && Boss==other.Boss && Tomb==other.Tomb && Night==other.Night && Seed==other.Seed && Generation==other.Generation && Experience==other.Experience;
        public bool Committed(SavedGame game,string profile)
        {
            if(game.Seed!=Seed || game.BaseGenerationVersion!=Generation || game.Bosses==null)return false;
            if(game.IsMultiplayer && game.Multiplayer.HostId!=profile)return false;
            foreach(var r in game.Bosses)if(r.Boss==Boss && r.Tomb==Tomb && r.LastNight==Night && r.HP==0 && !r.Active && !r.PendingReward)return true;
            return false;
        }
    }
    // One outstanding local boss reward per profile. No permanent per-kill receipt history.
    public static class ClassRewardPersistence
    {
        private static readonly HashSet<string> inFlight=new HashSet<string>();
        public static bool Active(string profile)=>inFlight.Contains(profile);
        private static AccountProgression Copy(AccountProgression data)=>JsonUtility.FromJson<AccountProgression>(JsonUtility.ToJson(data));
        internal static AccountProgression Recover(string profile,AccountProgression data,IAccountProgressionStore store,bool discardUncommitted=true)
        {
            var intent=data.PendingXP;if(intent==null)return data;
            try
            {
                var repo=new SaveRepository(intent.WorldPath);if(!repo.Exists)return data;
                var rules=Resources.Load<SaveGameRules>("SaveGameRules");var game=repo.Read(rules.Validate);var committed=intent.Committed(game,profile);
                if(!committed && !discardUncommitted)return data;
                var next=Copy(data);if(committed){var c=next.Find(intent.ClassId);if(c?.Owned!=true)return data;c.XP=checked(c.XP+intent.Experience);}next.PendingXP=null;
                return store.Write(profile,next)?next:data;
            }
            catch(Exception e)when(e is IOException || e is UnauthorizedAccessException || e is ArgumentException || e is InvalidOperationException || e is OverflowException){return data;}
        }
        public static ClassXpCommit Begin(string profile,ClassXpIntent intent)
        {
            intent.Validate();if(Active(profile))return null;var store=ClassProfiles.Store;var data=store.Read(profile);
            if(data.PendingXP!=null || data.Find(intent.ClassId)?.Owned!=true)return null;
            inFlight.Add(profile);
            try{data=Copy(data);data.PendingXP=intent;if(store.Write(profile,data))return new ClassXpCommit(profile,intent,store);}
            catch{inFlight.Remove(profile);throw;}
            inFlight.Remove(profile);return null;
        }
        public sealed class ClassXpCommit:IDisposable
        {
            private readonly string profile;private readonly ClassXpIntent intent;private readonly IAccountProgressionStore store;
            internal ClassXpCommit(string p,ClassXpIntent i,IAccountProgressionStore s){profile=p;intent=i;store=s;}
            public bool Complete(out long total)
            {total=0;try{var data=store.Read(profile);data=Recover(profile,data,store,false);total=data.Find(intent.ClassId).XP;return data.PendingXP==null;}catch(Exception e)when(e is IOException || e is UnauthorizedAccessException || e is ArgumentException || e is InvalidOperationException){return false;}}
            public void Rollback()
            {try{var data=store.Read(profile);if(data.PendingXP?.Same(intent)==true){data=Copy(data);data.PendingXP=null;store.Write(profile,data);}}catch(Exception e)when(e is IOException || e is UnauthorizedAccessException || e is ArgumentException || e is InvalidOperationException){}}
            public void Dispose()=>inFlight.Remove(profile);
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]private static void Reset()=>inFlight.Clear();
    }
}
