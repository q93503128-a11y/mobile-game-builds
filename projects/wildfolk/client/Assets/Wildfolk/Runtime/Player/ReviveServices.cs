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
    public interface IPremiumCurrencyWallet
    {
        bool Available {get;}
        long Balance {get;}
        // Provider MUST atomically debit + durably commit the revive, or neither.
        // Transaction keys are persistent and idempotent across callback/retry/reload.
        // Future remote provider must reconcile its receipt with the world commit before success.
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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public static void SetDevelopmentProviders(IRewardedReviveService ad,IPremiumCurrencyWallet wallet)
        {Rewarded=ad??disabled;Wallet=wallet??disabled;}
#endif
        public static void Reset(){Rewarded=disabled;Wallet=disabled;}
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Initialize()=>Reset();
    }
}
