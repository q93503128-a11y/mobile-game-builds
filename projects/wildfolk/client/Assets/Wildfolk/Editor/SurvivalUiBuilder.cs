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
            Text("Title",panel.transform,"Paused",new Vector2(0,220),new Vector2(380,58),34);
            var resume=Image("Continue",panel.transform,"panel",new Vector2(.5f,.5f),new Vector2(0,130),new Vector2(280,70),true);
            Text("Label",resume.transform,"Continue",Vector2.zero,new Vector2(260,60),27);
            UnityEventTools.AddPersistentListener(Button(resume).onClick,session.Resume);
            var settings=Image("Settings",panel.transform,"panel",new Vector2(.5f,.5f),new Vector2(0,-150),new Vector2(280,70),true);
            Text("Label",settings.transform,"Settings",Vector2.zero,new Vector2(260,60),27);
            UnityEventTools.AddPersistentListener(Button(settings).onClick,menu.OpenSettings);
            var save=Image("Save Game",panel.transform,"panel",new Vector2(.5f,.5f),new Vector2(0,35),new Vector2(280,70),true);Text("Label",save.transform,"Save game",Vector2.zero,new Vector2(260,60),27);UnityEventTools.AddPersistentListener(Button(save).onClick,session.GetComponent<WorldSaveService>().SaveFromMenu);
            var load=Image("Load Game",panel.transform,"panel",new Vector2(.5f,.5f),new Vector2(0,-60),new Vector2(280,70),true);Text("Label",load.transform,"Load game",Vector2.zero,new Vector2(260,60),27);UnityEventTools.AddPersistentListener(Button(load).onClick,session.GetComponent<WorldSaveService>().LoadFromMenu);
            menu.Settings=Settings(scrim.transform,preferences,menu);
            menu.Settings.SetActive(false);scrim.gameObject.SetActive(false);return scrim.gameObject;
        }
        private static GameObject Settings(Transform parent,UserPreferences preferences,PauseMenu menu)
        {
            var panel=Image("Settings Panel",parent,"panel",new Vector2(.5f,.5f),Vector2.zero,new Vector2(790,690),true);
            var settings=panel.gameObject.AddComponent<SettingsPanel>();settings.Preferences=preferences;
            Text("Title",panel.transform,"Settings",new Vector2(-247,283),new Vector2(180,44),34,TextAnchor.MiddleLeft);
            Text("Controls Label",panel.transform,"Controls",new Vector2(-237,222),new Vector2(200,32),23,TextAnchor.MiddleLeft);
            settings.Control=SliderRow(panel.transform,"Control Size",161,.8f,1.4f,out settings.ControlValue);
            Text("Audio Label",panel.transform,"Audio",new Vector2(-237,83),new Vector2(200,32),23,TextAnchor.MiddleLeft);
            settings.Master=SliderRow(panel.transform,"Master Volume",22,0,1,out settings.MasterValue);
            settings.Music=SliderRow(panel.transform,"BGM Volume",-66,0,1,out settings.MusicValue);
            settings.Sfx=SliderRow(panel.transform,"SFX Volume",-154,0,1,out settings.SfxValue);
            UnityEventTools.AddPersistentListener(settings.Control.onValueChanged,preferences.SetControl);
            UnityEventTools.AddPersistentListener(settings.Master.onValueChanged,preferences.SetMaster);
            UnityEventTools.AddPersistentListener(settings.Music.onValueChanged,preferences.SetMusic);
            UnityEventTools.AddPersistentListener(settings.Sfx.onValueChanged,preferences.SetSfx);
            var reset=Image("Reset",panel.transform,"panel",new Vector2(.5f,.5f),new Vector2(-122,-268),new Vector2(272,62),true);
            Text("Label",reset.transform,"Reset to default",Vector2.zero,new Vector2(260,54),24);
            UnityEventTools.AddPersistentListener(Button(reset).onClick,preferences.ResetDefaults);
            var back=Image("Back",panel.transform,"panel",new Vector2(.5f,.5f),new Vector2(190,-268),new Vector2(190,62),true);
            Text("Label",back.transform,"Back",Vector2.zero,new Vector2(178,54),25);
            UnityEventTools.AddPersistentListener(Button(back).onClick,menu.Back);
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
        internal static void Hud(Transform safe,PlayerSurvival player)
        {
            var holder=Image("Survival",safe,"panel",new Vector2(0,1),new Vector2(150,-82),new Vector2(244,112));
            var hud=holder.gameObject.AddComponent<SurvivalHud>();hud.Player=player;
            hud.Hunger=Meter(holder.transform,"Hunger","cooked_meat",24,out hud.HungerValue);
            hud.Thirst=Meter(holder.transform,"Thirst","water",-24,out hud.ThirstValue);
        }
        private static Image Meter(Transform parent,string name,string art,float y,out Text value)
        {
            var icon=Image(name+" Icon",parent,art,new Vector2(.5f,.5f),new Vector2(-89,y),new Vector2(36,38));icon.preserveAspect=true;
            var track=Image(name+" Track",parent,"panel",new Vector2(.5f,.5f),new Vector2(0,y),new Vector2(112,16));
            var fill=Image(name+" Fill",track.transform,"white",new Vector2(.5f,.5f),Vector2.zero,new Vector2(106,8));fill.type=UnityEngine.UI.Image.Type.Filled;fill.fillMethod=UnityEngine.UI.Image.FillMethod.Horizontal;
            value=Text(name+" Value",parent,"100",new Vector2(83,y),new Vector2(50,34),20);return fill;
        }
    }
}
