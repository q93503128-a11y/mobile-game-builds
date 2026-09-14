using UnityEngine;

namespace StackfallMobile.Runtime.Player
{
    public sealed class MobilePlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 4.8f;
        [SerializeField] private Vector2 worldHalfExtents = new(4.6f, 8.2f);
        [SerializeField] private float touchRadiusPixels = 110f;

        private int _activeFingerId = -1;
        private Vector2 _touchOrigin;
        private Vector2 _moveInput;

        public Vector2 MoveInput => _moveInput;
        public float MoveSpeed => moveSpeed;

        private void Update()
        {
            ReadInput();
            Move(Time.deltaTime);
        }

        private void ReadInput()
        {
            if (Input.touchSupported && Input.touchCount > 0)
            {
                ReadTouchInput();
                return;
            }

            var horizontal = 0f;
            var vertical = 0f;

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) horizontal -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) horizontal += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) vertical -= 1f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) vertical += 1f;

            _moveInput = Vector2.ClampMagnitude(new Vector2(horizontal, vertical), 1f);
        }

        private void ReadTouchInput()
        {
            for (var i = 0; i < Input.touchCount; i++)
            {
                var touch = Input.GetTouch(i);

                if (_activeFingerId < 0 && touch.phase == TouchPhase.Began)
                {
                    _activeFingerId = touch.fingerId;
                    _touchOrigin = touch.position;
                }

                if (touch.fingerId != _activeFingerId)
                {
                    continue;
                }

                if (touch.phase is TouchPhase.Ended or TouchPhase.Canceled)
                {
                    _activeFingerId = -1;
                    _moveInput = Vector2.zero;
                    return;
                }

                var delta = touch.position - _touchOrigin;
                _moveInput = Vector2.ClampMagnitude(delta / Mathf.Max(1f, touchRadiusPixels), 1f);
                return;
            }

            if (_activeFingerId >= 0)
            {
                _activeFingerId = -1;
                _moveInput = Vector2.zero;
            }
        }

        private void Move(float deltaTime)
        {
            var next = (Vector2)transform.position + _moveInput * (moveSpeed * deltaTime);
            next.x = Mathf.Clamp(next.x, -worldHalfExtents.x, worldHalfExtents.x);
            next.y = Mathf.Clamp(next.y, -worldHalfExtents.y, worldHalfExtents.y);
            transform.position = next;
        }
    }
}
