using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KadakChai
{
    /// <summary>
    /// Plays N full runs with a random policy and reports difficulty per day.
    /// Run from the Unity menu (Kadak Chai > Simulate 500 Runs) after changing numbers in the JSON.
    /// </summary>
    public static class RunSimulator
    {
        const int MaxAttemptsPerDay = 10;

        public static string Simulate(int runs, int seed = 1, bool shop = true)
        {
            int days = GameData.Days.Count;
            var losses = new int[days];
            var reached = new int[days];
            var timeouts = new int[days];
            var firstTryWins = new int[days];
            var rounds = new List<int>[days];
            var fightSeconds = new List<float>[days];
            for (int i = 0; i < days; i++) { rounds[i] = new List<int>(); fightSeconds[i] = new List<float>(); }
            var finalLevels = new List<int>();
            var skillsExpired = new List<int>();
            int stuck = 0, laathis = 0, tolls = 0;
            var gearAtFinal = new Dictionary<string, int>();
            var earned = new List<int>();
            var spent = new List<int>();

            for (int n = 0; n < runs; n++)
            {
                var run = new Run(seed + n);
                var rng = run.Rng;
                PickSkill(run, rng); // Astrologer Baba's first offer
                Shop(run, shop, ref laathis);
                int expired = 0;
                bool gaveUp = false;

                while (!run.Finished)
                {
                    int d = run.S.dayIndex;
                    reached[d]++;
                    run.BeginDay();
                    if (d == days - 1) { string w = run.S.weapon?.data.id ?? "none"; gearAtFinal[w] = gearAtFinal.GetValueOrDefault(w) + 1; }
                    if (run.IsFightDay)
                    {
                        BattleResult res;
                        int attempts = 0;
                        do
                        {
                            res = run.Fight();
                            rounds[d].Add(res.rounds);
                            fightSeconds[d].Add(EstimateSeconds(res));
                            if (!res.won) { losses[d]++; if (res.loseReason == "time") timeouts[d]++; run.RetryDay(); }
                        } while (!res.won && ++attempts < MaxAttemptsPerDay);
                        if (!res.won) { gaveUp = true; break; }
                        if (attempts == 0) firstTryWins[d]++;
                    }
                    else run.Choose(rng.Next(2));

                    for (int l = run.ConsumeLevelUps(); l > 0; l--) PickSkill(run, rng);
                    if (run.DayNumber < run.TotalDays) Shop(run, shop, ref laathis);
                    expired += run.EndDay().Count;
                    if (run.Border != null && !run.UsedFastag) tolls++;
                }

                if (gaveUp) stuck++;
                finalLevels.Add(run.S.level);
                earned.Add(run.S.earned);
                spent.Add(run.S.spent);
                skillsExpired.Add(expired);
            }

            var sb = new StringBuilder();
            sb.AppendLine($"=== {runs} simulated runs ({(shop ? "shopping" : "never shops")}) ===");
            sb.AppendLine($"Rupees earned avg: {earned.Average():0}  spent avg: {spent.Average():0}   Laathi drops: {laathis}   Toll Taxes taken per run: {(float)tolls / runs:0.0}");
            sb.AppendLine("Weapon at the final boss: " + string.Join(", ", gearAtFinal.OrderByDescending(k => k.Value).Select(k => $"{k.Key} {k.Value}")));
            sb.AppendLine($"Runs stuck (lost {MaxAttemptsPerDay}x on one day): {stuck}");
            sb.AppendLine($"Final level avg: {finalLevels.Average():0.0}   Skills expired per run avg: {skillsExpired.Average():0.0}");
            sb.AppendLine("Day  Type   Reached  First-try win%  Avg losses (timeouts)  Avg rounds  Avg fight secs (1x)");
            for (int i = 0; i < days; i++)
            {
                var day = GameData.Days[i];
                if (rounds[i].Count == 0) { sb.AppendLine($"{day.day,3}  {day.type,-6}  {reached[i],7}"); continue; }
                int n = Math.Max(1, reached[i]);
                sb.AppendLine($"{day.day,3}  {day.type,-6}  {reached[i],7}  {100f * firstTryWins[i] / n,13:0}%  {(float)losses[i] / n,10:0.00} ({(float)timeouts[i] / n:0.00})  {rounds[i].Average(),10:0.0}  {fightSeconds[i].Average(),10:0.0}");
            }
            return sb.ToString();
        }

        /// <summary>A sensible shopper: chai when hurt, the strongest weapon it can afford, armour, a FASTag, then boxes.</summary>
        static void Shop(Run run, bool enabled, ref int laathis)
        {
            run.RestockShop();
            if (!enabled) return;
            var s = run.S;
            var chai = s.shop.FirstOrDefault(i => i.slot == "consumable");
            while (chai != null && s.hp < s.maxHp * 0.6f && run.CanAfford(chai.price)) run.Buy(chai);

            float rangedShare = NextEnemiesRangedShare(run);
            float Score(ItemData i) => i.atk * Math.Min(i.fights, 3) * (i.type == "close" ? 1f + 0.4f * rangedShare : 1.2f - 0.5f * rangedShare);
            var weapon = s.shop.Where(i => i.slot == "weapon" && run.CanAfford(i.price)).OrderByDescending(Score).FirstOrDefault();
            if (weapon != null && (s.weapon == null || s.weapon.fightsLeft <= 1 || weapon.atk > s.weapon.data.atk + 2)) run.Buy(weapon);
            var armour = s.shop.FirstOrDefault(i => i.slot == "armour");
            if (armour != null && s.armour == null && run.CanAfford(armour.price + 100)) run.Buy(armour);
            var tag = s.shop.FirstOrDefault(i => i.slot == "pass");
            if (tag != null && s.fastags == 0 && run.CanAfford(tag.price + 150)) run.Buy(tag);

            while (run.CanAfford(GameData.BoxPrice + 60))
            {
                var it = run.OpenBox();
                if (it.id == "laathi") laathis++;
                bool better = it.slot == "weapon" && (s.weapon == null || it.atk > s.weapon.data.atk) ||
                              it.slot == "armour" && s.armour == null || it.slot == "pass" || it.slot == "consumable";
                if (better) run.Take(it); else run.Sell(it);
            }
        }

        /// <summary>Fraction of tomorrow's possible enemies that are ranged (0 if tomorrow isn't a fight).</summary>
        public static float NextEnemiesRangedShare(Run run)
        {
            var ids = run.NextEnemyIds();
            if (ids.Count == 0) return 0;
            return (float)ids.Count(id => GameData.Combatants.TryGetValue(id, out var c) && c.Ranged) / ids.Count;
        }

        static void PickSkill(Run run, Random rng)
        {
            var offer = run.S.OfferSkills(3, rng);
            if (offer.Count > 0) run.S.AddSkill(offer[rng.Next(offer.Count)]);
        }

        /// <summary>Rough playback length, matching the delays in BattleView.</summary>
        public static float EstimateSeconds(BattleResult r)
        {
            float t = 0;
            foreach (var e in r.events)
                t += e.type switch
                {
                    BattleEventType.Round => 0.15f,
                    BattleEventType.Shout => 0.9f,
                    BattleEventType.Revive => 1.2f,
                    BattleEventType.Death => 0.5f,
                    _ => 0.45f,
                };
            return t / 0.65f; // BattleView plays "1x" at 0.65 pace
        }
    }
}
