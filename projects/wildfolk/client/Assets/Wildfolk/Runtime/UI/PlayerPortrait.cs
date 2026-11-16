using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Wildfolk
{
    [DefaultExecutionOrder(200)]
    public sealed class PlayerPortrait : MonoBehaviour
    {
        public PlayerEquipment Equipment;
        public Image Head,Body,LeftHand,RightHand,Utility,Accessory;
        public bool MirrorWorldAppearance;
        public float InventoryBodyWidth=90f;

        private PlayerEquipment boundEquipment;
        private Transform mirroredRoot;
        private SpriteRenderer[] mirroredSources=new SpriteRenderer[0];
        private readonly List<WorldLayer> worldLayers=new List<WorldLayer>();
        private readonly List<Image> renderedImages=new List<Image>();

        private sealed class WorldLayer
        {
            public SpriteRenderer Source;
            public Image Target;
        }

        public IReadOnlyList<Image> RenderedImages=>renderedImages;
        public bool UsesWorldAppearance=>MirrorWorldAppearance;

        private void OnEnable(){BindEquipment(Equipment);}
        private void OnDisable(){UnbindEquipment();}
        private void LateUpdate()
        {
            if(isActiveAndEnabled&&Application.isPlaying&&UsesWorldAppearance)SyncWorldAppearance();
        }

        public void BindEquipment(PlayerEquipment equipment)
        {
            if(boundEquipment==equipment){Equipment=equipment;Refresh();return;}
            UnbindEquipment();
            Equipment=boundEquipment=equipment;
            if(isActiveAndEnabled&&boundEquipment!=null)boundEquipment.Changed+=Refresh;
            mirroredRoot=null;
            Refresh();
        }

        private void UnbindEquipment()
        {
            if(boundEquipment!=null)boundEquipment.Changed-=Refresh;
            boundEquipment=null;
        }

        public void Refresh()
        {
            if(Equipment==null)return;
            if(UsesWorldAppearance)
            {
                MirrorWorldAppearance=true;
                if(Application.isPlaying)SyncWorldAppearance();
                else foreach(var image in GetComponentsInChildren<Image>(true))image.enabled=false;
                return;
            }

            Show(Head,Equipment.HeadArmor==null ? null : Equipment.HeadArmor.Armor.WornSprite);
            Show(Body,Equipment.BodyArmor==null ? null : Equipment.BodyArmor.Armor.WornSprite);
            var hand=Equipment.BodyArmor==null ? null : Equipment.BodyArmor.Armor.HandSprite;
            Show(LeftHand,hand);Show(RightHand,hand);
            var scale=WorldToUiScale();
            RefreshUtility(scale);
            RefreshAccessory(scale);
        }

        public Image ImageFor(SpriteRenderer source)
        {
            if(source==null)return null;
            for(var i=0;i<worldLayers.Count;i++)if(worldLayers[i].Source==source)return worldLayers[i].Target;
            return null;
        }

        private void SyncWorldAppearance()
        {
            var avatar=Equipment==null?null:Equipment.GetComponent<PlayerAvatar>();
            var sourceRoot=avatar==null?null:avatar.Body;
            if(sourceRoot==null)return;
            if(mirroredRoot!=sourceRoot||mirroredSources.Length==0)EnsureWorldLayers(sourceRoot);

            var body=sourceRoot.Find("Body")?.GetComponent<SpriteRenderer>();
            if(body==null||body.sprite==null)return;
            var bodyScale=ScaleRelativeTo(Equipment.transform,body.transform);
            var bodyWidth=body.sprite.bounds.size.x*Mathf.Abs(bodyScale.x);
            if(bodyWidth<=.0001f)return;
            var uiPerWorldUnit=Mathf.Max(1f,InventoryBodyWidth)/bodyWidth;

            for(var i=0;i<worldLayers.Count;i++)
            {
                var layer=worldLayers[i];
                var source=layer.Source;
                var target=layer.Target;
                if(source==null||target==null)continue;

                var sprite=source.sprite;
                target.sprite=sprite;
                target.enabled=sprite!=null&&source.enabled&&source.gameObject.activeInHierarchy;
                target.color=source.color;
                target.raycastTarget=false;
                target.type=Image.Type.Simple;
                target.preserveAspect=false;
                if(sprite==null)continue;

                var relativeScale=ScaleRelativeTo(Equipment.transform,source.transform);
                var rect=target.rectTransform;
                rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);
                rect.pivot=new Vector2(sprite.rect.width>.001f?sprite.pivot.x/sprite.rect.width:.5f,
                    sprite.rect.height>.001f?sprite.pivot.y/sprite.rect.height:.5f);
                var localPosition=Equipment.transform.InverseTransformPoint(source.transform.position);
                rect.anchoredPosition=new Vector2(localPosition.x,localPosition.y)*uiPerWorldUnit;
                rect.sizeDelta=new Vector2(sprite.bounds.size.x*Mathf.Abs(relativeScale.x),
                    sprite.bounds.size.y*Mathf.Abs(relativeScale.y))*uiPerWorldUnit;
                rect.localRotation=Quaternion.Inverse(Equipment.transform.rotation)*source.transform.rotation;
                rect.localScale=new Vector3(Sign(relativeScale.x)*(source.flipX?-1f:1f),
                    Sign(relativeScale.y)*(source.flipY?-1f:1f),Sign(relativeScale.z));
            }
        }

        private void EnsureWorldLayers(Transform sourceRoot)
        {
            var sources=sourceRoot.GetComponentsInChildren<SpriteRenderer>(true).ToList();
            if(Equipment.UtilityVisual!=null&&!sources.Contains(Equipment.UtilityVisual))sources.Add(Equipment.UtilityVisual);
            sources=sources.Where(source=>source!=null).OrderBy(source=>source.sortingOrder).ToList();
            if(mirroredRoot==sourceRoot&&SameSources(sources))return;

            mirroredRoot=sourceRoot;
            mirroredSources=sources.ToArray();
            worldLayers.Clear();
            renderedImages.Clear();
            for(var i=0;i<mirroredSources.Length;i++)
            {
                var source=mirroredSources[i];
                var target=FindTarget(source);
                var layer=new WorldLayer{Source=source,Target=target};
                worldLayers.Add(layer);
                renderedImages.Add(target);
                target.transform.SetSiblingIndex(i);
            }
        }

        private bool SameSources(List<SpriteRenderer> current)
        {
            if(current.Count!=mirroredSources.Length)return false;
            for(var i=0;i<current.Count;i++)if(current[i]!=mirroredSources[i])return false;
            return true;
        }

        private Image FindTarget(SpriteRenderer source)
        {
            var name=Equipment.UtilityVisual==source?"Utility":TargetName(source.name);
            var existing=transform.Find(name);
            var image=existing==null?null:existing.GetComponent<Image>();
            if(image!=null)
            {
                // Placeholder layers can be inactive in the prefab. Enabling only
                // their Image component leaves them outside the rendered hierarchy.
                if(!image.gameObject.activeSelf)image.gameObject.SetActive(true);
                return image;
            }

            var target=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
            target.transform.SetParent(transform,false);
            image=target.GetComponent<Image>();
            image.raycastTarget=false;
            return image;
        }

        private static string TargetName(string sourceName)
        {
            switch(sourceName)
            {
                case "Body":return "Skin";
                case "Armor Socket":return "Body";
                case "Head Gear":return "Head";
                case "Left Hand":return "Left Hand";
                case "Right Hand":return "Right Hand";
                case "Left Eye":return "Left Eye";
                case "Right Eye":return "Right Eye";
                case "Left Hand Gear":return "Left Hand Gear";
                case "Right Hand Gear":return "Right Hand Gear";
                case "Held Tool":return "Held Tool";
                case "Accessory":return "Accessory";
                default:return "World Sprite " + sourceName;
            }
        }

        private static Vector3 ScaleRelativeTo(Transform root,Transform source)
        {
            var scale=Vector3.one;
            for(var current=source;current!=null&&current!=root;current=current.parent)
                scale=Vector3.Scale(scale,current.localScale);
            return scale;
        }

        private float WorldToUiScale()
        {
            var avatar=Equipment.GetComponent<PlayerAvatar>();
            var body=avatar==null||avatar.Body==null ? null : avatar.Body.Find("Body")?.GetComponent<SpriteRenderer>();
            if(body==null||body.sprite==null||Body==null)return 1f;
            var width=body.sprite.bounds.size.x*body.transform.localScale.x;
            return width>.001f ? Body.rectTransform.rect.width/width : 1f;
        }

        private void RefreshUtility(float scale)
        {
            if(Utility==null)return;
            var source=Equipment.UtilityVisual;
            var sprite=Equipment.Utility==null||source==null||!source.enabled ? null : source.sprite;
            Show(Utility,sprite);
            if(sprite==null)return;
            Utility.rectTransform.anchoredPosition=Equipment.transform.InverseTransformPoint(source.transform.position)*scale;
            var rootScale=Equipment.transform.lossyScale;
            var size=new Vector2(source.bounds.size.x/Mathf.Max(Mathf.Abs(rootScale.x),.001f),
                source.bounds.size.y/Mathf.Max(Mathf.Abs(rootScale.y),.001f));
            Utility.rectTransform.sizeDelta=size*scale;
        }

        private void RefreshAccessory(float scale)
        {
            if(Accessory==null)return;
            var definition=Equipment.Accessory==null ? null : Equipment.Accessory.Accessory;
            var sprite=definition==null ? null : definition.Visual;
            Show(Accessory,sprite);
            if(sprite==null)return;
            var rootScale=Equipment.transform.lossyScale;
            var localSize=new Vector2(sprite.bounds.size.x*definition.VisualScale/Mathf.Max(Mathf.Abs(rootScale.x),.001f),
                sprite.bounds.size.y*definition.VisualScale/Mathf.Max(Mathf.Abs(rootScale.y),.001f));
            Accessory.rectTransform.anchoredPosition=AccessoryVisual.AnchorOffset(definition)*scale;
            Accessory.rectTransform.sizeDelta=localSize*scale;
        }

        private static float Sign(float value)=>value<0?-1f:1f;
        private static void Show(Image image,Sprite sprite){if(image==null)return;image.sprite=sprite;image.enabled=sprite!=null;}
    }
}
