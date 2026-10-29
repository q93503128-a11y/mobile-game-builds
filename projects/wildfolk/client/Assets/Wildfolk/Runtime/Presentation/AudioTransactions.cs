using System;
using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
 // Presentation receipts only: bounded and session-local, never an economic authority or save field.
 public static class AudioTransactions
 {
  private static readonly HashSet<string> seen=new HashSet<string>(StringComparer.Ordinal);private static readonly Queue<string> order=new Queue<string>();
  public static bool Confirmed(string receipt,Sound cue)
  {if(string.IsNullOrEmpty(receipt)||!seen.Add(receipt))return false;order.Enqueue(receipt);if(order.Count>256)seen.Remove(order.Dequeue());AudioRuntime.Play(cue);return true;}
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]private static void Reset(){seen.Clear();order.Clear();}
 }
}
