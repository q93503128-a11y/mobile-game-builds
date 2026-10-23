using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Wildfolk.Editor
{
    // UI-only ink atlas. World feedback, equipment and Character Sheet keep their own art.
    public static class JournalSymbolAssets
    {
        public const string Path="Assets/Wildfolk/Art/journal_symbols.png";
        public const int Cell=128,Columns=11,Rows=6;
        public static string[] Names => ClassCatalog.Current.Classes.Select(c=>"class_"+c.Id)
            .Concat(new[]{"vitality","endurance","recovery","stride","metabolism","field_care"}.Select(id=>"skill_"+id))
            .Concat(ClassCatalog.Current.Classes.SelectMany(c=>c.Nodes.Select(n=>"node_"+c.Id+"_"+n.Id))).ToArray();

        // Legacy slicing is supported by this project's Unity 6000.3 importer. Keep
        // existing metadata when it matches so repeated authoring preserves sprite IDs.
#pragma warning disable CS0618
        public static void Import()
        {
            var importer=(TextureImporter)AssetImporter.GetAtPath(Path);
            if(importer==null)throw new InvalidOperationException("Import journal_symbols.png first");
            var names=Names;
            var slices=names.Select((name,i)=>new SpriteMetaData{name=name,
                rect=new Rect(i%Columns*Cell,(Rows-1-i/Columns)*Cell,Cell,Cell),
                alignment=(int)SpriteAlignment.Center,pivot=Vector2.one*.5f}).ToArray();
            var previous=importer.spritesheet;
            var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);
            if(importer.textureType==TextureImporterType.Sprite && importer.spriteImportMode==SpriteImportMode.Multiple
                && importer.alphaIsTransparency && !importer.mipmapEnabled && importer.filterMode==FilterMode.Bilinear
                && importer.wrapMode==TextureWrapMode.Clamp && importer.textureCompression==TextureImporterCompression.Uncompressed
                && importer.maxTextureSize==2048 && importer.npotScale==TextureImporterNPOTScale.None
                && Mathf.Approximately(importer.spritePixelsPerUnit,100) && settings.spriteMeshType==SpriteMeshType.FullRect
                && previous.Length==slices.Length && previous.Zip(slices,(a,b)=>a.name==b.name && a.rect==b.rect).All(x=>x))return;
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit=100;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;
            importer.filterMode=FilterMode.Bilinear;importer.wrapMode=TextureWrapMode.Clamp;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=2048;
            importer.npotScale=TextureImporterNPOTScale.None;
            importer.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);importer.spritesheet=slices;importer.SaveAndReimport();
        }
#pragma warning restore CS0618
        public static Sprite Sprite(string name) => AssetDatabase.LoadAllAssetsAtPath(Path).OfType<Sprite>()
            .Single(s=>s.name==name);
        public static Sprite[] Classes() => ClassCatalog.Current.Classes.Select(c=>Sprite("class_"+c.Id)).ToArray();
        public static Sprite[] Nodes() => ClassCatalog.Current.Classes.SelectMany(c=>c.Nodes.Select(n=>Sprite("node_"+c.Id+"_"+n.Id))).ToArray();

        internal static void Ownership(Text label)
        {
            label.rectTransform.anchoredPosition=new Vector2(-108,-31);
            label.rectTransform.sizeDelta=new Vector2(90,38);
            label.alignment=TextAnchor.MiddleCenter;
        }
        internal static void CommonState(Text label)
        {var size=label.rectTransform.sizeDelta;size.y=40;label.rectTransform.sizeDelta=size;}

        [MenuItem("Wildfolk/Art/Author Journal Symbols")]
        public static void Author()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play first");
            // Save existing objects in place: preserve book geometry, connections and actions.
            Import();
            var growth=AssetDatabase.LoadAssetAtPath<CharacterGrowthDefinition>("Assets/Wildfolk/Data/CharacterGrowth.asset");
            foreach(var skill in growth.Skills)skill.Icon=Sprite("skill_"+skill.Id);
            EditorUtility.SetDirty(growth);AssetDatabase.SaveAssets();
            var scene=EditorSceneManager.OpenScene("Assets/Wildfolk/Scenes/MainMenu.unity");
            var lobby=UnityEngine.Object.FindFirstObjectByType<ClassLobbyScreen>(FindObjectsInactive.Include);
            lobby.ClassIcons=Classes();lobby.SkillIcons=Nodes();
            lobby.ClassIcon.sprite=lobby.TreeIcon.sprite=lobby.ClassIcons[0];
            for(var i=0;i<lobby.Cards.Length;i++)
            {lobby.Cards[i].transform.Find("Class Icon").GetComponent<Image>().sprite=lobby.ClassIcons[i];Ownership(lobby.CardStates[i]);}
            for(var i=0;i<lobby.NodeIcons.Length;i++)lobby.NodeIcons[i].sprite=lobby.SkillIcons[i];
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            foreach(var path in new[]{"Assets/Wildfolk/Scenes/Wildfolk.unity","Assets/Wildfolk/Scenes/Multiplayer.unity"})
            {
                scene=EditorSceneManager.OpenScene(path);
                var tree=UnityEngine.Object.FindFirstObjectByType<SkillTreeScreen>(FindObjectsInactive.Include);
                for(var i=0;i<tree.NodeViews.Length;i++)
                {tree.NodeViews[i].Icon.sprite=growth.Skills[i].Icon;CommonState(tree.NodeStates[i]);}
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            }
            EditorSceneManager.OpenScene("Assets/Wildfolk/Scenes/MainMenu.unity");
        }
    }
}
