using UnityEngine;
namespace Wildfolk
{
    public sealed class GameAudio : MonoBehaviour
    {
        public UserPreferences Preferences;
        public AudioSource Music;
        public AudioSource Sfx;
        private void OnEnable() => Preferences.Changed+=Apply;
        private void Start() => Apply();
        private void OnDisable() => Preferences.Changed-=Apply;
        private void Apply()
        {
            Music.volume=Preferences.Values.Master*Preferences.Values.Music;
            Sfx.volume=Preferences.Values.Master*Preferences.Values.Sfx;
        }
        public void PlaySfx(AudioClip clip) { if(clip!=null) Sfx.PlayOneShot(clip); }
    }
}
