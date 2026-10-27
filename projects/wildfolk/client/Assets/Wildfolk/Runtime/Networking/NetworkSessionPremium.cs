using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
namespace Wildfolk
{
    public sealed partial class NetworkSession
    {
        private const string PremiumMessage="lowfire.premium.v1";
        private sealed class PendingPremium{public PremiumChallenge Challenge;public bool Verifying;}
        [Serializable]private sealed class PremiumWire{public int Kind;public PremiumChallenge Challenge;public string PlayerId,ProofId;public bool Accepted;}
        private readonly Dictionary<ulong,PendingPremium> premiumPending=new Dictionary<ulong,PendingPremium>();
        private IPremiumAdmissionClient premiumClient;private string premiumSession;private AccountProgression hostPremiumSnapshot;
        public string PremiumMessageKey{get;private set;}
        private IPremiumAdmissionClient PremiumClient=>premiumClient??(premiumClient=new PremiumAdmissionClient());
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public void DevelopmentPremiumClient(IPremiumAdmissionClient client)
        {if(!DevelopmentProfile.Isolated||!BackendPolicy.Fixtures)throw new InvalidOperationException("Isolated Development only");premiumClient=client;}
#endif
        private void BeginPremiumSession(){premiumSession=Guid.NewGuid().ToString("N");premiumPending.Clear();hostPremiumSnapshot=null;PremiumMessageKey=null;}
        private PremiumChallenge Challenge(string profile)=>new PremiumChallenge{HostPlayerId=PremiumClient.PlayerId,SessionId=premiumSession,AdmissionId=Guid.NewGuid().ToString("N"),Profile=profile,OperationId=Guid.NewGuid().ToString("N")};
        private async Task PrepareHostPremium()
        {
            hostPremiumSnapshot=JsonUtility.FromJson<AccountProgression>(JsonUtility.ToJson(ClassProfiles.Store.Read(LocalProfile)));
            if(hostPremiumSnapshot.Selected!="engineer")return;
            var accepted=false;
            try{
                if(PremiumClient.Available){var work=VerifyHostPremium(Challenge(LocalProfile));if(await Task.WhenAny(work,Task.Delay(20000))==work)accepted=await work;}
            }catch(Exception){}
            if(!accepted){hostPremiumSnapshot.Selected="survivor";PremiumMessageKey="net.premium_unavailable";}
        }
        private async Task<bool> VerifyHostPremium(PremiumChallenge challenge)
        {try{var issued=await PremiumClient.Issue(challenge);if(issued?.Matches(challenge,PremiumClient.PlayerId)!=true)return false;var verified=await PremiumClient.Verify(challenge,issued.PlayerId,issued.ProofId);return verified?.Matches(challenge,issued.PlayerId)==true;}catch(Exception){return false;}}
        private void RegisterPremiumMessages()=>Manager.CustomMessagingManager.RegisterNamedMessageHandler(PremiumMessage,PremiumReceived);
        private void UnregisterPremiumMessages(){if(Manager!=null&&Manager.CustomMessagingManager!=null)Manager.CustomMessagingManager.UnregisterNamedMessageHandler(PremiumMessage);}
        private void SendPremium(ulong peer,PremiumWire message)
        {
            if(!Manager.IsListening)return;var json=JsonUtility.ToJson(message);if(System.Text.Encoding.UTF8.GetByteCount(json)>1024)return;
            using(var writer=new FastBufferWriter(2048,Allocator.Temp)){writer.WriteValueSafe(new FixedString4096Bytes(json));Manager.CustomMessagingManager.SendNamedMessage(PremiumMessage,peer,writer,NetworkDelivery.ReliableSequenced);}
        }
        private void QueuePremium(ulong peer,ClassAdmission admission)
        {
            if(peer==NetworkManager.ServerClientId)return;
            var engineer=admission.Selected=="engineer"||classLocks.TryGet(admission.Profile,out var locked)&&locked.Account.Selected=="engineer";
            if(engineer)premiumPending[peer]=new PendingPremium{Challenge=Challenge(admission.Profile)};
        }
        private IEnumerator StartPremiumChallenge(ulong peer)
        {
            yield return null;if(!premiumPending.TryGetValue(peer,out var pending))yield break;
            if(!PremiumClient.Available||!pending.Challenge.Valid){CompletePremium(peer,pending,null,null);yield break;}
            SendPremium(peer,new PremiumWire{Kind=0,Challenge=pending.Challenge});var deadline=Time.realtimeSinceStartupAsDouble+20;
            while(Manager.IsListening&&premiumPending.TryGetValue(peer,out var current)&&ReferenceEquals(current,pending)&&Time.realtimeSinceStartupAsDouble<deadline)yield return null;
            if(premiumPending.TryGetValue(peer,out var latest)&&ReferenceEquals(latest,pending))CompletePremium(peer,pending,null,null);
        }
        private void PremiumReceived(ulong sender,FastBufferReader reader)
        {
            try{
                if(reader.Length>2048)return;reader.ReadValueSafe(out FixedString4096Bytes raw);var message=JsonUtility.FromJson<PremiumWire>(raw.ToString());if(message==null)return;
                if(Manager.IsServer){if(sender!=NetworkManager.ServerClientId&&message.Kind==1)VerifyRemotePremium(sender,message);return;}
                if(sender!=NetworkManager.ServerClientId)return;
                if(message.Kind==0&&message.Challenge?.Valid==true)IssueRemotePremium(message.Challenge);
                else if(message.Kind==2){PremiumMessageKey=message.Accepted?null:"net.premium_unavailable";if(PremiumMessageKey!=null){MessageKey=PremiumMessageKey;if(State==NetworkSessionState.Playing)PlayerFeedback.ShowKey(PremiumMessageKey);}Notify();}
            }catch(Exception){}
        }
        private string lastClientPremiumAdmission;
        private async void IssueRemotePremium(PremiumChallenge challenge)
        {
            if(challenge.Profile!=LocalProfile||lastClientPremiumAdmission!=null)return;lastClientPremiumAdmission=challenge.AdmissionId;var sessionOperation=operation;
            PremiumAdmissionSnapshot issued=null;try{if(PremiumClient.Available)issued=await PremiumClient.Issue(challenge);}catch(Exception){}
            if(sessionOperation!=operation||!Manager.IsClient||Manager.IsServer||!Manager.IsListening)return;if(issued!=null&&(!PremiumClient.Available||issued.PlayerId!=PremiumClient.PlayerId))issued=null;
            SendPremium(NetworkManager.ServerClientId,new PremiumWire{Kind=1,Challenge=challenge,PlayerId=issued?.PlayerId,ProofId=issued?.ProofId});
        }
        private async void VerifyRemotePremium(ulong peer,PremiumWire message)
        {
            if(!premiumPending.TryGetValue(peer,out var pending)||pending.Verifying)return;pending.Verifying=true;
            PremiumAdmissionSnapshot verified=null;
            try{
                var c=message.Challenge;
                if(c!=null&&c.SessionId==pending.Challenge.SessionId&&c.AdmissionId==pending.Challenge.AdmissionId&&c.Profile==pending.Challenge.Profile&&c.OperationId==pending.Challenge.OperationId&&!string.IsNullOrEmpty(message.PlayerId)&&!string.IsNullOrEmpty(message.ProofId)&&PremiumClient.Available)
                    verified=await PremiumClient.Verify(pending.Challenge,message.PlayerId,message.ProofId);
            }catch(Exception){}
            if(premiumPending.TryGetValue(peer,out var current)&&ReferenceEquals(current,pending)&&Manager.IsServer&&Manager.IsListening)CompletePremium(peer,pending,verified,message.PlayerId);
        }
        private void CompletePremium(ulong peer,PendingPremium pending,PremiumAdmissionSnapshot snapshot,string player)
        {
            var accepted=classLocks.CompletePremium(pending.Challenge.Profile,pending.Challenge,snapshot,player);premiumPending.Remove(peer);
            SendPremium(peer,new PremiumWire{Kind=2,Accepted=accepted});
        }
    }
}
