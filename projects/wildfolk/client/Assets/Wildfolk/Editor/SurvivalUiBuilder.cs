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
            ConfigurePauseActions(panel.transform,session.GetComponent<WorldSaveService>());
            menu.Settings=Settings(scrim.transform,preferences,menu.Back);
            menu.Settings.SetActive(false);scrim.gameObject.SetActive(false);return scrim.gameObject;
        }
        internal static void ConfigurePauseActions(Transform panel,WorldSaveService save)
        {
            var load=panel.Find("Load Game");if(load!=null)Object.DestroyImmediate(load.gameObject);
            ((RectTransform)panel).sizeDelta=new Vector2(840,840);
            ((RectTransform)panel.Find("Title")).anchoredPosition=new Vector2(0,330);
            foreach(var pair in new[]{("Continue",220f),("Save Game",65f),("Settings",-90f)}){var rect=(RectTransform)panel.Find(pair.Item1);rect.anchoredPosition=new Vector2(0,pair.Item2);rect.sizeDelta=new Vector2(640,126);var label=rect.GetComponentInChildren<Text>();label.rectTransform.sizeDelta=new Vector2(610,110);label.fontSize=36;}
            var existing=panel.Find("Save and Home");
            if(existing!=null){((RectTransform)existing).anchoredPosition=new Vector2(0,-260);((RectTransform)existing).sizeDelta=new Vector2(640,126);var label=existing.GetComponentInChildren<Text>();label.rectTransform.sizeDelta=new Vector2(610,110);label.fontSize=34;return;}
            var home=Image("Save and Home",panel,"panel",new Vector2(.5f,.5f),new Vector2(0,-260),new Vector2(640,126),true);
            Text("Label",home.transform,"home.save_exit",Vector2.zero,new Vector2(610,110),34);
            UnityEventTools.AddPersistentListener(Button(home).onClick,save.SaveAndHome);
        }
        internal static GameObject Settings(Transform parent,UserPreferences preferences,UnityEngine.Events.UnityAction backAction)
        {
            var page=JournalUiBuilder.Sheet(parent,"Settings Panel",out var view);var settings=view.AddComponent<SettingsPanel>();settings.Preferences=preferences;
            JournalUiBuilder.Label(page,"Title","ui.settings.title",new Vector2(-450,405),new Vector2(620,80),44);
            JournalUiBuilder.Action(page,"Back","ui.action.back",new Vector2(690,405),new Vector2(180,120),backAction,out _);
            JournalUiBuilder.Label(page,"Controls Label","setting.controls",new Vector2(-420,280),new Vector2(650,65),36);
            settings.Control=SliderRow(page,"setting.control_size",new Vector2(-420,130),.8f,1.4f,out settings.ControlValue);
            JournalUiBuilder.Label(page,"Audio Label","setting.audio",new Vector2(420,280),new Vector2(650,65),36);
            settings.Master=SliderRow(page,"setting.master",new Vector2(420,150),0,1,out settings.MasterValue);
            settings.Music=SliderRow(page,"setting.music",new Vector2(420,-40),0,1,out settings.MusicValue);
            settings.Sfx=SliderRow(page,"setting.sfx",new Vector2(420,-230),0,1,out settings.SfxValue);
            UnityEventTools.AddPersistentListener(settings.Control.onValueChanged,preferences.SetControl);UnityEventTools.AddPersistentListener(settings.Master.onValueChanged,preferences.SetMaster);UnityEventTools.AddPersistentListener(settings.Music.onValueChanged,preferences.SetMusic);UnityEventTools.AddPersistentListener(settings.Sfx.onValueChanged,preferences.SetSfx);
            JournalUiBuilder.Label(page,"Language","setting.language",new Vector2(-420,-60),new Vector2(650,60),34);
            JournalUiBuilder.Action(page,"Language Selector","",new Vector2(-420,-170),new Vector2(650,120),settings.NextLanguage,out settings.LanguageLabel);
            JournalUiBuilder.Action(page,"Reset","setting.reset",new Vector2(-420,-360),new Vector2(650,120),preferences.ResetDefaults,out _);
            return view;
        }
        private static Slider SliderRow(Transform parent,string label,Vector2 position,float min,float max,out Text value)
        {
            var row=Rect(label,parent,Vector2.one*.5f,position,new Vector2(650,160));
            JournalUiBuilder.Label(row,"Name",label,new Vector2(-65,55),new Vector2(520,55),32);value=JournalUiBuilder.Label(row,"Value","",new Vector2(270,55),new Vector2(110,55),32,TextAnchor.MiddleRight);
            var surface=Image("Slider",row,"white",Vector2.one*.5f,new Vector2(0,-30),new Vector2(650,120),true);surface.color=Color.clear;
            var track=Image("Track",surface.transform,"white",Vector2.one*.5f,Vector2.zero,new Vector2(620,14));track.color=JournalStyle.Muted;
            var area=Rect("Fill Area",surface.transform,Vector2.one*.5f,Vector2.zero,new Vector2(580,10));var fill=Image("Fill",area,"white",Vector2.one*.5f,Vector2.zero,Vector2.zero);fill.color=JournalStyle.Ember;
            var handles=Rect("Handle Area",surface.transform,Vector2.one*.5f,Vector2.zero,new Vector2(580,80));var handle=Image("Handle",handles,"journal_node",Vector2.one*.5f,Vector2.zero,new Vector2(65,0));handle.color=JournalStyle.Learned;
            var slider=surface.gameObject.AddComponent<Slider>();slider.minValue=min;slider.maxValue=max;slider.fillRect=fill.rectTransform;slider.handleRect=handle.rectTransform;slider.targetGraphic=handle;slider.navigation=new Navigation{mode=Navigation.Mode.None};return slider;
        }
    }
}
