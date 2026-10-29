using UnityEngine;
namespace Wildfolk
{
    public sealed class GameAudio : MonoBehaviour
    {
        public UserPreferences Preferences;
        public float MusicGain=>Preferences.Values.Master*Preferences.Values.Music;
        public float SfxGain=>Preferences.Values.Master*Preferences.Values.Sfx;
        private GameplaySession session;private SessionState previousState;
        private void Awake(){AudioRuntime.Ensure();session=GetComponent<GameplaySession>();if(session!=null)session.StateChanged+=Context;}
        private void Context(){if(session.State==previousState)return;AudioRuntime.Play(session.State==SessionState.Inventory||session.State==SessionState.Character||session.State==SessionState.SkillTree?Sound.Journal:Sound.UiBack);previousState=session.State;}
        private void OnDestroy(){if(session!=null)session.StateChanged-=Context;}
        private void OnEnable(){if(Preferences!=null)Preferences.Changed+=Apply;}
        private void Start() => Apply();
        private void OnDisable(){if(Preferences!=null)Preferences.Changed-=Apply;}
        private void Apply()
        {
            if(Preferences!=null&&Preferences.Values!=null)AudioRuntime.Ensure().Apply(Preferences.Values);
        }
    }
}
