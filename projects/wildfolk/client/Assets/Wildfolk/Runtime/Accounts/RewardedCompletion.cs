using System;
namespace Wildfolk
{
    // Shared by native callbacks: only the first durable earned event and the first
    // terminal result count. Close-before-earned, cancellation and late events grant nothing.
    public sealed class RewardedCompletion
    {
        private readonly Action earned;private readonly Action<AdOutcome> finished;
        private bool rewarded,closed;
        public RewardedCompletion(Action earned,Action<AdOutcome> finished){this.earned=earned;this.finished=finished;}
        public void Earn(){if(closed||rewarded)return;try{earned();rewarded=true;}catch{Fail();}}
        public void Close()=>Finish(rewarded?AdOutcome.Earned:AdOutcome.Cancelled);
        public void Fail()=>Finish(AdOutcome.Failed);
        private void Finish(AdOutcome outcome){if(closed)return;closed=true;finished(outcome);}
    }
}
