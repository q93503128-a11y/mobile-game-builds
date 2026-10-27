using System;
namespace Wildfolk
{
    public interface IRewardedReviveService
    {
        bool Available {get;}
        // Exactly one completed reward is success; close/no-fill/error is false.
        // true means the SDK/provider official reward-earned completion, never started/closed/skipped/store-open/timer.
        void Show(string transaction,Action<bool> completed);
    }
    public interface IScopedRewardedRevive {bool CanRetry{get;}bool Preparing{get;}bool AvailableFor(string transaction);void Prepare(string transaction,bool retry=false);void Resolve(string transaction,bool worldSaved);}
    public interface IPremiumCurrencyWallet
    {
        bool Available {get;}
        long Balance {get;}
        // Synchronous local providers debit and durably commit together. Remote providers use IAsyncPremiumCurrencyWallet.
        // Transaction keys are persistent and idempotent across callback/retry/reload.
        // The async remote path reconciles reservations with the world save and compensates known failures.
        bool TryCommitRevive(string transaction,int cost,Func<bool> commit);
    }
    public static class ReviveServices
    {
        private sealed class Unavailable : IRewardedReviveService,IPremiumCurrencyWallet
        {
            public bool Available=>false;public long Balance=>0;
            public void Show(string id,Action<bool> done)=>done(false);
            public bool TryCommitRevive(string id,int cost,Func<bool> commit)=>false;
        }
        private static readonly Unavailable disabled=new Unavailable();
        // No automatic development success and no release fallback grants.
        public static IRewardedReviveService Rewarded {get;private set;}=disabled;
        public static IPremiumCurrencyWallet Wallet {get;private set;}=disabled;
public static void InstallRewarded(IRewardedReviveService provider){Rewarded=provider??disabled;}
        internal static void InstallCloud(IPremiumCurrencyWallet wallet){Wallet=wallet??disabled;}
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public static void SetDevelopmentProviders(IRewardedReviveService ad,IPremiumCurrencyWallet wallet)
        {Rewarded=ad??disabled;Wallet=wallet??disabled;}
#endif
        public static void Reset(){Rewarded=disabled;Wallet=disabled;}
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Initialize()=>Reset();
    }
}
