using System;
using System.Collections.Generic;
namespace Wildfolk.Editor
{
    public sealed class DevelopmentRewardedRevive : IRewardedReviveService
    {
        public bool Available {get;set;}=true;
        public int Requests {get;private set;}
        public Action<bool> Callback {get;private set;}
        public void Show(string id,Action<bool> callback){Requests++;Callback=callback;}
        public void Complete(bool success)=>Callback?.Invoke(success);
    }
    public sealed class DevelopmentPremiumWallet : IPremiumCurrencyWallet
    {
        public bool Available {get;set;}=true;
        public long Balance {get;private set;}
        public int Commits {get;private set;}
        private bool busy;
        private readonly HashSet<string> committed=new HashSet<string>();
        public DevelopmentPremiumWallet(long balance){Balance=balance;}
        public bool TryCommitRevive(string id,int cost,Func<bool> commit)
        {
            if(committed.Contains(id))return true;
            if(!Available || busy || cost<=0 || Balance<cost)return false;
            busy=true;try{if(!commit())return false;Balance-=cost;committed.Add(id);Commits++;return true;}finally{busy=false;}
        }
    }
}
