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
        private PlayerExplorationUtility utility;
        private void Update()
        {
            if(Time.unscaledTime<nextTick)return;nextTick=Time.unscaledTime+.15f;
            if(Player==null){var arena=FindFirstObjectByType<MultiplayerArena>();if(arena!=null && arena.LocalPlayer!=null)Player=arena.LocalPlayer.GetComponent<PlayerNavigation>();}
            if(Player!=null && (utility==null || utility.gameObject!=Player.gameObject))utility=Player.GetComponent<PlayerExplorationUtility>();
            var hint=UtilityHint(out var hintDirection,out var hintArrow,out var hintLabel);
            var compass=Player!=null && Player.HasCompass && Player.Mode!=NavigationMode.Off;
            var visible=hint || compass;View.SetActive(visible);if(!visible)return;
            if(world==null)world=FindFirstObjectByType<WorldGenerator>();if(world==null || world.Map==null)return;
            North.localRotation=Quaternion.Euler(0,0,NavigationBearing.Angle(Vector2.up,Camera));
            North.gameObject.SetActive(compass);
            if(hint){HomeArrow.gameObject.SetActive(hintArrow);if(hintArrow)HomeArrow.localRotation=Quaternion.Euler(0,0,NavigationBearing.Angle(hintDirection,Camera));SetLabel(hintLabel);return;}
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
            SetLabel(label);
        }
        private void SetLabel(string label){if(previous!=label){Destination.text=label;previous=label;}}
        private bool UtilityHint(out Vector2 direction,out bool arrow,out string label)
        {
            direction=Vector2.zero;arrow=false;label=null;if(utility==null)return false;
            var stack=utility.GetComponent<PlayerInventory>().SelectedStack;var d=stack.IsEmpty ? null : stack.Item.ExplorationUtility;
            if(d!=null && d.Kind==ExplorationUtilityKind.Survey)
            {
                var s=utility.Survey;
                if(s.Phase==SurveyPhase.Found && s.Item>=0 && s.Item<utility.Catalog.Items.Length && s.Until>utility.Now){direction=PlayerExplorationUtility.Direction(s.Bearing);arrow=true;label=utility.Catalog.Items[s.Item].LocalizedName;return true;}
                if(s.Phase!=SurveyPhase.None && s.Until>utility.Now){label=L.Text("ui.survey."+s.Phase.ToString().ToLowerInvariant());return true;}
                if(s.Ready>utility.Now){label=L.Format("ui.exploration.ready",Mathf.CeilToInt((float)(s.Ready-utility.Now)));return true;}
                label=L.Text("ui.survey.ready");return true;
            }
            if(utility.TryPeerSignal(out var point)){direction=point-(Vector2)utility.transform.position;arrow=direction.sqrMagnitude>9;label=L.Text(arrow ? "ui.signal.peer" : "ui.signal.near");return true;}
            var signal=utility.Signal;if(signal.Until>utility.Now && utility.Now-signal.Started<2){label=L.Text("ui.signal.sent");return true;}
            if(d!=null && d.Kind==ExplorationUtilityKind.Signal){label=utility.SignalReady>utility.Now ? L.Format("ui.exploration.ready",Mathf.CeilToInt((float)(utility.SignalReady-utility.Now))) : L.Text("ui.signal.ready");return true;}
            return false;
        }
    }
}
