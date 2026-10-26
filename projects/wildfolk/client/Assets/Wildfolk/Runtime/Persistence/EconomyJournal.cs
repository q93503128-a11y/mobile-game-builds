using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    // A bounded aggregate outbox, not one request per action. Never changes server balances.
    [Serializable]public sealed class ProgressBatch {public string Operation;public int First,Last;public List<EconomyDelta> Metrics=new List<EconomyDelta>();}
    [Serializable]public sealed class GemReviveIntent {public string Source,WorldPath,Run,Stage,BeginOperation,FinishOperation;public long Death;}
    [Serializable]public sealed class EconomyJournalState
    {
        public int Schema=1,Next=1;public string Scope,Device=Guid.NewGuid().ToString("N");public List<EconomyDelta> Totals=new List<EconomyDelta>();public List<string> Recent=new List<string>(),Critical=new List<string>();
        public List<ProgressBatch> Outbox=new List<ProgressBatch>();public ProgressBatch Mutable=new ProgressBatch();public EconomySnapshot Cached;public GemReviveIntent Revive;public string AdoptedBy;
    }
    public sealed class EconomyJournal
    {
        [Serializable]private sealed class Envelope {public string Payload,Checksum;}
        public string Path{get;}public EconomyJournalState State{get;private set;}
        public static string Hash(string s){using(var h=SHA256.Create())return BitConverter.ToString(h.ComputeHash(Encoding.UTF8.GetBytes(s))).Replace("-","").ToLowerInvariant();}
        public static string Root=>System.IO.Path.Combine(System.IO.Path.GetDirectoryName(SaveRepository.Default().Path),"Economy");
        public static EconomyJournal Open(string scope)=>new EconomyJournal(System.IO.Path.Combine(Root,Hash(scope)+".json"),scope);
        public EconomyJournal(string path,string scope){Path=path;State=new EconomyJournalState{Scope=scope};if(File.Exists(path)||File.Exists(path+".backup")){Exception last=null;foreach(var p in new[]{path,path+".backup"})if(File.Exists(p))try{State=Decode(File.ReadAllText(p),scope);return;}catch(Exception e)when(e is IOException||e is ArgumentException||e is InvalidOperationException){last=e;}throw new IOException("Economic journal unavailable",last);}}
        private static EconomyJournalState Decode(string text,string scope){var e=JsonUtility.FromJson<Envelope>(text);if(e==null||e.Checksum!=Hash(e.Payload))throw new IOException("Economic journal checksum");var d=JsonUtility.FromJson<EconomyJournalState>(e.Payload);if(d.Schema!=1||d.Scope!=scope||!Guid.TryParseExact(d.Device,"N",out _)||d.Next<1||d.Outbox==null||d.Outbox.Count>32||d.Totals==null||d.Recent==null||d.Critical==null||d.Mutable==null)throw new IOException("Economic journal schema");if(string.IsNullOrEmpty(d.Revive?.Source))d.Revive=null;if(string.IsNullOrEmpty(d.Cached?.PlayerId))d.Cached=null;return d;}
        public void Save(){var payload=JsonUtility.ToJson(State);Atomic(Path,JsonUtility.ToJson(new Envelope{Payload=payload,Checksum=Hash(payload)}));}
        public static void Atomic(string path,string text){Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));var bytes=Encoding.UTF8.GetBytes(text);using(var f=new FileStream(path+".temporary",FileMode.Create,FileAccess.Write,FileShare.None)){f.Write(bytes,0,bytes.Length);f.Flush(true);}if(File.Exists(path))File.Replace(path+".temporary",path,path+".backup");else File.Move(path+".temporary",path);}
        public bool Record(string metric,int value,string source)
        {
            var m=EconomyCatalog.Current.Metric(metric);if(m==null||value<=0||value>(m.Mode=="mask"?15:m.Maximum)||string.IsNullOrEmpty(source)||State.Next==int.MaxValue||State.Outbox.Count>=32)return false;
            var id=metric+":"+Hash(source);if(State.Recent.Contains(id)||m.Critical&&State.Critical.Contains(id)||m.Critical&&State.Critical.Count>=4096)return false;
            var total=State.Totals.Find(x=>x.Id==metric);var old=total?.Value??0;var next=m.Mode=="mask"?old|value:Math.Min(m.Maximum,old+value);if(next==old)return false;
            var before=JsonUtility.ToJson(State);try{
                if(total==null){total=new EconomyDelta{Id=metric};State.Totals.Add(total);}total.Value=next;
                if(m.Critical)State.Critical.Add(id);State.Recent.Add(id);if(State.Recent.Count>2048)State.Recent.RemoveAt(0);
                var b=State.Mutable;if(b.First==0)b.First=State.Next;b.Last=State.Next++;
                var delta=b.Metrics.Find(x=>x.Id==metric);if(delta==null){delta=new EconomyDelta{Id=metric};b.Metrics.Add(delta);}delta.Value=m.Mode=="mask"?delta.Value|value:delta.Value+(next-old);if(m.Critical)delta.Sources=delta.Sources.Concat(new[]{Hash(source)}).ToArray();Save();return true;
            }catch{State=JsonUtility.FromJson<EconomyJournalState>(before);throw;}
        }
        public ProgressBatch Freeze(){if(State.Outbox.Count==0&&State.Mutable.First>0){State.Mutable.Operation=Guid.NewGuid().ToString("N");State.Outbox.Add(State.Mutable);State.Mutable=new ProgressBatch();Save();}return State.Outbox.FirstOrDefault();}
        public void Acknowledge(int sequence){State.Outbox.RemoveAll(b=>b.Last<=sequence);State.Next=Math.Max(State.Next,sequence+1);Save();}
        public void Cache(EconomySnapshot snapshot){State.Cached=snapshot;Save();}
        public void Clear(){foreach(var suffix in new[]{"",".backup",".temporary"})if(File.Exists(Path+suffix))File.Delete(Path+suffix);State=new EconomyJournalState{Scope=State.Scope};}
        // Adoption is a two-file journal: binding first prevents a second account from taking it; server source receipts + transferred stream preserve retries.
        public void Adopt(EconomyJournal guest)
        {
            if(State.AdoptedBy=="complete"||!string.IsNullOrEmpty(guest.State.AdoptedBy)&&guest.State.AdoptedBy!=State.Scope)return;
            if(State.Next!=1||State.Outbox.Count>0||State.Mutable.First>0)return;
            guest.State.AdoptedBy=State.Scope;guest.Save();var cached=State.Cached;var scope=State.Scope;State=JsonUtility.FromJson<EconomyJournalState>(JsonUtility.ToJson(guest.State));State.Scope=scope;State.Cached=cached;State.Revive=null;State.AdoptedBy="complete";Save();
        }
    }
}
