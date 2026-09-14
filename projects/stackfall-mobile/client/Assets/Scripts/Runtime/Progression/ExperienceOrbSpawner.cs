using StackfallMobile.Runtime.Rendering;
using UnityEngine;
using UnityEngine.Pool;

namespace StackfallMobile.Runtime.Progression
{
    public sealed class ExperienceOrbSpawner : MonoBehaviour
    {
        private ObjectPool<ExperienceOrb> _pool;
        private Transform _player;
        private PlayerProgression _progression;

        public void Initialize(Transform player, PlayerProgression progression)
        {
            _player = player;
            _progression = progression;
            _pool = new ObjectPool<ExperienceOrb>(
                CreateOrb,
                orb => orb.gameObject.SetActive(true),
                orb => orb.gameObject.SetActive(false),
                orb => Destroy(orb.gameObject),
                false,
                32,
                256);
        }

        public void Spawn(Vector2 position, int amount)
        {
            if (_pool == null)
            {
                return;
            }

            var orb = _pool.Get();
            orb.Configure(position, amount, _player, _progression, Release);
        }

        private ExperienceOrb CreateOrb()
        {
            var gameObject = new GameObject("ExperienceOrb");
            gameObject.transform.SetParent(transform, false);
            var renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeSpriteFactory.Circle;
            renderer.color = new Color(0.35f, 0.95f, 1f, 1f);
            renderer.sortingOrder = 4;
            gameObject.transform.localScale = Vector3.one * 0.18f;
            return gameObject.AddComponent<ExperienceOrb>();
        }

        private void Release(ExperienceOrb orb)
        {
            _pool?.Release(orb);
        }
    }
}
