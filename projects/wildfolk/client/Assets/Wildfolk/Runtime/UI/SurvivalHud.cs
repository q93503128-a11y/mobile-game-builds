using UnityEngine;
using UnityEngine.UI;
namespace Wildfolk
{
    public sealed class SurvivalHud : MonoBehaviour
    {
        public PlayerSurvival Player;
        public Image Hunger,Thirst;
        public Text HungerValue,ThirstValue;
        private int previousHunger=-1,previousThirst=-1;
        private void Update()
        {
            var model=Player.Model;
            Hunger.fillAmount=model.Hunger/model.Definition.MaximumHunger;
            Thirst.fillAmount=model.Thirst/model.Definition.MaximumThirst;
            var hunger=Mathf.CeilToInt(model.Hunger);var thirst=Mathf.CeilToInt(model.Thirst);
            if(hunger!=previousHunger) {HungerValue.text=hunger.ToString();previousHunger=hunger;}
            if(thirst!=previousThirst) {ThirstValue.text=thirst.ToString();previousThirst=thirst;}
            Hunger.color=Hunger.fillAmount<.2f ? new Color32(184,103,73,255) : new Color32(181,154,96,255);
            Thirst.color=Thirst.fillAmount<.2f ? new Color32(184,103,73,255) : new Color32(110,162,166,255);
        }
    }
}
