using UnityEngine;
using UnityEngine.UI;
namespace Wildfolk
{
    public sealed class CharacterScreen : MonoBehaviour
    {
        public PlayerInventory Player;
        public GameplaySession Session;
        public GameObject View;
        public GameObject[] Tabs;
        public Button[] TabButtons;
        public Text Title,Information,Points,Weapon;
        public ItemDefinition[] ArmorItems;
        public Text[] ArmorNames,ArmorActions,SkillNames,SkillDetails,SkillActions;
        public Button[] ArmorButtons,SkillButtons;
        public Image PortraitBody,PortraitHead,PortraitLeftHand,PortraitRightHand;
        private PlayerEquipment equipment;private PlayerProgression growth;private Health health;private PlayerLocomotion locomotion;
        private int tab;private float nextRefresh;
        private void Start()
        {equipment=Player.GetComponent<PlayerEquipment>();growth=Player.GetComponent<PlayerProgression>();health=Player.GetComponent<Health>();locomotion=Player.GetComponent<PlayerLocomotion>();Session.StateChanged+=Refresh;Player.Model.Changed+=Refresh;equipment.Changed+=Refresh;growth.Changed+=Refresh;Refresh();}
        private void OnDestroy(){if(Session!=null)Session.StateChanged-=Refresh;if(Player!=null && Player.Model!=null)Player.Model.Changed-=Refresh;if(equipment!=null)equipment.Changed-=Refresh;if(growth!=null)growth.Changed-=Refresh;}
        private void Update(){if(Session.State==SessionState.Character && Time.unscaledTime>=nextRefresh){nextRefresh=Time.unscaledTime+.25f;Refresh();}}
        public void SelectTab(int index){tab=index;Refresh();}
        public void Equip(int index)
        {var item=ArmorItems[index];var current=item.Armor.Slot==ArmorSlot.Head ? equipment.HeadArmor : equipment.BodyArmor;if(current==item)equipment.Unequip(item.Armor.Slot);else if(!equipment.TryEquip(item))PlayerFeedback.Show("Carry this armor in your bag");Refresh();}
        public void Invest(int index){growth.Invest(index);Refresh();}
        public void Close()=>Session.CloseCharacter();
        public void Refresh()
        {
            if(growth==null)return;View.SetActive(Session.State==SessionState.Character);if(!View.activeSelf)return;
            Title.text="Survivor · Level "+growth.Level;
            for(var i=0;i<Tabs.Length;i++){Tabs[i].SetActive(i==tab);TabButtons[i].image.color=i==tab ? new Color32(124,130,85,255) : new Color32(69,83,64,255);}
            var needs=Player.GetComponent<PlayerSurvival>();var stats=Player.GetComponent<CharacterStats>();
            Information.text="PROGRESS\n"+"Level "+growth.Level+"   ·   XP "+growth.CurrentXP+" / "+(growth.RequiredXP==0 ? "—" : growth.RequiredXP.ToString())+"\n\nVITALS\n"+"Health   "+health.Current+" / "+health.Maximum+"\nDefense   "+equipment.Defense+"\nStamina   "+Mathf.CeilToInt(locomotion.Stamina.Current)+" / "+locomotion.Stamina.Maximum.ToString("0")+"\n\nSURVIVAL\n"+"Hunger   "+needs.Model.Hunger.ToString("0")+"   ·   Thirst   "+needs.Model.Thirst.ToString("0")+"\nStamina recovery   "+(locomotion.Definition.Recovery*stats.RecoveryMultiplier).ToString("0.#")+" / s at full condition\nSprint drain   "+(locomotion.Definition.SprintDrain*stats.SprintCostMultiplier).ToString("0.#")+" / s at full condition";
            Weapon.text=equipment.ActiveTool==null ? "Hands · No weapon equipped" : equipment.SelectedItem.DisplayName+" · Damage "+equipment.ActiveTool.Damage;
            for(var i=0;i<ArmorItems.Length;i++)
            {var item=ArmorItems[i];var current=item.Armor.Slot==ArmorSlot.Head ? equipment.HeadArmor : equipment.BodyArmor;var active=current==item;ArmorNames[i].text=item.DisplayName+"\n"+item.Armor.Slot+" · Defense +"+item.Armor.Defense;ArmorActions[i].text=active ? "Remove" : "Equip";ArmorButtons[i].interactable=active || Player.Model.CountItem(item)>0;}
            Points.text=growth.SkillPoints+" skill point"+(growth.SkillPoints==1 ? "" : "s")+" available";
            for(var i=0;i<SkillButtons.Length;i++)
            {var skill=growth.Definition.Skills[i];var rank=growth.Rank(i);SkillNames[i].text=skill.Name+"   "+rank+" / "+skill.MaximumRank;SkillDetails[i].text=skill.Description;SkillActions[i].text=rank>=skill.MaximumRank ? "Complete" : "Learn · 1 point";SkillButtons[i].interactable=growth.SkillPoints>0 && rank<skill.MaximumRank;}
            PortraitBody.sprite=equipment.BodyArmor==null ? null : equipment.BodyArmor.Armor.WornSprite;PortraitBody.enabled=PortraitBody.sprite!=null;
            PortraitHead.sprite=equipment.HeadArmor==null ? null : equipment.HeadArmor.Armor.WornSprite;PortraitHead.enabled=PortraitHead.sprite!=null;
            PortraitLeftHand.sprite=PortraitRightHand.sprite=equipment.BodyArmor==null ? null : equipment.BodyArmor.Armor.HandSprite;PortraitLeftHand.enabled=PortraitRightHand.enabled=PortraitLeftHand.sprite!=null;
        }
    }
}
