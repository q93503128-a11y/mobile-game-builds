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
            var meter=Rect("Player Stamina",safe,new Vector2(.5f,.5f),Vector2.zero,new Vector2(140,140));var gauge=root.AddComponent<PlayerStaminaHud>();gauge.Player=player.GetComponent<PlayerLocomotion>();gauge.Session=session;gauge.Anchor=meter;gauge.CanvasRect=(RectTransform)safe;gauge.Group=meter.gameObject.AddComponent<CanvasGroup>();gauge.Group.alpha=0;gauge.Group.blocksRaycasts=false;gauge.Arc=meter.gameObject.AddComponent<StaminaArc>();gauge.Arc.raycastTarget=false;gauge.Arc.color=new Color32(196,175,105,255);
            var screen=root.AddComponent<CharacterScreen>();screen.Player=player;screen.Session=session;
            var surface=QualityUiBuilder.Surface(safe,"Character Information",new Vector2(1120,740),out screen.View);
            Text("Title",surface.transform,"ui.character.title",new Vector2(-223,310),new Vector2(590,50),32,TextAnchor.MiddleLeft);
            QualityUiBuilder.Action(surface.transform,"Close","ui.action.close",new Vector2(450,310),new Vector2(152,62),screen.Close,out var closeLabel);
            QualityUiBuilder.Portrait(surface.transform,player,new Vector2(-330,115),1.15f);
            screen.Level=Text("Level",surface.transform,"",new Vector2(-330,-78),new Vector2(330,42),28);
            screen.Points=Text("Points",surface.transform,"",new Vector2(-330,-150),new Vector2(330,65),23);
            screen.TreeButton=QualityUiBuilder.Action(surface.transform,"Skill Tree","ui.skill.title",new Vector2(-330,-249),new Vector2(320,70),screen.OpenTree,out var treeLabel);
            QualityUiBuilder.Line(surface.transform,new Vector2(-105,230),new Vector2(-105,-286),2);
            screen.Information=Text("Information",surface.transform,"",new Vector2(210,8),new Vector2(540,540),26,TextAnchor.UpperLeft);
            Text("Condition",surface.transform,"ui.character.condition",new Vector2(210,-294),new Vector2(540,32),18,TextAnchor.MiddleLeft);
            screen.View.SetActive(false);BuildTree(root,safe,session,player.GetComponent<PlayerProgression>());
        }
        private static void BuildTree(GameObject root,Transform safe,GameplaySession session,PlayerProgression growth)
        {
            var tree=root.AddComponent<SkillTreeScreen>();tree.Session=session;tree.Growth=growth;
            var surface=QualityUiBuilder.Surface(safe,"Skill Tree",new Vector2(1380,800),out tree.View);
            Text("Title",surface.transform,"ui.skill.title",new Vector2(-424,340),new Vector2(440,50),34,TextAnchor.MiddleLeft);
            tree.Points=Text("Points",surface.transform,"",new Vector2(20,340),new Vector2(400,50),24,TextAnchor.MiddleRight);
            QualityUiBuilder.Action(surface.transform,"Back","ui.action.back",new Vector2(505,340),new Vector2(160,62),tree.Back,out var back);
            var field=Rect("Tree",surface.transform,new Vector2(.5f,.5f),new Vector2(-225,-5),new Vector2(850,580));
            var origin=new Vector2(-350,10);Image("Origin",field,"profile",new Vector2(.5f,.5f),origin,new Vector2(66,66));Text("Origin Caption",field,"ui.skill.origin",origin+new Vector2(0,-63),new Vector2(150,30),19);
            tree.Nodes=new Button[growth.Definition.Skills.Length];tree.Ranks=new Text[tree.Nodes.Length];tree.Names=new Text[tree.Nodes.Length];tree.NodeStates=new Text[tree.Nodes.Length];tree.Links=new Image[tree.Nodes.Length];
            for(var i=0;i<tree.Nodes.Length;i++)
            {
                var skill=growth.Definition.Skills[i];var from=origin;for(var j=0;j<tree.Nodes.Length;j++)if(growth.Definition.Skills[j].Id==skill.Prerequisite)from=growth.Definition.Skills[j].NodePosition;
                var direction=(skill.NodePosition-from).normalized;tree.Links[i]=QualityUiBuilder.Line(field,from+direction*(from==origin ? 39 : 62),skill.NodePosition-direction*62,4);
            }
            for(var i=0;i<tree.Nodes.Length;i++)
            {
                var skill=growth.Definition.Skills[i];var node=Image("Skill Node",field,"panel",new Vector2(.5f,.5f),skill.NodePosition,new Vector2(112,112),true);tree.Nodes[i]=Button(node);UnityEventTools.AddIntPersistentListener(tree.Nodes[i].onClick,tree.Select,i);
                var icon=Image("Icon",node.transform,"white",new Vector2(.5f,.5f),new Vector2(0,7),new Vector2(58,58));icon.sprite=skill.Icon;icon.preserveAspect=true;
                tree.Ranks[i]=Text("Rank",node.transform,"",new Vector2(0,-36),new Vector2(108,25),19);tree.Names[i]=Text("Name",field,"",skill.NodePosition+new Vector2(0,-88),new Vector2(242,35),22);tree.NodeStates[i]=Text("State",field,"",skill.NodePosition+new Vector2(0,-119),new Vector2(240,28),16);tree.NodeStates[i].color=new Color32(167,178,141,255);
            }
            QualityUiBuilder.Line(surface.transform,new Vector2(246,267),new Vector2(246,-340),2);
            tree.SelectedName=Text("Selected Node",surface.transform,"",new Vector2(456,220),new Vector2(358,52),30,TextAnchor.MiddleLeft);
            tree.State=Text("Node State",surface.transform,"",new Vector2(456,158),new Vector2(358,40),22,TextAnchor.MiddleLeft);tree.State.color=new Color32(197,185,124,255);
            tree.Detail=Text("Node Detail",surface.transform,"",new Vector2(456,-13),new Vector2(358,260),25,TextAnchor.UpperLeft);
            tree.InvestButton=QualityUiBuilder.Action(surface.transform,"Invest","",new Vector2(456,-280),new Vector2(350,76),tree.Invest,out tree.Action);tree.View.SetActive(false);
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
