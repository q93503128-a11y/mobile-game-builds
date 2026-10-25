using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
namespace Wildfolk.Editor
{
    public static class AccountFoundationQa
    {
        private sealed class Cloud:IAccountCloudTransport
        {
            private CloudAccountProfile profile=new CloudAccountProfile{Schema=1,PlayerId="isolated-editor-account",SupportId="LF-1234-5678-90AB-CDEF-1234",CreatedAt="2026-10-25T00:00:00Z",ProviderLinked=true,AlphaRecorded=true,ClassProfile=CloudClassProfile.From(AccountProgression.New()),Entitlements=Array.Empty<AccountEntitlement>()};
            public Task<AccountReply> Call(string operation,Dictionary<string,object> input)
            {if(operation=="RequestAccountDeletionPreparation")return Task.FromResult(new AccountReply{ok=true,deletionPending=true,deletionId="isolated-request"});if(operation=="SubmitFeedback")return Task.FromResult(new AccountReply{ok=true,ticketId="isolated-ticket"});return Task.FromResult(new AccountReply{ok=true,profile=profile});}
        }
        public static Task Running{get;private set;}
        public static void ConnectedFixture()
        {
            if(!EditorApplication.isPlaying||!DevelopmentProfile.Isolated||!BackendPolicy.Fixtures)throw new InvalidOperationException("Explicit isolated Editor Play only");
            var service=AccountService.InstallDevelopment(new DevelopmentIdentityProvider("isolated-editor-account",true),new Cloud());
            UnityEngine.Object.FindFirstObjectByType<AccountProfileScreen>().Open();Running=service.Connect();
        }
    }
}
