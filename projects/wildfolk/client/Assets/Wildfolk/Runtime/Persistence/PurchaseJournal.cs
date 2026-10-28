using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Wildfolk
{
    [Serializable]public sealed class PendingPurchase {public string OperationId,Token,ProductId;}
    [Serializable]public sealed class PurchaseJournalState {public int Schema=1;public string Scope;public List<PendingPurchase> Pending=new List<PendingPurchase>();}
    public sealed class PurchaseJournal
    {
        [Serializable]private sealed class Envelope {public string Payload,Checksum;}
        public string Path{get;}public PurchaseJournalState State{get;private set;}
        private static string ScopePath(string scope)=>System.IO.Path.Combine(EconomyJournal.Root,"Purchases",EconomyJournal.Hash(scope)+".json");
        public static PurchaseJournal Open(string scope)=>new PurchaseJournal(ScopePath(scope),scope);
        public static void Erase(string scope){var path=ScopePath(scope);foreach(var suffix in new[]{"",".backup",".temporary"})if(File.Exists(path+suffix))File.Delete(path+suffix);}
        public PurchaseJournal(string path,string scope){Path=path;State=new PurchaseJournalState{Scope=scope};if(!File.Exists(path)&&!File.Exists(path+".backup"))return;foreach(var file in new[]{path,path+".backup"})try{if(!File.Exists(file))continue;var envelope=JsonUtility.FromJson<Envelope>(File.ReadAllText(file));if(envelope?.Checksum!=EconomyJournal.Hash(envelope.Payload))continue;var state=JsonUtility.FromJson<PurchaseJournalState>(envelope.Payload);Validate(state,scope);State=state;return;}catch{}throw new IOException("Purchase journal unavailable");}
        private static void Validate(PurchaseJournalState s,string scope){if(s==null||s.Schema!=1||s.Scope!=scope||s.Pending==null||s.Pending.Count>64)throw new IOException("Purchase journal schema");var ids=new HashSet<string>();foreach(var p in s.Pending)if(p==null||!Guid.TryParseExact(p.OperationId,"N",out _)||string.IsNullOrWhiteSpace(p.Token)||p.Token.Length>4096||!ids.Add(p.Token)||string.IsNullOrEmpty(p.ProductId))throw new IOException("Purchase journal entry");}
        public void Save(){Validate(State,State.Scope);var payload=JsonUtility.ToJson(State);EconomyJournal.Atomic(Path,JsonUtility.ToJson(new Envelope{Payload=payload,Checksum=EconomyJournal.Hash(payload)}));}
        public PendingPurchase Add(StorePurchase purchase){var old=State.Pending.Find(x=>x.Token==purchase.Token);if(old!=null)return old;if(State.Pending.Count>=64)throw new IOException("Purchase journal capacity");var entry=new PendingPurchase{OperationId=Guid.NewGuid().ToString("N"),ProductId=purchase.ProductId,Token=purchase.Token};State.Pending.Add(entry);try{Save();return entry;}catch{State.Pending.Remove(entry);throw;}}
        public void Remove(string token){var old=State.Pending.ToArray();State.Pending.RemoveAll(p=>p.Token==token);try{Save();Save();}catch{State.Pending=new List<PendingPurchase>(old);throw;}}
        public void Clear(){foreach(var suffix in new[]{"",".backup",".temporary"})if(File.Exists(Path+suffix))File.Delete(Path+suffix);State=new PurchaseJournalState{Scope=State.Scope};}
    }
}
