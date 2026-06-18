using System;

namespace Dechange
{
    /// <summary>
    /// Solves Keplerian orbits. Mean motion is derived from parent mass, not
    /// hardcoded for Sol — correct for any star.
    /// </summary>
    public static class OrbitSolver
    {
        // km³ kg⁻¹ s⁻²
        public const double G = 6.674e-20;

        private const double TwoPi = Math.PI * 2.0;

        /// <summary>
        /// Returns the body's position in km relative to its parent, in the
        /// ecliptic J2000 reference frame mapped to Unity's Y-up coordinate system.
        /// </summary>
        public static Vector3d SolvePosition(OrbitalElements orbit, double parentMassKg, double julianDate)
        {
            // Seconds since J2000 epoch
            double tSeconds = (julianDate - 2451545.0) * 86400.0;

            double a = orbit.aKm;
            double e = orbit.e;
            double mu = G * parentMassKg;        // gravitational parameter km³/s²
            double n = Math.Sqrt(mu / (a * a * a)); // mean motion rad/s

            double M0 = orbit.meanAnomalyDeg * Math.PI / 180.0;
            double M = M0 + n * tSeconds;
            M = Mod(M, TwoPi);

            double E = SolveKepler(M, e);

            // True anomaly via half-angle formula
            double nu = 2.0 * Math.Atan2(
                Math.Sqrt(1.0 + e) * Math.Sin(E / 2.0),
                Math.Sqrt(1.0 - e) * Math.Cos(E / 2.0)
            );

            // Radius
            double r = a * (1.0 - e * Math.Cos(E));

            // Position in the orbital plane (perifocal frame)
            double xOrb = r * Math.Cos(nu);
            double yOrb = r * Math.Sin(nu);

            // Rotate to ecliptic J2000 reference frame
            double iRad = orbit.iDeg * Math.PI / 180.0;
            double bigO = orbit.lonAscNodeDeg * Math.PI / 180.0;
            double w = orbit.argPeriapsisDeg * Math.PI / 180.0;

            double cosBigO = Math.Cos(bigO), sinBigO = Math.Sin(bigO);
            double cosI = Math.Cos(iRad), sinI = Math.Sin(iRad);
            double cosW = Math.Cos(w), sinW = Math.Sin(w);

            // Standard rotation: perifocal → ecliptic
            double rx = (cosBigO * cosW - sinBigO * sinW * cosI) * xOrb
                      + (-cosBigO * sinW - sinBigO * cosW * cosI) * yOrb;
            double ry = (sinBigO * cosW + cosBigO * sinW * cosI) * xOrb
                      + (-sinBigO * sinW + cosBigO * cosW * cosI) * yOrb;
            double rz = (sinW * sinI) * xOrb + (cosW * sinI) * yOrb;

            // Ecliptic is XZ-plane; map to Unity Y-up: ecliptic Y → Unity Z
            return new Vector3d(rx, rz, ry);
        }

        // Newton-Raphson solve for Kepler's equation M = E - e·sin(E)
        private static double SolveKepler(double M, double e, int maxIter = 50, double tol = 1e-10)
        {
            double E = M;
            for (int i = 0; i < maxIter; i++)
            {
                double dE = (M - E + e * Math.Sin(E)) / (1.0 - e * Math.Cos(E));
                E += dE;
                if (Math.Abs(dE) < tol) break;
            }
            return E;
        }

        private static double Mod(double x, double m) => ((x % m) + m) % m;
    }
}
