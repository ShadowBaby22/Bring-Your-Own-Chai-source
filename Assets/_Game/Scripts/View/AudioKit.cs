using System.Collections.Generic;
using UnityEngine;

namespace KadakChai
{
    /// <summary>
    /// Optional audio. Drop clips into Resources/Audio and they play; missing clips are silently skipped.
    ///   SFX/   click, hit, crit, heal, ko, win, lose, stamp, levelup
    ///   Music/ day, battle
    ///   Barks/ &lt;combatant id&gt;   (played when that enemy appears)
    /// </summary>
    public static class AudioKit
    {
        static AudioSource sfx, music;
        static AudioSource[] pool;
        static int next;
        static readonly Dictionary<string, AudioClip> cache = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { cache.Clear(); sfx = music = null; pool = null; next = 0; settingsLoaded = false; } // no domain reload between Play sessions

        // ---- Player options (Options panel), remembered between visits via PlayerPrefs ----
        static bool sfxOn = true, musicOn = true, settingsLoaded;

        static void LoadSettings()
        {
            if (settingsLoaded) return;
            settingsLoaded = true;
            sfxOn = PlayerPrefs.GetInt("kc_sfx_on", 1) == 1;
            musicOn = PlayerPrefs.GetInt("kc_music_on", 1) == 1;
        }

        public static bool SfxOn
        {
            get { LoadSettings(); return sfxOn; }
            set { LoadSettings(); sfxOn = value; PlayerPrefs.SetInt("kc_sfx_on", value ? 1 : 0); PlayerPrefs.Save(); }
        }

        public static bool MusicOn
        {
            get { LoadSettings(); return musicOn; }
            set
            {
                LoadSettings(); musicOn = value;
                PlayerPrefs.SetInt("kc_music_on", value ? 1 : 0); PlayerPrefs.Save();
                if (music != null) music.mute = !value; // takes effect immediately on the playing loop
            }
        }

        static void Ensure()
        {
            if (sfx != null) return;
            var go = new GameObject("Audio");
            Object.DontDestroyOnLoad(go);
            // No AudioListener anywhere = total silence (the generated Main scene shipped without one).
            if (Object.FindAnyObjectByType<AudioListener>() == null) go.AddComponent<AudioListener>();
            sfx = go.AddComponent<AudioSource>();
            pool = new AudioSource[6];
            for (int i = 0; i < pool.Length; i++) pool[i] = go.AddComponent<AudioSource>();
            music = go.AddComponent<AudioSource>();
            music.loop = true;
            music.volume = 0.5f;
            music.mute = !MusicOn;
        }

        static AudioClip Clip(string path)
        {
            if (!cache.TryGetValue(path, out var c)) cache[path] = c = Resources.Load<AudioClip>(path);
            return c;
        }

        public static void Sfx(string name, float volume = 1f)
        {
            if (!SfxOn) return;
            var c = Clip("Audio/SFX/" + name);
            if (c == null) return;
            Ensure();
            sfx.PlayOneShot(c, volume);
        }

        /// <summary>Plays with a random pitch so repeated hits don't sound like a machine gun.</summary>
        public static void SfxVaried(string name, float volume = 1f, float jitter = 0.12f)
        {
            if (!SfxOn) return;
            var c = Clip("Audio/SFX/" + name);
            if (c == null) return;
            Ensure();
            var src = pool[next = (next + 1) % pool.Length];
            src.pitch = 1f + Random.Range(-jitter, jitter);
            src.PlayOneShot(c, volume);
        }

        /// <summary>A random punch variant (hit1..hit3), falling back to "hit".</summary>
        public static void Punch(float volume = 1f)
        {
            string n = "hit" + Random.Range(1, 4);
            if (Clip("Audio/SFX/" + n) == null) n = "hit";
            SfxVaried(n, volume);
        }

        public static void Bark(string combatantId) { if (!SfxOn) return; var c = Clip("Audio/Barks/" + combatantId); if (c != null) { Ensure(); sfx.PlayOneShot(c); } }

        public static void Music(string name)
        {
            if (name == null) { if (music != null) music.Stop(); return; } // silence (game over)
            var c = Clip("Audio/Music/" + name);
            if (c == null) return;
            Ensure();
            if (music.clip == c && music.isPlaying) return;
            music.clip = c;
            music.Play();
        }
    }
}
