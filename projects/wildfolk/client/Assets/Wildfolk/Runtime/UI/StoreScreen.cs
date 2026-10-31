using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace Wildfolk
{
    public sealed class StoreScreen:MonoBehaviour
    {
        public EconomyScreen Meta;public GameObject View,HistoryView,SelectionView;public Button Entry,Purchase,Retry,HelpButton,SupportButton;public Text Wallet,Status,Detail,History,HelpLabel,FixtureNote;public Button[] Cards;public Text[] Amounts,Prices;public ScrollRect HistoryScroll;
        private StoreService service;private int selected=-1;private bool history,fromHome;
        public Button HomeEntry;public Text HomeBalance;
        public Text[] Uses;public Image[] CardMarks;
        private void Awake()=>Bind();private void OnEnable(){Bind();L.Changed+=Refresh;EconomyService.Get().Changed+=Refresh;Refresh();}
        private void OnDisable(){L.Changed-=Refresh;if(EconomyService.Current!=null)EconomyService.Current.Changed-=Refresh;if(service!=null)service.Changed-=Refresh;service=null;}
        private void Bind(){var current=StoreService.Get();if(service==current)return;if(service!=null)service.Changed-=Refresh;service=current;service.Changed+=Refresh;}
        public async void Open(){Bind();selected=-1;history=false;fromHome=Meta.Home.Menu.activeSelf;Meta.HubView.SetActive(false);Meta.Home.Menu.SetActive(false);View.SetActive(true);Refresh();await service.Open();Refresh();}
        public void Close(){View.SetActive(false);if(fromHome)Meta.Home.ShowHome();else Meta.OpenHub();}
        public void Select(int index){if(service.Busy||index<0||index>=service.Catalog.Products.Length)return;selected=index;Refresh();}
        public async void Buy(){if(selected<0||!service.CanBuy)return;var product=service.Catalog.Products[selected];await service.Purchase(product.ProductId);Refresh();}
        public async void Recover(){await service.Recover();Refresh();}
        public void ToggleHistory(){history=!history;Refresh();}
        public void Contact(){View.SetActive(false);Meta.Profile.Open();Meta.Profile.Help();}
        private void Refresh()
        {
            if(service==null||Wallet==null)return;var balance=EconomyService.Get().Snapshot?.Balance;Wallet.text=L.Format("economy.balance",balance?.ToString()??"—");FixtureNote.gameObject.SetActive(service.IsDevelopment);FixtureNote.text=L.Text("store.fixture");
            for(var i=0;i<Cards.Length;i++){var item=service.Catalog.Products[i];Amounts[i].text=L.Format("store.amount",item.GemAmount);var price=Array.Find(service.Prices,p=>p.ProductId==item.ProductId);Prices[i].text=price?.FormattedPrice??L.Text(service.Phase==StorePhase.FetchingProducts?"store.loading":"store.price_unavailable");Cards[i].gameObject.SetActive(!history);Cards[i].interactable=!service.Busy&&item.Enabled;Amounts[i].fontStyle=i==selected?FontStyle.Bold:FontStyle.Normal;}
            HistoryView.SetActive(history);HelpLabel.text=L.Text(history?"store.products":"store.history");SelectionView.SetActive(!history);Purchase.interactable=!history&&selected>=0&&service.CanBuy;Retry.interactable=!service.Busy&&service.AccountReady;SupportButton.interactable=!service.Busy;
            if(selected>=0){var item=service.Catalog.Products[selected];var price=Array.Find(service.Prices,p=>p.ProductId==item.ProductId);Detail.text=L.Format("store.detail",item.GemAmount,price?.FormattedPrice??L.Text("store.price_unavailable"));}else Detail.text=L.Text("store.choose");
            var phase=service.Phase;var key=phase==StorePhase.FetchingProducts?"store.loading":phase==StorePhase.PurchaseStarting?"store.starting":phase==StorePhase.Pending?"store.pending":phase==StorePhase.Validating?"store.verifying":phase==StorePhase.Consuming?"store.consuming":phase==StorePhase.Recovering?"store.recovering":phase==StorePhase.Completed?"store.completed":phase==StorePhase.Cancelled?"store.cancelled":phase==StorePhase.AccountRequired?"store.account":phase==StorePhase.Failed?"store.failed":phase==StorePhase.Unavailable?(BackendPolicy.Channel==BuildChannel.Alpha?"store.alpha":"store.unavailable"):"store.optional";Status.text=L.Text(key);
            if(service.State?.ReviewRequired==true)Status.text=L.Text("store.review");var recent=service.State?.Recent??Array.Empty<PurchaseView>();History.text=recent.Length==0?L.Text("store.no_history"):string.Join("\n\n",recent.Select(p=>L.Format("store.history_row",p.GemAmount,LocalizedState(p.State),SafeDate(p.GrantedAt),p.Reference??"—")));
            if(!service.PurchasesConfigured&&!service.IsDevelopment)Status.text=L.Text(BackendPolicy.Channel==BuildChannel.Alpha?"store.alpha":"store.unavailable");
            HomeBalance.text=L.Text("store.title")+" · "+(balance?.ToString()??"—");
            for(var i=0;i<Cards.Length;i++)Cards[i].image.color=i==selected?JournalStyle.Learned:Color.white;
            var configured=service.PurchasesConfigured||service.IsDevelopment;Purchase.gameObject.SetActive(configured);Detail.rectTransform.sizeDelta=new Vector2(configured?900:1420,110);Detail.rectTransform.anchoredPosition=new Vector2(configured?-270:0,-155);
            for(var i=0;i<Cards.Length;i++){CardMarks[i].enabled=i==selected;if(!configured)Prices[i].text=L.Text("store.unavailable_short");var amount=service.Catalog.Products[i].GemAmount;var cost=EconomyCatalog.Current.ReviveCost;Uses[i].text=cost>0?L.Format("store.revive_uses",amount/cost):"";}
            if(!configured&&selected>=0)Detail.text=Amounts[selected].text+" · "+Uses[selected].text;
            Retry.GetComponentInChildren<Text>(true).color=Retry.interactable?JournalStyle.Ink:JournalStyle.Muted;
        }
        private static string SafeDate(string value)=>DateTimeOffset.TryParse(value,out var date)?date.UtcDateTime.ToString("yyyy-MM-dd HH:mm 'UTC'"):"—";
        private static string LocalizedState(string state)=>L.Text(state=="Completed"?"store.completed":state=="Reversed"?"store.reversed":"store.verifying");
    }
}
