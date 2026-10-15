using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
namespace Wildfolk
{
    public enum FrontendPage { Home,Settings,Multiplayer,ConfirmNew }
    public sealed class HomeScreen : MonoBehaviour
    {
        public SaveGameRules Rules;
        public UserPreferences Preferences;
        public GameObject Menu,Settings,Multiplayer,Confirm;
        public Button ContinueButton,SingleButton,MultiplayerButton,SettingsButton;
        public Text SaveSummary,Status;
        public FrontendPage Page {get;private set;}
        public bool CanContinue=>saved!=null;
        private SavedGame saved;
        private string statusKey;
        private void OnEnable(){L.Changed+=RefreshText;SceneFlow.HomeReady();ShowHome();}
        private void OnDisable(){L.Changed-=RefreshText;Preferences.Flush();}
        public void ShowHome(){SetPage(FrontendPage.Home);RefreshSave();}
        public void OpenSettings()=>SetPage(FrontendPage.Settings);
        public void OpenMultiplayer()=>SetPage(FrontendPage.Multiplayer);
        public void SinglePlayer(){if(SceneFlow.IsTransitioning)return;if(SceneFlow.Repository.Exists)SetPage(FrontendPage.ConfirmNew);else BeginNew();}
        public void BeginNew(){Preferences.Flush();SceneFlow.NewGame();}
        public void TryContinue()
        {
            if(SceneFlow.IsTransitioning)return;RefreshSave();if(saved==null)return;
            Preferences.Flush();SceneFlow.Continue(saved,SceneFlow.Repository);
        }
        public void RefreshSave()
        {
            saved=null;statusKey=null;
            if(SceneFlow.Repository.Exists)
            {
                try{saved=SceneFlow.Repository.Read(Rules.Validate);}
                catch(Exception error)when(error is InvalidDataException || error is IOException || error is ArgumentException || error is InvalidOperationException){statusKey="home.save_unavailable";}
            }
            ContinueButton.interactable=saved!=null;RefreshText();
        }
        private void SetPage(FrontendPage page)
        {Page=page;Menu.SetActive(page==FrontendPage.Home);Settings.SetActive(page==FrontendPage.Settings);Multiplayer.SetActive(page==FrontendPage.Multiplayer);Confirm.SetActive(page==FrontendPage.ConfirmNew);}
        private void RefreshText()
        {
            SaveSummary.text=saved==null ? L.Text("home.no_save") : L.Format("home.saved_day",Math.Floor(saved.Elapsed/Rules.Clock.CycleSeconds)+1);
            Status.text=statusKey==null ? string.Empty : L.Text(statusKey);
        }
    }
}
