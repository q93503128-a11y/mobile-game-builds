using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using static Wildfolk.Editor.GameplayUiFactory;

namespace Wildfolk.Editor
{
    internal static class InventoryUiBuilder
    {
        internal static void Build(GameObject root,Transform safe,GameplaySession session,PlayerInventory owner,PlayerCrafting crafting,int count)
        {
            var panel=root.AddComponent<InventoryPanel>();panel.Session=session;panel.Owner=owner;
            var drag=root.AddComponent<InventoryDragController>();drag.Owner=owner;drag.Session=session;drag.CanvasRect=(RectTransform)safe;
            var overlay=Scrim("Inventory Overlay",safe);panel.View=overlay.gameObject;
            var surface=Image("Inventory Panel",overlay.transform,"panel",new Vector2(.5f,.5f),Vector2.zero,new Vector2(1520,850),true);
            Text("Title",surface.transform,"Inventory",new Vector2(-562,363),new Vector2(296,52),34,TextAnchor.MiddleLeft);
            var close=Image("Close",surface.transform,"panel",new Vector2(.5f,.5f),new Vector2(680,363),new Vector2(64,64),true);
            Text("Close Symbol",close.transform,"×",Vector2.zero,new Vector2(58,58),40);
            UnityEventTools.AddPersistentListener(Button(close).onClick,session.CloseInventory);
            Text("Storage Label",surface.transform,"Storage",new Vector2(-604,299),new Vector2(180,32),23,TextAnchor.MiddleLeft);
            var storage=Grid("Storage Slots",surface.transform,new Vector2(-248,114),new Vector2(892,292));
            Text("Hotbar Label",surface.transform,"Hotbar",new Vector2(-604,-67),new Vector2(180,32),23,TextAnchor.MiddleLeft);
            var quick=Grid("Hotbar Slots",surface.transform,new Vector2(-248,-154),new Vector2(892,92));
            panel.Slots=new InventorySlotView[count];
            for(var i=0;i<count;i++)
            {
                var slot=Slot(i<PlayerInventory.HotbarSize ? quick : storage,i,92,i<PlayerInventory.HotbarSize);
                var view=slot.GetComponent<InventorySlotView>();panel.Slots[i]=view;
                UnityEventTools.AddIntPersistentListener(Button(slot).onClick,panel.Select,i);
                var handler=slot.gameObject.AddComponent<InventorySlotDrag>();handler.Controller=drag;handler.View=view;handler.Index=i;
            }
            panel.SelectedIcon=Image("Selected Icon",surface.transform,"white",new Vector2(.5f,.5f),new Vector2(-646,-300),new Vector2(58,72));panel.SelectedIcon.preserveAspect=true;
            panel.SelectedName=Text("Selected Name",surface.transform,string.Empty,new Vector2(-409,-284),new Vector2(320,36),28,TextAnchor.MiddleLeft);
            panel.SelectedCategory=Text("Category",surface.transform,string.Empty,new Vector2(-409,-319),new Vector2(320,28),20,TextAnchor.MiddleLeft);
            BuildCrafting(root,surface.transform,crafting);
            overlay.gameObject.SetActive(false);
            var ghost=Rect("Dragged Item",safe,new Vector2(.5f,.5f),Vector2.zero,new Vector2(94,94));
            var group=ghost.gameObject.AddComponent<CanvasGroup>();group.blocksRaycasts=false;group.interactable=false;
            Image("Frame",ghost,"panel",new Vector2(.5f,.5f),Vector2.zero,new Vector2(94,94));
            drag.Ghost=ghost;
            drag.GhostIcon=Image("Icon",ghost,"white",new Vector2(.5f,.5f),Vector2.zero,new Vector2(78,78));drag.GhostIcon.preserveAspect=true;
            drag.GhostCount=Text("Stack Count",ghost,string.Empty,new Vector2(25,-32),new Vector2(50,28),23,TextAnchor.MiddleRight);
            ghost.gameObject.SetActive(false);
        }

        private static RectTransform Grid(string name,Transform parent,Vector2 position,Vector2 size)
        {
            var grid=Rect(name,parent,new Vector2(.5f,.5f),position,size);
            var layout=grid.gameObject.AddComponent<GridLayoutGroup>();
            layout.cellSize=new Vector2(92,92);layout.spacing=new Vector2(8,8);
            layout.constraint=GridLayoutGroup.Constraint.FixedColumnCount;layout.constraintCount=9;layout.childAlignment=TextAnchor.UpperLeft;
            return grid;
        }

        private static void BuildCrafting(GameObject root,Transform surface,PlayerCrafting owner)
        {
            var panel=root.AddComponent<CraftingPanel>();panel.Owner=owner;
            panel.Title=Text("Crafting Title",surface,"Hand crafting",new Vector2(488,312),new Vector2(416,44),28,TextAnchor.MiddleLeft);
            var list=Rect("Recipes",surface,new Vector2(.5f,.5f),new Vector2(488,-24),new Vector2(416,600));
            var layout=list.gameObject.AddComponent<VerticalLayoutGroup>();layout.spacing=18;layout.childControlHeight=false;layout.childControlWidth=false;layout.childForceExpandHeight=false;layout.childForceExpandWidth=false;
            panel.Cards=new RecipeCardView[owner.Catalog.Recipes.Length];
            for(var i=0;i<panel.Cards.Length;i++)
            {
                var card=Image("Recipe",list,"panel",new Vector2(.5f,.5f),Vector2.zero,new Vector2(416,132));
                var view=card.gameObject.AddComponent<RecipeCardView>();view.Recipe=owner.Catalog.Recipes[i];view.Crafting=owner;
                view.Icon=Image("Result",card.transform,"white",new Vector2(.5f,.5f),new Vector2(-148,22),new Vector2(64,64));view.Icon.preserveAspect=true;
                view.Name=Text("Name",card.transform,string.Empty,new Vector2(46,33),new Vector2(254,38),24,TextAnchor.MiddleLeft);
                view.Materials=Text("Materials",card.transform,string.Empty,new Vector2(-12,-6),new Vector2(350,32),21,TextAnchor.MiddleLeft);
                view.Status=Text("Availability",card.transform,string.Empty,new Vector2(-59,-44),new Vector2(240,28),18,TextAnchor.MiddleLeft);
                var craft=Image("Craft",card.transform,"panel",new Vector2(.5f,.5f),new Vector2(121,-38),new Vector2(130,48),true);
                Text("Label",craft.transform,"Craft",Vector2.zero,new Vector2(118,40),23);
                view.CraftButton=Button(craft);UnityEventTools.AddPersistentListener(view.CraftButton.onClick,view.Craft);
                panel.Cards[i]=view;
            }
        }
    }
}
