using System;
using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    [Serializable] public sealed class ClassAdmission
    {
        public string Profile,Selected;public bool Owned;public long XP;public string[] Nodes;
        public static ClassAdmission Local(string profile)
        {var d=ClassProfiles.Store.Read(profile);var p=d.Find(d.Selected);return new ClassAdmission{Profile=profile,Selected=d.Selected,Owned=p.Owned,XP=p.XP,Nodes=p.Nodes};}
    }
    public interface IClassAdmissionVerifier
    {bool Verify(ClassAdmission claim,out AccountProgression account);}
    // Base admission: Gold/local claims remain untrusted. Premium verification runs before spawn.
    public sealed class SurvivorClassAdmission:IClassAdmissionVerifier
    {
        public bool Verify(ClassAdmission claim,out AccountProgression account)
        {account=null;if(claim==null || !LocalPlayerIdentity.Valid(claim.Profile))return false;account=AccountProgression.New();return true;}
    }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    // Explicit QA injection only. Never reads or writes a peer's durable profile on the Host.
    public sealed class DevelopmentAlphaClassAdmission:IClassAdmissionVerifier
    {
        public bool Verify(ClassAdmission claim,out AccountProgression account)
        {
            account=null;if(claim==null || !LocalPlayerIdentity.Valid(claim.Profile) || !claim.Owned || claim.XP<0 || claim.Nodes==null || claim.Nodes.Length>64 || ClassCatalog.Current.Find(claim.Selected)==null)return false;
            var d=AccountProgression.New();var p=d.Find(claim.Selected);p.Owned=true;p.XP=claim.XP;p.Nodes=(string[])claim.Nodes.Clone();d.Selected=claim.Selected;
            try{d.Validate();}catch(Exception){return false;}account=d;return true;
        }
    }
#endif
    // The session retains this lock across disconnects. A remote GUID is routing identity, never proof of ownership.
    public sealed class ClassAdmissionLocks
    {
        private readonly Dictionary<string,ClassProgressSession> locks=new Dictionary<string,ClassProgressSession>();
        private readonly Dictionary<string,string> premiumPlayers=new Dictionary<string,string>();
        public void Clear(){locks.Clear();premiumPlayers.Clear();}
        public bool TryGet(string profile,out ClassProgressSession progress)=>locks.TryGetValue(profile,out progress);
        public bool Admit(string localProfile,ClassAdmission claim,bool localHost,IClassAdmissionVerifier verifier,out ClassProgressSession progress,AccountProgression localSnapshot=null)
        {
            progress=null;if(claim==null || !LocalPlayerIdentity.Valid(claim.Profile) || localHost && claim.Profile!=localProfile || !localHost && claim.Profile==localProfile)return false;
            if(locks.TryGetValue(claim.Profile,out progress))return true;
            AccountProgression account;
            try{if(localHost)account=localSnapshot??ClassProfiles.Store.Read(localProfile);else if(verifier==null || !verifier.Verify(claim,out account))return false;account.Validate();}
            catch(Exception){return false;}
            progress=new ClassProgressSession(account,localHost);locks.Add(claim.Profile,progress);return true;
        }
        public bool CompletePremium(string profile,PremiumChallenge challenge,PremiumAdmissionSnapshot verified,string player)
        {
            if(!locks.TryGetValue(profile,out var current)||current.Persistent||challenge?.Profile!=profile)return false;
            var accepted=verified?.Matches(challenge,player)==true;
            if(accepted){
                if(current.Account.Selected=="engineer"&&premiumPlayers.TryGetValue(profile,out var old)&&old==player)return true;
                var account=AccountProgression.New();account.Find("engineer").Owned=true;account.Selected="engineer";
                locks[profile]=new ClassProgressSession(account,false);premiumPlayers[profile]=player;return true;
            }
            premiumPlayers.Remove(profile);if(current.Account.Selected=="engineer")locks[profile]=new ClassProgressSession(AccountProgression.New(),false);return false;
        }
    }
}
