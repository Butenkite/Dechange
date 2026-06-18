using System;
using UnityEngine;

namespace Dechange
{
    /// <summary>
    /// Draws a planet's Keplerian orbital ellipse as a LineRenderer.
    /// Precomputes the ellipse shape once; each frame shifts it to follow the parent body.
    /// </summary>
    public class OrbitRing : MonoBehaviour
    {
        private LineRenderer _lr;
        private Vector3[] _localPoints;
        private BodyRenderer _parentRenderer;
        private OrbitalElements _orbit;

        private const int Segments = 256;

        public void Initialize(OrbitalElements orbit, BodyRenderer parentRenderer, Color color)
        {
            _orbit = orbit;
            _parentRenderer = parentRenderer;

            ScaleService.Instance.OnScaleModeChanged += RebuildPoints;

            _lr = gameObject.AddComponent<LineRenderer>();
            _lr.useWorldSpace = true;
            _lr.loop = true;
            _lr.positionCount = Segments;
            _lr.startWidth = 0.08f;
            _lr.endWidth   = 0.08f;
            _lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _lr.receiveShadows = false;

            // Dim, slightly transparent version of the planet's color
            Color ringColor = new Color(color.r * 0.6f, color.g * 0.6f, color.b * 0.6f, 0.45f);
            var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            if (mat.shader.name == "Hidden/InternalErrorShader")
                mat = new Material(Shader.Find("Sprites/Default")); // fallback
            mat.color = ringColor;
            _lr.material = mat;

            BuildLocalPoints(orbit);
        }

        void OnDestroy()
        {
            if (ScaleService.Instance != null)
                ScaleService.Instance.OnScaleModeChanged -= RebuildPoints;
        }

        private void RebuildPoints() => BuildLocalPoints(_orbit);

        private void BuildLocalPoints(OrbitalElements orbit)
        {
            _localPoints = new Vector3[Segments];

            double a = orbit.aKm;
            double e = orbit.e;
            double b = a * Math.Sqrt(1.0 - e * e); // semi-minor axis

            double iRad    = orbit.iDeg            * Math.PI / 180.0;
            double bigO    = orbit.lonAscNodeDeg   * Math.PI / 180.0;
            double w       = orbit.argPeriapsisDeg * Math.PI / 180.0;

            double cosBigO = Math.Cos(bigO), sinBigO = Math.Sin(bigO);
            double cosI    = Math.Cos(iRad),  sinI   = Math.Sin(iRad);
            double cosW    = Math.Cos(w),      sinW   = Math.Sin(w);

            for (int i = 0; i < Segments; i++)
            {
                double E = 2.0 * Math.PI * i / Segments; // eccentric anomaly sweep

                // Position in orbital plane (perifocal)
                double xOrb = a * Math.Cos(E);
                double yOrb = b * Math.Sin(E);

                // Rotate to ecliptic J2000 → Unity Y-up (same as OrbitSolver)
                double rx = (cosBigO * cosW - sinBigO * sinW * cosI) * xOrb
                          + (-cosBigO * sinW - sinBigO * cosW * cosI) * yOrb;
                double ry = (sinBigO * cosW + cosBigO * sinW * cosI) * xOrb
                          + (-sinBigO * sinW + cosBigO * cosW * cosI) * yOrb;
                double rz = (sinW * sinI) * xOrb + (cosW * sinI) * yOrb;

                // Ecliptic Y → Unity Z
                var kmPos = new Vector3d(rx, rz, ry);
                _localPoints[i] = ScaleService.Instance.PositionToRender(kmPos);
            }
        }

        void Update()
        {
            if (_localPoints == null || _lr == null) return;

            Vector3 parentPos = _parentRenderer != null
                ? _parentRenderer.transform.position
                : Vector3.zero;

            var worldPoints = new Vector3[Segments];
            for (int i = 0; i < Segments; i++)
                worldPoints[i] = _localPoints[i] + parentPos;

            _lr.SetPositions(worldPoints);
        }
    }
}
