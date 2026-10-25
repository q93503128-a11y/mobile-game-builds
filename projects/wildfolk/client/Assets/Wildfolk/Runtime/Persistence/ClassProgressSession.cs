using System;
using System.Collections.Generic;
namespace Wildfolk
{
    // Defense in depth for recent duplicate callbacks. Death sources themselves own the once-only commit.
    public sealed class RewardReceiptWindow
    {
        public const int Capacity=512;
        private readonly HashSet<string> seen=new HashSet<string>();
        private readonly Queue<string> order=new Queue<string>();
        public int Count=>order.Count;
        public bool Contains(string receipt)=>seen.Contains(receipt);
        public void Commit(string receipt)
        {if(!seen.Add(receipt))return;order.Enqueue(receipt);if(order.Count>Capacity)seen.Remove(order.Dequeue());}
    }
    public sealed class ClassProgressSession
    {
        public AccountProgression Account {get;}
        public bool Persistent {get;}
        public RewardReceiptWindow Rewards {get;}=new RewardReceiptWindow();
        public ClassProgressSession(AccountProgression account,bool persistent)
        {account.Validate();Account=account;Persistent=persistent;}
        public void ApplyCommittedXP(string classId,long xp,string receipt)
        {Account.Find(classId).XP=xp;if(!string.IsNullOrEmpty(receipt))Rewards.Commit(receipt);}
        public bool Grant(string profile,string classId,int xp,string receipt)
        {
            if(xp<0 || string.IsNullOrEmpty(receipt) || Rewards.Contains(receipt) || Account.Selected!=classId)return false;
            var p=Account.Find(classId);if(p?.Owned!=true)return false;
            var total=checked(p.XP+xp);
            if(Persistent)
            {
                if(!ClassProfiles.Update(profile,d=>{var saved=d.Find(classId);if(saved?.Owned!=true)return false;total=checked(saved.XP+xp);saved.XP=total;return true;}))return false;
            }
            p.XP=total;Rewards.Commit(receipt);return true;
        }
    }
}
