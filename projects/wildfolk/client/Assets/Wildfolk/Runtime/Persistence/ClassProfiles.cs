using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
namespace Wildfolk
{
    [Serializable] public sealed class ClassProgress
    {public string Id;public bool Owned;public long XP;public string[] Nodes=Array.Empty<string>();}
    [Serializable] public sealed class ClassPurchaseIntent
    {public string Transaction,ClassId,WorldPath;public int Cost;}
    [Serializable] public sealed class AccountProgression
    {
        public int Version=2;public string Selected="survivor";public ClassProgress[] Classes;
        // Profile1 legacy Receipts is ignored by JsonUtility and omitted on the next successful write.
        public ClassPurchaseIntent Pending;
        public ClassXpIntent PendingXP;
        public static AccountProgression New()=>new AccountProgression{Classes=ClassCatalog.Current.Classes.Select(c=>new ClassProgress{Id=c.Id,Owned=c.Price==0 && c.Currency=="Gold"}).ToArray()};
        public ClassProgress Find(string id)=>Array.Find(Classes,c=>c.Id==id);
        public void Validate()
        {
            var catalog=ClassCatalog.Current;
            if(Version!=2 || Classes==null || Classes.Length!=catalog.Classes.Length)throw new InvalidDataException("Invalid class profile");
            var seen=new System.Collections.Generic.HashSet<string>();
            foreach(var p in Classes)
            {
                var d=p==null?null:catalog.Find(p.Id);if(d==null || !seen.Add(p.Id) || p.XP<0 || p.Nodes==null || p.Nodes.Length>catalog.Earned(p.XP) || p.Nodes.Distinct().Count()!=p.Nodes.Length || !p.Owned && (p.XP!=0 || p.Nodes.Length>0))throw new InvalidDataException("Invalid class progression");
                foreach(var id in p.Nodes){var node=Array.Find(d.Nodes,n=>n.Id==id);if(node==null || !string.IsNullOrEmpty(node.Prerequisite) && !p.Nodes.Contains(node.Prerequisite))throw new InvalidDataException("Invalid class allocation");}
            }
            if(Find("survivor")?.Owned!=true || Find(Selected)?.Owned!=true)throw new InvalidDataException("Invalid selected class");
            if(Pending!=null && string.IsNullOrEmpty(Pending.Transaction) && string.IsNullOrEmpty(Pending.ClassId) && string.IsNullOrEmpty(Pending.WorldPath) && Pending.Cost==0)Pending=null;
            if(Pending!=null && (!Guid.TryParseExact(Pending.Transaction,"N",out _) || catalog.Find(Pending.ClassId)==null || Pending.Cost<=0 || string.IsNullOrEmpty(Pending.WorldPath)))throw new InvalidDataException("Invalid class purchase");
            if(PendingXP!=null && string.IsNullOrEmpty(PendingXP.Receipt) && string.IsNullOrEmpty(PendingXP.ClassId) && string.IsNullOrEmpty(PendingXP.WorldPath) && string.IsNullOrEmpty(PendingXP.Boss) && string.IsNullOrEmpty(PendingXP.Tomb) && PendingXP.Night==0 && PendingXP.Generation==0 && PendingXP.Experience==0 && PendingXP.Seed==0)PendingXP=null;
            PendingXP?.Validate();
        }
    }
    public interface IAccountProgressionStore
    {AccountProgression Read(string profile);bool Write(string profile,AccountProgression data);}
    public interface IClassEntitlementStore {bool Owned(string profile,string classId);}
    // Local Alpha durability, not authenticated ownership. Future account/cloud providers replace this boundary.
    public sealed class LocalAccountProgressionStore:IAccountProgressionStore,IClassEntitlementStore
    {
        [Serializable]private sealed class Envelope {public string Payload,Checksum;}
        public string Directory {get;}
        public LocalAccountProgressionStore(string directory)=>Directory=directory;
        private string PathFor(string profile){if(!LocalPlayerIdentity.Valid(profile))throw new ArgumentException("Invalid profile");return Path.Combine(Directory,"class-"+profile+".json");}
        private static string Hash(string value){using(var hash=SHA256.Create())return Convert.ToBase64String(hash.ComputeHash(Encoding.UTF8.GetBytes(value)));}
        private static AccountProgression Decode(string text){var e=JsonUtility.FromJson<Envelope>(text);if(e==null || e.Checksum!=Hash(e.Payload))throw new InvalidDataException("Invalid class checksum");var d=JsonUtility.FromJson<AccountProgression>(e.Payload);if(d.Version==1){d.Version=2;d.PendingXP=null;}d.Validate();return d;}
        public AccountProgression Read(string profile)
        {
            var path=PathFor(profile);if(!File.Exists(path) && !File.Exists(path+".backup"))return AccountProgression.New();
            foreach(var p in new[]{path,path+".backup"}){if(!File.Exists(p))continue;try{var data=Decode(File.ReadAllText(p));return ClassRewardPersistence.Active(profile)?data:ClassRewardPersistence.Recover(profile,data,this);}catch(Exception e)when(e is InvalidDataException || e is IOException || e is ArgumentException || e is InvalidOperationException){}}
            throw new InvalidDataException("Class profile unavailable");
        }
        public bool Write(string profile,AccountProgression data)
        {
            try
            {
                data.Validate();var path=PathFor(profile);System.IO.Directory.CreateDirectory(Directory);var payload=JsonUtility.ToJson(data);var text=JsonUtility.ToJson(new Envelope{Payload=payload,Checksum=Hash(payload)});var bytes=Encoding.UTF8.GetBytes(text);
                using(var stream=new FileStream(path+".temporary",FileMode.Create,FileAccess.Write,FileShare.None)){stream.Write(bytes,0,bytes.Length);stream.Flush(true);}Decode(File.ReadAllText(path+".temporary"));
                if(File.Exists(path)){var valid=false;try{Decode(File.ReadAllText(path));valid=true;}catch(Exception e)when(e is InvalidDataException || e is IOException || e is ArgumentException || e is InvalidOperationException){}File.Replace(path+".temporary",path,valid?path+".backup":null);}else File.Move(path+".temporary",path);return true;
            }
            catch(Exception e)when(e is InvalidDataException || e is IOException || e is UnauthorizedAccessException || e is ArgumentException || e is InvalidOperationException){return false;}
        }
        public bool Owned(string profile,string classId)=>Read(profile).Find(classId)?.Owned==true;
    }
    public interface IClassPurchaseWallet
    {bool Available{get;}bool Commit(string transaction,int price,Func<bool> grant);}
    public static class ClassProfiles
    {
        private sealed class NoWallet:IClassPurchaseWallet {public bool Available=>false;public bool Commit(string transaction,int price,Func<bool> grant)=>false;}
        private static IAccountProgressionStore store;
        public static IClassPurchaseWallet Premium {get;private set;}=new NoWallet();
        public static IAccountProgressionStore Store=>store??(store=new LocalAccountProgressionStore(Path.Combine(Path.GetDirectoryName(SaveRepository.Default().Path),"Profiles")));
        public static event Action Changed;
        public static string LocalId=>NetworkSession.Instance!=null && LocalPlayerIdentity.Valid(NetworkSession.Instance.LocalProfile)?NetworkSession.Instance.LocalProfile:LocalPlayerIdentity.Get();
        public static AccountProgression Local=>Store.Read(LocalId);
        public static bool Lobby=>!SceneFlow.IsTransitioning && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name==SceneFlow.HomeScene && (NetworkSession.Instance==null || NetworkSession.Instance.State==NetworkSessionState.Idle || NetworkSession.Instance.State==NetworkSessionState.Waiting && !NetworkSession.Instance.Manager.IsListening);
        public static bool Update(string id,Func<AccountProgression,bool> edit)
        {var d=Store.Read(id);if(!edit(d) || !Store.Write(id,d))return false;Changed?.Invoke();return true;}
        public static bool Select(string id)=>Lobby && Update(LocalId,d=>{if(d.Find(id)?.Owned!=true)return false;d.Selected=id;return true;});
        public static bool Spend(string id,string node)=>Lobby && Update(LocalId,d=>
        {var p=d.Find(id);var def=ClassCatalog.Current.Find(id);var n=def==null?null:Array.Find(def.Nodes,x=>x.Id==node);if(p==null || !p.Owned || n==null || p.Nodes.Contains(node) || p.Nodes.Length>=ClassCatalog.Current.Earned(p.XP) || !string.IsNullOrEmpty(n.Prerequisite) && !p.Nodes.Contains(n.Prerequisite))return false;p.Nodes=p.Nodes.Concat(new[]{node}).ToArray();return true;});
        public static bool Respec(string id)=>Lobby && Update(LocalId,d=>{var p=d.Find(id);if(p?.Owned!=true)return false;p.Nodes=Array.Empty<string>();return true;});
        public static SaveRepository[] GoldSources=>new[]{SceneFlow.Repository,SaveRepository.MultiplayerDefault()};
        private static PlayerSnapshot Payer(SavedGame game)=>game.IsMultiplayer ? game.Multiplayer.HostId==LocalId?Array.Find(game.Multiplayer.Players,p=>p.Id==LocalId)?.State:null : game.Player;
        public static long GoldAvailable(SaveGameRules rules)
        {long best=0;foreach(var repo in GoldSources)try{if(repo.Exists)best=Math.Max(best,Payer(repo.Read(rules.Validate))?.Gold??0);}catch(Exception e)when(e is InvalidDataException || e is IOException || e is ArgumentException || e is InvalidOperationException){}return best;}
        public static bool Reconcile(SaveGameRules rules)
        {
            var pending=Local.Pending;if(pending==null)return true;
            var repo=Array.Find(GoldSources,r=>r.Path==pending.WorldPath);if(repo==null || !repo.Exists)return false;
            try
            {
                var game=repo.Read(rules.Validate);var payer=Payer(game);if(payer==null)return false;
                // A crash before the world commit leaves only the intent. No debit receipt means no grant.
                // Clear that intent so a fresh purchase can retry; a committed receipt grants exactly once.
                if(!(payer.ClassPurchases??Array.Empty<string>()).Contains(pending.Transaction))return Update(LocalId,d=>{if(d.Pending?.Transaction!=pending.Transaction)return false;d.Pending=null;return true;});
                return Update(LocalId,d=>{if(d.Pending?.Transaction!=pending.Transaction)return false;d.Find(pending.ClassId).Owned=true;d.Pending=null;return true;});
            }catch(Exception e)when(e is InvalidDataException || e is IOException || e is ArgumentException || e is InvalidOperationException){return false;}
        }
        public static bool Purchase(string id,SaveGameRules rules)
        {
            if(!Lobby || !Reconcile(rules))return false;var definition=ClassCatalog.Current.Find(id);if(definition==null || Local.Find(id)?.Owned==true)return false;
            if(definition.Currency=="PremiumGem")return definition.Price>0 && Premium.Available && Premium.Commit(Guid.NewGuid().ToString("N"),definition.Price,()=>Update(LocalId,d=>{if(d.Find(id).Owned)return false;d.Find(id).Owned=true;return true;}));
            foreach(var repo in GoldSources)
            {
                if(!repo.Exists)continue;
                try
                {
                    var game=repo.Read(rules.Validate);var payer=Payer(game);if(payer==null || payer.Gold<definition.Price)continue;
                    var intent=new ClassPurchaseIntent{Transaction=Guid.NewGuid().ToString("N"),ClassId=id,WorldPath=repo.Path,Cost=definition.Price};
                    if(!Update(LocalId,d=>{if(d.Pending!=null)return false;d.Pending=intent;return true;}))return false;
                    payer.Gold-=definition.Price;payer.ClassPurchases=(payer.ClassPurchases??Array.Empty<string>()).Concat(new[]{intent.Transaction}).ToArray();
                    // game.Player mirrors the host record; update both references before atomic world commit.
                    if(game.IsMultiplayer)game.Player=payer;
                    try{repo.Write(game,rules.Validate);}catch(Exception e)when(e is InvalidDataException || e is IOException || e is UnauthorizedAccessException || e is ArgumentException || e is InvalidOperationException){Update(LocalId,d=>{d.Pending=null;return true;});return false;}
                    return Reconcile(rules);
                }catch(Exception e)when(e is InvalidDataException || e is IOException || e is ArgumentException || e is InvalidOperationException){return false;}
            }
            return false;
        }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public static void DevelopmentProviders(IAccountProgressionStore profiles,IClassPurchaseWallet wallet=null){store=profiles;Premium=wallet??new NoWallet();}
#endif
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]private static void Reset(){store=null;Premium=new NoWallet();Changed=null;}
    }
}
