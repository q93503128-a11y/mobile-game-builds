using System;
using System.Linq;
using UnityEngine;
namespace Wildfolk
{
    [Serializable]public sealed class ClassAdmission
    {
        public string Profile,Selected;public bool Owned;public long XP;public string[] Nodes;
        public static ClassAdmission Local(string profile)
        {var d=ClassProfiles.Store.Read(profile);var p=d.Find(d.Selected);return new ClassAdmission{Profile=profile,Selected=d.Selected,Owned=p.Owned,XP=p.XP,Nodes=p.Nodes};}
    }
    public interface IClassAdmissionVerifier
    {bool Verify(ClassAdmission claim,out AccountProgression account);}
    // Alpha local-profile claims are not backend-authenticated. No client supplies modifier magnitudes.
    public sealed class LocalAlphaClassAdmission:IClassAdmissionVerifier
    {
        public bool Verify(ClassAdmission claim,out AccountProgression account)
        {
            account=null;
            if(claim==null || !LocalPlayerIdentity.Valid(claim.Profile) || !claim.Owned || claim.Nodes==null || claim.Nodes.Length>64 || ClassCatalog.Current.Find(claim.Selected)==null || claim.XP<0)return false;
            var d=ClassProfiles.Store.Read(claim.Profile);var p=d.Find(claim.Selected);p.Owned=true;p.XP=Math.Max(p.XP,claim.XP);p.Nodes=(string[])claim.Nodes.Clone();d.Selected=claim.Selected;
            try{d.Validate();}catch(Exception){return false;}
            if(!ClassProfiles.Store.Write(claim.Profile,d))return false;account=d;return true;
        }
    }
}
