using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object=UnityEngine.Object;
namespace Wildfolk.Editor
{
    public static class BossUiRegressionQa
    {
        public static IEnumerator Run()
        {
            var folder=Path.Combine(DevelopmentProfile.Directory,"boss-verification");Directory.CreateDirectory(folder);var before=L.Language;
            for(var i=0;i<3;i++)
            {
                L.SetLanguage("ko");yield return SceneManager.LoadSceneAsync(SceneFlow.HomeScene);yield return null;L.SetLanguage("en");
                var labels=Object.FindObjectsByType<LocalizedLabel>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(l=>l.Key=="class.title" && l.gameObject.activeInHierarchy).ToArray();
                if(labels.Length==0 || labels.Any(l=>l.GetComponent<Text>().text!="Class"))throw new InvalidOperationException("MainMenu Class label not refreshed");
                if(i==2){VisualGameView.ShowReference();yield return null;yield return null;ScreenCapture.CaptureScreenshot(Path.Combine(folder,"class-menu-1920.png"));yield return null;}
                L.SetLanguage("ko");WorldGenerator.SelectedSeed=9472026;yield return SceneManager.LoadSceneAsync(SceneFlow.GameplayScene);yield return null;ContentExpansionQa.Prepare();L.SetLanguage("en");
                File.AppendAllText(Path.Combine(folder,"language.tsv"),"PASS\tGameplay/Home/Gameplay KO/EN cycle "+(i+1)+"\n");
            }
            L.SetLanguage(before);yield return null;
        }
    }
}
