using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    public sealed partial class WorldTerrainRenderer
    {
        private sealed class TerrainTile {public Texture2D Texture;public Sprite Sprite;}
        private readonly Dictionary<WorldChunk,TerrainTile> tiles=new Dictionary<WorldChunk,TerrainTile>();
        public int TerrainTileCount=>tiles.Count;
        public void BuildChunk(WorldMap map,WorldChunk chunk,Transform parent)
        {
            var pixels=map.Definition.ChunkTexturePixels;var edge=pixels+2;var texture=new Texture2D(edge,edge,TextureFormat.RGB24,false){name="Local landscape",filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};var colors=new Color32[edge*edge];var origin=chunk.Origin(map.Definition.ChunkSize);var step=(float)map.Definition.ChunkSize/pixels;
            // One-texel halo samples the same world coordinates as the neighboring tile.
            for(var y=0;y<edge;y++)for(var x=0;x<edge;x++)colors[y*edge+x]=SampleContinental(map,origin+new Vector2(x-.5f,y-.5f)*step);
            texture.SetPixels32(colors);texture.Apply(false,true);var sprite=Sprite.Create(texture,new Rect(1,1,pixels,pixels),Vector2.zero,1/step);var go=new GameObject("Terrain");go.transform.SetParent(parent,false);go.transform.position=origin;var sr=go.AddComponent<SpriteRenderer>();sr.sharedMaterial=Ground.sharedMaterial;sr.sprite=sprite;sr.sortingOrder=WorldSorting.Ground;tiles[chunk]=new TerrainTile{Texture=texture,Sprite=sprite};
        }
        public void ReleaseChunk(WorldChunk key){if(!tiles.TryGetValue(key,out var tile))return;Destroy(tile.Sprite);Destroy(tile.Texture);tiles.Remove(key);}
        private void ClearChunks(){foreach(var tile in tiles.Values){Destroy(tile.Sprite);Destroy(tile.Texture);}tiles.Clear();}
        public static Color SampleContinental(WorldMap map,Vector2 p)
        {
            var layout=map.Continental;var e=layout.Environment(p);var grain=WorldSeeds.Field(map.Seed,SeedDomain.Vegetation,p,1.8f);
            if(layout.Underground(p))
            {var c=layout.CaveAt(p);var distance=c.FloorDistance(p);var floor=Color.Lerp(new Color(.31f,.32f,.28f),new Color(.19f,.25f,.25f),c.Nearest(p).Depth);floor*=Mathf.Lerp(.8f,1.08f,grain);var wall=Color.Lerp(new Color(.075f,.095f,.09f),new Color(.16f,.18f,.16f),grain);return c.Water(p) ? new Color(.11f,.23f,.25f) : Color.Lerp(floor,wall,Mathf.SmoothStep(0,1,Mathf.InverseLerp(-1,.8f,distance)));}
            var color=Color.black;foreach(var profile in map.Definition.SurfaceEnvironments){var v=e.Weights[(int)profile.Biome];color+=Color.Lerp(profile.Ground,profile.DenseGround,e.Forestation)*v;}color*=Mathf.Lerp(.88f,1.08f,grain);
            if((e.Tags&EnvironmentTag.RockFace)!=0){var stratum=Mathf.SmoothStep(.8f,1,Mathf.Abs(Mathf.Sin(p.x*.09f+p.y*.035f+grain*.3f)));color=Color.Lerp(color,new Color(.27f,.3f,.27f),stratum*e.Rockiness*.55f);}
            var road=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(1.2f,3.2f,layout.RoadDistance(p)));color=Color.Lerp(color,new Color(.43f,.38f,.27f),road*.8f);
            var distanceWater=e.WaterDistance;var coast=layout.CoastDistance(p);
            if(coast<18)color=Color.Lerp(color,new Color(.57f,.53f,.39f),1-Mathf.Clamp01(coast/18));
            var bank=1-Mathf.Clamp01((distanceWater-1)/3);color=Color.Lerp(color,new Color(.35f,.37f,.27f),bank*.6f);
            if(distanceWater<1.4f){var water=coast< -10 ? new Color(.085f,.2f,.26f) : layout.DeepWater(p) ? new Color(.19f,.36f,.37f) : new Color(.33f,.46f,.41f);color=Color.Lerp(color,water,1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(-.6f,1.4f,distanceWater)));}return color;
        }
    }
}
