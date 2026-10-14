using UnityEngine;
namespace Wildfolk
{
    public sealed class LocalSettings
    {
        public const string Prefix="wildfolk.user.";
        public float ControlSize { get; private set; }=1;
        public float Master { get; private set; }=1;
        public float Music { get; private set; }=.65f;
        public float Sfx { get; private set; }=.85f;
        public string Language {get;private set;}="en";
        public void SetLanguage(string value){Language=System.Array.Exists(L.Catalog.Languages,x=>x.Id==value) ? value : "en";}
        public void SetControl(float value) => ControlSize=Mathf.Clamp(value,.8f,1.4f);
        public void SetMaster(float value) => Master=Mathf.Clamp01(value);
        public void SetMusic(float value) => Music=Mathf.Clamp01(value);
        public void SetSfx(float value) => Sfx=Mathf.Clamp01(value);
        public static LocalSettings Load(string prefix=Prefix)
        {
            var settings=new LocalSettings();
            settings.SetControl(PlayerPrefs.GetFloat(prefix+"controls",settings.ControlSize));
            settings.SetMaster(PlayerPrefs.GetFloat(prefix+"master",settings.Master));
            settings.SetMusic(PlayerPrefs.GetFloat(prefix+"music",settings.Music));
            settings.SetSfx(PlayerPrefs.GetFloat(prefix+"sfx",settings.Sfx));settings.SetLanguage(PlayerPrefs.GetString(prefix+"language","en"));return settings;
        }
        public void Store(string prefix=Prefix)
        {
            PlayerPrefs.SetFloat(prefix+"controls",ControlSize);PlayerPrefs.SetFloat(prefix+"master",Master);
            PlayerPrefs.SetFloat(prefix+"music",Music);PlayerPrefs.SetFloat(prefix+"sfx",Sfx);
            PlayerPrefs.SetString(prefix+"language",Language);
        }
    }
}
