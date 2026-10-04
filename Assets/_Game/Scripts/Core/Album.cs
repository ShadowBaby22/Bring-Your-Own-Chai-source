using System;
using System.Collections.Generic;
using System.Linq;

namespace KadakChai
{
    /// <summary>One sticker on the sheet. The art is Resources/Art/Stickers/{id}.png (and {id}_blank.png when missing).</summary>
    public class StickerData
    {
        public string id, name, how;   // how = the unlock hint shown on the blank sticker
        public bool big;                // bosses and Bablu are drawn a bit larger
    }

    /// <summary>
    /// The sticker album, collected across runs (the view saves it in PlayerPrefs). Everything else in a run is
    /// temporary; the stickers are the souvenir. Engine-free so the tests can check it.
    /// </summary>
    public static class Album
    {
        public static readonly StickerData[] All =
        {
            S("bablu", "Bablu", "Sign the agreement", big: true), S("scooty_uncle", "Scooty Uncle", "Beat Wrong-Side Scooty Uncle"),
            S("auto_bhaiya", "Auto Bhaiya", "Beat Auto Bhaiya"), S("south_delhi_girl", "South Delhi Girl", "Beat South Delhi Girl"),
            S("pinky_aunty", "Pinky Aunty", "Make a friend in Dilli"),
            S("jaanu", "Jaanu", "Win the metro fight"), S("shona", "Shona", "Win the metro fight"),
            S("broker", "The Broker", "Beat the Day 4 boss", big: true), S("kadak", "KADAK Brew", "Brew a KADAK chai"),
            S("laathi", "Laathi", "Find the ultra-rare Laathi"),
            S("techbro", "Tech Bro", "Beat the LinkedIn Tech Bro"), S("daaru_uncle", "Daaru Uncle", "Make a friend in Gurugram"),
            S("ceo", "Lalaji", "Beat the Day 6 boss", big: true), S("cfo_bunty", "Bunty (CFO)", "Beat Lalaji's board"),
            S("cto_pappu", "Pappu (CTO)", "Beat Lalaji's board"),
            S("monkey", "Monkey Gang", "Beat the monkeys"), S("sector_cabbie", "Lost Cab Driver", "Beat the lost cab driver"),
            S("firecracker", "Firecracker Guy", "Beat the firecracker guy"), S("sheru", "Sheru", "Make a friend in Noida"),
            S("rwa_aunty", "RWA Aunty", "Survive the RWA meeting"),
            S("bald_bablu", "Bald Bablu", "Get your head shaved"), S("rival_student", "Kush", "Make a friend at the Daftar"),
            S("babu", "The Babu", "Beat the final boss", big: true), S("zombie_bablu", "Zombie Bablu", "Wake up at Amma's"),
            S("stamp", "The Stamp", "Finish the lease"),
        };

        static StickerData S(string id, string name, string how, bool big = false) => new() { id = id, name = name, how = how, big = big };

        static readonly HashSet<string> collected = new();
        /// <summary>Stickers first collected in the current run (marked NEW on the sheet).</summary>
        public static readonly HashSet<string> NewThisRun = new();
        /// <summary>Set by the view to persist the album; null in tests.</summary>
        public static Action<string> Save;

        public static int Count => collected.Count;
        public static int Total => All.Length;
        public static bool Has(string id) => collected.Contains(id);
        public static StickerData Get(string id) => All.FirstOrDefault(s => s.id == id);
        public static bool Exists(string id) => Get(id) != null;

        public static void Load(string saved)
        {
            collected.Clear();
            if (string.IsNullOrEmpty(saved)) return;
            foreach (var id in saved.Split(',')) if (Exists(id)) collected.Add(id);
        }

        public static string Serialize() => string.Join(",", All.Where(s => collected.Contains(s.id)).Select(s => s.id));

        public static void StartRun() => NewThisRun.Clear();

        /// <summary>Adds a sticker. Returns it if it is new, null if unknown or already collected.</summary>
        public static StickerData Unlock(string id)
        {
            if (!Exists(id) || !collected.Add(id)) return null;
            NewThisRun.Add(id);
            Save?.Invoke(Serialize());
            return Get(id);
        }

        /// <summary>Sticker ids earned by beating these enemies (Lalaji's board comes with him).</summary>
        public static IEnumerable<string> ForDefeated(IEnumerable<string> enemyIds)
        {
            foreach (var id in enemyIds)
            {
                yield return id;
                if (id == "ceo") { yield return "cfo_bunty"; yield return "cto_pappu"; }
            }
        }

        /// <summary>Stickers the current run state has earned (friends, a shaved head, the Laathi).</summary>
        public static IEnumerable<string> ForState(RunState s)
        {
            if (!string.IsNullOrEmpty(s.companion)) yield return s.companion;
            if (s.conditions.Contains("shaved")) yield return "bald_bablu";
            if (s.weapon?.data.id == "laathi") yield return "laathi";
        }

        /// <summary>Test helper: forget everything.</summary>
        public static void Reset() { collected.Clear(); NewThisRun.Clear(); }
    }
}
