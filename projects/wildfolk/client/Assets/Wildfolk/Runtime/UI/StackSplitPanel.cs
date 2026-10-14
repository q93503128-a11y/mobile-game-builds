using UnityEngine;
using UnityEngine.UI;
namespace Wildfolk
{
    public sealed class StackSplitPanel : MonoBehaviour
    {
        public InventoryDragController Drag;
        public GameObject View;
        public GameObject Actions;
        public Text ActionTitle;
        public Image ActionIcon;
        public Button ActionSplitButton;
        public Text Title,Quantity;
        public Image Icon;
        public Button Minus,Plus,HalfButton,ConfirmButton,CancelButton;
        public int Amount {get;private set;}
        private int total,index;private bool chest;private InventorySlotView source;
        public void Open(int slot,InventorySlotView view,bool fromChest,int count,ItemDefinition item)
        {if(count<2)return;index=slot;source=view;chest=fromChest;total=count;Amount=count/2;Title.text=item.LocalizedName;Icon.sprite=item.Icon;View.SetActive(true);Refresh();}
        public void OpenActions(int slot,InventorySlotView view,bool fromChest,int count,ItemDefinition item)
        {if(count<2)return;Open(slot,view,fromChest,count,item);View.SetActive(false);ActionTitle.text=item.LocalizedName;ActionIcon.sprite=item.Icon;Actions.SetActive(true);}
        public void ChooseSplit(){Actions.SetActive(false);View.SetActive(true);Refresh();}
        public void Decrease(){Amount=Mathf.Max(1,Amount-1);Refresh();}
        public void Increase(){Amount=Mathf.Min(total-1,Amount+1);Refresh();}
        public void Half(){Amount=total/2;Refresh();}
        private void Refresh(){Quantity.text=Amount+" / "+total;Minus.interactable=Amount>1;Plus.interactable=Amount<total-1;}
        public void Confirm(){View.SetActive(false);Drag.HoldPartial(index,source,chest,Amount);}
        public void Cancel(){View.SetActive(false);Actions.SetActive(false);}
    }
}
