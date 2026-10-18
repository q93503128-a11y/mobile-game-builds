using UnityEngine;
namespace Wildfolk
{
    public enum PlayerLightMount { Held, Equipped }
    [DefaultExecutionOrder(100)]
    public sealed class PlayerLightSource : MonoBehaviour
    {
        public PlayerEquipment Equipment;
        public GameplayLight Gameplay;
        public LocalFirelight Presentation;
        private GameplayLightDefinition equipped;
        public PlayerLightMount Mount {get;private set;}
        private void OnEnable(){Equipment.Changed+=Refresh;Refresh();}
        private void OnDisable(){Equipment.Changed-=Refresh;Set(null);}
        // A future wearable uses this hook; movement, caves and replication need no torch-specific branch.
        public void SetEquipped(GameplayLightDefinition definition){equipped=definition;Refresh();}
        private void Refresh()
        {
            var held=Equipment.SelectedItem==null ? null : Equipment.SelectedItem.PortableLight;
            Mount=held!=null ? PlayerLightMount.Held : PlayerLightMount.Equipped;
            Set(held!=null ? held : equipped);
        }
        private void LateUpdate(){if(Mount==PlayerLightMount.Held && Equipment.SelectedItem!=null && Equipment.SelectedItem.PortableLight!=null)Equipment.HeldVisual.transform.rotation=Quaternion.identity;Presentation.transform.position=Mount==PlayerLightMount.Held ? Equipment.HeldVisual.transform.position+Vector3.up*.35f : transform.position;}
        private void Set(GameplayLightDefinition definition)
        {
            Gameplay.Definition=definition;Gameplay.enabled=definition!=null;
            Presentation.Definition=definition;
        }
    }
}
