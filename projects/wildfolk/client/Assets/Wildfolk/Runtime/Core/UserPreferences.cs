using System;
using UnityEngine;
namespace Wildfolk
{
    public sealed class UserPreferences : MonoBehaviour
    {
        public LocalSettings Values { get; private set; }
        public event Action Changed;
        private float saveAt=-1;
        private void Awake() => Values=LocalSettings.Load();
        public void SetControl(float value) { Values.SetControl(value);Notify(); }
        public void SetMaster(float value) { Values.SetMaster(value);Notify(); }
        public void SetMusic(float value) { Values.SetMusic(value);Notify(); }
        public void SetSfx(float value) { Values.SetSfx(value);Notify(); }
        public void ResetDefaults() { Values=new LocalSettings();Notify(); }
        private void Notify() { Values.Store();saveAt=Time.unscaledTime+.5f;Changed?.Invoke(); }
        private void Update() { if(saveAt>=0 && Time.unscaledTime>=saveAt) Flush(); }
        public void Flush() { if(saveAt<0) return;PlayerPrefs.Save();saveAt=-1; }
        private void OnApplicationPause(bool paused) { if(paused) Flush(); }
        private void OnApplicationQuit() => Flush();
        private void OnDisable() => Flush();
    }
}
