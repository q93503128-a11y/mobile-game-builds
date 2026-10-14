using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using static Wildfolk.Editor.GameplayUiFactory;
namespace Wildfolk.Editor
{
    internal static class CraftingUiBuilder
    {
        internal static void Build(GameObject root,Transform safe,PlayerCrafting owner)
        {
            var panel=root.AddComponent<CraftingPanel>();panel.Owner=owner;var surface=QualityUiBuilder.Surface(safe,"Crafting Screen",new Vector2(1500,770),out panel.View);
            panel.Title=Text("Title",surface.transform,"",new Vector2(-393,320),new Vector2(620,52),32,TextAnchor.MiddleLeft);
            QualityUiBuilder.Action(surface.transform,"Close","ui.action.close",new Vector2(600,320),new Vector2(170,62),owner.CloseCrafting,out var close);
            var tabs=Rect("Categories",surface.transform,new Vector2(.5f,.5f),new Vector2(-219,249),new Vector2(962,62));var tabLayout=tabs.gameObject.AddComponent<HorizontalLayoutGroup>();tabLayout.spacing=8;tabLayout.childControlWidth=false;tabLayout.childControlHeight=false;tabLayout.childForceExpandWidth=false;tabLayout.childForceExpandHeight=false;
            panel.CategoryButtons=new Button[7];for(var i=0;i<7;i++){var button=Image("Category",tabs,"panel",new Vector2(.5f,.5f),Vector2.zero,new Vector2(128,62),true);Text("Label",button.transform,i==0 ? "ui.crafting.all" : "ui.category."+((RecipeCategory)(i-1)),Vector2.zero,new Vector2(118,52),22);panel.CategoryButtons[i]=Button(button);UnityEventTools.AddIntPersistentListener(panel.CategoryButtons[i].onClick,panel.SelectCategory,i-1);}
            var viewport=Image("Recipe Viewport",surface.transform,"white",new Vector2(.5f,.5f),new Vector2(-219,-37),new Vector2(964,477),true);viewport.color=Color.clear;viewport.gameObject.AddComponent<RectMask2D>();
            var list=Rect("Recipes",viewport.transform,new Vector2(.5f,1),Vector2.zero,new Vector2(944,600));list.pivot=new Vector2(.5f,1);var layout=list.gameObject.AddComponent<VerticalLayoutGroup>();layout.spacing=12;layout.childControlHeight=false;layout.childControlWidth=false;layout.childForceExpandHeight=false;layout.childForceExpandWidth=false;list.gameObject.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            panel.Scroll=viewport.gameObject.AddComponent<ScrollRect>();panel.Scroll.content=list;panel.Scroll.viewport=(RectTransform)viewport.transform;panel.Scroll.horizontal=false;panel.Scroll.inertia=false;panel.Scroll.movementType=ScrollRect.MovementType.Clamped;panel.Scroll.scrollSensitivity=65;
            var sorted=RecipePresentation.Sorted(owner.Catalog);panel.Cards=new RecipeCardView[sorted.Length];panel.Sections=new GameObject[6];panel.Grids=new GameObject[6];var last=-1;RectTransform recipeGrid=null;
            for(var i=0;i<sorted.Length;i++)
            {
                var recipe=sorted[i];if((int)recipe.Category!=last){last=(int)recipe.Category;var header=Text("Category Section",list,"ui.category."+recipe.Category,Vector2.zero,new Vector2(924,46),25,TextAnchor.MiddleLeft);header.color=new Color32(196,184,128,255);panel.Sections[last]=header.gameObject;recipeGrid=Rect("Category Recipes",list,new Vector2(.5f,.5f),Vector2.zero,new Vector2(944,160));var grid=recipeGrid.gameObject.AddComponent<GridLayoutGroup>();grid.cellSize=new Vector2(462,164);grid.spacing=new Vector2(14,14);grid.constraint=GridLayoutGroup.Constraint.FixedColumnCount;grid.constraintCount=2;grid.childAlignment=TextAnchor.UpperLeft;recipeGrid.gameObject.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;panel.Grids[last]=recipeGrid.gameObject;}
                var card=Image("Recipe",recipeGrid,"panel",new Vector2(.5f,.5f),Vector2.zero,new Vector2(462,164),true);card.color=new Color32(185,199,171,255);var view=card.gameObject.AddComponent<RecipeCardView>();view.Recipe=recipe;view.Crafting=owner;
                view.Icon=Image("Result",card.transform,"white",new Vector2(.5f,.5f),new Vector2(-190,50),new Vector2(46,50));view.Icon.preserveAspect=true;
                view.Name=Text("Name",card.transform,"",new Vector2(19,50),new Vector2(332,43),23,TextAnchor.MiddleLeft);
                view.Materials=Text("Materials",card.transform,"",new Vector2(0,0),new Vector2(426,62),18,TextAnchor.MiddleLeft);view.Materials.supportRichText=true;
                view.InputIcon=Image("Ingredient",card.transform,"white",new Vector2(.5f,.5f),Vector2.zero,new Vector2(28,28));view.InputIcon.gameObject.SetActive(false);
                view.Status=Text("Availability",card.transform,"",new Vector2(-112,-53),new Vector2(210,30),16,TextAnchor.MiddleRight);
                view.NewLabel=Text("New",card.transform,"ui.crafting.new",new Vector2(177,71),new Vector2(74,24),14);view.NewLabel.color=new Color32(206,192,123,255);
                var craft=Image("Craft",card.transform,"panel",new Vector2(.5f,.5f),new Vector2(141,-49),new Vector2(144,58),true);view.ButtonLabel=Text("Label",craft.transform,"",Vector2.zero,new Vector2(132,50),21);view.CraftButton=Button(craft);UnityEventTools.AddPersistentListener(view.CraftButton.onClick,view.Craft);
                view.Progress=Image("Progress",card.transform,"white",new Vector2(.5f,.5f),new Vector2(0,-78),new Vector2(432,4));view.Progress.type=UnityEngine.UI.Image.Type.Filled;view.Progress.fillMethod=UnityEngine.UI.Image.FillMethod.Horizontal;view.Progress.color=new Color32(188,166,106,255);panel.Cards[i]=view;
            }
            var filter=QualityUiBuilder.Action(surface.transform,"Filter","",new Vector2(-570,-324),new Vector2(272,58),panel.ToggleFilter,out panel.FilterLabel);
            panel.InventoryButton=QualityUiBuilder.Action(surface.transform,"Inventory","ui.inventory.title",new Vector2(-263,-324),new Vector2(270,58),owner.OpenBag,out var bag);
            QualityUiBuilder.Line(surface.transform,new Vector2(288,218),new Vector2(288,-347),2);
            panel.SelectedTitle=Text("Selected Recipe",surface.transform,"",new Vector2(501,192),new Vector2(358,68),26,TextAnchor.MiddleLeft);
            panel.SelectedIcon=Image("Selected Item",surface.transform,"white",new Vector2(.5f,.5f),new Vector2(501,73),new Vector2(122,136));panel.SelectedIcon.preserveAspect=true;panel.MaterialSummary=Text("Materials Summary",surface.transform,"",new Vector2(501,-101),new Vector2(358,174),23,TextAnchor.UpperLeft);
            panel.UpgradeButton=QualityUiBuilder.Action(surface.transform,"Upgrade","",new Vector2(501,-287),new Vector2(358,92),owner.UpgradeFromUI,out panel.UpgradeLabel);
            var queue=Rect("Processing Queue",surface.transform,new Vector2(.5f,.5f),new Vector2(501,-100),new Vector2(358,390));panel.QueueView=queue.gameObject;
            panel.QueueState=Text("Queue State",queue,"",new Vector2(0,108),new Vector2(358,92),23,TextAnchor.UpperLeft);
            panel.CurrentItem=Text("Current Item",queue,"",new Vector2(0,1),new Vector2(358,120),22,TextAnchor.UpperLeft);
            panel.AddOne=QualityUiBuilder.Action(queue,"Add One","",new Vector2(-127,-95),new Vector2(88,64),panel.EnqueueOne,out var add);add.text="+1";
            panel.Withdraw=QualityUiBuilder.Action(queue,"Withdraw","",new Vector2(-28,-95),new Vector2(88,64),panel.WithdrawOne,out var minus);minus.text="−1";
            panel.AddAll=QualityUiBuilder.Action(queue,"Add All","ui.action.add_all",new Vector2(101,-95),new Vector2(150,64),panel.EnqueueAll,out var all);
            panel.Collect=QualityUiBuilder.Action(queue,"Collect","ui.action.take",new Vector2(0,-180),new Vector2(350,66),panel.CollectResults,out var collect);
            panel.View.SetActive(false);
        }
    }
}
