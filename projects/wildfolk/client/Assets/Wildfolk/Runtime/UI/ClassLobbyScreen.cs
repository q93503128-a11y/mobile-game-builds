using System;
using UnityEngine;
using UnityEngine.UI;
namespace Wildfolk
{
    public sealed class ClassLobbyScreen:MonoBehaviour
    {
        public HomeScreen Home;public GameObject View,Confirmation;public SaveGameRules Rules;
        public Button[] Cards,Nodes;public Text[] CardNames,CardRoles,CardStates,NodeNames,Branches;
        public Text Title,Role,Description,Progress,NodeDetail,PurchaseLabel,SelectLabel,Currency,ConfirmText,Status;
        public Button PurchaseButton,SelectButton,RespecButton,InvestButton;
        private int selected,node;private AccountProgression account;
        private void OnEnable(){ClassProfiles.Changed+=Refresh;L.Changed+=Refresh;}
        private void OnDisable(){ClassProfiles.Changed-=Refresh;L.Changed-=Refresh;}
        public void Open(){Home.Menu.SetActive(false);Home.Settings.SetActive(false);Home.Multiplayer.SetActive(false);Home.Confirm.SetActive(false);View.SetActive(true);Confirmation.SetActive(false);ClassProfiles.Reconcile(Rules);Refresh();}
        public void Back(){Confirmation.SetActive(false);View.SetActive(false);Home.ShowHome();}
        public void SelectCard(int index){selected=index;node=0;Confirmation.SetActive(false);Refresh();}
        public void SelectNode(int index){node=index;Refresh();}
        public void Invest(){var d=ClassCatalog.Current.Classes[selected];if(!ClassProfiles.Spend(d.Id,d.Nodes[node].Id))Status.text=L.Text("class.cannot_spend");Refresh();}
        public void SelectClass(){if(ClassProfiles.Select(ClassCatalog.Current.Classes[selected].Id))Status.text=L.Text("class.selected");Refresh();}
        public void Respec(){ClassProfiles.Respec(ClassCatalog.Current.Classes[selected].Id);Refresh();}
        public void AskPurchase(){var d=ClassCatalog.Current.Classes[selected];if(!ClassProfiles.Lobby)return;ConfirmText.text=L.Format("class.confirm",d.Name,d.Price,d.Currency=="Gold"?L.Text("ui.gold"):L.Text("class.gem"));Confirmation.SetActive(true);}
        public void CancelPurchase()=>Confirmation.SetActive(false);
        public void ConfirmPurchase(){Confirmation.SetActive(false);Status.text=L.Text(ClassProfiles.Purchase(ClassCatalog.Current.Classes[selected].Id,Rules)?"class.purchased":"class.purchase_failed");Refresh();}
        public void Refresh()
        {
            if(View==null || !View.activeSelf)return;
            try{account=ClassProfiles.Local;}catch(Exception){Status.text=L.Text("class.profile_failed");PurchaseButton.interactable=SelectButton.interactable=RespecButton.interactable=InvestButton.interactable=false;return;}
            var catalog=ClassCatalog.Current;var d=catalog.Classes[selected];var p=account.Find(d.Id);var owned=p.Owned;var lobby=ClassProfiles.Lobby;
            for(var i=0;i<Cards.Length;i++){var c=catalog.Classes[i];var progress=account.Find(c.Id);CardNames[i].text=c.Name;CardRoles[i].text=c.Role;CardStates[i].text=L.Text(account.Selected==c.Id?"class.selected":progress.Owned?"class.owned":"class.locked");Cards[i].image.color=selected==i?new Color32(116,129,83,255):new Color32(54,70,55,255);}
            Title.text=d.Name;Role.text=d.Role+" · "+L.Text(d.Active?"class.has_active":"class.passive");Description.text=d.Description;
            Progress.text=L.Format("class.progress",p.XP,catalog.Earned(p.XP)-p.Nodes.Length,catalog.Earned(p.XP));
            var next=Array.Find(catalog.Milestones,x=>x>p.XP);if(next>0)Progress.text+="   ·   "+L.Format("class.next",next);
            for(var i=0;i<Nodes.Length;i++)
            {
                var n=d.Nodes[i];var learned=Array.IndexOf(p.Nodes,n.Id)>=0;var available=owned && (string.IsNullOrEmpty(n.Prerequisite)||Array.IndexOf(p.Nodes,n.Prerequisite)>=0);
                NodeNames[i].text=n.Name+"\n"+L.Text(learned?"class.learned":available?"class.available":"class.locked");Nodes[i].image.color=i==node?new Color32(139,133,85,255):learned?new Color32(77,115,77,255):new Color32(46,60,49,255);
            }
            for(var i=0;i<Branches.Length;i++)Branches[i].text=d.Nodes[i*3].Branch;
            NodeDetail.text=d.Nodes[node].Name+" · "+d.Nodes[node].Description;
            SelectButton.interactable=lobby && owned && account.Selected!=d.Id;SelectLabel.text=L.Text(account.Selected==d.Id?"class.selected":"class.select");
            PurchaseButton.gameObject.SetActive(!owned);PurchaseButton.interactable=lobby && (d.Currency=="Gold"?ClassProfiles.GoldAvailable(Rules)>=d.Price:d.Price>0 && ClassProfiles.Premium.Available);
            PurchaseLabel.text=d.Currency=="Gold"?L.Format("class.price",d.Price,L.Text("ui.gold")):d.Price>0?L.Format("class.price",d.Price,L.Text("class.gem")):L.Text("class.gem_unavailable");
            InvestButton.interactable=lobby && owned && Array.IndexOf(p.Nodes,d.Nodes[node].Id)<0 && catalog.Earned(p.XP)>p.Nodes.Length && (string.IsNullOrEmpty(d.Nodes[node].Prerequisite) || Array.IndexOf(p.Nodes,d.Nodes[node].Prerequisite)>=0);
            RespecButton.interactable=lobby && owned && p.Nodes.Length>0;Currency.text=L.Format("class.gold_available",ClassProfiles.GoldAvailable(Rules));
        }
    }
}
