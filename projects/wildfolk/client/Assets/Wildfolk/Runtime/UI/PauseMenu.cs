using UnityEngine;
namespace Wildfolk
{
    public sealed class PauseMenu : MonoBehaviour
    {
        public GameObject Menu, Settings;
        public UserPreferences Preferences;
        private void OnEnable() => Back();
        public void OpenSettings() { Menu.SetActive(false);Settings.SetActive(true); }
        public void Back() { Preferences.Flush();Settings.SetActive(false);Menu.SetActive(true); }
    }
}
