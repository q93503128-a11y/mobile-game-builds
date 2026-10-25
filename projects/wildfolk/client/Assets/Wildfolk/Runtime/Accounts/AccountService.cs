using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudCode;
using UnityEngine;
namespace Wildfolk
{
    public interface IAccountCloudTransport {Task<AccountReply> Call(string operation,Dictionary<string,object> input);}
    public sealed class UgsAccountCloudTransport:IAccountCloudTransport
    {
        public async Task<AccountReply> Call(string operation,Dictionary<string,object> input)
        {
            var payload=new Dictionary<string,object>{{"operation",operation},{"channel",BackendPolicy.Channel.ToString()},{"buildVersion",Application.version},{"input",input}};
            for(var attempt=0;attempt<3;attempt++)try{var reply=await CloudCodeService.Instance.CallEndpointAsync<AccountReply>("LowfireAccount",payload);if(attempt<2&&(reply.error=="busy"||reply.error=="cloud_unavailable")){await Task.Delay(250*(attempt+1));continue;}return reply;}catch{if(attempt==2)throw;await Task.Delay(250*(attempt+1));}
            throw new InvalidOperationException("Cloud unavailable");
        }
    }
    public sealed class AccountService
    {
        public static AccountService Current{get;private set;}
        public AccountConnection State{get;private set;}=AccountConnection.Offline;
        public event Action Changed;
        public string LastError{get;private set;}public string CorrelationId{get;private set;}
        public CloudAccountProfile Profile=>cache?.Profile;
        public bool Linked=>identity.Linked;public bool Dirty=>cache?.Dirty==true;
        private bool development;
        private readonly IAccountIdentityProvider identity;private readonly IAccountCloudTransport cloud;private AccountCloudCache cache;private bool busy;
        public AccountService(IAccountIdentityProvider identity,IAccountCloudTransport cloud){this.identity=identity;this.cloud=cloud;}
        public static AccountService Get()
        {if(Current!=null)return Current;var s=new AccountService(new GooglePlayGamesIdentityProvider(),new UgsAccountCloudTransport());s.cache=AccountCloudCache.Restore();if(s.cache?.Profile!=null){ClassProfiles.AttachCloud(s.cache);s.cache.Changed+=s.CacheChanged;}return Current=s;}
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public static AccountService InstallDevelopment(IAccountIdentityProvider identity,IAccountCloudTransport cloud)
        {if(!BackendPolicy.Fixtures||!DevelopmentProfile.Isolated)throw new InvalidOperationException("Isolated Development only");var s=new AccountService(identity,cloud);s.development=true;Current=s;return s;}
#endif
        private void CacheChanged(){if(cache.Dirty)State=AccountConnection.CloudError;Changed?.Invoke();}
        private bool CanOperate=>ClassProfiles.Lobby&&!busy&&(BackendSettings.Current.Enabled||development);
        private void Set(AccountConnection state,string error=null){State=state;LastError=error;Changed?.Invoke();}
        public async Task Connect(bool useExisting=false)
        {
            if(!CanOperate)return;busy=true;Set(AccountConnection.Connecting);
            try{if(useExisting)await identity.UseExisting();else await identity.Connect();await SyncCore(false);}
            catch(AccountConflictException e){Set(AccountConnection.AccountConflict,e.Code);}
            catch(Exception){Set(AccountConnection.Offline,"cloud_unavailable");}
            finally{busy=false;}
        }
        private static string OperationId()=>DateTime.UtcNow.ToString("yyyy-MM-dd")+":"+Guid.NewGuid().ToString("N");
        public async Task Sync(bool useCloud=false)
        {if(!CanOperate||string.IsNullOrEmpty(identity.PlayerId))return;busy=true;try{await SyncCore(useCloud);}catch(AccountConflictException){Set(AccountConnection.AccountConflict,"cloud_conflict");}catch(Exception){Set(AccountConnection.CloudError,"cloud_unavailable");}finally{busy=false;}}
        private async Task SyncCore(bool useCloud)
        {
            Set(AccountConnection.CloudSyncing);var previous=cache;
            var reply=await cloud.Call("GetAccountProfile",new Dictionary<string,object>());
            if(!reply.ok&&reply.error=="profile_missing")
            {
                var input=new Dictionary<string,object>{{"operationId",OperationId()}};
                if(BackendPolicy.Channel!=BuildChannel.Production && previous?.Profile==null)input["legacy"]=CloudClassProfile.From(ClassProfiles.Local);
                reply=await cloud.Call("EnsureAccountProfile",input);
            }
            if(!reply.ok){Failure(reply);return;}reply.profile.Validate(identity.PlayerId);
            if(reply.profile.DeletionPending){ClearRequestedAccount("server-confirmed-pending");return;}
            if(previous?.Profile?.PlayerId==identity.PlayerId && previous.Dirty && !useCloud)
            {
                var input=new Dictionary<string,object>{{"operationId",OperationId()},{"baseRevision",previous.Profile.ClassRevision},{"classProfile",CloudClassProfile.From(previous.Read(ClassProfiles.LocalId))}};
                var committed=await cloud.Call("CommitClassProgress",input);if(!committed.ok){Failure(committed);return;}
                reply=await cloud.Call("GetAccountProfile",new Dictionary<string,object>());if(!reply.ok){Failure(reply);return;}
            }
            if(previous!=null)previous.Changed-=CacheChanged;
            cache=AccountCloudCache.Open(identity.PlayerId,BackendSettings.Current.Environment.Name);
            reply.profile.Validate(identity.PlayerId);cache.Materialize(reply.profile,useCloud || previous?.Profile?.PlayerId==identity.PlayerId);ClassProfiles.AttachCloud(cache);cache.Changed+=CacheChanged;
            Set(reply.profile.DeletionPending?AccountConnection.DeletionPending:AccountConnection.Connected);
        }
        private void Failure(AccountReply reply){CorrelationId=reply.correlationId;Set(reply.error=="cloud_conflict"?AccountConnection.AccountConflict:AccountConnection.CloudError,reply.error);}
        public async Task<AccountReply> Submit(string category,string text,bool consent,FeedbackDiagnostic diagnostic)
        {if(!CanOperate||string.IsNullOrEmpty(identity.PlayerId))return new AccountReply{error="cloud_unavailable"};busy=true;try{var r=await cloud.Call("SubmitFeedback",new Dictionary<string,object>{{"operationId",OperationId()},{"category",category},{"text",text},{"consent",consent},{"diagnostic",consent?diagnostic:null}});if(!r.ok)Failure(r);return r;}catch(Exception){return new AccountReply{error="cloud_unavailable"};}finally{busy=false;}}
        public async Task EnrollAlpha()
        {
            if(!CanOperate||!BackendPolicy.AlphaEnrollment||!Linked||AndroidAccountBridge.IntegrityToken==null)return;busy=true;Set(AccountConnection.CloudSyncing);
            try{var op=OperationId();var hash=AccountRequestHash.ForAlpha(identity.PlayerId,BackendSettings.Current.Environment.Id,op,Application.version);var token=await AndroidAccountBridge.IntegrityToken(hash);var r=await cloud.Call("EnrollAlphaParticipation",new Dictionary<string,object>{{"operationId",op},{"integrityToken",token}});if(!r.ok)Failure(r);else await SyncCore(false);}catch(Exception){Set(AccountConnection.CloudError,"alpha_unavailable");}finally{busy=false;}
        }
        public async Task PrepareDeletion(bool confirmed)
        {if(!confirmed||!CanOperate||!Linked)return;busy=true;try{var r=await cloud.Call("RequestAccountDeletionPreparation",new Dictionary<string,object>{{"operationId",OperationId()},{"confirmation","DELETE"}});if(r.ok){
            ClearRequestedAccount(r.deletionId);
        }else Failure(r);}catch(Exception){Set(AccountConnection.CloudError,"cloud_unavailable");}finally{busy=false;}}
        private void ClearRequestedAccount(string receipt)
        {
            string cleanupError=null;try{if(cache!=null){cache.Changed-=CacheChanged;if(cache.Profile?.PlayerId==identity.PlayerId)cache.ClearForDeletion(receipt);}}catch{cleanupError="local_cleanup_failed";}finally{cache=null;ClassProfiles.AttachCloud(null);}
            try{identity.SignOut(true);}catch{cleanupError="local_cleanup_failed";}Set(AccountConnection.DeletionPending,cleanupError);
        }
        public void SignOut(){if(!ClassProfiles.Lobby||busy)return;identity.SignOut();if(cache!=null)cache.Changed-=CacheChanged;cache=null;AccountCloudCache.Deselect();Set(AccountConnection.Offline);}
        public void KeepLocal(){Set(AccountConnection.Offline);}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]private static void Reset()=>Current=null;
    }
    public static class AccountRequestHash
    {
        public static string ForAlpha(string player,string environment,string operation,string build)
        {using(var h=System.Security.Cryptography.SHA256.Create())return Convert.ToBase64String(h.ComputeHash(System.Text.Encoding.UTF8.GetBytes(player+"\n"+environment+"\n"+operation+"\n"+build))).TrimEnd('=').Replace('+','-').Replace('/','_');}
    }
}
