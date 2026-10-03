using System;
using System.Collections.Generic;
using System.Globalization;

namespace KadakChai
{
    /// <summary>
    /// Tiny effect language used in dialogue.csv, e.g. "hp:-15%;atk:+5;flag:aunty;aqi:-80;skill:random".
    /// Keys: hp, heal, maxhp, atk, def, xp, aqi, flag, skill (an id or "random"), rs (rupees).
    /// hp/heal accept a trailing % meaning percent of max HP.
    /// </summary>
    public static class Effects
    {
        /// <returns>Human-readable changes for the log, e.g. "+5 ATK".</returns>
        public static List<string> Apply(string effect, RunState s, Random rng)
        {
            var notes = new List<string>();
            if (string.IsNullOrWhiteSpace(effect)) return notes;

            foreach (var part in effect.Split(';'))
            {
                var kv = part.Split(new[] { ':' }, 2);
                if (kv.Length != 2) continue;
                string key = kv[0].Trim().ToLowerInvariant(), val = kv[1].Trim();

                switch (key)
                {
                    case "hp":
                    case "heal":
                    {
                        int amount = Amount(val, s.maxHp);
                        int before = s.hp;
                        s.hp = Math.Clamp(s.hp + amount, 1, s.maxHp); // events never kill you
                        int d = s.hp - before;
                        if (d != 0) notes.Add($"{Signed(d)} HP");
                        break;
                    }
                    case "maxhp":
                    {
                        int amount = Amount(val, s.maxHp);
                        s.maxHp = Math.Max(10, s.maxHp + amount);
                        s.hp = Math.Clamp(s.hp + Math.Max(0, amount), 1, s.maxHp);
                        notes.Add($"{Signed(amount)} Max HP");
                        break;
                    }
                    case "atk": { int a = Amount(val, s.atk); s.atk = Math.Max(1, s.atk + a); notes.Add($"{Signed(a)} ATK"); break; }
                    case "def": { int a = Amount(val, s.def); s.def = Math.Max(0, s.def + a); notes.Add($"{Signed(a)} DEF"); break; }
                    case "rs":
                    {
                        int before = s.rupees;
                        s.rupees = Math.Max(0, s.rupees + Amount(val, 0)); // you can be broke, not in debt
                        int d = s.rupees - before;
                        if (d != 0) notes.Add(d > 0 ? $"+₹{d}" : $"-₹{-d}");
                        break;
                    }
                    case "xp": { int a = Amount(val, 0); s.xp += a; notes.Add($"+{a} EXP"); break; }
                    case "aqi":
                    {
                        int a = Amount(val, 0);
                        s.aqi = Math.Clamp(s.aqi + a, 60, 999);
                        notes.Add($"AQI {Signed(a)}");
                        break;
                    }
                    case "flag":
                        s.flags.Add(val);
                        if (val == "aunty") { s.companion = "pinky_aunty"; notes.Add("Pinky Aunty joined!"); }
                        break;
                    case "skill":
                    {
                        SkillData skill = null;
                        if (val == "random")
                        {
                            var offer = s.OfferSkills(1, rng);
                            if (offer.Count > 0) skill = offer[0];
                        }
                        else GameData.Skills.TryGetValue(val, out skill);
                        if (skill != null) { s.AddSkill(skill); notes.Add($"New skill: {skill.name}"); }
                        break;
                    }
                }
            }
            return notes;
        }

        static int Amount(string val, int percentOf)
        {
            bool pct = val.EndsWith("%");
            if (pct) val = val.Substring(0, val.Length - 1);
            float f = float.Parse(val, NumberStyles.Float, CultureInfo.InvariantCulture);
            return pct ? (int)Math.Round(percentOf * f / 100f) : (int)Math.Round(f);
        }

        static string Signed(int v) => v >= 0 ? "+" + v : v.ToString();
    }
}
