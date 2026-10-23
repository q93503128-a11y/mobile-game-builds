using System.IO;
using UnityEditor;
using UnityEngine;

namespace Wildfolk.Editor
{
    internal static class VisualArtImporter
    {
        internal const string ArtPath = "Assets/Wildfolk/Art/";

        internal static void Import()
        {
            AssetDatabase.Refresh();
            foreach (var file in Directory.GetFiles(ArtPath, "*.png"))
            {
                var path = file.Replace('\\', '/');
                if(path==JournalSymbolAssets.Path){JournalSymbolAssets.Import();continue;}
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                var ppu=Path.GetFileName(path)=="ground.png" ? 2560f/48f : 100f;
                var border=Path.GetFileName(path)=="panel.png" ? new Vector4(18,18,18,18) : Vector4.zero;
                if(importer.textureType==TextureImporterType.Sprite && importer.spriteImportMode==SpriteImportMode.Single && Mathf.Approximately(importer.spritePixelsPerUnit,ppu) && importer.alphaIsTransparency && !importer.mipmapEnabled && importer.filterMode==FilterMode.Bilinear && importer.wrapMode==TextureWrapMode.Clamp && importer.textureCompression==TextureImporterCompression.CompressedHQ && importer.maxTextureSize==4096 && importer.spriteBorder==border)continue;
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = Path.GetFileName(path) == "ground.png" ? 2560f / 48f : 100f;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                importer.maxTextureSize = 4096;
                if (Path.GetFileName(path) == "panel.png") importer.spriteBorder = new Vector4(18, 18, 18, 18);
                importer.SaveAndReimport();
            }
        }

        internal static Sprite Sprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(ArtPath + name + ".png");
    }
}
