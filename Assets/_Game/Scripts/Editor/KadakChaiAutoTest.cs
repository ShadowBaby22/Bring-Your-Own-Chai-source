using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace KadakChai.EditorTools
{
    /// <summary>
    /// Dev helper so the game can be play-tested without clicking around:
    ///  - Create the file "Temp/kc_play_trigger" (or use Kadak Chai > Auto Test Play) and the editor
    ///    enters Play mode by itself, captures the Game view and a diagnostic report, then exits Play.
    ///  - Whenever you press Play yourself, the same report is written 4 s in (Play keeps running).
    /// Output: Temp/kc_diag/game.png and Temp/kc_diag/diag.txt (Temp/ is never committed or built).
    /// </summary>
    [InitializeOnLoad]
    static class KadakChaiAutoTest
    {
        const string Trigger = "Temp/kc_play_trigger";
        const string OutDir = "Temp/kc_diag";

        static double playStartedAt = -1, stopAt = -1, reportAt = -1;
        static bool shotTaken, autoStop, deep;
        static int step;
        static (double at, string click, string shot)[] steps;

        /// <summary>Custom script, one step per line: "&lt;seconds&gt; click &lt;ButtonName&gt;" or "&lt;seconds&gt; shot &lt;file&gt;".</summary>
        static (double, string, string)[] ParseSteps(string text) =>
            text.Split('\n').Select(l => l.Trim().Split(' ')).Where(p => p.Length >= 3 && double.TryParse(p[0], out _))
                .Select(p => (double.Parse(p[0]), p[1] == "click" ? p[2] : p[1] == "audio" ? "@audio:" + p[2] : null, p[1] == "shot" ? p[2] : null)).ToArray();

        // "deep" trigger: click through title -> skip intro -> astrologer -> skill -> fight, screenshotting along the way.
        static readonly (double at, string click, string shot)[] DeepSteps =
        {
            (4, "Start", "1_title"),
            (9, "Skip", null),
            (12, "Main", "2_intro_card"),
            (15, "Skill0", "3_skill_pick"),
            (19, "Main", "4_day1"),
            (23, null, "5_fight"),
            (27, null, "6_fight_later"),
        };
        static readonly StringBuilder messages = new();

        static KadakChaiAutoTest()
        {
            EditorApplication.update += Update;
            EditorApplication.playModeStateChanged += OnPlayMode;
            Application.logMessageReceived += (msg, stack, type) =>
            {
                if (type == LogType.Log) return;
                lock (messages) messages.AppendLine($"[{type}] {msg}\n{stack}");
            };
        }

        [MenuItem("Kadak Chai/Auto Test Play (screenshot + report)", priority = 40)]
        static void Menu() => File.WriteAllText(Trigger, "go");

        static void OnPlayMode(PlayModeStateChange s)
        {
            if (s == PlayModeStateChange.EnteredPlayMode)
            {
                playStartedAt = EditorApplication.timeSinceStartup;
                shotTaken = false;
            }
            if (s == PlayModeStateChange.ExitingEditMode) lock (messages) messages.Clear();
        }

        static void Update()
        {
            double now = EditorApplication.timeSinceStartup;

            if (!EditorApplication.isPlaying && !EditorApplication.isCompiling && File.Exists(Trigger))
            {
                var text = File.ReadAllText(Trigger).Trim();
                deep = text == "deep" || text.Contains("\n") || char.IsDigit(text.FirstOrDefault());
                steps = text == "deep" || !deep ? DeepSteps : ParseSteps(text);
                File.Delete(Trigger);
                autoStop = true;
                step = 0;
                if (EditorApplication.isPaused) EditorApplication.isPaused = false;
                EditorApplication.EnterPlaymode();
                return;
            }

            if (deep && EditorApplication.isPlaying && playStartedAt > 0)
            {
                if (step < steps.Length && now - playStartedAt > steps[step].at)
                {
                    var st = steps[step++];
                    Directory.CreateDirectory(OutDir);
                    if (st.shot != null) ScreenCapture.CaptureScreenshot($"{OutDir}/{st.shot}.png");
                    if (st.click != null && st.click.StartsWith("@audio:")) lock (messages) messages.AppendLine($"[audio {st.click.Substring(7)}] " + AudioSnapshot());
                    else if (st.click != null) Click(st.click);
                    if (step == steps.Length) { reportAt = now + 0.5; shotTaken = true; }
                }
                if (reportAt > 0 && now > reportAt) { reportAt = -1; WriteReport(); deep = false; stopAt = now + 0.5; }
                if (stopAt > 0 && now > stopAt) { stopAt = -1; autoStop = false; EditorApplication.ExitPlaymode(); }
                return;
            }

            if (EditorApplication.isPlaying && playStartedAt > 0 && !shotTaken && now - playStartedAt > 4)
            {
                shotTaken = true;
                Directory.CreateDirectory(OutDir);
                ScreenCapture.CaptureScreenshot(OutDir + "/game.png"); // written at end of frame
                reportAt = now + 0.5;
            }

            if (reportAt > 0 && now > reportAt)
            {
                reportAt = -1;
                WriteReport();
                if (autoStop) stopAt = now + 0.5;
            }

            if (stopAt > 0 && now > stopAt)
            {
                stopAt = -1;
                autoStop = false;
                EditorApplication.ExitPlaymode();
            }
        }

        /// <summary>Presses the first active Button with this GameObject name (as if clicked).</summary>
        static void Click(string name)
        {
            var b = Object.FindObjectsByType<Button>(FindObjectsSortMode.None).FirstOrDefault(x => x.name == name && x.isActiveAndEnabled);
            lock (messages) messages.AppendLine(b != null ? $"[click] {name}" : $"[click] {name} NOT FOUND");
            b?.onClick.Invoke();
        }

        /// <summary>One-line audio state: listeners, user options, and every AudioSource that is playing.</summary>
        static string AudioSnapshot()
        {
            var listeners = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length;
            var playing = Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None)
                .Where(a => a.isPlaying).Select(a => $"{(a.clip ? a.clip.name : "oneshot")}{(a.mute ? "(muted)" : "")}");
            return $"listeners={listeners} listenerVolume={AudioListener.volume} sfxOn={AudioKit.SfxOn} musicOn={AudioKit.MusicOn} playing=[{string.Join(", ", playing)}]";
        }

        static void WriteReport()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"time: {System.DateTime.Now:HH:mm:ss}");
            sb.AppendLine($"scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().path}");
            sb.AppendLine($"screen: {Screen.width}x{Screen.height}  paused: {EditorApplication.isPaused}  timeScale: {Time.timeScale}");
            sb.AppendLine($"GameControllers: {Object.FindObjectsByType<GameController>(FindObjectsSortMode.None).Length}");
            sb.AppendLine($"cameras: {string.Join(", ", Camera.allCameras.Select(c => $"{c.name}(enabled={c.enabled})"))}");
            foreach (var c in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
                sb.AppendLine($"canvas: {c.name} mode={c.renderMode} rect={((RectTransform)c.transform).rect.size} children={c.transform.childCount}");
            sb.AppendLine("roots: " + string.Join(", ", UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().Select(g => g.name)));
            sb.AppendLine("audio: " + AudioSnapshot());
            sb.AppendLine("--- steps, warnings/errors since Play:");
            lock (messages) sb.Append(messages.Length > 0 ? messages.ToString() : "(none)\n");
            File.WriteAllText(OutDir + "/diag.txt", sb.ToString());
        }
    }
}
