using System;
using System.Collections.Generic;
using System.Text;

namespace KadakChai
{
    // Pure C# (no UnityEngine) so the whole run can be simulated outside Unity for balancing.
    // Content lives in Resources/Data: days.json, combatants.json, skills.json, dialogue.csv.

    [Serializable]
    public class CombatantData
    {
        public string id;
        public string name;
        public int hp;
        public int atk;
        public int def;
        public int xp;          // EXP granted on defeat
        public string ai;       // default, broker, monkey, firecracker, guard, techbro, ceo, babu
        public string hpLabel;  // "Runway" for the CEO; empty = "HP"
        public string color;    // placeholder blob colour, hex
        public string quip;     // line shown when its gimmick fires ("Meter se nahi jayenge!")
        public string voice;    // VN typewriter blip: lo, mid, hi
        public string range;    // "close" or "ranged" (GDD: Combat: close vs ranged)
        public string shot;     // ranged only: label for the opening shot ("Iced coffee!")
        public string flavor;
        public bool Ranged => range == "ranged";
    }

    /// <summary>A JhatPat shop item (items.json). Weapons and armour last a number of fights.</summary>
    [Serializable]
    public class ItemData
    {
        public string id;
        public string name;
        public string type;      // close, ranged, none
        public string slot;      // weapon, armour, pass (FASTag), consumable, junk
        public int price;
        public int atk, def;
        public int fights;       // durability: fights won before it breaks (borders, for a FASTag)
        public string perk;      // cover_drive, double_tap, yorker, sure_shot, stun, lathicharge, heal
        public float chance, power;
        public string rarity;    // common, rare, epic, ultra
        public int boxWeight;    // odds in the Mystery Box (weights out of the total)
        public bool boxOnly;     // never stocked in the shop, only found in the Mystery Box
        public string desc;
        public string breakText; // story-log card when it breaks
        public bool IsGear => slot == "weapon" || slot == "armour";
    }

    [Serializable]
    public class SkillData
    {
        public string id;
        public string name;
        public string shortName; // chip label
        public string desc;
        public int lease;        // days before it expires
        public string kind;      // extra_hit, aoe, heal_every, big_hit, start_buff, ignore_aqi, half_damage, dodge, confuse, steal_atk
        public float chance;     // per-attack trigger chance (0-1) where relevant
        public float power;      // damage multiplier / heal fraction / buff fraction
        public int every;        // rounds, for heal_every; hit count for half_damage; buff rounds for start_buff
        public float backfire;   // big_hit: chance to hit yourself
        public string sfx;       // optional sound when it triggers (Resources/Audio/SFX/<sfx>)
    }

    [Serializable]
    public class DayData
    {
        public int day;
        public string zone;      // dilli, noida, daftar (gurugram if built)
        public string type;      // fight, event, chai, boss
        public string card;      // dialogue.csv id for the intro card; "<card>_win" is the victory card
        public string[] enemies;
        public string[] enemyGroups; // optional: one is picked at random, e.g. "monkey+monkey". Intro card = "fight_<first id>"
        public string pool;      // optional: event days draw a random unused card whose id starts with this
        public string scene;     // optional: VN scene (scenes.csv) played before the day's card
        public string outro;     // optional: VN scene played after winning / choosing
        public int aqiDelta;     // nudges the daily AQI roll (e.g. Diwali week)
        public int rent;         // rupees the landlord takes at the start of this day
    }

    /// <summary>One line of a visual-novel scene (scenes.csv).</summary>
    public class SceneLine
    {
        public string speaker, side, text;
        public bool IsNarrator => string.IsNullOrEmpty(speaker) || speaker == "narrator";
    }

    [Serializable] public class CombatantList { public CombatantData[] combatants; }
    [Serializable] public class SkillList { public SkillData[] skills; }
    [Serializable] public class DayList { public DayData[] days; }
    [Serializable] public class ItemList { public ItemData[] items; }

    /// <summary>One row of dialogue.csv.</summary>
    public class Card
    {
        public string id, text;
        public string choiceA, effectA, resultA;
        public string choiceB, effectB, resultB;
        public bool HasChoices => !string.IsNullOrEmpty(choiceA);
    }

    public static class GameData
    {
        public static readonly Dictionary<string, CombatantData> Combatants = new();
        public static readonly Dictionary<string, SkillData> Skills = new();
        public static readonly List<DayData> Days = new();
        public static readonly Dictionary<string, Card> Cards = new();
        public static readonly Dictionary<string, List<SceneLine>> Scenes = new();
        public static readonly Dictionary<string, ItemData> Items = new();
        public const int BoxPrice = 200;   // JhatPat Mystery Box
        public static bool Loaded { get; private set; }

        public static readonly string[] Zones = { "dilli", "gurugram", "noida", "daftar" };
        public static string ZoneName(string zone) => zone switch
        {
            "dilli" => "Republic of Dilli",
            "gurugram" => "Gurugram Corporate Emirates",
            "noida" => "Noida Free Trade Zone",
            "border" => "Toll Plaza",
            "daftar" => "Sarkari Daftar",
            _ => zone,
        };

