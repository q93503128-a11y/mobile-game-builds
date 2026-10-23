using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace Wildfolk.Tests
{
    public sealed class JournalIconFinishTests
    {
        private const string Atlas="Assets/Wildfolk/Art/journal_symbols.png";
        private static Sprite Symbol(string name)=>AssetDatabase.LoadAllAssetsAtPath(Atlas).OfType<Sprite>().Single(s=>s.name==name);
        private static T Find<T>(Scene scene) where T:Component =>scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<T>(true)).Single();
        private static void InScene(string name,Action<Scene> action)
        {
            var scene=SceneManager.GetSceneByPath("Assets/Wildfolk/Scenes/"+name+".unity");var loaded=scene.isLoaded;
            if(!loaded)scene=EditorSceneManager.OpenScene("Assets/Wildfolk/Scenes/"+name+".unity",OpenSceneMode.Additive);
            try{action(scene);}finally{if(!loaded)EditorSceneManager.CloseScene(scene,true);}
        }

        [Test] public void InkAtlasContainsExplicitClassCommonAndNodeSymbols()
        {
            var expected=ClassCatalog.Current.Classes.Select(c=>"class_"+c.Id)
                .Concat(new[]{"vitality","endurance","recovery","stride","metabolism","field_care"}.Select(id=>"skill_"+id))
                .Concat(ClassCatalog.Current.Classes.SelectMany(c=>c.Nodes.Select(n=>"node_"+c.Id+"_"+n.Id))).ToArray();
            var sprites=AssetDatabase.LoadAllAssetsAtPath(Atlas).OfType<Sprite>().ToArray();
            Assert.That(sprites.Select(s=>s.name),Is.EquivalentTo(expected));Assert.That(sprites.Length,Is.EqualTo(66));
            foreach(var s in sprites)
            {Assert.That(s.rect.size,Is.EqualTo(new Vector2(128,128)),s.name);Assert.That(s.pivot,Is.EqualTo(new Vector2(64,64)),s.name);}
            var importer=(TextureImporter)AssetImporter.GetAtPath(Atlas);
            Assert.That(importer.spriteImportMode,Is.EqualTo(SpriteImportMode.Multiple));
            Assert.That(importer.textureCompression,Is.EqualTo(TextureImporterCompression.Uncompressed));
            Assert.That(importer.alphaIsTransparency && !importer.mipmapEnabled,Is.True);
            Assert.That(importer.filterMode,Is.EqualTo(FilterMode.Bilinear));
            Assert.That(sprites[0].texture.width,Is.EqualTo(1408));Assert.That(sprites[0].texture.height,Is.EqualTo(768));
        }

        [Test] public void RosterStatesFitCardWithoutShrinkingFontAndClassNodesAreMapped()
        {
            InScene("MainMenu",scene=>
            {
                var ui=Find<ClassLobbyScreen>(scene);Assert.That(ui.ClassIcons.Length,Is.EqualTo(6));Assert.That(ui.SkillIcons.Length,Is.EqualTo(54));
                for(var i=0;i<6;i++)
                {
                    var c=ClassCatalog.Current.Classes[i];Assert.That(ui.ClassIcons[i],Is.SameAs(Symbol("class_"+c.Id)));
                    Assert.That(ui.Cards[i].transform.Find("Class Icon").GetComponent<Image>().sprite,Is.SameAs(ui.ClassIcons[i]));
                    var label=ui.CardStates[i];var rect=label.rectTransform;
                    Assert.That(rect.anchoredPosition,Is.EqualTo(new Vector2(-108,-31)));
                    Assert.That(rect.sizeDelta,Is.EqualTo(new Vector2(90,38)));
                    Assert.That(rect.anchoredPosition.y-rect.sizeDelta.y/2,Is.GreaterThanOrEqualTo(-50));
                    Assert.That(label.fontSize,Is.EqualTo(20));Assert.That(label.alignment,Is.EqualTo(TextAnchor.MiddleCenter));
                    var old=label.text;
                    try
                    {
                        foreach(var text in new[]{"Locked","Selected","Owned","Available","Learned","잠김","선택됨","보유"})
                        {label.text=text;Assert.That(label.preferredHeight+8,Is.LessThanOrEqualTo(rect.sizeDelta.y),text);Assert.That(label.preferredWidth,Is.LessThanOrEqualTo(rect.sizeDelta.x),text);}
                    }finally{label.text=old;}
                    for(var j=0;j<9;j++)Assert.That(ui.SkillIcons[i*9+j],Is.SameAs(Symbol("node_"+c.Id+"_"+c.Nodes[j].Id)));
                }
                Assert.That(ui.InvestLabel.rectTransform.sizeDelta,Is.EqualTo(new Vector2(266,62)));
                foreach(var n in ui.NodeViews)Assert.That(n.Icon.raycastTarget,Is.False);
                foreach(var b in ui.Nodes)Assert.That(b.onClick.GetPersistentEventCount(),Is.EqualTo(1));
            });
        }

        [TestCase("Wildfolk")][TestCase("Multiplayer")]
        public void CommonTreeHasSixDistinctSymbolsAndPaddedSharedStates(string name)
        {
            InScene(name,scene=>
            {
                var tree=Find<SkillTreeScreen>(scene);
                var growth=AssetDatabase.LoadAssetAtPath<CharacterGrowthDefinition>("Assets/Wildfolk/Data/CharacterGrowth.asset");
                Assert.That(growth.Skills.Select(s=>s.Icon).Distinct().Count(),Is.EqualTo(6));
                for(var i=0;i<6;i++)
                {
                    Assert.That(growth.Skills[i].Icon,Is.SameAs(Symbol("skill_"+growth.Skills[i].Id)));
                    Assert.That(tree.NodeViews[i].Icon.sprite,Is.SameAs(growth.Skills[i].Icon));
                    var label=tree.NodeStates[i];Assert.That(label.fontSize,Is.EqualTo(20));
                    Assert.That(label.rectTransform.sizeDelta.y,Is.EqualTo(40));
                    var old=label.text;
                    try{foreach(var s in new[]{"Locked","Available","Learned","Selected","잠김","배움","투자 가능"})
                    {label.text=s;Assert.That(label.preferredHeight+8,Is.LessThanOrEqualTo(40),s);Assert.That(label.preferredWidth,Is.LessThanOrEqualTo(label.rectTransform.sizeDelta.x),s);}}
                    finally{label.text=old;}
                    Assert.That(label.rectTransform.anchoredPosition.y-20,Is.GreaterThan(-430));
                    Assert.That(tree.NodeViews[i].Icon.raycastTarget,Is.False);
                    Assert.That(tree.Nodes[i].image.raycastTarget,Is.True);
                    Assert.That(tree.Nodes[i].onClick.GetPersistentEventCount(),Is.EqualTo(1));
                }
                // Existing sheet uses its original art, outside the scope of this atlas.
                foreach(var image in scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Image>(true))
                    .Where(i=>i.name=="Health Icon" || i.name=="Stamina Icon"))
                    Assert.That(AssetDatabase.GetAssetPath(image.sprite),Is.Not.EqualTo(Atlas));
            });
        }

        [Test] public void NodeStatesPreserveInkAndIndependentFocusLockSeal()
        {
            var root=new GameObject("State view",typeof(RectTransform));
            try
            {
                var node=root.AddComponent<JournalSkillNode>();node.Frame=root.AddComponent<Image>();
                node.Icon=new GameObject("Ink",typeof(RectTransform),typeof(Image)).GetComponent<Image>();node.Icon.transform.SetParent(root.transform);
                node.Focus=new GameObject("Focus");node.Focus.transform.SetParent(root.transform);
                node.Locked=new GameObject("Locked");node.Locked.transform.SetParent(root.transform);
                node.Learned=new GameObject("Learned");node.Learned.transform.SetParent(root.transform);
                node.SetState(false,false,true);
                Assert.That(node.Icon.color,Is.EqualTo(new Color(1,1,1,.72f)));Assert.That(node.Locked.activeSelf && node.Focus.activeSelf,Is.True);
                node.SetState(false,true,false);Assert.That(node.Icon.color,Is.EqualTo(Color.white));Assert.That(node.Locked.activeSelf || node.Learned.activeSelf,Is.False);
                node.SetState(true,false,true);Assert.That(node.Icon.color,Is.EqualTo(Color.white));Assert.That(node.Learned.activeSelf && node.Focus.activeSelf && !node.Locked.activeSelf,Is.True);
            }finally{Object.DestroyImmediate(root);}
        }
    }
}
