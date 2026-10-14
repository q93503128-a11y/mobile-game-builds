using UnityEngine;
using UnityEngine.UI;
namespace Wildfolk
{
    public sealed class SettingsPanel : MonoBehaviour
    {
        public UserPreferences Preferences;
        public Slider Control,Master,Music,Sfx;
        public Text ControlValue,MasterValue,MusicValue,SfxValue,LanguageLabel;
        public void NextLanguage(){var index=System.Array.FindIndex(L.Catalog.Languages,x=>x.Id==Preferences.Values.Language);Preferences.SetLanguage((index+1)%L.Catalog.Languages.Length);}
        private void OnEnable() { Preferences.Changed+=Refresh;Refresh(); }
        private void OnDisable() => Preferences.Changed-=Refresh;
        private void Refresh()
        {
            var values=Preferences.Values;LanguageLabel.text=System.Array.Find(L.Catalog.Languages,x=>x.Id==values.Language).Name;
            Control.SetValueWithoutNotify(values.ControlSize);Master.SetValueWithoutNotify(values.Master);
            Music.SetValueWithoutNotify(values.Music);Sfx.SetValueWithoutNotify(values.Sfx);
            ControlValue.text=Mathf.RoundToInt(values.ControlSize*100)+"%";
            MasterValue.text=Mathf.RoundToInt(values.Master*100)+"%";
            MusicValue.text=Mathf.RoundToInt(values.Music*100)+"%";
            SfxValue.text=Mathf.RoundToInt(values.Sfx*100)+"%";
        }
    }
}
