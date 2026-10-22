using System;
using UnityEngine;
namespace Wildfolk
{
    public sealed class ClassPurchaseReceipts:MonoBehaviour
    {public string[] Receipts{get;private set;}=Array.Empty<string>();public void Restore(string[] receipts)=>Receipts=(string[])(receipts??Array.Empty<string>()).Clone();}
}
