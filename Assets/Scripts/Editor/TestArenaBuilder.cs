#if UNITY_EDITOR
using System.IO;
using Emberpath.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Emberpath.EditorTools
{
    /// <summary>
    /// Editor convenience for (re)generating the Movement &amp; Combat test scene.
    /// The scene only needs a camera and a <see cref="TestArenaBootstrap"/> object;
    /// the arena itself is built procedurally when you press Play.
    /// </summary>
    public static class TestArenaBuilder
    {
        private const string ScenePath = "Assets/Scenes/TestArena.unity";

        [MenuItem("Emberpath/Create or Reset Test Scene")]
        public static void CreateTestScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Orthographic 2D camera.
            Camera cam = Object.FindFirstObjectByType<Camera>();
            if (cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = 6.5f;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.08f, 0.07f, 0.10f);
                cam.transform.position = new Vector3(0f, 1f, -10f);
            }

            // A 2D scene does not need the default directional light.
            Light light = Object.FindFirstObjectByType<Light>();
            if (light != null) Object.DestroyImmediate(light.gameObject);

            var bootstrap = new GameObject("[TestArenaBootstrap]");
            bootstrap.AddComponent<TestArenaBootstrap>();

            Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings(ScenePath);

            EditorUtility.DisplayDialog(
                "Emberpath",
                "Test scene created at:\n" + ScenePath + "\n\nPress Play to build and test the arena.",
                "OK");
        }

        private static void AddSceneToBuildSettings(string path)
        {
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (scenes.Exists(s => s.path == path)) return;
            scenes.Insert(0, new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
#endif
