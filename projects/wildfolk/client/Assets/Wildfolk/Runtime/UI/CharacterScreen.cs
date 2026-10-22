using UnityEngine;
using UnityEngine.UI;
namespace Wildfolk
{
    public sealed class CharacterScreen : MonoBehaviour
    {
        public PlayerInventory Player;public GameplaySession Session;public GameObject View;
        public Text Level,Information,Points,ClassName,Experience,Health,Stamina,Defense,Recovery,Consumption,Needs;public Button TreeButton;
        private float nextRefresh;
        private void Start(){Session.StateChanged+=Refresh;L.Changed+=Refresh;Refresh();}
        private void OnDestroy(){Session.StateChanged-=Refresh;L.Changed-=Refresh;}
        private void Update(){if(Session.State==SessionState.Character && Time.unscaledTime>=nextRefresh){nextRefresh=Time.unscaledTime+.25f;Refresh();}}
        public void Close()=>Session.CloseCharacter();
        public void OpenTree()=>Session.OpenSkillTree();
        public void Refresh()
        {
            View.SetActive(Session.State==SessionState.Character);if(!View.activeSelf)return;
            var growth=Player.GetComponent<PlayerProgression>();var health=Player.GetComponent<Health>();var move=Player.GetComponent<PlayerLocomotion>();var stats=Player.GetComponent<CharacterStats>();var needs=Player.GetComponent<PlayerSurvival>();
            Level.text=L.Format("ui.character.level",growth.Level);Points.text=L.Format("ui.skill.points",growth.SkillPoints);
            ClassName.text=Player.GetComponent<PlayerClass>()?.Definition?.Name??L.Text("ui.skill.origin");
            Experience.text=L.Format("ui.character.xp",growth.CurrentXP,growth.RequiredXP==0?"—":growth.RequiredXP.ToString());
            Health.text=L.Format("ui.character.health",health.Current,health.Maximum);Stamina.text=L.Format("ui.character.stamina",Mathf.CeilToInt(move.Stamina.Current),Mathf.RoundToInt(move.Stamina.Maximum));
            Defense.text=L.Format("ui.character.defense",Player.GetComponent<PlayerEquipment>().Defense);Recovery.text=L.Format("ui.character.recovery",move.Definition.Recovery*stats.RecoveryMultiplier);
            Consumption.text=L.Format("ui.character.drain",move.Definition.SprintDrain*stats.SprintCostMultiplier);Needs.text=L.Format("ui.character.needs",needs.Model.Hunger,needs.Model.Thirst);

        }
    }
}
