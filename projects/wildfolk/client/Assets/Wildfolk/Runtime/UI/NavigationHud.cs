using UnityEngine;
using UnityEngine.UI;
namespace Wildfolk
{
    public sealed class NavigationHud : MonoBehaviour
    {
        public PlayerNavigation Player;
        public GameObject View;
        public RectTransform North,HomeArrow;
        public Text Destination;
        public Camera Camera;
        private WorldGenerator world;
        private float nextTick;
        private string previous;
        private void Update()
        {
            if(Time.unscaledTime<nextTick)return;nextTick=Time.unscaledTime+.15f;
            if(Player==null){var arena=FindFirstObjectByType<MultiplayerArena>();if(arena!=null && arena.LocalPlayer!=null)Player=arena.LocalPlayer.GetComponent<PlayerNavigation>();}
            var visible=Player!=null && Player.HasCompass && Player.Mode!=NavigationMode.Off;View.SetActive(visible);if(!visible)return;
            if(world==null)world=FindFirstObjectByType<WorldGenerator>();if(world==null || world.Map==null)return;
            North.localRotation=Quaternion.Euler(0,0,NavigationBearing.Angle(Vector2.up,Camera));
            Vector2 target;string label;bool valid;
            var p=(Vector2)Player.transform.position;
            if(Player.Mode==NavigationMode.Beacon){valid=Player.TryBeacon(out target,out var number);label=valid ? L.Format("ui.beacon.number",number) : L.Text("ui.navigation.no_target");}
            else if(Player.Mode==NavigationMode.CaveExit || Player.Mode==NavigationMode.Automatic && world.Map.Streamed && world.Map.Continental.Underground(p)){valid=world.Map.Streamed && world.Map.Continental.Underground(p);target=valid ? NavigationBearing.CaveExit(world.Map.Continental.CaveAt(p),p) : p;label=L.Text("ui.navigation.exit");}
            else
            {
                var network=Player.GetComponent<NetworkNavigation>();
                if(network!=null && !network.IsServer){valid=network.HomeValid.Value;target=network.HomePosition.Value;}
                else valid=Player.TryHome(world,out target);
                label=valid ? L.Text("ui.navigation.home") : L.Text("ui.navigation.no_home");
            }
            HomeArrow.gameObject.SetActive(valid);if(valid){var delta=target-p;HomeArrow.localRotation=Quaternion.Euler(0,0,NavigationBearing.Angle(delta,Camera));label=delta.magnitude<3 ? label+" · "+L.Text("ui.navigation.near") : label+" · "+Mathf.RoundToInt(delta.magnitude)+" m";}
            if(previous!=label){Destination.text=label;previous=label;}
        }
    }
}
