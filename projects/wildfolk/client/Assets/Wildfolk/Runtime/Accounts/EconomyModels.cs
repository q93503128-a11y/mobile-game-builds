using System;
using System.Linq;
using UnityEngine;
namespace Wildfolk
{
    [Serializable]public sealed class EconomyMetric {public string Id,Mode;public int Maximum,MaxBatch,MaxDaily;public bool Critical;}
    [Serializable]public sealed class EconomyTier {public int Threshold,GemReward,AchievementPoints;}
    [Serializable]public sealed class EconomyFamily {public string Id,Category,Metric,NameKey,DescriptionKey;public EconomyTier[] Tiers;}
    [Serializable]public sealed class EconomyCatalog
    {
        public int Schema,Revision,EngineerCost,ReviveCost,MaximumBalance;public EconomyMetric[] Metrics;public EconomyFamily[] Families;public string[] Categories;public int[] Newcomer,Monthly;
        private static EconomyCatalog current;public static EconomyCatalog Current=>current??(current=JsonUtility.FromJson<EconomyCatalog>(Resources.Load<TextAsset>("EconomyCatalog").text));
        public EconomyMetric Metric(string id)=>Array.Find(Metrics,m=>m.Id==id);
    }
    [Serializable]public sealed class EconomyDelta {public string Id;public int Value;public string[] Sources=Array.Empty<string>();}
    [Serializable]public sealed class EconomyBoard {public string Id,Period,NameKo,NameEn,State,DefinitionHash;public int Count;public int[] Rewards;public bool Available;}
    [Serializable]public sealed class EconomyRevive {public string Source,State,TransactionId;public int Cost;}
    [Serializable]public sealed class GemTransaction {public string TransactionId,PlayerId,Timestamp,Reason,SourceId,OperationId,BuildChannel,Operator,CaseReference,AdjustmentReason;public int Sequence,Delta,BalanceBefore,BalanceAfter,MetadataVersion;}
    [Serializable]public sealed class EconomySnapshot
    {
        public int Schema,CatalogRevision,Revision,Balance,EngineerCost,ReviveCost,points,completed,total;public string PlayerId,EnvironmentId,ServerTime,NextReset;public bool EngineerOwned;
        public EconomyDelta[] Metrics=Array.Empty<EconomyDelta>();public string[] Claimed=Array.Empty<string>();public EconomyBoard[] Boards=Array.Empty<EconomyBoard>();public EconomyRevive[] PendingRevives=Array.Empty<EconomyRevive>();
        public int Value(string metric){var n=Array.Find(Metrics,m=>m.Id==metric)?.Value??0;return metric=="biomes"?Convert.ToString(n,2).Count(c=>c=='1'):n;}
        public bool Claimable=>EconomyCatalog.Current.Families.Any(f=>f.Tiers.Where((t,i)=>Value(f.Metric)>=t.Threshold&&!Claimed.Contains(f.Id+":"+i)).Any());
        public void Validate(string player,string environment){if(Schema!=1||CatalogRevision!=EconomyCatalog.Current.Revision||PlayerId!=player||EnvironmentId!=environment||Revision<0||Balance<0||Balance>EconomyCatalog.Current.MaximumBalance||!DateTimeOffset.TryParse(ServerTime,out _)||!DateTimeOffset.TryParse(NextReset,out _)||Metrics==null||Claimed==null||Boards==null||PendingRevives==null)throw new InvalidOperationException("Invalid economic snapshot");}
    }
    [Serializable]public sealed class EconomyReply {public bool ok,alreadyOwned,alreadyClaimed;public string error,correlationId,transactionId;public int granted,claimedCount,claimedDay,acknowledged,nextOffset,transactionCount;public EconomySnapshot snapshot;public EconomyRevive revive;public GemTransaction[] transactions;}
}
