using UnityEngine;
using UnityEngine.UI;
namespace Wildfolk
{
 [DisallowMultipleComponent]public sealed class AudioButton:MonoBehaviour
 {
  public static UnityEngine.UI.Button Add(GameObject go){var b=go.AddComponent<UnityEngine.UI.Button>();if(go.GetComponent<AudioButton>()==null)go.AddComponent<AudioButton>();return b;}
  public Sound Cue=Sound.UiClick;private Button button;private void Awake(){button=GetComponent<Button>();var label=gameObject.name.ToLowerInvariant();if(label.Contains("back")||label.Contains("close")||label.Contains("cancel"))Cue=Sound.UiBack;else if(label.Contains("category")||label.Contains("tab")||label.Contains("board"))Cue=Sound.Page;if(button!=null)button.onClick.AddListener(Play);}
  private void Play(){if(button!=null&&button.interactable)AudioRuntime.Play(Cue);}
  private void OnDestroy(){if(button!=null)button.onClick.RemoveListener(Play);}
 }
}
