using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace Wildfolk
{
    public sealed class EconomyScreen:MonoBehaviour
    {
        public HomeScreen Home;public AccountProfileScreen Profile;public GameObject AchievementView,AttendanceView;public Text Wallet,Summary,Detail,Status,AttendanceWallet,AttendanceStatus,ResetLabel,BoardTitle,ProfileSummary,AchievementEntry,AttendanceEntry,HomeEntry;
        public Button Claim,ClaimAll,Attend;public Image[] SlotMarks;public Button[] Categories,Rows,Tabs,Slots;public Text[] CategoryLabels,RowLabels,SlotLabels;public ScrollRect FamilyScroll,DetailScroll;
        private EconomyService service;private int category,selection,tab,eventIndex;private bool fromProfile;private string notice;private float nextClock;
        private void Awake(){service=EconomyService.Get();}
        private void OnEnable(){service=EconomyService.Get();service.Changed+=Refresh;L.Changed+=Refresh;Refresh();}
        private void OnDisable(){if(service!=null)service.Changed-=Refresh;L.Changed-=Refresh;}
        private void SelectService(){if(service==EconomyService.Get())return;service.Changed-=Refresh;service=EconomyService.Get();service.Changed+=Refresh;}
        public async void OpenAchievements(){SelectService();fromProfile=Profile.View.activeSelf;Profile.View.SetActive(false);Home.Menu.SetActive(false);AchievementView.SetActive(true);AttendanceView.SetActive(false);notice=null;Refresh();await service.Sync();Refresh();}
        public async void OpenAttendance(){SelectService();fromProfile=Profile.View.activeSelf;Profile.View.SetActive(false);Home.Menu.SetActive(false);AttendanceView.SetActive(true);AchievementView.SetActive(false);notice=null;Refresh();await service.Sync();Refresh();}
        public void Back(){AchievementView.SetActive(false);AttendanceView.SetActive(false);if(fromProfile)Profile.Open();else Home.ShowHome();}
        public void Category(int i){category=i;selection=0;FamilyScroll.verticalNormalizedPosition=1;DetailScroll.verticalNormalizedPosition=1;notice=null;Refresh();}
        public void Select(int i){selection=i;notice=null;Refresh();DetailScroll.verticalNormalizedPosition=1;}
        public void Board(int i){if(i==2&&tab==2)eventIndex++;tab=i;notice=null;Refresh();}
        public async void ClaimSelected(){var f=Families();if(selection>=f.Length)return;var r=await service.Claim(f[selection].Id);notice=L.Text(r.ok?"economy.claimed":"economy.retry");Refresh();}
        public async void ClaimEverything(){var r=await service.Claim();notice=L.Text(r.ok?"economy.claimed":"economy.retry");Refresh();}
        public async void ClaimAttendance(){var b=CurrentBoard();if(b==null)return;var r=await service.Attend(b.Id);notice=L.Text(r.ok?"economy.claimed":"economy.retry");Refresh();}
        public async void Sync(){await service.Sync();if(ReviveServices.Wallet is IAsyncPremiumCurrencyWallet w)await w.Reconcile();Refresh();}
        private EconomyFamily[] Families()=>EconomyCatalog.Current.Families.Where(f=>f.Category==EconomyCatalog.Current.Categories[category]).ToArray();
        private EconomyBoard CurrentBoard(){var boards=service.Snapshot?.Boards;if(boards==null)return null;if(tab<2)return Array.Find(boards,b=>b.Id==(tab==0?"newcomer":"monthly"));var events=boards.Where(b=>b.Id!="newcomer"&&b.Id!="monthly").ToArray();return events.Length==0?null:events[eventIndex%events.Length];}
        private string Connection=>!service.Online?L.Text("economy.offline"):service.Busy?L.Text("economy.syncing"):service.JournalPending?L.Text("economy.pending_revive"):service.LastError!=null?L.Text("economy.retry"):service.PendingProgress?L.Text("economy.pending"):L.Text("economy.synced");
        private void Refresh()
        {
            if(service==null||Wallet==null)return;var s=service.Snapshot;var currency=L.Format("economy.balance",s?.Balance.ToString()??"—");Wallet.text=AttendanceWallet.text=currency;
            var prestige=s==null?L.Text("economy.no_account"):L.Format("economy.prestige",s.points,s.completed,s.total);Summary.text=prestige;Status.text=notice??Connection;AttendanceStatus.text=notice??Connection;
            if(ProfileSummary!=null){var representatives=s==null?"":string.Join(" · ",EconomyCatalog.Current.Families.Where(f=>s.Value(f.Metric)>=f.Tiers[0].Threshold).Take(3).Select(f=>L.Text(f.NameKey)));ProfileSummary.text=currency+"\n"+prestige+"\n"+representatives;}
            var dot=service.Badge?"  •":"";AchievementEntry.text=L.Text("economy.achievements")+(s?.Claimable==true?"  •":"");AttendanceEntry.text=L.Text("economy.attendance")+(s!=null&&s.Boards.Any(b=>b.Available)?"  •":"");HomeEntry.text=L.Text("account.title")+dot;
            var fs=Families();FamilyScroll.content.sizeDelta=new Vector2(700,fs.Length*132);selection=Mathf.Clamp(selection,0,fs.Length-1);
            for(var i=0;i<Categories.Length;i++){CategoryLabels[i].text=L.Text("economy.category."+EconomyCatalog.Current.Categories[i]);CategoryLabels[i].fontStyle=i==category?FontStyle.Bold:FontStyle.Normal;}
            for(var i=0;i<Rows.Length;i++){var on=i<fs.Length;Rows[i].gameObject.SetActive(on);if(!on)continue;var f=fs[i];var value=s?.Value(f.Metric)??0;var completed=f.Tiers.Count(t=>value>=t.Threshold);RowLabels[i].text=L.Text(f.NameKey)+"\n"+L.Format("economy.tier",completed,f.Tiers.Length)+(s!=null&&f.Tiers.Where((t,j)=>value>=t.Threshold&&!s.Claimed.Contains(f.Id+":"+j)).Any()?"  •":"");RowLabels[i].fontStyle=i==selection?FontStyle.Bold:FontStyle.Normal;}
            if(fs.Length>0){var f=fs[selection];var value=s?.Value(f.Metric)??0;Detail.text=L.Text(f.NameKey)+"\n"+L.Text(f.DescriptionKey)+"\n\n"+string.Join("\n",f.Tiers.Select((t,i)=>L.Format("economy.tier_detail",Math.Min(value,t.Threshold),t.Threshold,t.GemReward,t.AchievementPoints,L.Text(s?.Claimed.Contains(f.Id+":"+i)==true?"economy.received":value>=t.Threshold?"economy.complete":"economy.progress"))));Detail.rectTransform.sizeDelta=new Vector2(660,Mathf.Max(414,Detail.preferredHeight+12));Claim.interactable=service.Online&&!service.Busy&&s!=null&&f.Tiers.Where((t,i)=>value>=t.Threshold&&!s.Claimed.Contains(f.Id+":"+i)).Any();}
            ClaimAll.interactable=service.Online&&!service.Busy&&s?.Claimable==true;
            var b=CurrentBoard();BoardTitle.text=b==null?L.Text(tab==2?"economy.no_event":"economy.no_account"):tab==0?L.Text("economy.newcomer"):tab==1?L.Format("economy.monthly",b.Period):L.Language=="ko"?b.NameKo:b.NameEn;
            for(var i=0;i<Slots.Length;i++){Slots[i].gameObject.SetActive(b!=null&&i<b.Rewards.Length);if(b==null||i>=b.Rewards.Length)continue;var reward=b.Rewards[i]>0?L.Format("economy.gem_reward",b.Rewards[i]):L.Text("economy.attendance_record");SlotLabels[i].text=L.Format("economy.day",i+1)+"\n"+reward;SlotMarks[i].enabled=i<b.Count;Slots[i].interactable=false;Slots[i].image.color=i<b.Count?new Color32(125,139,91,70):i==b.Count?new Color32(165,111,68,65):new Color32(119,108,77,20);}
            Attend.interactable=service.Online&&!service.Busy&&b?.Available==true;RefreshClock();
        }
        private void Update(){if(!AttendanceView.activeSelf||Time.unscaledTime<nextClock)return;nextClock=Time.unscaledTime+1;RefreshClock();}
        private void RefreshClock(){if(ResetLabel!=null)ResetLabel.text=service.Snapshot==null?L.Text("economy.server_time"):L.Format("economy.reset",Math.Floor(service.ResetSeconds/3600),Math.Floor(service.ResetSeconds%3600/60));}
    }
}
