using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;
using UnityEngine;
namespace Wildfolk
{
    [Serializable]public sealed class PremiumChallenge
    {
        public string HostPlayerId,SessionId,AdmissionId,Profile,SelectedClassId="engineer",OperationId;
        public bool Valid=>!string.IsNullOrEmpty(HostPlayerId)&&HostPlayerId.Length<=96&&LocalPlayerIdentity.Valid(SessionId)&&LocalPlayerIdentity.Valid(AdmissionId)&&LocalPlayerIdentity.Valid(Profile)&&LocalPlayerIdentity.Valid(OperationId)&&SelectedClassId=="engineer";
        public Dictionary<string,object> Input()=>new Dictionary<string,object>{{"HostPlayerId",HostPlayerId},{"SessionId",SessionId},{"AdmissionId",AdmissionId},{"Profile",Profile},{"SelectedClassId",SelectedClassId},{"OperationId",OperationId}};
    }
    [Serializable]public sealed class PremiumAdmissionSnapshot
    {
        public string ProofId,PlayerId,HostPlayerId,SessionId,AdmissionId,Profile,SelectedClassId,IssuedAt,ExpiresAt,OperationId;
        public string[] AllowedClassIds;public long AccountRevision,EconomyRevision;
        public bool Matches(PremiumChallenge c,string player)
        {
            return c!=null&&c.Valid&&PlayerId==player&&HostPlayerId==c.HostPlayerId&&SessionId==c.SessionId&&AdmissionId==c.AdmissionId&&Profile==c.Profile&&SelectedClassId=="engineer"&&OperationId==c.OperationId&&AllowedClassIds!=null&&Array.IndexOf(AllowedClassIds,"engineer")>=0&&AccountRevision>=0&&EconomyRevision>=0&&ProofId!=null&&System.Text.RegularExpressions.Regex.IsMatch(ProofId,"^[a-f0-9]{64}$")&&DateTimeOffset.TryParse(IssuedAt,out var start)&&DateTimeOffset.TryParse(ExpiresAt,out var end)&&(end-start).TotalSeconds==90;
        }
    }
    public interface IPremiumAdmissionClient
    {
        bool Available{get;}string PlayerId{get;}
        Task<PremiumAdmissionSnapshot> Issue(PremiumChallenge challenge);
        Task<PremiumAdmissionSnapshot> Verify(PremiumChallenge challenge,string player,string proof);
    }
    public sealed class PremiumAdmissionClient:IPremiumAdmissionClient
    {
        [Serializable]private sealed class Reply{public bool ok;public string error;public PremiumAdmissionSnapshot admission;}
        public string PlayerId=>AccountService.Get().Profile?.PlayerId;
        public bool Available=>AccountService.Get().CloudAvailable&&(BackendSettings.Current.Enabled||AccountService.Get().IsDevelopment);
        private async Task<PremiumAdmissionSnapshot> Call(string op,Dictionary<string,object> input)
        {
            if(!Available||!AuthenticationService.Instance.IsSignedIn||AuthenticationService.Instance.PlayerId!=PlayerId)return null;
            try{
                UgsBootstrap.VerifyEnvironment();
                for(var attempt=0;attempt<2;attempt++){
                    var r=await CloudCodeService.Instance.CallEndpointAsync<Reply>("LowfirePremiumAdmission",new Dictionary<string,object>{{"operation",op},{"channel",BackendPolicy.Channel.ToString()},{"buildVersion",Application.version},{"input",input}});
                    if(r.ok)return r.admission;if(attempt==0&&(r.error=="busy"||r.error=="cloud_unavailable")){await Task.Delay(250);continue;}return null;
                }
            }catch(Exception){}return null;
        }
        public Task<PremiumAdmissionSnapshot> Issue(PremiumChallenge c)=>c?.Valid==true?Call("IssuePremiumAdmission",c.Input()):Task.FromResult<PremiumAdmissionSnapshot>(null);
        public Task<PremiumAdmissionSnapshot> Verify(PremiumChallenge c,string player,string proof)
        {if(c?.Valid!=true)return Task.FromResult<PremiumAdmissionSnapshot>(null);var input=c.Input();input["PlayerId"]=player;input["ProofId"]=proof;return Call("VerifyPremiumAdmission",input);}
    }
}
