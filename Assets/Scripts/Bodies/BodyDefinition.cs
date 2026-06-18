using System.Collections.Generic;
using Newtonsoft.Json;

namespace Dechange
{

    public class PhysicalData
    {
        public double radiusKm;
        public double massKg;
        public double rotationPeriodH;  // negative = retrograde
        public double axialTiltDeg;
        public double albedo;
    }

    public class OrbitalElements
    {
        public double aKm;              // semi-major axis
        public double e;                // eccentricity
        public double iDeg;             // inclination
        public double lonAscNodeDeg;    // longitude of ascending node (Ω)
        public double argPeriapsisDeg;  // argument of periapsis (ω)
        public double meanAnomalyDeg;   // mean anomaly at epoch (M₀)
        public string epoch;            // "J2000"
    }

    public class RenderHints
    {
        public string texture;
        public string color;
        public bool emissive;
        public bool hasRings;
    }

    public class BodyData
    {
        public double meanTempK;
        public double minTempK;
        public double maxTempK;
        public double? surfacePressureBar;
        public Dictionary<string, float> atmosphere;
        public string weather;
    }

    public class BodyDefinition
    {
        public string id;
        public string name;
        public string type;     // star | planet | dwarf | moon | asteroid | comet
        public string parent;   // null for system roots
        public PhysicalData physical;
        public OrbitalElements orbit;
        public RenderHints render;
        public BodyData data;
        public string[] modules;

        public bool HasModule(string module)
        {
            if (modules == null) return false;
            foreach (var m in modules)
                if (m == module) return true;
            return false;
        }

        public bool IsSystemRoot => string.IsNullOrEmpty(parent);
    }

    public class SystemDefinition
    {
        public string id;
        public string name;
        public List<BodyDefinition> bodies;
    }

    public class SystemEntry
    {
        public string id;
        public string name;
        public string file;
    }

    public class CatalogDefinition
    {
        public List<SystemEntry> systems;

        [JsonProperty("default")]
        public string DefaultSystem;
    }
}
