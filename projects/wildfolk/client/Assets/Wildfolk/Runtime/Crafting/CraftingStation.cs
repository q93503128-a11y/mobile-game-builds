using UnityEngine;

namespace Wildfolk
{
    public sealed class CraftingStation : MonoBehaviour, IPlayerInteractable
    {
        public static readonly System.Collections.Generic.List<CraftingStation> Active=new System.Collections.Generic.List<CraftingStation>();
        public static event System.Action ProgressionChanged;
        private void OnEnable(){Active.Add(this);Health.Died+=NotifyChanged;if(Definition.Station==StationKind.Workbench)ProgressionChanged?.Invoke();}
        private void OnDisable(){Active.Remove(this);Health.Died-=NotifyChanged;if(Definition.Station==StationKind.Workbench)ProgressionChanged?.Invoke();}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]private static void ResetRegistry(){Active.Clear();ProgressionChanged=null;}
        public event System.Action Changed;
        public void NotifyChanged(){Changed?.Invoke();if(Definition.Station==StationKind.Workbench)ProgressionChanged?.Invoke();}
        public PlaceableDefinition Definition;
        public Health Health;
        public Transform Location => transform;
        public Sprite Icon => Definition.Item.Icon;
        public bool CanInteract(PlayerInteraction actor) => CanInteract(actor.transform.position,actor.Settings.StationInteractionRange);
        public bool Use(PlayerInteraction actor) => actor.Crafting.OpenStation(this);
        public bool CanInteract(Vector2 actorPosition,float range) =>
            Health.IsAlive && ((Vector2)transform.position-actorPosition).sqrMagnitude<=range*range;
    }
}
