using System;

namespace KadakChai
{
    public enum GrapStage { RareGoodDay, Grap1, Grap2, Grap3, Grap4 }

    /// <summary>AQI of the day (GDD §3.2). Rolled once per day; each band applies one rule to that day's fight.</summary>
    public static class Aqi
    {
        public const int Start = 260;

        public static GrapStage StageFor(int aqi) =>
            aqi <= 200 ? GrapStage.RareGoodDay :
            aqi <= 300 ? GrapStage.Grap1 :
            aqi <= 400 ? GrapStage.Grap2 :
            aqi <= 450 ? GrapStage.Grap3 : GrapStage.Grap4;

        /// <summary>Daily drift skews upward: NCR air only gets worse unless an event cleans it.</summary>
        public static int Roll(int current, int dayDelta, Random rng) =>
            Math.Clamp(current + rng.Next(-90, 111) + dayDelta, 60, 999);

        public static string StageName(GrapStage s) => s switch
        {
            GrapStage.RareGoodDay => "Rare Good Day",
            GrapStage.Grap1 => "GRAP 1",
            GrapStage.Grap2 => "GRAP 2",
            GrapStage.Grap3 => "GRAP 3",
            _ => "GRAP 4",
        };

        public static string RuleText(GrapStage s) => s switch
        {
            GrapStage.RareGoodDay => "Rare Good Day: you feel suspiciously strong (+20% ATK).",
            GrapStage.Grap1 => "GRAP 1: enemies are extra irritable (+10% ATK).",
            GrapStage.Grap2 => "GRAP 2: nobody can see anything (15% miss chance).",
            GrapStage.Grap3 => "GRAP 3, Construction Ban: skills trigger half as often.",
            _ => "GRAP 4, Odd-Even: you attack on odd rounds, enemies on even rounds.",
        };

        // RGB 0-1 for the dial, kept engine-free.
        public static (float r, float g, float b) Color(GrapStage s) => s switch
        {
            GrapStage.RareGoodDay => (0.30f, 0.75f, 0.35f),
            GrapStage.Grap1 => (0.95f, 0.80f, 0.20f),
            GrapStage.Grap2 => (0.95f, 0.55f, 0.15f),
            GrapStage.Grap3 => (0.85f, 0.25f, 0.20f),
            _ => (0.50f, 0.15f, 0.45f),
        };
    }
}
