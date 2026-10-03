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

        /// <summary>Plays N full runs. Death is permanent (Health Insurance revives once), so this reports how
        /// often runs clear and where Bablu dies. shop = false is a player who never opens JhatPat.</summary>
        public static string Simulate(int runs, int seed = 1, bool shop = true)
        {
            int days = GameData.Days.Count;
            var deaths = new int[days];
            var reached = new int[days];
            var revives = new int[days];
            var rounds = new List<int>[days];
            var fightSeconds = new List<float>[days];
            var hpIn = new List<float>[days];
            for (int i = 0; i < days; i++) { rounds[i] = new List<int>(); fightSeconds[i] = new List<float>(); hpIn[i] = new List<float>(); }
            var killers = new Dictionary<string, int>();
            int clears = 0, laathis = 0, tolls = 0;
            var earned = new List<int>();

            for (int n = 0; n < runs; n++)
            {
                var run = new Run(seed + n);
                var rng = run.Rng;
                PickSkill(run, rng); // Astrologer Baba's first offer
                Shop(run, shop, ref laathis);

                while (!run.Finished && !run.Dead)
                {
                    int d = run.S.dayIndex;
                    reached[d]++;
                    run.BeginDay();
                    if (run.IsFightDay)
                    {
                        hpIn[d].Add((float)run.S.hp / run.S.maxHp);
                        BattleResult res;
                        do
                        {
                            res = run.Fight();
                            rounds[d].Add(res.rounds);
                            fightSeconds[d].Add(EstimateSeconds(res));
                            if (res.revived) revives[d]++;
                        } while (res.revived);
                        if (res.died)
                        {
                            deaths[d]++;
                            killers[run.Killer] = killers.GetValueOrDefault(run.Killer) + 1;
                            break;
                        }
                    }
                    else run.Choose(rng.Next(2));

                    for (int l = run.ConsumeLevelUps(); l > 0; l--) PickSkill(run, rng);
                    if (run.DayNumber < run.TotalDays) Shop(run, shop, ref laathis);
                    run.EndDay();
                    if (run.Border != null && !run.UsedFastag) tolls++;
                }
                if (!run.Dead) clears++;
                earned.Add(run.S.earned);
            }

            var sb = new StringBuilder();
            sb.AppendLine($"=== {runs} simulated runs ({(shop ? "shopping" : "never shops")}) ===");
            sb.AppendLine($"CLEARED: {100f * clears / runs:0}%   Laathi drops: {laathis}   Rupees earned avg: {earned.Average():0}");
            sb.AppendLine("Killers: " + string.Join(", ", killers.OrderByDescending(k => k.Value).Select(k => $"{k.Key} {k.Value}")));
            sb.AppendLine("Day  Type    Khatra  Reached  Died here  (of those reaching)  Revives  HP in  Rounds  Secs");
            for (int i = 0; i < days; i++)
            {
                var day = GameData.Days[i];
                string sp = $"{(day.spice > 0 ? day.spice : 0)}/8";
                if (rounds[i].Count == 0) { sb.AppendLine($"{day.day,3}  {day.type,-6}  {sp,6}  {reached[i],7}"); continue; }
                int n = Math.Max(1, reached[i]);
                sb.AppendLine($"{day.day,3}  {day.type,-6}  {sp,6}  {reached[i],7}  {deaths[i],9}  {100f * deaths[i] / n,18:0}%  {revives[i],7}  {hpIn[i].Average() * 100,4:0}%  {rounds[i].Average(),6:0.0}  {fightSeconds[i].Average(),4:0}");
            }
            return sb.ToString();
        }

        /// <summary>A sensible shopper: chai when hurt, the strongest weapon it can afford, armour, a FASTag, then boxes.</summary>
        static void Shop(Run run, bool enabled, ref int laathis)
        {
            run.RestockShop();
            if (!enabled) return;
            var s = run.S;
            var cure = s.shop.FirstOrDefault(i => i.slot == "antidote");
            if (cure != null && s.conditions.Count > 0 && run.CanAfford(cure.price)) run.Buy(cure);
            var chai = s.shop.FirstOrDefault(i => i.slot == "consumable");
            while (chai != null && s.hp < s.maxHp * 0.6f && run.CanAfford(chai.price)) run.Buy(chai);

            float rangedShare = NextEnemiesRangedShare(run);
            float Score(ItemData i) => i.atk * Math.Min(i.fights, 3) * (i.type == "close" ? 1f + 0.4f * rangedShare : 1.2f - 0.5f * rangedShare);
            var weapon = s.shop.Where(i => i.slot == "weapon" && run.CanAfford(i.price)).OrderByDescending(Score).FirstOrDefault();
            if (weapon != null && (s.weapon == null || s.weapon.fightsLeft <= 1 || weapon.atk > s.weapon.data.atk + 2)) run.Buy(weapon);
            var armour = s.shop.FirstOrDefault(i => i.slot == "armour");
            if (armour != null && s.armour == null && run.CanAfford(armour.price + 100)) run.Buy(armour);
            var ins = s.shop.FirstOrDefault(i => i.slot == "insurance");
            bool bossNext = run.DayNumber < run.TotalDays && GameData.Days[run.S.dayIndex + 1].type == "boss";
            if (ins != null && s.insuranceDays == 0 && (bossNext || s.hp < s.maxHp * 0.5f) && run.CanAfford(ins.price)) run.Buy(ins);
            var tag = s.shop.FirstOrDefault(i => i.slot == "pass");
            if (tag != null && s.fastags == 0 && run.CanAfford(tag.price + 150)) run.Buy(tag);

            while (run.CanAfford(run.BoxPrice + 60))
            {
                var it = run.OpenBox();
                if (it.id == "laathi") laathis++;
                bool better = it.slot == "weapon" && (s.weapon == null || it.atk > s.weapon.data.atk) ||
                              it.slot == "armour" && s.armour == null || it.slot == "pass" || it.slot == "consumable" || it.slot == "insurance" || it.slot == "antidote" && s.conditions.Count > 0;
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
            if (offer.Count > 0) run.TakeBlessing(offer[rng.Next(offer.Count)]);
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
