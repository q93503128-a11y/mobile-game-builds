using System;
using System.Linq;
using UnityEngine;
namespace Wildfolk
{
    [Serializable]public sealed class CloudClassProfile
    {
        public int Version=2;public string Selected;public ClassProgress[] Classes;
        public static CloudClassProfile From(AccountProgression p)
        {p.Validate();if(p.Pending!=null||p.PendingXP!=null)throw new InvalidOperationException("Local transaction pending");return new CloudClassProfile{Selected=p.Selected,Classes=JsonUtility.FromJson<AccountProgression>(JsonUtility.ToJson(p)).Classes};}
        public AccountProgression Local(){var p=new AccountProgression{Version=Version,Selected=Selected,Classes=Classes};p.Validate();return p;}
    }
    [Serializable]public sealed class AccountEntitlement {public string Id,GrantedAt,Source,ReceiptId;public int MetadataVersion;}
    [Serializable]public sealed class CloudAccountProfile
    {
        public int Schema,Revision,ClassRevision;public string PlayerId,SupportId,CreatedAt;public bool ProviderLinked,AlphaRecorded,DeletionPending;
        public CloudClassProfile ClassProfile;public AccountEntitlement[] Entitlements;
        public void Validate(string player)
        {if(Schema!=1||PlayerId!=player||Revision<0||!System.Text.RegularExpressions.Regex.IsMatch(SupportId??"","^LF-(?:[A-F0-9]{4}-){4}[A-F0-9]{4}$")||!DateTime.TryParse(CreatedAt,out _)||ClassProfile==null||Entitlements==null||Entitlements.Length>256)throw new InvalidOperationException("Invalid cloud profile");ClassProfile.Local();}
    }
    [Serializable]public sealed class AccountReply {public bool ok,existing,alphaRecorded,deletionPending;public string error,correlationId,ticketId,deletionId;public CloudAccountProfile profile;public AccountEntitlement[] entitlements;}
    [Serializable]public sealed class FeedbackDiagnostic
    {public string appVersion,channel,platform,deviceModel,os,locale,saveVersion="14",profileVersion="2",protocolVersion="21",scene,sessionId,lastErrorCode,cloudState;}
}
