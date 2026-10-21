using System;
using UnityEngine;
namespace Wildfolk
{
    public interface IWorldSupplies
    {
        bool IsAvailable{get;}Transform Location{get;}ContentCatalog Catalog{get;}SavedStack[] Loot{get;}uint Revision{get;}bool Secured{get;}
        string TitleKey{get;}string ExplainKey{get;}
        event Action Changed;
        bool InRange(PlayerInventory visitor);
        bool RequestTake(PlayerInventory visitor,int slot);
    }
}
