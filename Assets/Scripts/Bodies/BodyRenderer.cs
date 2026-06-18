using System.Collections.Generic;
using UnityEngine;

namespace Dechange
{
    /// <summary>
    /// Drives a body's Unity transform from the orbital solution each frame.
    /// World position = recursive sum up the parent chain, so moons work automatically.
    /// </summary>
    [RequireComponent(typeof(MeshRenderer))]
    public class BodyRenderer : MonoBehaviour
    {
        private BodyDefinition _body;
        private BodyRenderer _parentRenderer;
        private MeshRenderer _meshRenderer;

        public BodyDefinition Definition => _body;

        public void Initialize(BodyDefinition body, Dictionary<string, BodyRenderer> allRenderers)
        {
            _body = body;

            if (!string.IsNullOrEmpty(body.parent))
                allRenderers.TryGetValue(body.parent, out _parentRenderer);

            _meshRenderer = GetComponent<MeshRenderer>();
            ApplyVisuals();
            ScaleService.Instance.OnScaleModeChanged += ApplyVisuals;
        }

        void OnDestroy()
        {
            if (ScaleService.Instance != null)
                ScaleService.Instance.OnScaleModeChanged -= ApplyVisuals;
        }

        void Update()
        {
            if (_body == null || SimClock.Instance == null || ScaleService.Instance == null) return;
            transform.position = ScaleService.Instance.PositionToRender(WorldPositionKm);
        }

        /// <summary>
        /// Position in km relative to the system root, computed recursively up the parent chain.
        /// </summary>
        public Vector3d WorldPositionKm
        {
            get
            {
                if (_body.IsSystemRoot) return Vector3d.Zero;

                Vector3d local = OrbitSolver.SolvePosition(
                    _body.orbit,
                    _body.parent != null && _parentRenderer != null
                        ? _parentRenderer.Definition.physical.massKg
                        : 1.989e30, // fallback to Sol mass
                    SimClock.Instance.JulianDate
                );

                return (_parentRenderer != null ? _parentRenderer.WorldPositionKm : Vector3d.Zero) + local;
            }
        }

        private void ApplyVisuals()
        {
            if (ScaleService.Instance != null)
            {
                float radius = ScaleService.Instance.RadiusToRender(_body.physical.radiusKm);
                transform.localScale = Vector3.one * radius * 2f;
            }

            if (_body.render?.color != null &&
                ColorUtility.TryParseHtmlString(_body.render.color, out Color color))
            {
                var mat = _meshRenderer.material;
                mat.color = color;

                if (_body.render.emissive)
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", color * 2f);
                }
            }
        }
    }
}
