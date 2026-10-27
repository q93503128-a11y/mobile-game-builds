#if UNITY_EDITOR || (DEVELOPMENT_BUILD && !LOWFIRE_ALPHA && !LOWFIRE_PRODUCTION)
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Wildfolk
{
    // Explicit isolated QA injection only; never installed by product startup.
    public sealed class DevelopmentRewardedAdProvider:IRewardedAdProvider
    {
        public event Action Changed;public bool Available=true;public Queue<AdOutcome> Outcomes=new Queue<AdOutcome>();public int Shows;public bool PrivacyOptionsRequired=>false;
        public DevelopmentRewardedAdProvider(){if(!DevelopmentProfile.Isolated||!BackendPolicy.Fixtures)throw new InvalidOperationException("Isolated Development only");}
        public bool Supported=>true;public bool Ready(AdPlacement p)=>Available;public Task Prepare(AdPlacement p)=>Task.CompletedTask;
        public Task<AdOutcome> Show(AdRequestContext c,Action earned){Shows++;var r=Available&&Outcomes.Count>0?Outcomes.Dequeue():AdOutcome.Unavailable;if(r==AdOutcome.Earned)earned();Changed?.Invoke();return Task.FromResult(r);}
        public void PrivacyOptions(){}public void Dispose(){}
    }
}
#endif
