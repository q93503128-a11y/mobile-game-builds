using UnityEngine;
namespace Wildfolk
{
 // Replicated semantic receipts contain no clip or sound-asset IDs.
 public enum ConfirmedPlayerAction { None,Food,Drink,Treatment }
 public static class PlayerActionAudio
 {
  public static void Committed(Component actor,ConfirmedPlayerAction action)
  {Present(action,actor.transform.position);actor.GetComponent<NetworkCommonGrowth>()?.ConfirmAction(action);}
  public static void Present(ConfirmedPlayerAction action,Vector2 position)
  {if(action==ConfirmedPlayerAction.None)return;AudioRuntime.Play(action==ConfirmedPlayerAction.Drink?Sound.Drink:action==ConfirmedPlayerAction.Treatment?Sound.Treat:Sound.Eat,position);}
 }
}
