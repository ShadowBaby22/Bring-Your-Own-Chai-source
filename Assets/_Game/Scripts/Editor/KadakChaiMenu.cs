using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace KadakChai.EditorTools
{
    /// <summary>Menu bar: Kadak Chai > ...</summary>
    public static class KadakChaiMenu
    {
        const string ScenePath = "Assets/_Game/Scenes/Main.unity";

        [MenuItem("Kadak Chai/Build Main Scene", priority = 0)]
        static void BuildScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cam = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cam.tag = "MainCamera";
            var c = cam.GetComponent<Camera>();
            c.orthographic = true;
            c.clearFlags = CameraClearFlags.SolidColor;
            c.backgroundColor = new Color(0.07f, 0.07f, 0.1f);
            cam.transform.position = new Vector3(0, 0, -10);

            new GameObject("Game", typeof(GameController));

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            ApplyWebSettings();
            Debug.Log("Kadak Chai: built " + ScenePath + ". Set the Game view to a 9:16 portrait size (e.g. 1080x1920) and press Play.");
        }

        [MenuItem("Kadak Chai/Apply WebGL Settings", priority = 1)]
        static void ApplyWebSettings()
        {
            PlayerSettings.defaultWebScreenWidth = 540;
            PlayerSettings.defaultWebScreenHeight = 960;
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true; // itch.io doesn't send Content-Encoding headers
            PlayerSettings.WebGL.template = "PROJECT:KadakChai";  // Assets/WebGLTemplates/KadakChai: full-screen, mobile-friendly
            PlayerSettings.WebGL.dataCaching = true;              // repeat visits load from the browser cache
            PlayerSettings.runInBackground = true;
            Debug.Log("Kadak Chai: WebGL set to 540x960, gzip with decompression fallback (itch.io friendly).");
        }

        /// <summary>Builds to Builds/WebGL. Zip that folder's contents and upload to itch.io as HTML.</summary>
        [MenuItem("Kadak Chai/Build WebGL", priority = 2)]
        public static void BuildWebGL()
        {
            if (!System.IO.File.Exists(ScenePath)) BuildScene();
            ApplyWebSettings();
            // Stamp the build so playtesters can say which build they're on (shown on the title screen).
            System.IO.File.WriteAllText("Assets/_Game/Resources/build_info.txt", "dev build " + System.DateTime.Now.ToString("ddd d MMM HH:mm"));
            AssetDatabase.ImportAsset("Assets/_Game/Resources/build_info.txt", ImportAssetOptions.ForceUpdate);
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = "Builds/WebGL",
                target = BuildTarget.WebGL,
                options = BuildOptions.None,
            });
            Debug.Log($"Kadak Chai: WebGL build {report.summary.result} in {report.summary.totalTime}. Output: Builds/WebGL");
            if (Application.isBatchMode) EditorApplication.Exit(report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded ? 0 : 1);
        }

        /// <summary>Re-applies ArtImportSettings (single full sprite, no mips) to every image under Resources/Art.</summary>
        [MenuItem("Kadak Chai/Reimport Art", priority = 3)]
        static void ReimportArt()
        {
            int n = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/_Game/Resources/Art" }))
            {
                AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(guid), ImportAssetOptions.ForceUpdate);
                n++;
            }
            Debug.Log($"Kadak Chai: reimported {n} art files.");
        }

        [MenuItem("Kadak Chai/Simulate 500 Runs (balance check)", priority = 20)]
        static void Simulate()
        {
            GameController.LoadData();
            Debug.Log(RunSimulator.Simulate(500));
        }
    }
}
