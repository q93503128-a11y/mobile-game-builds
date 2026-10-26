using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
namespace Wildfolk
{
    public interface IAsyncPremiumCurrencyWallet
    {Task<bool> CommitRevive(string transaction,int cost,Func<bool> commit);Task Reconcile();}
    // Compensating transaction across Cloud Save and the existing atomic world save. Unknown disk/network outcomes remain pending.
    public sealed class CloudGemReviveWallet:IPremiumCurrencyWallet,IAsyncPremiumCurrencyWallet
    {
        private EconomyService service;private bool resolving;
        public CloudGemReviveWallet(EconomyService service)=>this.service=service;
        public bool Available=>service.CanSpend;public long Balance=>service.Snapshot?.Balance??0;
        public bool TryCommitRevive(string transaction,int cost,Func<bool> commit)=>false;
        public async Task<bool> CommitRevive(string transaction,int cost,Func<bool> commit)
        {
            if(!Available||cost!=EconomyCatalog.Current.ReviveCost||Balance<cost||resolving)return false;
            var world=UnityEngine.Object.FindFirstObjectByType<WorldSaveService>();var death=world?.Player?.GetComponent<PlayerDeath>();if(world==null||death==null||transaction!=death.Run+":"+death.DeathNumber+":gem")return false;
            var journal=service.Journal;var intent=new GemReviveIntent{Source=Guid.NewGuid().ToString("N"),BeginOperation=Guid.NewGuid().ToString("N"),FinishOperation=Guid.NewGuid().ToString("N"),WorldPath=world.Repository.Path,Run=death.Run,Death=death.DeathNumber,Stage="Prepared"};
            try{journal.State.Revive=intent;journal.Save();}catch{return false;}
            resolving=true;try{
                var r=await Begin(intent);if(!r.ok){ClearRejected(journal,r);return false;}if(r.revive?.State!="Reserved"){journal.State.Revive=null;journal.Save();service.PendingChanged();return false;}
                if(service.Journal!=journal)return false;
                intent.Stage="CommitStarted";journal.Save();var committed=commit();
                if(committed){intent.Stage="Committed";journal.Save();await Finish(journal,intent,"committed");return true;}
                var outcome=DiskOutcome(intent);if(outcome==null&&death!=null&&(death.Run!=intent.Run||death.DeathNumber!=intent.Death))outcome="failed";if(outcome!=null){intent.Stage=outcome=="failed"?"Failed":"Committed";journal.Save();await Finish(journal,intent,outcome);}return false;
            }catch(Exception){return false;}finally{resolving=false;}
        }
        private Task<EconomyReply> Begin(GemReviveIntent i)=>service.ReviveCall("BeginRevive",new Dictionary<string,object>{{"operationId",i.BeginOperation},{"source",i.Source},{"expectedCost",EconomyCatalog.Current.ReviveCost}});
        private async Task Finish(EconomyJournal journal,GemReviveIntent i,string outcome)
        {
            if(service.Journal!=journal)return;var r=await service.ReviveCall("FinishRevive",new Dictionary<string,object>{{"operationId",i.FinishOperation},{"source",i.Source},{"outcome",outcome}});
            if(r.ok&&service.Journal==journal){journal.State.Revive=null;journal.Save();service.PendingChanged();}
        }
        public static string DiskOutcome(GemReviveIntent i)
        {
            if(i.Stage=="Committed")return "committed";if(i.Stage=="Prepared"||i.Stage=="Failed")return "failed";
            // Do not fall back to an older backup to decide a refund after an uncertain successful write.
            try{var game=SaveRepository.Decode(File.ReadAllText(i.WorldPath));if(game.IsMultiplayer)return null;var d=game.Player.Death;if(d.Run!=i.Run||d.Death!=i.Death)return null;return d.Dead?"failed":"committed";}catch(Exception){return null;}
        }
        private void ClearRejected(EconomyJournal journal,EconomyReply r)
        {if(service.Journal==journal&&r.snapshot!=null&&Array.Find(r.snapshot.PendingRevives,x=>x.Source==journal.State.Revive.Source)==null){journal.State.Revive=null;journal.Save();service.PendingChanged();}}
        public async Task Reconcile()
        {
            if(resolving||!service.Online||service.Busy)return;var journal=service.Journal;var i=journal.State.Revive;if(i==null)return;
            resolving=true;try{var r=await Begin(i);if(service.Journal!=journal)return;if(!r.ok){ClearRejected(journal,r);return;}if(r.revive?.State!="Reserved"){journal.State.Revive=null;journal.Save();service.PendingChanged();return;}var outcome=DiskOutcome(i);if(outcome!=null)await Finish(journal,i,outcome);}catch(Exception){}finally{resolving=false;}
        }
    }
}
