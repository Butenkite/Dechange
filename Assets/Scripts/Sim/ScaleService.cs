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

        // Visual mode: log-compressed distances
        // renderDist = distScale * ln(1 + realKm / distRef)
        [SerializeField] private float _visualDistScale = 20f;
        [SerializeField] private float _visualDistRefKm = 1.5e7f; // 0.1 AU

        // Body sizes in visual mode
        [SerializeField] private float _visualSizeKmPerUnit = 50000f;
        [SerializeField] private float _minRenderedRadius = 0.15f;

        // True scale: 1 Unity unit = N km
        [SerializeField] private float _trueDistKmPerUnit = 3e6f; // 1 AU ≈ 50 units
        [SerializeField] private float _trueSizeKmPerUnit = 50000f;

        public event Action OnScaleModeChanged;

        public ScaleMode Mode
        {
            get => _mode;
            set
            {
                if (_mode == value) return;
                _mode = value;
                OnScaleModeChanged?.Invoke();
            }
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        /// <summary>Maps a real position in km (relative to system root) to a Unity world position.</summary>
        public Vector3 PositionToRender(Vector3d realKm)
        {
            if (_mode == ScaleMode.True)
            {
                float s = 1f / _trueDistKmPerUnit;
                return new Vector3((float)realKm.x * s, (float)realKm.y * s, (float)realKm.z * s);
            }

            double dist = realKm.Magnitude;
            if (dist < 1e-6) return Vector3.zero;
            double renderDist = _visualDistScale * Math.Log(1.0 + dist / _visualDistRefKm);
            float scale = (float)(renderDist / dist);
            return new Vector3((float)realKm.x * scale, (float)realKm.y * scale, (float)realKm.z * scale);
        }

        /// <summary>Maps a real radius in km to a Unity render radius.</summary>
        public float RadiusToRender(double realRadiusKm)
        {
            float kmPerUnit = _mode == ScaleMode.True ? _trueSizeKmPerUnit : _visualSizeKmPerUnit;
            return Mathf.Max((float)(realRadiusKm / kmPerUnit), _minRenderedRadius);
        }

        public void ToggleMode() => Mode = _mode == ScaleMode.Visual ? ScaleMode.True : ScaleMode.Visual;
    }
}
