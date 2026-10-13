using UnityEngine;

namespace Wildfolk
{
    public sealed class WorldHealthBar : MonoBehaviour
    {
        public Health Health;
        public GameObject View;
        public Transform Fill;
        public bool AlwaysVisible;
        private Vector3 fullScale;
        private float fillWidth;

        private void Awake()
        {
            fullScale = Fill.localScale;
            fillWidth = Fill.GetComponent<SpriteRenderer>().sprite.bounds.size.x * fullScale.x;
        }

        private void OnEnable() => Health.Changed += Refresh;
        private void OnDisable() => Health.Changed -= Refresh;
        private void Start() => Refresh();

        private void Refresh()
        {
            View.SetActive(Health.IsAlive && (AlwaysVisible || Health.Current < Health.Maximum));
            Fill.localScale = new Vector3(fullScale.x * Health.Ratio, fullScale.y, fullScale.z);
            Fill.localPosition = new Vector3(-fillWidth * .5f + fillWidth * Health.Ratio * .5f, 0, 0);
        }
    }
}
