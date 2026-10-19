using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace Wildfolk.Tests
{
    // Automated offscreen render evidence. This is not manual Game View or MCP input QA.
    public sealed class MetalRenderPlayTests
    {
        private static void Capture(string name)
        {
            var camera=Camera.main;var texture=RenderTexture.GetTemporary(1920,1080,24);var previous=camera.targetTexture;var active=RenderTexture.active;
            var canvases=Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.isRootCanvas).ToArray();var modes=canvases.Select(c=>c.renderMode).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();var orders=canvases.Select(c=>c.sortingOrder).ToArray();
            try
            {
                camera.targetTexture=texture;
                foreach(var canvas in canvases){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;canvas.sortingOrder=32760;}
                Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=texture;var pixels=new Texture2D(1920,1080,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,1920,1080),0,0);pixels.Apply();
                var directory=Path.GetFullPath(Path.Combine(Application.dataPath,"../Review/metal"));Directory.CreateDirectory(directory);File.WriteAllBytes(Path.Combine(directory,name+".png"),pixels.EncodeToPNG());Object.Destroy(pixels);
            }
            finally{camera.targetTexture=previous;RenderTexture.active=active;for(var i=0;i<canvases.Length;i++){canvases[i].renderMode=modes[i];canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];canvases[i].sortingOrder=orders[i];}RenderTexture.ReleaseTemporary(texture);}
        }
        [UnityTest]public IEnumerator NaturalOreViewsAndFiveMaterialWorkbenchLayoutAt1920By1080()
        {
            if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)Assert.Ignore("Requires graphics; actual MCP Game View evidence is recorded separately.");Time.timeScale=1;WorldGenerator.SelectedGenerationVersion=1;WorldGenerator.SelectedSeed=9472026;yield return SceneManager.LoadSceneAsync("Wildfolk");yield return null;yield return null;
            var save=Object.FindFirstObjectByType<WorldSaveService>();save.AutoSaveEnabled=false;save.Clock.enabled=false;save.World.GetComponent<ZombieSpawner>().enabled=false;save.World.GetComponent<GoblinSpawner>().enabled=false;save.World.Animals.enabled=false;save.World.Renewal.enabled=false;
            var player=save.Player;player.GetComponent<PlayerInputAdapter>().enabled=false;player.GetComponent<PlayerMotor>().enabled=false;L.SetLanguage("en");
            foreach(var kind in new[]{ResourceKind.CopperOre,ResourceKind.TinOre,ResourceKind.Coal})
            {
                var node=Object.FindObjectsByType<ResourceNode>(FindObjectsSortMode.None).First(r=>r.Definition.Kind==kind);
                var point=(Vector2)node.transform.position+Vector2.down*2;player.GetComponent<Rigidbody2D>().position=point;player.transform.position=point;Physics2D.SyncTransforms();yield return null;
                Camera.main.transform.position=new Vector3(node.transform.position.x,node.transform.position.y,-10);Capture(kind.ToString().ToLowerInvariant());
            }
            player.GetComponent<Rigidbody2D>().position=Vector2.zero;player.transform.position=Vector2.zero;
            var bench=Object.Instantiate(save.Catalog.Item("workbench").Placement.Prefab,new Vector2(0,-2),Quaternion.identity).GetComponent<CraftingStation>();var clone=Object.Instantiate(bench.Definition);clone.UpgradeCosts=clone.UpgradeCosts.Concat(new[]{new ItemAmount{Item=save.Catalog.Item("bronze"),Count=2},new ItemAmount{Item=save.Catalog.Item("steel"),Count=1}}).ToArray();bench.Definition=clone;
            player.Model.Add(save.Catalog.Item("wood"),8);player.Model.Add(save.Catalog.Item("iron_ingot"),2);player.Model.Add(save.Catalog.Item("steel"),1);yield return null;Physics2D.SyncTransforms();Assert.That(player.GetComponent<PlayerCrafting>().OpenStation(bench),Is.True);yield return null;
            var panel=Object.FindFirstObjectByType<CraftingPanel>();panel.Refresh();Camera.main.transform.position=new Vector3(0,0,-10);Capture("upgrade-five-materials");panel.UpgradeScroll.verticalNormalizedPosition=0;Canvas.ForceUpdateCanvases();Capture("upgrade-five-materials-scroll");
            Assert.That(panel.UpgradeRequirements.text,Does.Contain("C5CE9B").And.Contain("D99078"));Assert.That(panel.UpgradeButton.interactable,Is.False);Object.Destroy(clone);
        }
    }
}
