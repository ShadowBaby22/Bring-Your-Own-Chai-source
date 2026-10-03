using System.Collections.Generic;

namespace KadakChai
{
    /// <summary>A lasting status (change log: "unique status changing ability" per boss and rival).</summary>
    public class ConditionData
    {
        public string id, name, desc;
    }

    /// <summary>
    /// Pokémon-style statuses that stay after the fight. Every active status bleeds 1.5% max HP each fight round
    /// on top of its own effect, until Bablu drinks Pudin Hara (JhatPat antidote, cures all).
    /// Which enemy inflicts which is in combatants.json ("inflicts", "inflictChance").
    /// </summary>
    public static class Conditions
    {
        public const float PoisonPerRound = 0.015f;  // of max HP, per status, per fight round
        public const float ConfusedChance = 0.15f;   // hit yourself
        public const float SmokedChance = 0.10f;     // cough instead of attacking
        public const float LostMiss = 0.15f;         // extra miss chance
        public const float ShavedDef = 0.70f, CancelledAtk = 0.85f;
        public const int RobbedPerDay = 30;
        public const float BrokerageCut = 0.85f, UnpaidXp = 0.75f, CringeHeal = 0.5f, GrilledHp = 0.08f;
        public const string Antidote = "pudin_hara";

        public static readonly Dictionary<string, ConditionData> All = new()
        {
            ["confused"] = new() { id = "confused", name = "Confused", desc = "15% chance each turn to hit yourself. Synergy overload." },
            ["robbed"] = new() { id = "robbed", name = "Robbed", desc = "Lose ₹30 every morning. The meter is broken." },
            ["shaved"] = new() { id = "shaved", name = "Head Shaved", desc = "-30% DEF. The whole society is staring." },
            ["cancelled"] = new() { id = "cancelled", name = "Cancelled", desc = "-15% ATK. You are trending, badly." },
            ["limping"] = new() { id = "limping", name = "Tyre Marks", desc = "Skip your first turn in every fight." },
            ["cringe"] = new() { id = "cringe", name = "Cringe", desc = "All healing is halved. You saw too much." },
            ["brokerage"] = new() { id = "brokerage", name = "Brokerage", desc = "Fights pay 15% less. One month, non-refundable." },
            ["unpaid"] = new() { id = "unpaid", name = "Unpaid Intern", desc = "25% less EXP from fights. Paid in exposure." },
            ["lost"] = new() { id = "lost", name = "Lost in Sectors", desc = "15% of your attacks miss. Sector 62 or 63?" },
            ["bitten"] = new() { id = "bitten", name = "Monkey Bite", desc = "Bleeds double: 3% HP per round." },
            ["smoked"] = new() { id = "smoked", name = "Smoked Out", desc = "10% chance each turn to cough instead of attacking." },
            ["grilled"] = new() { id = "grilled", name = "Interrogated", desc = "Lose 8% HP at the start of every fight. Kahan jaana hai?" },
        };

        public static string Name(string id) => All.TryGetValue(id, out var c) ? c.name : id;
    }
}
