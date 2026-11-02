using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace Wildfolk.Editor
{
    // Canonical authored scene changes. Does not recreate navigation, accounts, or gameplay.
    public static class QualityPresentationBuilder
    {
        [MenuItem("Wildfolk/Build Quality Coherence Presentation")]
        public static void Apply()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Clean edit mode required.");
            foreach(var path in new[]{FrontendSceneBuilder.ScenePath,GameplaySceneBuilder.ScenePath,MultiplayerSceneBuilder.ScenePath})
            {
                var scene=EditorSceneManager.OpenScene(path);
                foreach(var profile in UnityEngine.Object.FindObjectsByType<ProfileHud>(FindObjectsInactive.Include,FindObjectsSortMode.None))CharacterUiBuilder.ArrangeHud(profile);
                foreach(var hud in UnityEngine.Object.FindObjectsByType<ClassHud>(FindObjectsInactive.Include,FindObjectsSortMode.None))MaintenanceUiBuilder.Arrange(hud);
                foreach(var menu in UnityEngine.Object.FindObjectsByType<PauseMenu>(FindObjectsInactive.Include,FindObjectsSortMode.None))SurvivalUiBuilder.ConfigurePauseActions(menu.Menu.transform,menu.GetComponentInParent<GameplaySession>()?.GetComponent<WorldSaveService>()??UnityEngine.Object.FindFirstObjectByType<WorldSaveService>());
                foreach(var image in UnityEngine.Object.FindObjectsByType<Image>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                {
                    if(image.name=="Field paper"||image.name=="Left Page"||image.name=="Right Page")image.color=JournalStyle.FieldPaper;
                    if(image.name.EndsWith(" Overlay",StringComparison.Ordinal)&&image.name!="Pause Overlay")image.color=new Color(.035f,.06f,.045f,.36f);
                }
                foreach(var button in UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                {var sheet=button.GetComponentInParent<ModalLayout>(true);if(button.name=="Back"&&sheet!=null&&sheet.transform.Find("Field paper")!=null)InventoryUiBuilder.ActionSize(button,new Vector2(680,400),new Vector2(180,112));}
                foreach(var classes in UnityEngine.Object.FindObjectsByType<ClassLobbyScreen>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                {var selection=(RectTransform)classes.SelectButton.transform;selection.anchoredPosition=new Vector2(selection.anchoredPosition.x,-365);var purchase=(RectTransform)classes.PurchaseButton.transform;purchase.anchoredPosition=new Vector2(purchase.anchoredPosition.x,-365);var respec=(RectTransform)classes.RespecButton.transform;respec.anchoredPosition=new Vector2(respec.anchoredPosition.x,-365);}
                foreach(var economy in UnityEngine.Object.FindObjectsByType<EconomyScreen>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                {
                    for(var i=0;i<economy.Tabs.Length;i++)InventoryUiBuilder.ActionSize(economy.Tabs[i],new Vector2(-510+i*510,295),new Vector2(460,112));
                    economy.AchievementRetry=Retry(economy.AchievementView);economy.AttendanceRetry=Retry(economy.AttendanceView);
                }
                foreach(var panel in UnityEngine.Object.FindObjectsByType<InventoryPanel>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                {var hint=panel.View.transform.Find("Inventory Panel/Slot Hint");if(hint!=null)InventoryUiBuilder.Caption(hint.GetComponent<Text>(),new Vector2(-815,-397),new Vector2(255,140),24);}
                foreach(var menu in UnityEngine.Object.FindObjectsByType<MultiplayerMenu>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                {InventoryUiBuilder.ActionSize(menu.SingleButton,new Vector2(0,-375),new Vector2(650,110));InventoryUiBuilder.Caption(menu.StatusLabel,new Vector2(0,-255),new Vector2(1400,100),32);}
                foreach(var slot in UnityEngine.Object.FindObjectsByType<InventorySlotView>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                {var count=slot.transform.Find("Stack Count");if(count!=null){var text=count.GetComponent<Text>();((RectTransform)count).sizeDelta=new Vector2(Mathf.Max(48,((RectTransform)count).sizeDelta.x),Mathf.Max(36,text.fontSize*1.6f));}}
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();EditorSceneManager.OpenScene(FrontendSceneBuilder.ScenePath);
        }
        private static Button Retry(GameObject root)
        {
            foreach(var button in root.GetComponentsInChildren<Button>(true))if(button.name=="Sync"||button.name=="Retry")
            {button.name="Retry";var label=button.GetComponentInChildren<Text>(true);var localized=label.GetComponent<LocalizedLabel>();if(localized!=null)localized.Key="economy.retry_action";label.text=L.Text("economy.retry_action");button.gameObject.SetActive(false);return button;}throw new InvalidOperationException("Missing existing economy retry action.");
        }
    }
}
