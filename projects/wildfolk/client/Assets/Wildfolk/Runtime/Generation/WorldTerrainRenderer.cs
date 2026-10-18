using UnityEngine;
namespace Wildfolk
{
    public sealed partial class WorldTerrainRenderer : MonoBehaviour
    {
        public SpriteRenderer Ground;
        private Texture2D texture;
        private Sprite sprite;
        public void Build(WorldMap map)
        {
            ClearChunks();Ground.enabled=!map.Streamed;
            if(sprite!=null)Destroy(sprite);if(texture!=null)Destroy(texture);if(map.Streamed)return;
            var d=map.Definition;var width=d.TextureWidth;var height=Mathf.RoundToInt(width*d.Size.y/d.Size.x);
            texture=new Texture2D(width,height,TextureFormat.RGB24,false);texture.name="Temperate Ground";texture.filterMode=FilterMode.Bilinear;texture.wrapMode=TextureWrapMode.Clamp;
            var pixels=new Color32[width*height];
            for(var y=0;y<height;y++)for(var x=0;x<width;x++)
            {
                var p=new Vector2((x+.5f)/width*d.Size.x-d.Size.x*.5f,(y+.5f)/height*d.Size.y-d.Size.y*.5f);
                pixels[y*width+x]=Sample(map,p);
            }
            texture.SetPixels32(pixels);texture.Apply(false,true);
            sprite=Sprite.Create(texture,new Rect(0,0,width,height),new Vector2(.5f,.5f),width/d.Size.x);sprite.name="Generated Terrain";
            Ground.sprite=sprite;Ground.transform.localScale=Vector3.one;Ground.sortingOrder=WorldSorting.Ground;
        }
        private static float Blend(float from,float to,float value)=>Mathf.SmoothStep(0,1,Mathf.InverseLerp(from,to,value));
        public static Color Sample(WorldMap map,Vector2 p)
        {
            if(map.Streamed)return SampleContinental(map,p);
            var d=map.Definition;var b=d.Biome;
            var forest=map.ForestDensity(p);var grain=Mathf.PerlinNoise(p.x*1.7f+map.Seed%713,p.y*1.7f+133)*.12f;
            var color=Color.Lerp(b.Grass,b.Forest,Mathf.SmoothStep(0,1,(forest-.35f)*2.2f));color*=.90f+grain;
            var road=1-Blend(d.RoadWidth-.35f,d.RoadWidth+.7f,map.RoadDistance(p));color=Color.Lerp(color,b.Dirt,road*.83f);
            var waterDistance=map.WaterDistance(p);
            color=Color.Lerp(color,b.Dirt,(1-Blend(.2f,1.2f,waterDistance))*.72f);
            color=Color.Lerp(color,b.Shallows,(1-Blend(-.25f,.45f,waterDistance))*.85f);
            if(waterDistance<.15f)
            {
                var shallow=1-Blend(WorldMap.FordHalfWidth-.5f,WorldMap.FordHalfWidth+1.1f,Mathf.Abs(p.y-map.FordY));
                if(map.LakeDistance(p,map.LakeCenter,map.LakeRadius)<0 || map.LakeDistance(p,map.PondCenter,map.PondRadius)<0)shallow=0;
                var water=Color.Lerp(b.Water,b.Shallows,shallow*.85f);
                var ripple=Mathf.Pow(Mathf.Max(0,Mathf.Sin(p.x*.9f+p.y*.5f)*Mathf.Sin(p.y*2.2f)),12)*.06f;
                water+=new Color(ripple,ripple,ripple);water*=.92f+grain;
                color=Color.Lerp(color,water,1-Blend(-.45f,.15f,waterDistance));
            }
            return color;
        }
        private void OnDestroy(){ClearChunks();if(sprite!=null)Destroy(sprite);if(texture!=null)Destroy(texture);}
    }
}
