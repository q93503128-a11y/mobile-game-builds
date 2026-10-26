using System;
using UnityEngine;
using UnityEngine.UI;
namespace Wildfolk
{
    public sealed class ClassLobbyScreen:MonoBehaviour
    {
        public HomeScreen Home;public GameObject View,Confirmation;public SaveGameRules Rules;
        public Button[] Cards,Nodes;public Text[] CardNames,CardRoles,CardStates,NodeNames,Branches;
        public Image ClassIcon,TreeIcon;public Image[] CardMarks,NodeIcons;public Sprite[] ClassIcons,SkillIcons;public JournalSkillNode[] NodeViews;
        public Text Title,Role,Description,Progress,NodeDetail,PurchaseLabel,SelectLabel,InvestLabel,Currency,ConfirmText,Status;
        public Button PurchaseButton,SelectButton,RespecButton,InvestButton;
        private int selected,node;private AccountProgression account;
        private void OnEnable(){ClassProfiles.Changed+=Refresh;L.Changed+=Refresh;EconomyService.Get().Changed+=Refresh;}
        private void OnDisable(){ClassProfiles.Changed-=Refresh;L.Changed-=Refresh;if(EconomyService.Current!=null)EconomyService.Current.Changed-=Refresh;}
        public void Open()
        {
            Home.Menu.SetActive(false);Home.Settings.SetActive(false);Home.Multiplayer.SetActive(false);Home.Confirm.SetActive(false);View.SetActive(true);Confirmation.SetActive(false);
            try{ClassProfiles.Reconcile(Rules);selected=Array.FindIndex(ClassCatalog.Current.Classes,c=>c.Id==ClassProfiles.Local.Selected);if(selected<0)selected=0;node=0;Status.text="";Refresh();}catch(Exception){Unavailable();}
        }
        public void Back(){Confirmation.SetActive(false);View.SetActive(false);Home.ShowHome();}
        public void SelectCard(int index){if(index<0 || index>=Cards.Length)return;selected=index;node=0;Status.text="";Confirmation.SetActive(false);Refresh();}
        public void SelectNode(int index){if(index<0 || index>=Nodes.Length)return;node=index;Status.text="";Refresh();}
        public void Invest(){var d=ClassCatalog.Current.Classes[selected];if(!ClassProfiles.Spend(d.Id,d.Nodes[node].Id))Status.text=L.Text("class.cannot_spend");Refresh();}
        public void SelectClass(){if(ClassProfiles.Select(ClassCatalog.Current.Classes[selected].Id))Status.text=L.Text("class.selected");Refresh();}
        public void Respec(){ClassProfiles.Respec(ClassCatalog.Current.Classes[selected].Id);Refresh();}
        public void AskPurchase(){var d=ClassCatalog.Current.Classes[selected];if(!ClassProfiles.Lobby || !PurchaseButton.interactable)return;ConfirmText.text=L.Format("class.confirm",d.Name,d.Price,d.Currency=="Gold"?L.Text("ui.gold"):L.Text("class.gem"));Confirmation.SetActive(true);}
        public void CancelPurchase()=>Confirmation.SetActive(false);
        public async void ConfirmPurchase(){Confirmation.SetActive(false);var d=ClassCatalog.Current.Classes[selected];var ok=d.Currency=="PremiumGem"?await EconomyService.Get().UnlockEngineer():ClassProfiles.Purchase(d.Id,Rules);Status.text=L.Text(ok?"class.purchased":"class.purchase_failed");Refresh();}
        private void Unavailable(){Status.text=L.Text("class.profile_failed");PurchaseButton.interactable=SelectButton.interactable=RespecButton.interactable=InvestButton.interactable=false;}
        public void Refresh()
        {
            if(View==null || !View.activeSelf)return;
            try{account=ClassProfiles.Local;}catch(Exception){Unavailable();return;}
            var catalog=ClassCatalog.Current;var d=catalog.Classes[selected];var p=account.Find(d.Id);var owned=p.Owned;var lobby=ClassProfiles.Lobby;var points=catalog.Earned(p.XP)-p.Nodes.Length;
            for(var i=0;i<Cards.Length;i++)
            {
                var c=catalog.Classes[i];var progress=account.Find(c.Id);CardNames[i].text=c.Name;CardRoles[i].text=L.Text("journal.role."+c.Id);CardStates[i].text=L.Text(account.Selected==c.Id?"class.selected":progress.Owned?"class.owned":"class.locked");
                Cards[i].image.color=selected==i?new Color32(137,88,56,38):Color.clear;CardNames[i].fontStyle=selected==i?FontStyle.Bold:FontStyle.Normal;CardMarks[i].enabled=account.Selected==c.Id;
            }
            ClassIcon.sprite=TreeIcon.sprite=ClassIcons[selected];Title.text=d.Name;Role.text=L.Text("journal.role."+d.Id)+"\n"+L.Text(d.Active?"class.has_active":"class.passive");Description.text=d.Description;
            Progress.text=L.Format("journal.class_progress",p.XP,points,catalog.Earned(p.XP));var next=Array.Find(catalog.Milestones,x=>x>p.XP);if(next>0)Progress.text+="\n"+L.Format("class.next",next);
            for(var i=0;i<Nodes.Length;i++)
            {
                var n=d.Nodes[i];var learned=Array.IndexOf(p.Nodes,n.Id)>=0;var prerequisite=string.IsNullOrEmpty(n.Prerequisite)||Array.IndexOf(p.Nodes,n.Prerequisite)>=0;var available=owned && prerequisite && points>0;
                NodeNames[i].text=n.Name;NodeNames[i].color=learned || available?JournalStyle.Ink:JournalStyle.Muted;
                NodeViews[i].SetState(learned,available,i==node);NodeIcons[i].sprite=SkillIcons[selected*9+i];
            }
            for(var i=0;i<Branches.Length;i++)Branches[i].text=d.Nodes[i*3].Branch;
            var chosen=d.Nodes[node];var has=Array.IndexOf(p.Nodes,chosen.Id)>=0;var requires=string.IsNullOrEmpty(chosen.Prerequisite)||Array.IndexOf(p.Nodes,chosen.Prerequisite)>=0;
            NodeDetail.text=chosen.Description;if(!requires)Status.text=L.Format("journal.requires",Array.Find(d.Nodes,n=>n.Id==chosen.Prerequisite).Name);
            SelectButton.interactable=lobby && owned && account.Selected!=d.Id;SelectLabel.text=L.Text(account.Selected==d.Id?"class.selected":"class.select");
            PurchaseButton.gameObject.SetActive(!owned);PurchaseButton.interactable=lobby && (d.Currency=="Gold"?ClassProfiles.GoldAvailable(Rules)>=d.Price:d.Price>0 && EconomyService.Get().CanSpend && (EconomyService.Get().Snapshot?.Balance??0)>=d.Price);
            PurchaseLabel.text=d.Currency=="Gold"?L.Format("class.price",d.Price,L.Text("ui.gold")):d.Price>0?L.Format("class.price",d.Price,L.Text("class.gem")):L.Text("journal.premium_locked");
            InvestButton.interactable=lobby && owned && !has && points>0 && requires;InvestLabel.text=L.Text(has?"class.learned":!owned || !requires?"class.locked":points<=0?"journal.need_point":"class.invest");
            RespecButton.interactable=lobby && owned && p.Nodes.Length>0;Currency.text=ClassProfiles.GoldAvailable(Rules)+" · "+L.Format("economy.balance",EconomyService.Get().Snapshot?.Balance.ToString()??"—");
        }
    }
}
