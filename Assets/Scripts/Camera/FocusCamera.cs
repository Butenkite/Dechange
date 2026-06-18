using UnityEngine;
using UnityEngine.InputSystem;

namespace Dechange
{
    /// <summary>
    /// Orbit camera with click-to-focus. Default target is the system origin.
    /// Click a body to switch focus; ESC to return to free view.
    /// </summary>
    public class FocusCamera : MonoBehaviour
    {
        public static FocusCamera Instance { get; private set; }

        [Header("Orbit")]
        [SerializeField] private float _distance = 1400f;
        [SerializeField] private float _minDistance = 0.5f;
        [SerializeField] private float _maxDistance = 6000f;
        [SerializeField] private float _mouseSensitivity = 0.3f;
        [SerializeField] private float _scrollSensitivity = 0.12f;

        [Header("Focus transition")]
        [SerializeField] private float _focusLerpSpeed = 5f;

        private float _yaw = 20f;
        private float _pitch = 25f;
        private Vector3 _orbitCenter;
        private BodyRenderer _focusTarget;
        private Camera _cam;
        private Vector2 _clickStartPos;

        // Desired orbit center — lerped toward each frame
        private Vector3 _desiredCenter;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            _cam = GetComponent<Camera>();
            if (_cam == null) _cam = Camera.main;
            _orbitCenter = Vector3.zero;
            _desiredCenter = Vector3.zero;
            ApplyTransform();
        }

        void Update()
        {
            HandleInput();

            // Track focus target as it moves
            if (_focusTarget != null)
                _desiredCenter = _focusTarget.transform.position;

            // Smooth transition to new center
            _orbitCenter = Vector3.Lerp(_orbitCenter, _desiredCenter, Time.deltaTime * _focusLerpSpeed);

            ApplyTransform();
        }

        private void HandleInput()
        {
            var mouse = Mouse.current;
            var keyboard = Keyboard.current;

            if (mouse == null) return;

            // Rotate — left mouse drag
            if (mouse.leftButton.isPressed)
            {
                var delta = mouse.delta.ReadValue();
                _yaw   += delta.x * _mouseSensitivity;
                _pitch -= delta.y * _mouseSensitivity;
                _pitch  = Mathf.Clamp(_pitch, -89f, 89f);
            }

            // Zoom — scroll wheel
            float scroll = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.001f)
            {
                _distance *= 1f - scroll * _scrollSensitivity;
                _distance  = Mathf.Clamp(_distance, _minDistance, _maxDistance);
            }

            // Click to focus — only on press, drag is handled by isPressed above
            if (mouse.leftButton.wasPressedThisFrame)
                _clickStartPos = mouse.position.ReadValue();

            if (mouse.leftButton.wasReleasedThisFrame)
            {
                // Only treat as a click if the mouse barely moved (not a drag)
                if ((_clickStartPos - mouse.position.ReadValue()).magnitude < 5f)
                    TryFocusAtScreenPoint(mouse.position.ReadValue());
            }

            // ESC — return to system view
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
                ClearFocus();
        }

        private void TryFocusAtScreenPoint(Vector2 screenPos)
        {
            Ray ray = _cam.ScreenPointToRay(new Vector3(screenPos.x, screenPos.y, 0));
            float bestDist = float.MaxValue;
            BodyRenderer bestHit = null;

            // Sphere-cast against each body's rendered radius
            foreach (var r in SystemLoader.Instance.GetAllRenderers())
            {
                Vector3 toBody = r.transform.position - ray.origin;
                float radius = r.transform.localScale.x * 0.5f * 1.5f; // 1.5× pick tolerance
                float proj = Vector3.Dot(toBody, ray.direction);
                if (proj < 0) continue;
                Vector3 closest = ray.origin + ray.direction * proj;
                if ((closest - r.transform.position).magnitude < radius && proj < bestDist)
                {
                    bestDist = proj;
                    bestHit = r;
                }
            }

            if (bestHit != null)
                SetFocus(bestHit);
        }

        public void SetFocus(BodyRenderer target)
        {
            _focusTarget = target;
            // Pull distance in to a sensible starting orbit
            float bodyRadius = target.transform.localScale.x * 0.5f;
            _distance = Mathf.Clamp(bodyRadius * 8f, _minDistance, _maxDistance);
        }

        public void ClearFocus()
        {
            _focusTarget = null;
            _desiredCenter = Vector3.zero;
        }

        /// <summary>Called by FloatingOrigin when the world is rebased.</summary>
        public void OnOriginShift(Vector3 offset)
        {
            _orbitCenter  -= offset;
            _desiredCenter -= offset;
        }

        private void ApplyTransform()
        {
            Quaternion rot = Quaternion.Euler(_pitch, _yaw, 0f);
            transform.position = _orbitCenter + rot * new Vector3(0f, 0f, -_distance);
            transform.LookAt(_orbitCenter, Vector3.up);
        }
    }
}
