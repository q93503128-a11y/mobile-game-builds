using UnityEditor;
using UnityEngine;

namespace Wildfolk.Editor
{
    internal static class WorldArt
    {
        internal static Material SpriteMaterial;

        internal static Transform Group(string name,Transform parent,float x=0,float y=0)
        {
            var root=new GameObject(name).transform;
            root.SetParent(parent,false);
            root.localPosition=new Vector3(x,y,0);
            return root;
        }

        internal static SpriteRenderer Sprite(string name,string art,Transform parent,Vector2 position,float scale,int order)
        {
            var root=Group(name,parent,position.x,position.y);
            root.localScale=Vector3.one*scale;
            var renderer=root.gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite=VisualArtImporter.Sprite(art);
            renderer.sharedMaterial=SpriteMaterial;
            renderer.sortingOrder=order;
            return renderer;
        }

        internal static void Shadow(Transform parent,float width,float height)
        {
            var shadow=Sprite("Contact Shadow","contact_shadow",parent,new Vector2(.12f,-.35f),1,WorldSorting.Shadow);
            shadow.transform.localScale=new Vector3(width/2.56f,height/1.28f,1);
        }

        internal static void HealthBar(Health health,float y,bool persistent,float width=1.5f)
        {
            var root=Group("Health Bar",health.transform,0,y);
            var bar=root.gameObject.AddComponent<WorldHealthBar>();
            bar.Health=health;bar.AlwaysVisible=persistent;
            var view=Group("View",root);bar.View=view.gameObject;
            var track=Sprite("Track","white",view,Vector2.zero,1,WorldSorting.Overlay);
            track.transform.localScale=new Vector3(width/.08f,.17f/.08f,1);
            track.color=new Color32(26,38,29,255);
            var fill=Sprite("Fill","white",view,Vector2.zero,1,WorldSorting.Overlay+1);
            fill.transform.localScale=new Vector3((width-.12f)/.08f,.085f/.08f,1);
            fill.color=persistent ? new Color32(175,183,109,255) : new Color32(185,134,85,255);
            bar.Fill=fill.transform;
            if(!persistent) view.gameObject.SetActive(false);
        }

        internal static void Feedback(Health health,Transform visual,HitReaction reaction)
        {
            var feedback=health.gameObject.AddComponent<HitFeedback>();
            feedback.Health=health;feedback.Visual=visual;feedback.Reaction=reaction;
        }

        internal static GameObject SavePrefab(Transform root,string name)
        {
            var position=root.position;
            var parent=root.parent;
            root.SetParent(null,true);root.position=Vector3.zero;
            var prefab=PrefabUtility.SaveAsPrefabAsset(root.gameObject,"Assets/Wildfolk/Prefabs/"+name+".prefab");
            Object.DestroyImmediate(root.gameObject);
            var instance=(GameObject)PrefabUtility.InstantiatePrefab(prefab,parent);
            instance.transform.position=position;
            PrefabUtility.RecordPrefabInstancePropertyModifications(instance.transform);
            return instance;
        }
    }
}
