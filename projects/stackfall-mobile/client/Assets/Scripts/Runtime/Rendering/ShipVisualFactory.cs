using UnityEngine;
using UnityEngine.UIElements;

namespace StackfallMobile.Runtime.Rendering
{
    public static class ShipVisualFactory
    {
        public static void BuildCombatShip(Transform root)
        {
            CreateSprite(root, "ReactorGlow", RuntimeSpriteFactory.Circle, new Color(0.2f, 0.85f, 1f, 0.24f), new Vector2(0.78f, 0.78f), new Vector2(0f, -0.04f), 3);
            CreateSprite(root, "Frame", RuntimeSpriteFactory.Square, new Color(0.14f, 0.45f, 0.66f, 1f), new Vector2(0.72f, 0.72f), Vector2.zero, 4, 45f);
            CreateSprite(root, "Core", RuntimeSpriteFactory.Circle, new Color(0.45f, 0.95f, 1f, 1f), new Vector2(0.34f, 0.34f), new Vector2(0f, 0.04f), 5);
            CreateSprite(root, "Impactor", RuntimeSpriteFactory.Square, new Color(0.72f, 0.9f, 1f, 1f), new Vector2(0.19f, 0.48f), new Vector2(0f, 0.49f), 4);
            CreateSprite(root, "DriveLeft", RuntimeSpriteFactory.Square, new Color(0.2f, 0.72f, 1f, 1f), new Vector2(0.17f, 0.38f), new Vector2(-0.31f, -0.46f), 4, -14f);
            CreateSprite(root, "DriveRight", RuntimeSpriteFactory.Square, new Color(0.2f, 0.72f, 1f, 1f), new Vector2(0.17f, 0.38f), new Vector2(0.31f, -0.46f), 4, 14f);
            CreateSprite(root, "OrbiterLeft", RuntimeSpriteFactory.Circle, new Color(0.95f, 0.72f, 0.2f, 1f), new Vector2(0.17f, 0.17f), new Vector2(-0.63f, 0.04f), 5);
            CreateSprite(root, "OrbiterRight", RuntimeSpriteFactory.Circle, new Color(0.95f, 0.72f, 0.2f, 1f), new Vector2(0.17f, 0.17f), new Vector2(0.63f, 0.04f), 5);
        }

        public static VisualElement BuildUiShip(float size)
        {
            var holder = new VisualElement();
            holder.style.width = size;
            holder.style.height = size;
            holder.style.position = Position.Relative;

            AddUiPart(holder, size * 0.16f, size * 0.16f, size * 0.11f, size * 0.42f, new Color(0.95f, 0.72f, 0.2f, 1f), true);
            AddUiPart(holder, size * 0.68f, size * 0.16f, size * 0.11f, size * 0.42f, new Color(0.95f, 0.72f, 0.2f, 1f), true);
            AddUiPart(holder, size * 0.25f, size * 0.73f, size * 0.16f, size * 0.21f, new Color(0.2f, 0.72f, 1f, 1f));
            AddUiPart(holder, size * 0.59f, size * 0.73f, size * 0.16f, size * 0.21f, new Color(0.2f, 0.72f, 1f, 1f));
            AddUiPart(holder, size * 0.36f, size * 0.09f, size * 0.28f, size * 0.26f, new Color(0.72f, 0.9f, 1f, 1f));
            AddUiPart(holder, size * 0.24f, size * 0.25f, size * 0.52f, size * 0.52f, new Color(0.14f, 0.45f, 0.66f, 1f), true);
            AddUiPart(holder, size * 0.38f, size * 0.39f, size * 0.24f, size * 0.24f, new Color(0.45f, 0.95f, 1f, 1f), true);
            return holder;
        }

        private static void CreateSprite(
            Transform parent,
            string name,
            Sprite sprite,
            Color color,
            Vector2 scale,
            Vector2 localPosition,
            int sortingOrder,
            float rotation = 0f)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent, false);
            child.transform.localPosition = localPosition;
            child.transform.localScale = new Vector3(scale.x, scale.y, 1f);
            child.transform.localRotation = Quaternion.Euler(0f, 0f, rotation);
            var renderer = child.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
        }

        private static void AddUiPart(VisualElement parent, float left, float top, float width, float height, Color color, bool round = false)
        {
            var part = new VisualElement();
            part.style.position = Position.Absolute;
            part.style.left = left;
            part.style.top = top;
            part.style.width = width;
            part.style.height = height;
            part.style.backgroundColor = color;
            if (round)
            {
                var radius = Mathf.Min(width, height) * 0.5f;
                part.style.borderTopLeftRadius = radius;
                part.style.borderTopRightRadius = radius;
                part.style.borderBottomLeftRadius = radius;
                part.style.borderBottomRightRadius = radius;
            }
            else
            {
                part.style.borderTopLeftRadius = 8;
                part.style.borderTopRightRadius = 8;
                part.style.borderBottomLeftRadius = 8;
                part.style.borderBottomRightRadius = 8;
            }

            parent.Add(part);
        }
    }
}
