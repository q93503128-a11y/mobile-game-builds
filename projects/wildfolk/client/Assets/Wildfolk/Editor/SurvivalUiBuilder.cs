using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using static Wildfolk.Editor.GameplayUiFactory;
namespace Wildfolk.Editor
{
    internal static class SurvivalUiBuilder
    {
        internal static GameObject Pause(Transform safe,GameplaySession session)
        {
            var preferences=session.GetComponent<UserPreferences>();
            var scrim=Scrim("Pause Overlay",safe);
            var menu=scrim.gameObject.AddComponent<PauseMenu>();menu.Preferences=preferences;
            var panel=Image("Pause Panel",scrim.transform,"panel",new Vector2(.5f,.5f),Vector2.zero,new Vector2(440,580),true);
            menu.Menu=panel.gameObject;
            Text("Title",panel.transform,"ui.pause.title",new Vector2(0,220),new Vector2(380,58),34);
            var resume=Image("Continue",panel.transform,"panel",new Vector2(.5f,.5f),new Vector2(0,130),new Vector2(280,70),true);
            Text("Label",resume.transform,"ui.action.continue",Vector2.zero,new Vector2(260,60),27);
            UnityEventTools.AddPersistentListener(Button(resume).onClick,session.Resume);
            var settings=Image("Settings",panel.transform,"panel",new Vector2(.5f,.5f),new Vector2(0,-150),new Vector2(280,70),true);
            Text("Label",settings.transform,"ui.settings.title",Vector2.zero,new Vector2(260,60),27);
            UnityEventTools.AddPersistentListener(Button(settings).onClick,menu.OpenSettings);
            var save=Image("Save Game",panel.transform,"panel",new Vector2(.5f,.5f),new Vector2(0,35),new Vector2(280,70),true);Text("Label",save.transform,"ui.pause.save",Vector2.zero,new Vector2(260,60),27);UnityEventTools.AddPersistentListener(Button(save).onClick,session.GetComponent<WorldSaveService>().SaveFromMenu);
            var load=Image("Load Game",panel.transform,"panel",new Vector2(.5f,.5f),new Vector2(0,-60),new Vector2(280,70),true);Text("Label",load.transform,"ui.pause.load",Vector2.zero,new Vector2(260,60),27);UnityEventTools.AddPersistentListener(Button(load).onClick,session.GetComponent<WorldSaveService>().LoadFromMenu);
            AddHomeAction(panel.transform,session.GetComponent<WorldSaveService>());
            menu.Settings=Settings(scrim.transform,preferences,menu.Back);
            menu.Settings.SetActive(false);scrim.gameObject.SetActive(false);return scrim.gameObject;
        }
        internal static void AddHomeAction(Transform panel,WorldSaveService save)
        {
            if(panel.Find("Save and Home")!=null)return;
            var home=Image("Save and Home",panel,"panel",new Vector2(.5f,.5f),new Vector2(0,-245),new Vector2(280,70),true);
            Text("Label",home.transform,"home.save_exit",Vector2.zero,new Vector2(270,60),24);
            UnityEventTools.AddPersistentListener(Button(home).onClick,save.SaveAndHome);
        }
        internal static GameObject Settings(Transform parent,UserPreferences preferences,UnityEngine.Events.UnityAction backAction)
        {
            var panel=Image("Settings Panel",parent,"panel",new Vector2(.5f,.5f),Vector2.zero,new Vector2(790,790),true);
            var settings=panel.gameObject.AddComponent<SettingsPanel>();settings.Preferences=preferences;
            Text("Title",panel.transform,"ui.settings.title",new Vector2(-247,338),new Vector2(180,44),34,TextAnchor.MiddleLeft);
            Text("Controls Label",panel.transform,"setting.controls",new Vector2(-237,222),new Vector2(200,32),23,TextAnchor.MiddleLeft);
            settings.Control=SliderRow(panel.transform,"setting.control_size",161,.8f,1.4f,out settings.ControlValue);
            Text("Audio Label",panel.transform,"setting.audio",new Vector2(-237,83),new Vector2(200,32),23,TextAnchor.MiddleLeft);
            settings.Master=SliderRow(panel.transform,"setting.master",22,0,1,out settings.MasterValue);
            settings.Music=SliderRow(panel.transform,"setting.music",-66,0,1,out settings.MusicValue);
            settings.Sfx=SliderRow(panel.transform,"setting.sfx",-154,0,1,out settings.SfxValue);
            UnityEventTools.AddPersistentListener(settings.Control.onValueChanged,preferences.SetControl);
            UnityEventTools.AddPersistentListener(settings.Master.onValueChanged,preferences.SetMaster);
            UnityEventTools.AddPersistentListener(settings.Music.onValueChanged,preferences.SetMusic);
            UnityEventTools.AddPersistentListener(settings.Sfx.onValueChanged,preferences.SetSfx);
            Text("Language",panel.transform,"setting.language",new Vector2(-210,-227),new Vector2(248,42),24,TextAnchor.MiddleLeft);
            var languageImage=Image("Language Selector",panel.transform,"panel",new Vector2(.5f,.5f),new Vector2(162,-227),new Vector2(308,58),true);
            settings.LanguageLabel=Text("Language Name",languageImage.transform,"",Vector2.zero,new Vector2(290,52),24);UnityEventTools.AddPersistentListener(Button(languageImage).onClick,settings.NextLanguage);
            var reset=Image("Reset",panel.transform,"panel",new Vector2(.5f,.5f),new Vector2(-122,-328),new Vector2(272,62),true);
            Text("Label",reset.transform,"setting.reset",Vector2.zero,new Vector2(260,54),24);
            UnityEventTools.AddPersistentListener(Button(reset).onClick,preferences.ResetDefaults);
            var back=Image("Back",panel.transform,"panel",new Vector2(.5f,.5f),new Vector2(190,-328),new Vector2(190,62),true);
            Text("Label",back.transform,"ui.action.back",Vector2.zero,new Vector2(178,54),25);
            UnityEventTools.AddPersistentListener(Button(back).onClick,backAction);
            return panel.gameObject;
        }
        private static Slider SliderRow(Transform parent,string label,float y,float min,float max,out Text value)
        {
            var row=Rect(label,parent,new Vector2(.5f,.5f),new Vector2(0,y),new Vector2(670,72));
            Text("Name",row,label,new Vector2(-159,23),new Vector2(350,32),24,TextAnchor.MiddleLeft);
            value=Text("Value",row,"",new Vector2(291,23),new Vector2(88,32),24,TextAnchor.MiddleRight);
            var slider=Image("Slider",row,"white",new Vector2(.5f,.5f),new Vector2(0,-13),new Vector2(670,42),true);slider.color=new Color(0,0,0,0);
            var track=Image("Track",slider.transform,"panel",new Vector2(.5f,.5f),Vector2.zero,new Vector2(654,12));
            var area=Rect("Fill Area",slider.transform,new Vector2(.5f,.5f),Vector2.zero,new Vector2(636,8));
            var fill=Image("Fill",area,"white",new Vector2(.5f,.5f),Vector2.zero,Vector2.zero);fill.color=new Color32(179,176,125,255);
            var handles=Rect("Handle Area",slider.transform,new Vector2(.5f,.5f),Vector2.zero,new Vector2(636,42));
            var handle=Image("Handle",handles,"panel",new Vector2(.5f,.5f),Vector2.zero,new Vector2(30,0));
            var component=slider.gameObject.AddComponent<Slider>();component.minValue=min;component.maxValue=max;
            component.fillRect=(RectTransform)fill.transform;component.handleRect=(RectTransform)handle.transform;component.targetGraphic=handle;
            component.navigation=new Navigation{mode=Navigation.Mode.None};return component;
        }
    }
}
