using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using static Wildfolk.Editor.GameplayUiFactory;
namespace Wildfolk.Editor
{
    internal static class CharacterUiBuilder
    {
        internal const float ProfileBottomReserve=270;
        internal static void Build(GameObject root,Transform safe,GameplaySession session,PlayerInventory player)
        {
            var hud=root.AddComponent<ProfileHud>();hud.Player=player;
            var profile=Image("Player Profile",safe,"profile",new Vector2(0,1),new Vector2(85,-84),new Vector2(114,114),true);
            UnityEventTools.AddPersistentListener(Button(profile).onClick,session.OpenCharacter);
            hud.Level=Text("Level",profile.transform,"Lv. 1",new Vector2(0,-75),new Vector2(116,32),22);
            hud.SkillNotice=Text("Skill Point Notice",profile.transform,"",new Vector2(52,45),new Vector2(62,32),22);hud.SkillNotice.color=new Color32(216,193,118,255);
            var health=Bar(safe,"Health",new Vector2(288,-71),new Vector2(260,23),new Color32(156,173,103,255));hud.HP=health;
            hud.HealthValue=Text("HP Value",health.transform,"100 / 100",new Vector2(0,26),new Vector2(260,30),22,TextAnchor.MiddleLeft);
            hud.XP=Bar(safe,"Experience",new Vector2(288,-101),new Vector2(260,5),new Color32(193,174,115,255));
            var needsHolder=Rect("Survival Indicators",safe,new Vector2(0,1),new Vector2(303,-146),new Vector2(350,40));var needs=needsHolder.gameObject.AddComponent<SurvivalHud>();needs.Player=player.GetComponent<PlayerSurvival>();
            needs.Hunger=Need(needsHolder,"Hunger","cooked_meat",new Vector2(-87,0),new Color32(181,154,96,255),out needs.HungerValue);
            needs.Thirst=Need(needsHolder,"Thirst","water",new Vector2(87,0),new Color32(110,162,166,255),out needs.ThirstValue);
            var stamina=Rect("Stamina",safe,new Vector2(0,1),new Vector2(286,-186),new Vector2(260,20));hud.StaminaGroup=stamina.gameObject.AddComponent<CanvasGroup>();hud.StaminaGroup.alpha=0;hud.StaminaGroup.blocksRaycasts=false;
            hud.Stamina=Bar(stamina,"Fill",Vector2.zero,new Vector2(260,8),new Color32(167,163,111,255),new Vector2(.5f,.5f));
            var screen=root.AddComponent<CharacterScreen>();screen.Player=player;screen.Session=session;
            var scrim=Scrim("Character Overlay",safe);screen.View=scrim.gameObject;
            var surface=Image("Character Screen",scrim.transform,"panel",new Vector2(.5f,.5f),Vector2.zero,new Vector2(1240,740),true);
            var fit=surface.gameObject.AddComponent<ModalLayout>();fit.SafeArea=(RectTransform)safe;fit.ReferenceSize=new Vector2(1240,740);fit.TopReserve=130;fit.BottomReserve=40;
            screen.Title=Text("Title",surface.transform,"Survivor",new Vector2(-275,308),new Vector2(540,50),32,TextAnchor.MiddleLeft);
            var close=Image("Close",surface.transform,"panel",new Vector2(.5f,.5f),new Vector2(555,308),new Vector2(70,62),true);Text("Label",close.transform,"×",Vector2.zero,new Vector2(66,56),38);UnityEventTools.AddPersistentListener(Button(close).onClick,screen.Close);
            var portrait=Rect("Survivor Portrait",surface.transform,new Vector2(.5f,.5f),new Vector2(-419,45),new Vector2(260,300));
            var ring=Image("Backdrop",portrait,"joystick_base",new Vector2(.5f,.5f),Vector2.zero,new Vector2(274,274));ring.color=new Color32(150,158,127,255);
            Image("Skin",portrait,"player_body",new Vector2(.5f,.5f),Vector2.zero,new Vector2(132,132));
            screen.PortraitBody=Image("Body Armor",portrait,"white",new Vector2(.5f,.5f),Vector2.zero,new Vector2(182,182));
            Image("Left Hand",portrait,"player_hand",new Vector2(.5f,.5f),new Vector2(-85,-38),new Vector2(36,36));Image("Right Hand",portrait,"player_hand",new Vector2(.5f,.5f),new Vector2(85,-38),new Vector2(36,36));
            screen.PortraitLeftHand=Image("Left Gear",portrait,"white",new Vector2(.5f,.5f),new Vector2(-85,-38),new Vector2(34,38));screen.PortraitRightHand=Image("Right Gear",portrait,"white",new Vector2(.5f,.5f),new Vector2(85,-38),new Vector2(34,38));
            screen.PortraitHead=Image("Head Gear",portrait,"white",new Vector2(.5f,.5f),Vector2.zero,new Vector2(182,182));
            Image("Left Eye",portrait,"player_eye",new Vector2(.5f,.5f),new Vector2(-18,-28),new Vector2(10,13));Image("Right Eye",portrait,"player_eye",new Vector2(.5f,.5f),new Vector2(18,-28),new Vector2(10,13));
            Text("Equipment Hint",surface.transform,"Field equipment",new Vector2(-419,-141),new Vector2(274,34),24);
            Text("Profile Caption",surface.transform,"Build your survivor",new Vector2(-419,-188),new Vector2(274,32),21);
            var tabs=new[]{"Information","Equipment","Skills"};screen.Tabs=new GameObject[3];screen.TabButtons=new Button[3];
            for(var i=0;i<3;i++)
            {var index=i;var button=Image(tabs[i],surface.transform,"panel",new Vector2(.5f,.5f),new Vector2(-50+i*231,233),new Vector2(215,54),true);Text("Label",button.transform,tabs[i],Vector2.zero,new Vector2(201,48),23);screen.TabButtons[i]=Button(button);UnityEventTools.AddIntPersistentListener(screen.TabButtons[i].onClick,screen.SelectTab,index);screen.Tabs[i]=Rect(tabs[i]+" Content",surface.transform,new Vector2(.5f,.5f),new Vector2(182,-48),new Vector2(735,478)).gameObject;}
            screen.Information=Text("Information",screen.Tabs[0].transform,"",new Vector2(0,0),new Vector2(708,470),24,TextAnchor.UpperLeft);
            screen.Weapon=Text("Current Tool",screen.Tabs[1].transform,"",new Vector2(0,201),new Vector2(710,44),24,TextAnchor.MiddleLeft);
            var ids=new[]{"LeatherHead","LeatherBody","IronHead","IronBody"};screen.ArmorItems=new ItemDefinition[4];screen.ArmorNames=new Text[4];screen.ArmorActions=new Text[4];screen.ArmorButtons=new Button[4];
            for(var i=0;i<4;i++)
            {
                var item=UnityEditor.AssetDatabase.LoadAssetAtPath<ItemDefinition>("Assets/Wildfolk/Data/"+ids[i]+".asset");screen.ArmorItems[i]=item;
                var card=Image("Armor",screen.Tabs[1].transform,"panel",new Vector2(.5f,.5f),new Vector2(i%2==0 ? -185 : 185,i<2 ? 85 : -113),new Vector2(350,174));
                var icon=Image("Icon",card.transform,"white",new Vector2(.5f,.5f),new Vector2(-122,29),new Vector2(72,72));icon.sprite=item.Icon;icon.preserveAspect=true;
                screen.ArmorNames[i]=Text("Name",card.transform,"",new Vector2(40,28),new Vector2(222,74),22,TextAnchor.MiddleLeft);
                var equip=Image("Equip",card.transform,"panel",new Vector2(.5f,.5f),new Vector2(35,-52),new Vector2(238,50),true);screen.ArmorActions[i]=Text("Label",equip.transform,"Equip",Vector2.zero,new Vector2(226,44),22);screen.ArmorButtons[i]=Button(equip);UnityEventTools.AddIntPersistentListener(screen.ArmorButtons[i].onClick,screen.Equip,i);
            }
            screen.Points=Text("Skill Points",screen.Tabs[2].transform,"",new Vector2(0,210),new Vector2(710,42),24,TextAnchor.MiddleLeft);
            var growth=CharacterPassAssets.Growth();screen.SkillNames=new Text[4];screen.SkillDetails=new Text[4];screen.SkillActions=new Text[4];screen.SkillButtons=new Button[4];
            for(var i=0;i<4;i++)
            {
                var card=Image("Skill",screen.Tabs[2].transform,"panel",new Vector2(.5f,.5f),new Vector2(i%2==0 ? -185 : 185,i<2 ? 81 : -124),new Vector2(350,188));
                var icon=Image("Icon",card.transform,"white",new Vector2(.5f,.5f),new Vector2(-135,50),new Vector2(48,48));icon.sprite=growth.Skills[i].Icon;
                screen.SkillNames[i]=Text("Name",card.transform,"",new Vector2(28,51),new Vector2(259,52),23,TextAnchor.MiddleLeft);
                screen.SkillDetails[i]=Text("Effect",card.transform,"",new Vector2(0,-1),new Vector2(316,50),20,TextAnchor.MiddleLeft);
                var learn=Image("Learn",card.transform,"panel",new Vector2(.5f,.5f),new Vector2(0,-62),new Vector2(310,50),true);screen.SkillActions[i]=Text("Label",learn.transform,"",Vector2.zero,new Vector2(294,44),22);screen.SkillButtons[i]=Button(learn);UnityEventTools.AddIntPersistentListener(screen.SkillButtons[i].onClick,screen.Invest,i);
            }
            scrim.gameObject.SetActive(false);
        }
        private static Image Need(Transform safe,string name,string art,Vector2 p,Color color,out Text value)
        {
            var holder=Rect(name,safe,new Vector2(.5f,.5f),p,new Vector2(160,40));var icon=Image("Icon",holder,art,new Vector2(.5f,.5f),new Vector2(-67,0),new Vector2(29,32));icon.preserveAspect=true;
            var fill=Bar(holder,"Meter",new Vector2(-5,0),new Vector2(75,7),color,new Vector2(.5f,.5f));value=Text("Value",holder,"",new Vector2(53,0),new Vector2(50,28),20);return fill;
        }
        private static Image Bar(Transform safe,string name,Vector2 p,Vector2 size,Color color,Vector2? anchor=null)
        {
            var track=Image(name,safe,"white",anchor ?? new Vector2(0,1),p,size);track.color=new Color32(32,47,36,230);
            var fill=Image("Fill",track.transform,"white",new Vector2(.5f,.5f),Vector2.zero,size-Vector2.one*2);fill.color=color;fill.type=UnityEngine.UI.Image.Type.Filled;fill.fillMethod=UnityEngine.UI.Image.FillMethod.Horizontal;return fill;
        }
    }
}
