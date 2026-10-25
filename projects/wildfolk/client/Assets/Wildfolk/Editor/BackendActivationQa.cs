using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Unity.Services.Authentication;
namespace Wildfolk.Editor
{
    public static class BackendActivationQa
    {
        private sealed class ActualAnonymous:IAccountIdentityProvider
        {
            public string PlayerId=>AuthenticationService.Instance.PlayerId;public bool Linked=>false;
            public Task Connect()=>Task.CompletedTask;public Task UseExisting()=>throw new InvalidOperationException("PGS is not part of anonymous QA");
            public void SignOut(bool clearSession=false)=>AuthenticationService.Instance.SignOut(clearSession);
        }
        public static Task Running{get;private set;}public static string Player{get;private set;}
        public static void ActualCloud()
        {
            if(!EditorApplication.isPlaying||!DevelopmentProfile.Isolated||!BackendPolicy.Fixtures)throw new InvalidOperationException("Isolated Development Play required");
            Running=RunCloud();
        }
        private static async Task RunCloud()
        {
            var profile="act-"+Guid.NewGuid().ToString("N").Substring(0,20);await UgsBootstrap.EnsureAnonymous(profile);Player=AuthenticationService.Instance.PlayerId;
            var service=AccountService.InstallDevelopment(new ActualAnonymous(),new UgsAccountCloudTransport());await service.Connect();
            if(service.State!=AccountConnection.Connected||service.Profile==null)throw new InvalidOperationException("Actual account roundtrip failed");
            UnityEngine.Object.FindFirstObjectByType<AccountProfileScreen>().Open();
            File.WriteAllText("Captures/launch-backend-activation/unity-actual-cloud.json",JsonUtility.ToJson(new Result{PlayerId=Player,SupportId=service.Profile.SupportId,EnvironmentId=BackendSettings.Current.Environment.Id,State=service.State.ToString(),ActualUgs=true,ActualGoogle=false},true));
        }
        [Serializable]private sealed class Result{public string PlayerId,SupportId,EnvironmentId,State;public bool ActualUgs,ActualGoogle;}
        private sealed class VisualIdentity:IAccountIdentityProvider
        {private string mode;public VisualIdentity(string mode){this.mode=mode;}public string PlayerId=>"simulated-viewport";public bool Linked=>true;public Task Connect(){if(mode=="offline")throw new IOException();if(mode=="conflict")throw new AccountConflictException();return Task.CompletedTask;}public Task UseExisting()=>Connect();public void SignOut(bool clearSession=false){}}
        private sealed class VisualCloud:IAccountCloudTransport
        {private string mode;public VisualCloud(string mode){this.mode=mode;}public Task<AccountReply> Call(string operation,Dictionary<string,object> input)=>Task.FromResult(mode=="unavailable"?new AccountReply{error="setup_required"}:new AccountReply{ok=true,profile=new CloudAccountProfile{Schema=1,PlayerId="simulated-viewport",SupportId="LF-1234-5678-90AB-CDEF-1234",CreatedAt="2026-10-25T00:00:00Z",ProviderLinked=true,AlphaRecorded=true,ClassProfile=CloudClassProfile.From(AccountProgression.New()),Entitlements=Array.Empty<AccountEntitlement>()}});}
        public static void Simulate(string mode)
        {if(!EditorApplication.isPlaying||!DevelopmentProfile.Isolated||!BackendPolicy.Fixtures)throw new InvalidOperationException("Isolated UI fixture only");var service=AccountService.InstallDevelopment(new VisualIdentity(mode),new VisualCloud(mode));UnityEngine.Object.FindFirstObjectByType<AccountProfileScreen>().Open();Running=service.Connect();}
        public static void ApplySafeInsets(float left,float right)
        {if(!EditorApplication.isPlaying||!DevelopmentProfile.Isolated)throw new InvalidOperationException("Isolated UI fixture only");var safe=UnityEngine.Object.FindFirstObjectByType<Wildfolk.Presentation.SafeAreaLayout>();safe.enabled=false;var rect=(RectTransform)safe.transform;rect.anchorMin=new Vector2(left/Screen.width,0);rect.anchorMax=new Vector2(1-right/Screen.width,1);rect.offsetMin=rect.offsetMax=Vector2.zero;Canvas.ForceUpdateCanvases();}
        public static void ClearQaSession(){if(!EditorApplication.isPlaying||!DevelopmentProfile.Isolated)throw new InvalidOperationException("Isolated Play required");AccountService.Current?.SignOut();AuthenticationService.Instance.SignOut(true);}
    }
}
