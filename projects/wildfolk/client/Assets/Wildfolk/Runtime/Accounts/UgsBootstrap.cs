using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
namespace Wildfolk
{
    // Shared by Relay and accounts; never initialize a second UGS container or silently switch environments.
    public static class UgsBootstrap
    {
        private static readonly System.Threading.SemaphoreSlim gate=new System.Threading.SemaphoreSlim(1,1);
        private static Task initializing;private static string configuredEnvironment;
        public static async Task EnsureAnonymous(string authenticationProfile)
        {
            await gate.WaitAsync();try{
            if(initializing==null)initializing=Initialize(authenticationProfile);
            try{await initializing;}catch{initializing=null;throw;}
            if(!AuthenticationService.Instance.IsSignedIn)await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }finally{gate.Release();}
        }
        private static async Task Initialize(string profile)
        {
            var config=BackendSettings.Current;var desired=config.Enabled?config.Environment?.Name:null;
            if(config.Enabled && (string.IsNullOrEmpty(desired)||string.IsNullOrEmpty(config.Environment.Id)))throw new InvalidOperationException("Backend environment is not configured");
            if(UnityServices.State==ServicesInitializationState.Initialized)
            {if(config.Enabled&&configuredEnvironment!=desired)throw new InvalidOperationException("UGS environment already initialized differently");return;}
            var options=new InitializationOptions().SetProfile(profile);if(desired!=null)options.SetEnvironmentName(desired);
            await UnityServices.InitializeAsync(options);configuredEnvironment=desired;
        }
    }
}
