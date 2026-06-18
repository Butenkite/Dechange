using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Dechange
{
    /// <summary>
    /// Reads catalog.json, loads the active system file, and instantiates a
    /// BodyRenderer for each body. Switching systems = call LoadSystem with a
    /// different id — no code changes needed.
    /// </summary>
    public class SystemLoader : MonoBehaviour
    {
        public static SystemLoader Instance { get; private set; }

        [SerializeField] private string _catalogFile = "catalog.json";
        [SerializeField] private GameObject _bodyPrefab;

        private readonly Dictionary<string, BodyDefinition> _bodyDefs = new();
        private readonly Dictionary<string, BodyRenderer> _renderers = new();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            StartCoroutine(LoadDefaultSystem());
        }

        public BodyRenderer GetRenderer(string id) =>
            _renderers.TryGetValue(id, out var r) ? r : null;

        public IReadOnlyDictionary<string, BodyDefinition> Bodies => _bodyDefs;

        private IEnumerator LoadDefaultSystem()
        {
            yield return null; // wait one frame for singletons

            string catalogPath = DataPath(_catalogFile);
            if (!File.Exists(catalogPath))
            {
                Debug.LogError($"[SystemLoader] catalog.json not found at: {catalogPath}");
                yield break;
            }

            var catalog = JsonConvert.DeserializeObject<CatalogDefinition>(File.ReadAllText(catalogPath));
            var entry = catalog.systems.Find(s => s.id == catalog.DefaultSystem) ?? catalog.systems[0];

            yield return LoadSystem(entry.file);
        }

        private IEnumerator LoadSystem(string relativeFile)
        {
            foreach (var r in _renderers.Values)
                if (r != null) Destroy(r.gameObject);
            _renderers.Clear();
            _bodyDefs.Clear();

            string path = DataPath(relativeFile);
            if (!File.Exists(path))
            {
                Debug.LogError($"[SystemLoader] System file not found at: {path}");
                yield break;
            }

            var system = JsonConvert.DeserializeObject<SystemDefinition>(File.ReadAllText(path));

            foreach (var body in system.bodies)
                _bodyDefs[body.id] = body;

            foreach (var body in system.bodies)
            {
                var go = Instantiate(_bodyPrefab, transform);
                go.name = body.name;
                _renderers[body.id] = go.AddComponent<BodyRenderer>();
            }

            // Two-pass init — every renderer must exist before parent lookups
            foreach (var (id, renderer) in _renderers)
                renderer.Initialize(_bodyDefs[id], _renderers);
        }

        private static string DataPath(string relative) =>
            Path.Combine(Application.streamingAssetsPath, "data", relative);
    }
}
