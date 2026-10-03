using System;
using System.Linq;
using KadakChai;
static class Probe2
{
    public static void Stuck()
    {
        var rs = typeof(RunSimulator);
        for (int n = 0; n < 500; n++)
        {
            var run = new Run(1 + n);
            var offer = run.S.OfferSkills(3, run.Rng); if (offer.Count > 0) run.S.AddSkill(offer[run.Rng.Next(offer.Count)]);
            // replicate: call private Shop via reflection
            var shop = rs.GetMethod("Shop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            object[] a = { run, true, 0 };
            shop.Invoke(null, a);
            while (!run.Finished)
            {
                run.BeginDay();
                if (run.IsFightDay)
                {
                    int tries = 0; BattleResult res;
                    do { res = run.Fight(); if (!res.won) run.RetryDay(); } while (!res.won && ++tries < 10);
                    if (!res.won)
                    {
                        Console.WriteLine($"STUCK run {n} day {run.DayNumber} weapon={run.S.weapon?.data.id} aqi={run.S.aqi} {Aqi.StageFor(run.S.aqi)} toll={run.S.tollDays} lv={run.S.level} atk={run.S.atk} hp={run.S.maxHp} skills={string.Join("/", run.S.skills.Select(o => o.data.id))} reason={res.loseReason} rounds={res.rounds}");
                        break;
                    }
                }
                else run.Choose(run.Rng.Next(2));
                for (int l = run.ConsumeLevelUps(); l > 0; l--) { var o = run.S.OfferSkills(3, run.Rng); if (o.Count > 0) run.S.AddSkill(o[run.Rng.Next(o.Count)]); }
                if (run.DayNumber < run.TotalDays) shop.Invoke(null, a);
                run.EndDay();
            }
        }
    }
}
