using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
namespace Wildfolk
{
    public static class UgsBootstrap
    {
        private static readonly System.Threading.SemaphoreSlim gate=new System.Threading.SemaphoreSlim(1,1);
        private static Task initializing;private static string configuredName,configuredId;
        public static async Task EnsureAnonymous(string authenticationProfile)
        {
            await gate.WaitAsync();try{
                if(initializing==null)initializing=Initialize(authenticationProfile);
                try{await initializing;}catch{initializing=null;throw;}
                if(!AuthenticationService.Instance.IsSignedIn)await AuthenticationService.Instance.SignInAnonymouslyAsync();
                VerifyEnvironment();
            }finally{gate.Release();}
        }
        private static async Task Initialize(string profile)
        {
            var desired=BackendSettings.Current.Environment;
            if(UnityServices.State==ServicesInitializationState.Initialized){
                if(configuredName!=desired.Name||configuredId!=desired.Id)throw new BackendSetupException("environment_already_initialized");return;
            }
            // Core1.18's public initialization API accepts a name. Check the Auth-resolved ID before service use.
            await UnityServices.InitializeAsync(new InitializationOptions().SetProfile(profile).SetEnvironmentName(desired.Name));
            configuredName=desired.Name;configuredId=desired.Id;
        }
        public static void VerifyEnvironment()
        {
            var expected=BackendSettings.Current.Environment;
            if(UnityServices.State!=ServicesInitializationState.Initialized||!AuthenticationService.Instance.IsSignedIn)throw new BackendSetupException("authentication_unavailable");
            var actual=Unity.Services.Core.Internal.CoreRegistry.Instance.GetServiceComponent<Unity.Services.Authentication.Internal.IEnvironmentId>().EnvironmentId;
            if(configuredName!=expected.Name||configuredId!=expected.Id||actual!=expected.Id)throw new BackendSetupException("environment_id_mismatch");
        }
    }
}
