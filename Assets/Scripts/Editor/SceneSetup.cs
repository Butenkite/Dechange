using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

namespace Dechange.Editor
{
    /// <summary>
    /// Menu item: Dechange > Setup Main Scene
    /// Creates the Main scene with all required GameObjects and wires everything up.
    /// Run this once after first opening the project.
    /// </summary>
    public static class SceneSetup
    {
        [MenuItem("Dechange/Setup Main Scene")]
        public static void Run()
        {
            // --- Scene ---
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // --- Camera ---
            var cameraGO = new GameObject("Main Camera");
            cameraGO.tag = "MainCamera";
            var cam = cameraGO.AddComponent<Camera>();
            cam.farClipPlane = 5000f;
            cam.backgroundColor = Color.black;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cameraGO.AddComponent<UniversalAdditionalCameraData>();
            cameraGO.transform.position = new Vector3(0, 50, -140);
            cameraGO.transform.LookAt(Vector3.zero);

            // --- Ambient light (deep space) ---
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.03f, 0.03f, 0.06f);

            // --- Sun point light ---
            var lightGO = new GameObject("Sun Light");
            var light = lightGO.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.97f, 0.88f);
            light.intensity = 5f;
            light.range = 2000f;
            light.shadows = LightShadows.None;
            lightGO.transform.position = Vector3.zero;

            // --- SimClock ---
            var clockGO = new GameObject("SimClock");
            clockGO.AddComponent<SimClock>();

            // --- ScaleService ---
            var scaleGO = new GameObject("ScaleService");
            scaleGO.AddComponent<ScaleService>();

            // --- Body prefab ---
            EnsureDir("Assets/Prefabs");
            string prefabPath = "Assets/Prefabs/Body.prefab";
            GameObject prefab;

            if (!File.Exists(prefabPath))
            {
                var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Object.DestroyImmediate(sphere.GetComponent<SphereCollider>());
                prefab = PrefabUtility.SaveAsPrefabAsset(sphere, prefabPath);
                Object.DestroyImmediate(sphere);
                Debug.Log("[SceneSetup] Created Body prefab at " + prefabPath);
            }
            else
            {
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            }

            // --- SystemLoader ---
            var loaderGO = new GameObject("SystemLoader");
            var loader = loaderGO.AddComponent<SystemLoader>();
            var loaderSO = new SerializedObject(loader);
            loaderSO.FindProperty("_bodyPrefab").objectReferenceValue = prefab;
            loaderSO.ApplyModifiedPropertiesWithoutUndo();

            // --- UI ---
            EnsureDir("Assets/UI");
            string panelSettingsPath = "Assets/UI/PanelSettings.asset";
            PanelSettings panelSettings;

            if (!File.Exists(panelSettingsPath))
            {
                panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
                panelSettings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
                panelSettings.referenceResolution = new Vector2Int(1920, 1080);
                AssetDatabase.CreateAsset(panelSettings, panelSettingsPath);
            }
            else
            {
                panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(panelSettingsPath);
            }

            string uxmlPath = "Assets/UI/TimeControls.uxml";
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);

            var uiGO = new GameObject("TimeControls UI");
            var uiDoc = uiGO.AddComponent<UIDocument>();
            uiDoc.panelSettings = panelSettings;
            if (uxml != null)
                uiDoc.visualTreeAsset = uxml;
            else
                Debug.LogWarning("[SceneSetup] TimeControls.uxml not found — assign it manually on the UIDocument.");
            EditorUtility.SetDirty(uiDoc);
            uiGO.AddComponent<TimeControlsUI>();

            // --- Save ---
            EnsureDir("Assets/Scenes");
            string scenePath = "Assets/Scenes/Main.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            AssetDatabase.Refresh();

            Debug.Log("[SceneSetup] Main scene created at " + scenePath + ". Press Play to run.");
        }

        private static void EnsureDir(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}
