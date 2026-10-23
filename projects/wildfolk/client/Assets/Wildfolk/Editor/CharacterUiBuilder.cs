using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using static Wildfolk.Editor.GameplayUiFactory;
namespace Wildfolk.Editor
{
    internal static class CharacterUiBuilder
    {
        internal static void Build(GameObject root,Transform safe,GameplaySession session,PlayerInventory player)
        {
            var hud=root.AddComponent<ProfileHud>();hud.Player=player;
            var profile=Image("Player Profile",safe,"profile",new Vector2(0,1),new Vector2(85,-84),new Vector2(114,114),true);
            UnityEventTools.AddPersistentListener(Button(profile).onClick,session.OpenCharacter);
            hud.Level=Text("Level",profile.transform,"Lv. 1",new Vector2(0,-75),new Vector2(180,32),22);
            hud.SkillNotice=Text("Skill Point Notice",profile.transform,"",new Vector2(52,45),new Vector2(62,32),22);hud.SkillNotice.color=new Color32(216,193,118,255);
            var health=Bar(safe,"Health",new Vector2(288,-71),new Vector2(260,23),new Color32(156,173,103,255));hud.HP=health;
            hud.HealthValue=Text("HP Value",health.transform,"100 / 100",new Vector2(0,26),new Vector2(260,30),22,TextAnchor.MiddleLeft);
            hud.XP=Bar(safe,"Experience",new Vector2(288,-101),new Vector2(260,5),new Color32(193,174,115,255));
            var needsHolder=Rect("Survival Indicators",safe,new Vector2(0,1),new Vector2(303,-146),new Vector2(350,40));var needs=needsHolder.gameObject.AddComponent<SurvivalHud>();needs.Player=player.GetComponent<PlayerSurvival>();
            needs.Hunger=Need(needsHolder,"Hunger","cooked_meat",new Vector2(-87,0),new Color32(181,154,96,255),out needs.HungerValue);
            needs.Thirst=Need(needsHolder,"Thirst","water",new Vector2(87,0),new Color32(110,162,166,255),out needs.ThirstValue);
            var meter=Rect("Player Stamina",safe,new Vector2(.5f,.5f),Vector2.zero,new Vector2(140,140));var gauge=root.AddComponent<PlayerStaminaHud>();gauge.Player=player.GetComponent<PlayerLocomotion>();gauge.Session=session;gauge.Anchor=meter;gauge.CanvasRect=(RectTransform)safe;gauge.Group=meter.gameObject.AddComponent<CanvasGroup>();gauge.Group.alpha=0;gauge.Group.blocksRaycasts=false;gauge.Arc=meter.gameObject.AddComponent<StaminaArc>();gauge.Arc.raycastTarget=false;gauge.Arc.color=new Color32(196,175,105,255);
            BuildInformation(root,safe,session,player);BuildTree(root,safe,session,player.GetComponent<PlayerProgression>());
        }
        internal static void BuildInformation(GameObject root,Transform safe,GameplaySession session,PlayerInventory player)
        {
            var screen=root.AddComponent<CharacterScreen>();screen.Player=player;screen.Session=session;
            var page=JournalUiBuilder.Book(safe,"Character Journal",out screen.View);
            JournalUiBuilder.Label(page,"Title","ui.character.title",new Vector2(-420,391),new Vector2(650,55),34);
            JournalUiBuilder.Label(page,"Vitals Heading","ui.character.vitals",new Vector2(370,391),new Vector2(570,55),34);
            JournalUiBuilder.Action(page,"Close","ui.action.close",new Vector2(695,394),new Vector2(160,72),screen.Close,out _);
            JournalUiBuilder.Rule(page,new Vector2(-760,348),new Vector2(-100,348));JournalUiBuilder.Rule(page,new Vector2(92,348),new Vector2(760,348));
            screen.ClassName=JournalUiBuilder.Label(page,"Class","",new Vector2(-420,282),new Vector2(580,55),35,TextAnchor.MiddleCenter);
            QualityUiBuilder.Portrait(page,player,new Vector2(-420,90),1.05f);
            screen.Level=JournalUiBuilder.Label(page,"Level","",new Vector2(-420,-102),new Vector2(580,60),38,TextAnchor.MiddleCenter);
            screen.Experience=JournalUiBuilder.Label(page,"Experience","",new Vector2(-420,-170),new Vector2(620,52),26,TextAnchor.MiddleCenter);
            screen.Points=JournalUiBuilder.Label(page,"Points","",new Vector2(-420,-242),new Vector2(620,64),25,TextAnchor.MiddleCenter);
            screen.TreeButton=JournalUiBuilder.Action(page,"Skill Tree","ui.skill.title",new Vector2(-420,-370),new Vector2(440,82),screen.OpenTree,out _,true);
            Image("Health Icon",page,"skill_vitality",Vector2.one*.5f,new Vector2(155,253),new Vector2(64,64));
            screen.Health=JournalUiBuilder.Label(page,"Health","",new Vector2(448,251),new Vector2(496,60),32);
            Image("Stamina Icon",page,"skill_endurance",Vector2.one*.5f,new Vector2(155,139),new Vector2(64,64));
            screen.Stamina=JournalUiBuilder.Label(page,"Stamina","",new Vector2(448,139),new Vector2(496,60),32);
            screen.Defense=JournalUiBuilder.Label(page,"Defense","",new Vector2(425,40),new Vector2(588,55),27);
            JournalUiBuilder.Rule(page,new Vector2(116,-20),new Vector2(736,-20));
            JournalUiBuilder.Label(page,"Field Condition","journal.condition",new Vector2(425,-72),new Vector2(588,45),24);
            screen.Recovery=JournalUiBuilder.Label(page,"Recovery","",new Vector2(425,-143),new Vector2(588,50),27);
            screen.Consumption=JournalUiBuilder.Label(page,"Consumption","",new Vector2(425,-211),new Vector2(588,50),27);
            screen.Needs=JournalUiBuilder.Label(page,"Needs","",new Vector2(425,-279),new Vector2(588,50),27);
            screen.Information=JournalUiBuilder.Label(page,"Condition","ui.character.condition",new Vector2(425,-382),new Vector2(588,72),22);
            screen.View.SetActive(false);
        }
        private static void BuildTree(GameObject root,Transform safe,GameplaySession session,PlayerProgression growth)
        {
            var tree=root.AddComponent<SkillTreeScreen>();tree.Session=session;tree.Growth=growth;
            var page=JournalUiBuilder.Book(safe,"Shared Growth Journal",out tree.View);
            JournalUiBuilder.Label(page,"Title","ui.skill.origin",new Vector2(-420,391),new Vector2(650,55),34);
            tree.Points=JournalUiBuilder.Label(page,"Points","",new Vector2(350,391),new Vector2(500,70),26);
            JournalUiBuilder.Action(page,"Back","ui.action.back",new Vector2(695,394),new Vector2(160,72),tree.Back,out _);
            JournalUiBuilder.Rule(page,new Vector2(-760,348),new Vector2(-100,348));JournalUiBuilder.Rule(page,new Vector2(92,348),new Vector2(760,348));
            var positions=new[]{new Vector2(-655,130),new Vector2(-420,130),new Vector2(-310,-93),new Vector2(-546,-93),new Vector2(-690,-250),new Vector2(-185,130)};
            var origin=new Vector2(-420,282);Image("Origin",page,"profile",Vector2.one*.5f,origin,new Vector2(62,62));
            var count=growth.Definition.Skills.Length;tree.Nodes=new Button[count];tree.Ranks=new Text[count];tree.Names=new Text[count];tree.NodeStates=new Text[count];tree.Links=new Image[count];tree.NodeViews=new JournalSkillNode[count];
            for(var i=0;i<count;i++)
            {
                var skill=growth.Definition.Skills[i];var from=origin;for(var j=0;j<count;j++)if(growth.Definition.Skills[j].Id==skill.Prerequisite)from=positions[j];
                var p=positions[i];var dir=(p-from).normalized;if(i==4){JournalUiBuilder.Rule(page,origin+Vector2.left*42,new Vector2(-780,282),3);JournalUiBuilder.Rule(page,new Vector2(-780,282),new Vector2(-780,p.y),3);tree.Links[i]=JournalUiBuilder.Rule(page,new Vector2(-780,p.y),p+Vector2.left*49,3);}else tree.Links[i]=JournalUiBuilder.Rule(page,from+dir*40,p-dir*49,3);
            }
            for(var i=0;i<count;i++)
            {
                var p=positions[i];tree.NodeViews[i]=JournalUiBuilder.Node(page,"Shared Skill "+i,p,growth.Definition.Skills[i].Icon,tree.Select,i,out tree.Nodes[i]);
                tree.Ranks[i]=JournalUiBuilder.Label(page,"Rank "+i,"",p+new Vector2(0,-61),new Vector2(i==4?200:210,34),22,TextAnchor.MiddleCenter);
                tree.Names[i]=JournalUiBuilder.Label(page,"Name "+i,"",p+new Vector2(0,-103),new Vector2(i==4?200:225,42),24,TextAnchor.MiddleCenter);
                tree.NodeStates[i]=JournalUiBuilder.Label(page,"State "+i,"",p+new Vector2(0,-143),new Vector2(i==4?200:210,40),20,TextAnchor.MiddleCenter);
            }
            JournalUiBuilder.Label(page,"Field Notes","journal.field_notes",new Vector2(418,285),new Vector2(590,48),24);
            tree.SelectedName=JournalUiBuilder.Label(page,"Selected Node","",new Vector2(418,205),new Vector2(590,80),38);
            tree.State=JournalUiBuilder.Label(page,"Node State","",new Vector2(418,127),new Vector2(590,55),26);
            JournalUiBuilder.Rule(page,new Vector2(116,75),new Vector2(736,75));
            tree.Detail=JournalUiBuilder.Label(page,"Node Detail","",new Vector2(418,-90),new Vector2(590,260),29,TextAnchor.UpperLeft);
            tree.InvestButton=JournalUiBuilder.Action(page,"Invest","",new Vector2(418,-350),new Vector2(590,82),tree.Invest,out tree.Action,true);tree.View.SetActive(false);
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
