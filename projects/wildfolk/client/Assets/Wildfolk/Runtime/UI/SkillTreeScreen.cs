using UnityEngine;
using UnityEngine.UI;
namespace Wildfolk
{
    public sealed class SkillTreeScreen : MonoBehaviour
    {
        public GameplaySession Session;
        public PlayerProgression Growth;
        public GameObject View;
        public Button[] Nodes;
        public Text[] Ranks,Names,NodeStates;
        public Image[] Links;public JournalSkillNode[] NodeViews;
        public Text Points,SelectedName,Detail,State,Action;
        public Button InvestButton;
        private int selected;
        private void Start(){Session.StateChanged+=Refresh;Growth.Changed+=Refresh;L.Changed+=Refresh;Refresh();}
        private void OnDestroy(){Session.StateChanged-=Refresh;Growth.Changed-=Refresh;L.Changed-=Refresh;}
        public void Select(int index){selected=index;Refresh();}
        public void Invest(){Growth.Invest(selected);Refresh();}
        public void Back()=>Session.BackToCharacter();
        public void Close()=>Session.CloseSkillTree();
        public void Refresh()
        {
            View.SetActive(Session.State==SessionState.SkillTree);if(!View.activeSelf)return;
            Points.text=L.Format("ui.skill.points",Growth.SkillPoints);
            for(var i=0;i<Nodes.Length;i++)
            {var rank=Growth.Rank(i);Ranks[i].text=rank+" / "+Growth.Definition.Skills[i].MaximumRank;Names[i].text=L.Text("ui.skill."+Growth.Definition.Skills[i].Id+".name");NodeStates[i].text=L.Text(!Growth.Unlocked(i) ? "ui.skill.locked" : rank>0 ? "ui.skill.learned" : "ui.skill.available");NodeViews[i].SetState(rank>0,Growth.CanInvest(i),i==selected);Names[i].color=Growth.Unlocked(i)?JournalStyle.Ink:JournalStyle.Muted;Links[i].color=rank>0?JournalStyle.Ember:new Color32(136,124,91,180);}
            var skill=Growth.Definition.Skills[selected];var current=Growth.Rank(selected);SelectedName.text=L.Text("ui.skill."+skill.Id+".name");
            Detail.text=L.Format("ui.skill.rank",current,skill.MaximumRank)+"\n\n"+L.Format("ui.skill."+skill.Effect+".effect",skill.PerRank*(skill.Percentage ? 100 : 1));
            if(!string.IsNullOrEmpty(skill.Prerequisite))Detail.text+="\n\n"+L.Format("ui.skill.requires",L.Text("ui.skill."+skill.Prerequisite+".name"),skill.PrerequisiteRank);
            State.text=L.Text(!Growth.Unlocked(selected) ? "ui.skill.locked" : current>=skill.MaximumRank ? "ui.skill.complete" : current>0 ? "ui.skill.learned" : "ui.skill.available");
            Action.text=current>=skill.MaximumRank ? L.Text("ui.skill.complete") : !Growth.Unlocked(selected) ? L.Text("ui.skill.locked") : Growth.SkillPoints<skill.PointCost ? L.Text("ui.skill.insufficient") : L.Format("ui.skill.learn",skill.PointCost);InvestButton.interactable=Growth.CanInvest(selected);
        }
    }
}