        /// <param name="readText">Returns file contents for "days.json" etc. Unity passes a Resources loader.</param>
        /// <param name="parseJson">JsonUtility in Unity, System.Text.Json in the simulator.</param>
        public static void Load(Func<string, string> readText, Func<string, Type, object> parseJson)
        {
            Combatants.Clear(); Skills.Clear(); Days.Clear(); Cards.Clear(); Scenes.Clear(); Items.Clear();
            foreach (var c in ((CombatantList)parseJson(readText("combatants.json"), typeof(CombatantList))).combatants) Combatants[c.id] = c;
            foreach (var s in ((SkillList)parseJson(readText("skills.json"), typeof(SkillList))).skills) Skills[s.id] = s;
            Days.AddRange(((DayList)parseJson(readText("days.json"), typeof(DayList))).days);
            Days.Sort((a, b) => a.day.CompareTo(b.day));
            var itemText = readText("items.json");
            if (itemText != null)
                foreach (var it in ((ItemList)parseJson(itemText, typeof(ItemList))).items) Items[it.id] = it;

            var rows = Csv.Parse(readText("dialogue.csv"));
            for (int i = 1; i < rows.Count; i++) // row 0 is the header
            {
                var r = rows[i];
                string Col(int n) => n < r.Count ? r[n].Trim() : "";
                if (string.IsNullOrEmpty(Col(0)) || Col(0).StartsWith("#")) continue;
                Cards[Col(0)] = new Card
                {
                    id = Col(0), text = Col(1),
                    choiceA = Col(2), effectA = Col(3), resultA = Col(4),
                    choiceB = Col(5), effectB = Col(6), resultB = Col(7),
                };
            }
            var sceneText = readText("scenes.csv");
            if (sceneText != null)
            {
                var srows = Csv.Parse(sceneText);
                for (int i = 1; i < srows.Count; i++) // scene,speaker,side,text
                {
                    var r = srows[i];
                    string Col(int n) => n < r.Count ? r[n].Trim() : "";
                    if (string.IsNullOrEmpty(Col(0)) || Col(0).StartsWith("#")) continue;
                    if (!Scenes.TryGetValue(Col(0), out var list)) Scenes[Col(0)] = list = new List<SceneLine>();
                    list.Add(new SceneLine { speaker = Col(1), side = Col(2), text = Col(3) });
                }
            }
            Loaded = true;
        }

        public static List<SceneLine> Scene(string id) => id != null && Scenes.TryGetValue(id, out var s) ? s : null;

        public static string SpeakerName(string id) => id switch
        {
            null or "" or "narrator" => "",
            "astrologer" => "Astrologer Baba",
            "delivery" => "Delivery Wale Bhaiya",
            "maamu" => "Maamu",
            _ => Combatants.TryGetValue(id, out var c) ? c.name : id,
        };

        /// <summary>Finds the skill whose name starts a battle label like "Forward This Message! Confused."</summary>
        public static SkillData SkillByLabel(string label)
        {
            if (string.IsNullOrEmpty(label)) return null;
            foreach (var s in Skills.Values) if (label.StartsWith(s.name)) return s;
            return null;
        }

        public static Card CardOrNull(string id) => id != null && Cards.TryGetValue(id, out var c) ? c : null;

        /// <summary>Random card whose id starts with prefix, e.g. "win_" or "news_". Skips ids in exclude.</summary>
        public static Card RandomCard(string prefix, Random rng, ICollection<string> exclude = null)
        {
            var pool = new List<Card>();
            foreach (var c in Cards.Values)
                if (c.id.StartsWith(prefix) && !c.id.EndsWith("_win") && (exclude == null || !exclude.Contains(c.id))) pool.Add(c);
            if (pool.Count == 0 && exclude != null) return RandomCard(prefix, rng); // pool exhausted: allow repeats
            return pool.Count == 0 ? null : pool[rng.Next(pool.Count)];
        }
    }

    /// <summary>RFC-4180-ish CSV: quoted fields, "" escapes, commas and newlines inside quotes.</summary>
    public static class Csv
    {
        public static List<List<string>> Parse(string text)
        {
            var rows = new List<List<string>>();
            var row = new List<string>();
            var field = new StringBuilder();
            bool quoted = false;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (quoted)
                {
                    if (c == '"' && i + 1 < text.Length && text[i + 1] == '"') { field.Append('"'); i++; }
                    else if (c == '"') quoted = false;
                    else field.Append(c);
                }
                else if (c == '"') quoted = true;
                else if (c == ',') { row.Add(field.ToString()); field.Clear(); }
                else if (c == '\n' || c == '\r')
                {
                    if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n') i++;
                    row.Add(field.ToString()); field.Clear();
                    rows.Add(row); row = new List<string>();
                }
                else field.Append(c);
            }
            if (field.Length > 0 || row.Count > 0) { row.Add(field.ToString()); rows.Add(row); }
            return rows;
        }
    }
}
