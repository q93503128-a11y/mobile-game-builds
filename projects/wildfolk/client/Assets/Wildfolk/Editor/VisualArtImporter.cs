using System;
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
                var fileName=Path.GetFileName(path);
                var menuSurface=fileName.StartsWith("menu_a_",StringComparison.Ordinal);
                var inventorySurface=fileName.StartsWith("inventory_",StringComparison.Ordinal);
                // These PNGs are 2x exports. PPU 200 against the Canvas reference PPU 100 keeps their UI-unit borders at half the raster pixels.
                var ppu=fileName=="ground.png" ? 2560f/48f : menuSurface ? 200f : 100f;
                var border=fileName=="panel.png" ? new Vector4(18,18,18,18)
                    : menuSurface && fileName=="menu_a_dialog_frame.png" ? new Vector4(64,64,64,64)
                    : inventorySurface && fileName=="inventory_panel.png" ? new Vector4(16,16,16,16)
                    : inventorySurface && fileName=="inventory_slot.png" ? new Vector4(10,10,10,10)
                    : inventorySurface && fileName=="inventory_selected_slot.png" ? new Vector4(10,10,10,10)
                    : inventorySurface && fileName=="inventory_button.png" ? new Vector4(14,10,14,10)
                    : inventorySurface && fileName=="inventory_rail.png" ? new Vector4(10,10,10,10)
                    : menuSurface ? new Vector4(96,56,96,56) : Vector4.zero;
                var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);
                var meshReady=!menuSurface&&!inventorySurface||settings.spriteMeshType==SpriteMeshType.FullRect;
                if(importer.textureType==TextureImporterType.Sprite && importer.spriteImportMode==SpriteImportMode.Single && Mathf.Approximately(importer.spritePixelsPerUnit,ppu) && importer.alphaIsTransparency && !importer.mipmapEnabled && importer.filterMode==FilterMode.Bilinear && importer.wrapMode==TextureWrapMode.Clamp && importer.textureCompression==TextureImporterCompression.CompressedHQ && importer.maxTextureSize==4096 && importer.spriteBorder==border && meshReady)continue;
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = ppu;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                importer.maxTextureSize = 4096;
                importer.spriteBorder = border;
                if(menuSurface||inventorySurface){importer.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;importer.SetTextureSettings(settings);}
                importer.SaveAndReimport();
            }
        }

        internal static Sprite Sprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(ArtPath + name + ".png");
    }
}
