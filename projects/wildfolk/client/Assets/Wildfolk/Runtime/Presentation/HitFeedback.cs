using UnityEngine;

namespace Wildfolk
{
    public enum HitReaction { Creature, Tree, Stone }

    public sealed class HitFeedback : MonoBehaviour,IPoolReset
    {
        public Health Health;
        public Transform Visual;
        public HitReaction Reaction;
        private SpriteRenderer[] renderers;
        private Color[] colors;
        private Vector3 restPosition;
        private Vector3 restScale;
        private Quaternion restRotation;
        private float elapsed = 1;
        private const float Duration = .16f;

        private void Awake()
        {
            restPosition = Visual.localPosition;
            restScale = Visual.localScale;
            restRotation = Visual.localRotation;
            renderers = Visual.GetComponentsInChildren<SpriteRenderer>();
            colors = new Color[renderers.Length];
            for (var i = 0; i < renderers.Length; i++) colors[i] = renderers[i].color;
        }

        private void OnEnable() => Health.Damaged += React;
        private void OnDisable() => Health.Damaged -= React;
        public void ResetForSpawn(){elapsed=Duration;Visual.localPosition=restPosition;Visual.localScale=restScale;Visual.localRotation=restRotation;for(var i=0;i<renderers.Length;i++)renderers[i].color=colors[i];}
        private void React() => elapsed = 0;

        private void Update()
        {
            if (elapsed >= Duration) return;
            elapsed = Mathf.Min(elapsed + Time.deltaTime, Duration);
            var t = elapsed / Duration;
            var damping = 1 - t;
            if (Reaction == HitReaction.Creature)
            {
                var squash = Mathf.Sin(t * Mathf.PI) * .10f;
                Visual.localScale = new Vector3(restScale.x * (1 + squash), restScale.y * (1 - squash), restScale.z);
                for (var i = 0; i < renderers.Length; i++) renderers[i].color = Color.Lerp(colors[i], new Color(1,.89f,.74f,1), damping);
            }
            else
            {
                var tree = Reaction == HitReaction.Tree;
                Visual.localPosition = restPosition + Vector3.right * (Mathf.Sin(t * (tree ? 15 : 25)) * damping * (tree ? .075f : .038f));
                Visual.localRotation = restRotation * Quaternion.Euler(0, 0, Mathf.Sin(t * 16) * damping * (tree ? 3 : .7f));
            }
            if (elapsed < Duration) return;
            Visual.localPosition = restPosition;
            Visual.localScale = restScale;
            Visual.localRotation = restRotation;
            for (var i = 0; i < renderers.Length; i++) renderers[i].color = colors[i];
        }
    }
}
