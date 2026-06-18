using System;
using UnityEngine;

namespace Dechange
{
    /// <summary>
    /// Converts real km positions and radii to Unity render units.
    /// This is the ONLY place in the codebase that knows the render scale.
    /// System-agnostic — operates on positions relative to the active system root.
    /// </summary>
    public class ScaleService : MonoBehaviour
    {
        public static ScaleService Instance { get; private set; }

        public enum ScaleMode { Visual, True }

        [SerializeField] private ScaleMode _mode = ScaleMode.Visual;

        // --- Visual mode: log-compressed distances ---
        // renderDist = distScale * ln(1 + realKm / distRef)
        [SerializeField] private float _visualDistScale = 20f;
        [SerializeField] private float _visualDistRefKm = 1.5e7f; // 0.1 AU

        // Visual mode body sizes: realRadius / sizeKmPerUnit, min clamp
        [SerializeField] private float _visualSizeKmPerUnit = 100000f;
        [SerializeField] private float _minRenderedRadius = 0.2f;

        // --- True scale ---
        [SerializeField] private float _trueScaleKmPerUnit = 1e6f; // 1 unit = 1,000,000 km

        public ScaleMode Mode
        {
            get => _mode;
            set => _mode = value;
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        /// <summary>
        /// Maps a real position in km (relative to system root) to a Unity world position.
        /// </summary>
        public Vector3 PositionToRender(Vector3d realKm)
        {
            if (_mode == ScaleMode.True)
                return new Vector3(
                    (float)(realKm.x / _trueScaleKmPerUnit),
                    (float)(realKm.y / _trueScaleKmPerUnit),
                    (float)(realKm.z / _trueScaleKmPerUnit));

            double dist = realKm.Magnitude;
            if (dist < 1e-6) return Vector3.zero;

            double renderDist = _visualDistScale * Math.Log(1.0 + dist / _visualDistRefKm);
            float scale = (float)(renderDist / dist);
            return new Vector3((float)realKm.x * scale, (float)realKm.y * scale, (float)realKm.z * scale);
        }

        /// <summary>
        /// Maps a real radius in km to a Unity diameter (applied as localScale).
        /// </summary>
        public float RadiusToRender(double realRadiusKm)
        {
            if (_mode == ScaleMode.True)
                return (float)(realRadiusKm / _trueScaleKmPerUnit);

            return Mathf.Max((float)(realRadiusKm / _visualSizeKmPerUnit), _minRenderedRadius);
        }

        public void ToggleMode()
        {
            _mode = _mode == ScaleMode.Visual ? ScaleMode.True : ScaleMode.Visual;
        }
    }
}
