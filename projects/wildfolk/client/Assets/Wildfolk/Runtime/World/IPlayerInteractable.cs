using UnityEngine;
namespace Wildfolk
{
    public interface IPlayerInteractable
    {
        Transform Location { get; }
        Sprite Icon { get; }
        PlayerActionKind ActionKind { get; }
        bool CanInteract(PlayerAction actor);
        bool Use(PlayerAction actor);
    }
}
