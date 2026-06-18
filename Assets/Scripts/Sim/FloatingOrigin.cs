using UnityEngine;

namespace Dechange
{
    /// <summary>
    /// Prevents single-precision float breakdown at solar-system distances.
    /// When the camera drifts beyond the threshold, all body transforms are
    /// rebased and the camera is reset to near-origin. Invisible to the player.
    /// </summary>
    public class FloatingOrigin : MonoBehaviour
    {
        public static FloatingOrigin Instance { get; private set; }

        [SerializeField] private float _rebaseThreshold = 1000f;

        private Camera _cam;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            _cam = Camera.main;
        }

        void LateUpdate()
        {
            if (_cam == null || SystemLoader.Instance == null) return;

            Vector3 offset = _cam.transform.position;
            if (offset.magnitude < _rebaseThreshold) return;

            // Shift every body transform so the camera ends up near origin
            foreach (var renderer in SystemLoader.Instance.GetAllRenderers())
                renderer.transform.position -= offset;

            _cam.transform.position = Vector3.zero;

            // Notify FocusCamera so its orbit state stays consistent
            FocusCamera.Instance?.OnOriginShift(offset);
        }
    }
}
