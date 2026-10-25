using System;
using UnityEngine;
using UnityEngine.UI;
namespace Wildfolk
{
    public sealed class AccountProfileScreen:MonoBehaviour
    {
        public HomeScreen Home;public GameObject View,FeedbackView,DeleteView;
        public Text Status,Cloud,Provider,Support,Alpha,Created,Character,FeedbackStatus,DeleteMessage;
        public Button ConnectButton,SyncButton,ExistingButton,KeepLocalButton,UseCloudButton,AlphaButton,SignOutButton,SubmitButton;
        public InputField FeedbackText;public Dropdown Category;public Toggle Consent;
        private AccountService service;private int deletionConfirmation;
        private static readonly string[] Categories={"Bug Report","Game Feedback","Account / Cloud","Purchase","Player Report","Other"};
        private void Awake(){service=AccountService.Get();}
        private void OnEnable(){if(service==null)service=AccountService.Get();service.Changed+=Refresh;L.Changed+=Refresh;}
        private void OnDisable(){service.Changed-=Refresh;L.Changed-=Refresh;}
        public void Open(){if(service!=AccountService.Current){service.Changed-=Refresh;service=AccountService.Get();service.Changed+=Refresh;}Home.Menu.SetActive(false);Home.Settings.SetActive(false);View.SetActive(true);FeedbackView.SetActive(false);DeleteView.SetActive(false);Refresh();}
        public void Back(){View.SetActive(false);FeedbackView.SetActive(false);DeleteView.SetActive(false);Home.ShowHome();}
        public async void Connect(){await service.Connect();Refresh();}
        public async void UseExisting(){await service.Connect(true);Refresh();}
        public void KeepLocal(){service.KeepLocal();Refresh();}
        public async void Sync(){await service.Sync();Refresh();}
        public async void UseCloud(){await service.Sync(true);Refresh();}
        public async void Enroll(){await service.EnrollAlpha();Refresh();}
        public void SignOut(){service.SignOut();Refresh();}
        public void CopySupport(){if(service.Profile!=null)GUIUtility.systemCopyBuffer=service.Profile.SupportId;}
        public void Privacy(){var url=BackendSettings.Current.PrivacyUrl;if(BackendPolicy.ValidWeb(url))Application.OpenURL(url);else Cloud.text=L.Text("account.privacy_unavailable");}
        public void Help(){View.SetActive(false);FeedbackView.SetActive(true);DeleteView.SetActive(false);FeedbackStatus.text="";}
        public void CloseHelp(){FeedbackView.SetActive(false);View.SetActive(true);}
        public async void Submit()
        {
            SubmitButton.interactable=false;var reply=await service.Submit(Categories[Category.value],FeedbackText.text,Consent.isOn,Consent.isOn?new FeedbackDiagnostic{appVersion=Application.version,channel=BackendPolicy.Channel.ToString(),platform=Application.platform.ToString(),deviceModel=SystemInfo.deviceModel,os=SystemInfo.operatingSystem,locale=L.Language,scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,sessionId=Guid.NewGuid().ToString("N"),lastErrorCode=service.LastError,cloudState=service.State.ToString()}:null);
            FeedbackStatus.text=reply.ok?L.Text("account.ticket_sent")+" "+reply.ticketId:L.Text(reply.error=="feedback_rate_limited"?"account.feedback_limit":"account.feedback_failed");SubmitButton.interactable=true;if(reply.ok)FeedbackText.text="";
        }
        public void Delete(){View.SetActive(false);deletionConfirmation=0;DeleteView.SetActive(true);FeedbackView.SetActive(false);DeleteMessage.text=L.Text("account.delete_warning");}
        public void CancelDelete(){DeleteView.SetActive(false);View.SetActive(true);}
        public async void ConfirmDelete()
        {if(deletionConfirmation++==0){DeleteMessage.text=L.Text("account.delete_confirm");return;}await service.PrepareDeletion(true);DeleteMessage.text=L.Text(service.State==AccountConnection.DeletionPending?"account.delete_pending":"account.delete_failed");deletionConfirmation=0;}
        private void Refresh()
        {
            if(Status==null)return;for(var i=0;i<Category.options.Count;i++)Category.options[i].text=L.Text("account.category."+i);Category.RefreshShownValue();var p=service.Profile;Status.text=L.Text("account.state."+service.State);
            Cloud.text=L.Text(service.LastError=="setup_required"||service.State==AccountConnection.Offline&&!BackendSettings.Current.Enabled?"account.backend_unavailable":service.Dirty?"account.cloud_attention":service.State==AccountConnection.Connected?"account.cloud_synced":"account.cloud_offline");
            Provider.text=L.Text(service.Linked?"account.google_connected":"account.google_disconnected");Support.text=p?.SupportId??"—";Alpha.text=L.Text(p?.AlphaRecorded==true?"account.alpha_recorded":"account.alpha_none");Created.text=p==null?"—":p.CreatedAt.Substring(0,Math.Min(10,p.CreatedAt.Length));
            try{var selected=ClassProfiles.Local.Selected;var level=1;var repo=SceneFlow.Repository;if(repo.Exists)level=repo.Read(Home.Rules.Validate).Player.Growth.Level;Character.text=L.Format("account.character",level,ClassCatalog.Current.Find(selected).Name);}catch{Character.text="—";}
            var idle=service.State!=AccountConnection.Connecting&&service.State!=AccountConnection.CloudSyncing;
            ConnectButton.interactable=idle&&BackendSettings.Current.Enabled;SyncButton.interactable=idle&&service.Linked;
            ExistingButton.gameObject.SetActive(service.State==AccountConnection.AccountConflict&&service.LastError=="account_conflict");KeepLocalButton.gameObject.SetActive(service.State==AccountConnection.AccountConflict);
            UseCloudButton.gameObject.SetActive(service.State==AccountConnection.AccountConflict&&service.LastError=="cloud_conflict");AlphaButton.gameObject.SetActive(BackendPolicy.AlphaEnrollment);AlphaButton.interactable=idle&&service.Linked;SignOutButton.interactable=idle&&service.Linked;
        }
    }
}
