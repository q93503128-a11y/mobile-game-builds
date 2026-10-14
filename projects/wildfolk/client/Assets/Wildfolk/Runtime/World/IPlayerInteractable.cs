using UnityEngine;
namespace Wildfolk
{
    public interface IPlayerInteractable
    {
        Transform Location { get; }
        Sprite Icon { get; }
        bool CanInteract(PlayerInteraction actor);
        bool Use(PlayerInteraction actor);
    }
}
