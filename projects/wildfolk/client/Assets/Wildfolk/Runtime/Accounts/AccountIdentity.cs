using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
namespace Wildfolk
{
    public enum AccountConnection { Offline,Connecting,Connected,CloudSyncing,CloudError,AccountConflict,DeletionPending }
    public interface IAccountIdentityProvider
    {
        string PlayerId {get;}bool Linked {get;}
        Task Connect();Task UseExisting();void SignOut(bool clearSession=false);
    }
    public sealed class AccountConflictException:Exception {public string Code{get;}public AccountConflictException(string code="account_conflict"){Code=code;}}
    public static class AndroidAccountBridge
    {
        public static Func<Task<string>> AuthorizationCode;
        public static Func<string,Task<string>> IntegrityToken;
    }
    public sealed class GooglePlayGamesIdentityProvider:IAccountIdentityProvider
    {
        private static bool Ready=>Unity.Services.Core.UnityServices.State==Unity.Services.Core.ServicesInitializationState.Initialized;
        public string PlayerId=>Ready&&AuthenticationService.Instance.IsSignedIn?AuthenticationService.Instance.PlayerId:null;
        public bool Linked=>Ready&&AuthenticationService.Instance.IsSignedIn&&!string.IsNullOrEmpty(AuthenticationService.Instance.PlayerInfo?.GetGooglePlayGamesId());
        public async Task Connect()
        {
            if(AndroidAccountBridge.AuthorizationCode==null)throw new InvalidOperationException("Platform sign-in unavailable");
            await UgsBootstrap.EnsureAnonymous(RelayRoomConnection.AuthenticationProfile);
            await AuthenticationService.Instance.GetPlayerInfoAsync();if(Linked)return;
            var code=await AndroidAccountBridge.AuthorizationCode();
            try{await AuthenticationService.Instance.LinkWithGooglePlayGamesAsync(code);}
            catch(AuthenticationException e)when(e.ErrorCode==AuthenticationErrorCodes.AccountAlreadyLinked){throw new AccountConflictException();}
            await AuthenticationService.Instance.GetPlayerInfoAsync();
        }
        public async Task UseExisting()
        {
            // Keep the cached guest token in its original SDK profile; account switch is explicit.
            var code=await AndroidAccountBridge.AuthorizationCode();AuthenticationService.Instance.SignOut(false);
            AuthenticationService.Instance.SwitchProfile("lowfire-recover");
            try{await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(code);await AuthenticationService.Instance.GetPlayerInfoAsync();}
            catch{AuthenticationService.Instance.SignOut(false);AuthenticationService.Instance.SwitchProfile(RelayRoomConnection.AuthenticationProfile);throw;}
        }
        public void SignOut(bool clearSession=false)
        {if(!Ready)return;AuthenticationService.Instance.SignOut(clearSession);if(AuthenticationService.Instance.Profile!="default")AuthenticationService.Instance.SwitchProfile(RelayRoomConnection.AuthenticationProfile);}
    }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public sealed class DevelopmentIdentityProvider:IAccountIdentityProvider
    {
        public string PlayerId{get;private set;}public bool Linked{get;private set;}
        public DevelopmentIdentityProvider(string player,bool linked){if(!BackendPolicy.Fixtures||!DevelopmentProfile.Isolated)throw new InvalidOperationException("Isolated Development only");PlayerId=player;Linked=linked;}
        public Task Connect()=>Task.CompletedTask;public Task UseExisting()=>Task.CompletedTask;public void SignOut(bool clearSession=false){PlayerId=null;Linked=false;}
    }
#endif
}
