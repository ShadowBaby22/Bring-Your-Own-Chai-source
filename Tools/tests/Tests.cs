using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using KadakChai;

/// <summary>
/// Regression suite for the engine-free game code: data integrity, JhatPat, day flow, Amma's Nursing Home,
/// battle rules and the balance band. No browser and no Unity: it runs the same C# the game runs.
///   bash Tools/test.sh            all groups
///   bash Tools/test.sh shop nurse only the groups whose name contains one of the words
/// </summary>
static class Tests
{
    static string root;
    static int passed, failed, warned;
    static readonly List<string> failures = new();

    static int Main(string[] args)
    {
        root = FindRoot();
        string dataDir = Path.Combine(root, "Assets/_Game/Resources/Data");
        var opts = new JsonSerializerOptions { IncludeFields = true, PropertyNameCaseInsensitive = true };
        GameData.Load(f => File.Exists(Path.Combine(dataDir, f)) ? File.ReadAllText(Path.Combine(dataDir, f)) : null,
                      (json, t) => JsonSerializer.Deserialize(json, t, opts));

        var groups = new (string name, Action body)[]
        {
            ("data", DataTests), ("art", ArtTests), ("shop", ShopTests), ("days", DayTests),
            ("nurse", NursingHomeTests), ("battle", BattleTests), ("stickers", StickerTests), ("balance", BalanceTests),
        };
        foreach (var (name, body) in groups)
        {
            if (args.Length > 0 && !args.Any(a => name.Contains(a))) continue;
            Console.WriteLine($"\n[{name}]");
            body();
        }

        Console.WriteLine($"\n{passed} passed, {failed} failed, {warned} warnings");
        foreach (var f in failures) Console.WriteLine("  FAIL " + f);
        return failed > 0 ? 1 : 0;
    }

    // --- tiny test framework ------------------------------------------------------

    static void Test(string name, Action body)
    {
        try { body(); passed++; Console.WriteLine($"  ok    {name}"); }
        catch (Exception e) { failed++; failures.Add($"{name}: {e.Message}"); Console.WriteLine($"  FAIL  {name}: {e.Message}"); }
    }

    /// <summary>Like Test, but a miss is a warning (art gaps the game falls back on), not a failure.</summary>
    static void Warn(string name, Action body)
    {
        try { body(); passed++; Console.WriteLine($"  ok    {name}"); }
        catch (Exception e) { warned++; Console.WriteLine($"  warn  {name}: {e.Message}"); }
    }

    static void Check(bool ok, string why) { if (!ok) throw new Exception(why); }
    static void Eq<T>(T expected, T actual, string what)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual)) throw new Exception($"{what}: expected {expected}, got {actual}");
    }
    static void Between(double lo, double hi, double actual, string what)
    {
        if (actual < lo || actual > hi) throw new Exception($"{what}: {actual:0.##} is outside {lo}-{hi}");
    }

    static string FindRoot()
    {
        foreach (var start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
            for (var d = new DirectoryInfo(start); d != null; d = d.Parent)
                if (Directory.Exists(Path.Combine(d.FullName, "Assets/_Game"))) return d.FullName;
        throw new Exception("Run from inside the Kadak Chai project");
    }

    // --- helpers -----------------------------------------------------------------

    static int DayIndexOf(Func<DayData, bool> pick) => GameData.Days.FindIndex(d => pick(d));

    /// <summary>A run parked on the morning of a day (0-based), ready to fight.</summary>
    static Run RunOn(int dayIndex, int seed = 1)
    {
        var r = new Run(seed);
        r.S.dayIndex = dayIndex;
        r.BeginDay();
        return r;
    }

    /// <summary>Bablu at 1 HP with no punch: he loses any fight.</summary>
    static void Weaken(RunState s) { s.hp = 1; s.atk = 1; s.def = 0; s.companion = null; s.skills.Clear(); }

    /// <summary>Bablu as a god: he wins any fight.</summary>
    static void Buff(RunState s) { s.maxHp = s.hp = 50000; s.atk = 5000; s.def = 2000; s.conditions.Clear(); }

    static IEnumerable<string> EnemyIds(DayData d)
    {
        if (d.enemies != null) foreach (var id in d.enemies) yield return id.Trim();
        if (d.enemyGroups != null) foreach (var g in d.enemyGroups) foreach (var id in g.Split('+')) yield return id.Trim();
    }

    static int HeroIndex(BattleResult r) => r.units.FindIndex(u => u.id == "new_kid");

    // --- data --------------------------------------------------------------------

    static void DataTests()
    {
        var days = GameData.Days;
        Test("11 days, numbered 1 to 11", () =>
        {
            Eq(11, days.Count, "day count");
            for (int i = 0; i < days.Count; i++) Eq(i + 1, days[i].day, $"days[{i}].day");
        });
        Test("the last day is the Babu boss fight", () =>
        {
            var last = days[days.Count - 1];
            Eq("boss", last.type, "Day 11 type");
            Check(EnemyIds(last).Contains("babu"), "Day 11 has no babu");
        });
        Test("KHATRA climbs from 1/8 to 8/8 and never drops", () =>
        {
            Eq(1, days[0].spice, "Day 1 spice");
            Eq(GameData.MaxSpice, days[days.Count - 1].spice, "Day 11 spice");
            for (int i = 1; i < days.Count; i++)
                Check(days[i].spice >= days[i - 1].spice, $"Day {i + 1} spice {days[i].spice} < Day {i} spice {days[i - 1].spice}");
        });
        Test("enemyScale: Days 1-4 gentler than Days 6-11, none above 1", () =>
        {
            foreach (var d in days) Between(0.01, 1.0, d.enemyScale > 0 ? d.enemyScale : 1f, $"Day {d.day} enemyScale");
            float early = days.Take(4).Max(d => d.enemyScale > 0 ? d.enemyScale : 1f);
            float late = days.Skip(5).Min(d => d.enemyScale > 0 ? d.enemyScale : 1f);
            Check(early < late, $"Days 1-4 scale up to {early}, Days 6-11 down to {late}");
        });
        Test("every enemy in days.json exists in combatants.json", () =>
        {
            var missing = days.SelectMany(d => EnemyIds(d).Where(id => !GameData.Combatants.ContainsKey(id)).Select(id => $"Day {d.day}: {id}")).ToList();
            Check(missing.Count == 0, string.Join(", ", missing));
        });
        Test("every companion exists, and each zone has at most one", () =>
        {
            foreach (var d in days.Where(d => !string.IsNullOrEmpty(d.companion)))
                Check(GameData.Combatants.ContainsKey(d.companion), $"Day {d.day} companion {d.companion} not in combatants.json");
            foreach (var z in days.GroupBy(d => d.zone))
                Check(z.Count(d => !string.IsNullOrEmpty(d.companion)) <= 1, $"zone {z.Key} has more than one companion");
        });
        Test("every VN scene the game plays exists in scenes.csv", () =>
        {
            var ids = new List<string> { "intro", "ending", "nursing_home" }; // hard-coded in GameController
            foreach (var d in days) { if (!string.IsNullOrEmpty(d.scene)) ids.Add(d.scene); if (!string.IsNullOrEmpty(d.outro)) ids.Add(d.outro); }
            var missing = ids.Where(id => GameData.Scene(id) == null).ToList();
            Check(missing.Count == 0, "missing: " + string.Join(", ", missing));
        });
        Test("every intro card and event pool in days.json exists in dialogue.csv", () =>
        {
            foreach (var d in days)
            {
                if (!string.IsNullOrEmpty(d.card)) Check(GameData.Cards.ContainsKey(d.card), $"Day {d.day} card {d.card}");
                if (!string.IsNullOrEmpty(d.pool)) Check(GameData.Cards.Keys.Any(k => k.StartsWith(d.pool)), $"Day {d.day} pool {d.pool} is empty");
                if (d.enemyGroups != null) foreach (var g in d.enemyGroups)
                {
                    string first = g.Split('+')[0].Trim();
                    Check(GameData.CardOrNull("fight_" + first) != null || GameData.CardOrNull(d.card) != null, $"Day {d.day}: no intro card for {first}");
                }
            }
        });
        Test("event cards with choices have both options and results", () =>
        {
            foreach (var c in GameData.Cards.Values.Where(c => c.HasChoices))
                Check(!string.IsNullOrEmpty(c.choiceB) && !string.IsNullOrEmpty(c.resultA) && !string.IsNullOrEmpty(c.resultB), $"card {c.id} is half-filled");
        });
        Test("every VN speaker has a display name", () =>
        {
            var special = new HashSet<string> { "", "narrator", "astrologer", "sadhu", "amma", "delivery", "maamu" };
            var bad = GameData.Scenes.SelectMany(kv => kv.Value.Where(l => !special.Contains(l.speaker ?? "") && !GameData.Combatants.ContainsKey(l.speaker))
                                                               .Select(l => $"{kv.Key}: '{l.speaker}'")).Distinct().ToList();
            Check(bad.Count == 0, "unknown speakers (shown as raw ids): " + string.Join(", ", bad));
        });
        Test("every status an enemy inflicts is a real status", () =>
        {
            foreach (var c in GameData.Combatants.Values.Where(c => !string.IsNullOrEmpty(c.inflicts)))
                Check(Conditions.All.ContainsKey(c.inflicts), $"{c.id} inflicts unknown status '{c.inflicts}'");
        });
        Test("Pinky Aunty's betrayal (Day 9) can shave Bablu's head", () =>
        {
            Eq("shaved", GameData.Combatants["rwa_aunty"].inflicts, "rwa_aunty inflicts");
            Check(EnemyIds(days[8]).Contains("rwa_aunty"), "Day 9 isn't the rwa_aunty fight");
        });
        Test("Lalaji's summons exist (cfo_bunty, cto_pappu)", () =>
        {
            Check(GameData.Combatants.ContainsKey("cfo_bunty") && GameData.Combatants.ContainsKey("cto_pappu"), "summon missing from combatants.json");
        });
        Test("every item has a sane price, slot and durability", () =>
        {
            var slots = new HashSet<string> { "weapon", "armour", "pass", "consumable", "junk", "antidote", "insurance" };
            foreach (var it in GameData.Items.Values)
            {
                Check(slots.Contains(it.slot), $"{it.id}: unknown slot '{it.slot}'");
                Check(it.boxOnly || it.price > 0, $"{it.id}: shop item with no price");
                if (it.IsGear) Check(it.fights > 0, $"{it.id}: gear that breaks before its first fight");
            }
        });
    }

    // --- art ---------------------------------------------------------------------

    static void ArtTests()
    {
        string art = Path.Combine(root, "Assets/_Game/Resources/Art");
        bool Has(string rel) => File.Exists(Path.Combine(art, rel + ".png"));

        Test("every fighter has a sprite and a hurt sprite", () =>
        {
            var ids = new HashSet<string> { "new_kid", "pinky_aunty", "cfo_bunty", "cto_pappu" };
            foreach (var d in GameData.Days) { ids.UnionWith(EnemyIds(d)); if (!string.IsNullOrEmpty(d.companion)) ids.Add(d.companion); }
            var missing = ids.SelectMany(id => new[] { id, id + "_hit" }).Where(n => !Has("Characters/" + n)).ToList();
            Check(missing.Count == 0, "missing Characters/: " + string.Join(", ", missing));
        });
        Test("bald Bablu sprites exist (Head Shaved)", () =>
        {
            Check(Has("Characters/new_kid_shaved") && Has("Characters/new_kid_shaved_hit"), "new_kid_shaved(.png/_hit.png) missing");
        });
        Test("every zone and day background exists", () =>
        {
            var missing = GameData.Days.Select(d => string.IsNullOrEmpty(d.bg) ? d.zone : d.bg).Distinct().Where(b => !Has("Backgrounds/" + b)).ToList();
            Check(missing.Count == 0, "missing Backgrounds/: " + string.Join(", ", missing));
        });
        Test("every skill has an icon", () =>
        {
            var missing = GameData.Skills.Keys.Where(id => !Has("UI/skill_" + id)).ToList();
            Check(missing.Count == 0, "missing UI/skill_: " + string.Join(", ", missing));
        });
        Warn("every JhatPat item has an icon (else the shop shows a dot)", () =>
        {
            var missing = GameData.Items.Keys.Where(id => !Has("UI/item_" + id)).ToList();
            Check(missing.Count == 0, "missing UI/item_: " + string.Join(", ", missing));
        });
    }

    // --- JhatPat -----------------------------------------------------------------

    static void ShopTests()
    {
        Test("first Mystery Box is free, then it costs Rs 200", () =>
        {
            var r = new Run(1);
            r.S.rupees = 200;
            Eq(0, r.BoxPrice, "first box price");
            Check(r.OpenBox() != null, "free box didn't open");
            Eq(200, r.S.rupees, "rupees after the free box");
            Check(r.S.freeBoxUsed, "freeBoxUsed not set");
            Eq(GameData.BoxPrice, r.BoxPrice, "second box price");
            Check(r.OpenBox() != null, "paid box didn't open");
            Eq(0, r.S.rupees, "rupees after a paid box");
            Check(r.OpenBox() == null, "opened a box with no money");
            Eq(0, r.S.rupees, "rupees after a failed box");
        });
        Test("Laathi: ultra rare, Mystery Box only, about 5%", () =>
        {
            var l = GameData.Items["laathi"];
            Check(l.boxOnly, "Laathi can be stocked in the shop");
            Eq("ultra", l.rarity, "Laathi rarity");
            Between(4.95, 5.05, Run.BoxOdds(l), "Laathi box odds % (the shop and GDD promise 5%)");
        });
        Test("Mystery Box draws match the listed odds (50,000 draws)", () =>
        {
            var rng = new Random(5);
            var counts = new Dictionary<string, int>();
            const int n = 50000;
            for (int i = 0; i < n; i++) { var it = Run.DrawBox(rng); counts[it.id] = counts.GetValueOrDefault(it.id) + 1; }
            foreach (var it in GameData.Items.Values.Where(i => i.boxWeight > 0))
                Between(Run.BoxOdds(it) - 0.7, Run.BoxOdds(it) + 0.7, 100.0 * counts.GetValueOrDefault(it.id) / n, $"{it.id} drop %");
        });
        Test("the shop always stocks chai and Pudin Hara, never box-only items", () =>
        {
            var r = new Run(2);
            for (int i = 0; i < 300; i++)
            {
                r.RestockShop();
                Check(r.S.shop.Any(x => x.id == "cutting_chai"), "no Cutting Chai");
                Check(r.S.shop.Any(x => x.id == Conditions.Antidote), "no Pudin Hara");
                Check(r.S.shop.All(x => !x.boxOnly), "box-only item on the shelf");
                Eq(r.S.shop.Count, r.S.shop.Select(x => x.id).Distinct().Count(), "duplicate shelf items");
                Eq(5, r.S.shop.Count, "shelf size");
            }
        });
        Test("buying with too little money changes nothing", () =>
        {
            var r = new Run(1);
            r.RestockShop();
            var pricey = r.S.shop.OrderByDescending(x => x.price).First();
            r.S.rupees = pricey.price - 1;
            Check(r.Buy(pricey) == null, "bought something unaffordable");
            Eq(pricey.price - 1, r.S.rupees, "rupees");
            Check(r.S.shop.Contains(pricey), "item left the shelf");
        });
        Test("Pudin Hara cures every status", () =>
        {
            var r = new Run(1);
            r.S.conditions.UnionWith(new[] { "shaved", "confused", "bitten" });
            r.Take(GameData.Items[Conditions.Antidote]);
            Eq(0, r.S.conditions.Count, "statuses left");
        });
        Test("Cutting Chai heals but never above max HP", () =>
        {
            var r = new Run(1);
            var chai = GameData.Items["cutting_chai"];
            r.S.hp = 10; r.Take(chai);
            Check(r.S.hp > 10, "chai didn't heal");
            r.S.hp = r.S.maxHp - 1; r.Take(chai);
            Eq(r.S.maxHp, r.S.hp, "hp after chai at nearly full");
        });
        Test("gear replaces the old piece and breaks after its fights", () =>
        {
            var r = RunOn(0);
            var w = GameData.Items.Values.First(i => i.slot == "weapon" && !i.boxOnly);
            r.Take(w);
            Eq(w.id, r.S.weapon.data.id, "equipped weapon");
            Buff(r.S);
            for (int i = 0; i < w.fights; i++) { var res = r.Fight(); Check(res.won, "buffed Bablu lost"); }
            Check(r.S.weapon == null, $"{w.id} survived {w.fights} fights");
        });
        Test("selling: junk is worthless, everything else Rs 50 or more", () =>
        {
            var r = new Run(1);
            foreach (var it in GameData.Items.Values)
                if (it.slot == "junk") Eq(0, r.SellValue(it), $"{it.id} sell value");
                else Check(r.SellValue(it) >= 50, $"{it.id} sells for {r.SellValue(it)}");
        });
    }

    // --- day flow ----------------------------------------------------------------

    static void DayTests()
    {
        Test("rent day: the landlord takes rent, never more than you have", () =>
        {
            int d = DayIndexOf(x => x.rent > 0);
            Check(d >= 0, "no rent day in days.json");
            int rent = GameData.Days[d].rent;
            var r = new Run(1); r.S.dayIndex = d; r.S.rupees = rent + 50; r.BeginDay();
            Eq(rent, r.RentPaid, "rent paid"); Eq(50, r.S.rupees, "rupees after rent");
            r = new Run(1); r.S.dayIndex = d; r.S.rupees = rent - 20; r.BeginDay();
            Eq(0, r.S.rupees, "rupees when short of rent");
        });
        Test("Robbed takes Rs 30 every morning", () =>
        {
            var r = new Run(1); r.S.rupees = 100; r.S.conditions.Add("robbed"); r.BeginDay();
            Eq(Conditions.RobbedPerDay, r.RobbedLoss, "robbed loss"); Eq(100 - Conditions.RobbedPerDay, r.S.rupees, "rupees");
        });
        Test("companions join on their day and stay behind at the border", () =>
        {
            var r = new Run(1);
            int borders = 0;
            while (!r.Finished)
            {
                r.BeginDay();
                if (r.DayNumber == 2) r.S.companion = "pinky_aunty"; // she joins through the Day 2 event card
                if (!string.IsNullOrEmpty(r.Day.companion))
                {
                    Eq(r.Day.companion, r.S.companion, $"Day {r.DayNumber} companion");
                    Eq(r.Day.companion, r.JoinedCompanion, $"Day {r.DayNumber} joined");
                }
                string had = r.S.companion;
                r.EndDay();
                if (r.Border != null)
                {
                    borders++;
                    Eq(had, r.LeftCompanion, $"companion left at border {r.Border}");
                    Check(r.S.companion == null, $"companion crossed border {r.Border}");
                }
            }
            Eq(3, borders, "borders crossed (Dilli > Gurugram > Noida > Daftar)");
        });
        Test("Toll Tax at each border unless you hold a FASTag", () =>
        {
            var r = new Run(1);
            r.S.fastags = 1;
            var seen = new List<(bool fastag, int toll)>();
            while (!r.Finished) { r.BeginDay(); r.EndDay(); if (r.Border != null) seen.Add((r.UsedFastag, r.S.tollDays)); }
            Check(seen[0].fastag && seen[0].toll == 0, "the FASTag wasn't used at the first border");
            Check(!seen[1].fastag && seen[1].toll == RunState.TollDays, $"no Toll Tax at the second border (tollDays {seen[1].toll})");
            Eq(0, r.S.fastags, "FASTags left");
        });
        Test("skill leases tick down and expire", () =>
        {
            var r = new Run(1);
            var sk = GameData.Skills.Values.First(s => s.lease >= 2);
            r.S.AddSkill(sk);
            for (int i = 0; i < sk.lease - 1; i++) { r.BeginDay(); r.EndDay(); }
            Check(r.S.skills.Any(o => o.data.id == sk.id && o.IsLastDay), "skill gone early or not on its last day");
            r.BeginDay();
            var expired = r.EndDay();
            Check(expired.Any(s => s.id == sk.id) && r.S.skills.All(o => o.data.id != sk.id), "skill outlived its lease");
        });
        Test("Baba's blessings: ~80% work, ~12% fizzle, ~8% backfire (pity after a dud), never fatal", () =>
        {
            var r = new Run(9);
            var sk = GameData.Skills.Values.First();
            var counts = new Dictionary<Blessing, int>();
            const int n = 20000;
            for (int i = 0; i < n; i++)
            {
                r.S.hp = 1; r.S.rupees = 30;
                var b = r.TakeBlessing(sk);
                counts[b] = counts.GetValueOrDefault(b) + 1;
                Check(r.S.hp >= 1, "a backfire killed Bablu");
                Check(r.S.rupees >= 0, "dakshina made rupees negative");
            }
            Between(78, 82, 100.0 * counts.GetValueOrDefault(Blessing.Granted) / n, "granted %");
            Between(10, 14, 100.0 * counts.GetValueOrDefault(Blessing.Fizzled) / n, "fizzled %");
            Between(6, 10, 100.0 * counts.GetValueOrDefault(Blessing.Backfired) / n, "backfired %");
        });
        Test("never two duds in a row from Baba", () =>
        {
            var r = new Run(11);
            var sk = GameData.Skills.Values.First();
            bool lastDud = false;
            for (int i = 0; i < 5000; i++)
            {
                bool dud = r.TakeBlessing(sk) != Blessing.Granted;
                Check(!(dud && lastDud), $"two duds in a row at try {i}");
                lastDud = dud;
            }
        });
        Test("Baba never offers the exact same cards twice in a row", () =>
        {
            var r = new Run(12);
            var rng = new Random(3);
            var last = new HashSet<string>();
            for (int i = 0; i < 500; i++)
            {
                var offer = r.S.OfferSkills(3, rng).Select(x => x.id).ToHashSet();
                Check(!offer.SetEquals(last), $"offer {i} repeated: {string.Join(",", offer)}");
                last = offer;
            }
        });
        Test("RunState.Clone is a deep copy", () =>
        {
            var r = new Run(1);
            r.S.AddSkill(GameData.Skills.Values.First());
            r.Take(GameData.Items.Values.First(i => i.slot == "weapon"));
            r.S.conditions.Add("shaved");
            var c = r.S.Clone();
            c.skills[0].daysLeft = 99; c.weapon.fightsLeft = 99; c.conditions.Add("robbed"); c.rupees = 1;
            Check(r.S.skills[0].daysLeft != 99, "skills shared");
            Check(r.S.weapon.fightsLeft != 99, "weapon shared");
            Check(!r.S.conditions.Contains("robbed"), "conditions shared");
            Check(r.S.rupees != 1, "rupees shared");
        });
        Test("level-ups raise every stat", () =>
        {
            var r = new Run(1);
            int hp = r.S.maxHp, atk = r.S.atk, def = r.S.def;
            r.S.xp = r.S.XpToNext;
            Eq(1, r.ConsumeLevelUps(), "level-ups");
            Check(r.S.maxHp > hp && r.S.atk > atk && r.S.def > def, "a stat didn't grow");
        });
    }

    // --- Amma's Nursing Home -----------------------------------------------------

    static void NursingHomeTests()
    {
        int last = GameData.Days.Count - 1;

        Test("losing with no insurance faints (no game over)", () =>
        {
            var r = RunOn(last); Weaken(r.S);
            var res = r.Fight();
            Check(!res.won && res.died && r.Fainted, "didn't faint");
            Check(!r.Dead, "run ended instead of a faint");
            Eq(0, r.S.hp, "hp after fainting");
        });
        Test("Health Insurance revives once at 50% HP, then you faint", () =>
        {
            var r = RunOn(last); Weaken(r.S); r.S.insuranceDays = 3;
            var res = r.Fight();
            Check(res.revived && !r.Fainted, "insurance didn't revive");
            Eq((int)Math.Round(r.S.maxHp * Run.ReviveHp), r.S.hp, "hp after the revive");
            Eq(0, r.S.insuranceDays, "insurance left");
            Weaken(r.S);
            Check(r.Fight().died && r.Fainted, "second loss didn't faint");
        });
        Test("wakes on yesterday's morning with yesterday's stuff, minus Rs 100, full HP", () =>
        {
            var r = new Run(4);
            r.BeginDay(); r.EndDay();                  // Day 1
            r.S.rupees = 500;
            r.BeginDay();                              // Day 2 morning: the snapshot we go back to
            int atk = r.S.atk, maxHp = r.S.maxHp;
            r.S.rupees = 50; r.S.conditions.Add("shaved");
            r.EndDay();
            r.BeginDay();                              // Day 3: lose
            Weaken(r.S);
            Check(r.Fight().died, "didn't lose");
            r.WakeAtNursingHome();
            Eq(1, r.S.dayIndex, "day index (Day 2)");
            Eq(500 - Run.NursingHomeBill, r.S.rupees, "rupees");
            Check(!r.S.conditions.Contains("shaved"), "kept a status picked up after that morning");
            Eq(atk, r.S.atk, "atk restored"); Eq(maxHp, r.S.hp, "hp (full)");
            Eq(1, r.S.faints, "faints"); Eq(1, r.S.zombieFights, "zombie fights");
            Check(!r.Fainted && !r.DayStarted, "still fainted / day still started");
        });
        Test("the Vaidya's jadi-booti cures every status at the nursing home", () =>
        {
            var r = new Run(1);
            r.S.conditions.UnionWith(new[] { "shaved", "robbed" });  // already sick that morning
            r.BeginDay(); Weaken(r.S); r.Fight(); r.WakeAtNursingHome();
            Eq(0, r.S.conditions.Count, "statuses after the nursing home");
        });
        Test("a Day 1 faint restarts Day 1", () =>
        {
            var r = RunOn(0); Weaken(r.S);
            r.Fight(); r.WakeAtNursingHome();
            Eq(0, r.S.dayIndex, "day index");
            Eq(200 - Run.NursingHomeBill, r.S.rupees, "rupees (pocket money minus the bill)");
        });
        Test("Amma's bill never makes rupees negative", () =>
        {
            var r = new Run(1); r.S.rupees = 40; r.BeginDay(); Weaken(r.S);
            r.Fight(); r.WakeAtNursingHome();
            Eq(0, r.S.rupees, "rupees");
        });
        Test("faints add up across the run", () =>
        {
            var r = RunOn(0);
            for (int i = 1; i <= 3; i++) { Weaken(r.S); r.Fight(); r.WakeAtNursingHome(); r.BeginDay(); Eq(i, r.S.faints, "faints"); }
        });
        Test("the zombie buff lasts exactly one fight", () =>
        {
            var r = RunOn(0); Weaken(r.S); r.Fight(); r.WakeAtNursingHome();
            r.BeginDay(); Buff(r.S);
            Check(r.Fight().won, "buffed zombie lost");
            Eq(0, r.S.zombieFights, "zombie fights after one fight");
        });
        Test("zombie Bablu lifesteals; normal Bablu doesn't", () =>
        {
            int Steals(bool zombie)
            {
                int n = 0;
                for (int seed = 1; seed <= 60; seed++)
                {
                    var s = new RunState { zombieFights = zombie ? 1 : 0 };
                    s.hp = s.maxHp / 2;
                    var res = AutoBattle.Simulate(s, new List<CombatantData> { GameData.Combatants["broker"] }, new Random(seed), 2);
                    int h = HeroIndex(res);
                    n += res.events.Count(e => e.type == BattleEventType.Heal && e.actor == h && e.target == h);
                }
                return n;
            }
            Check(Steals(true) > 0, "no lifesteal heals as a zombie");
            Eq(0, Steals(false), "self-heals without the buff, skills or companion");
        });
        Test("zombie Bablu un-dies exactly once", () =>
        {
            for (int seed = 1; seed <= 40; seed++)
            {
                var s = new RunState { zombieFights = 1 }; Weaken(s);
                var res = AutoBattle.Simulate(s, new List<CombatantData> { GameData.Combatants["babu"] }, new Random(seed), 8);
                int undeaths = res.events.Count(e => e.type == BattleEventType.Revive && e.target == HeroIndex(res));
                Eq(1, undeaths, $"un-deaths (seed {seed})");
            }
        });
        Test("Amma's blessing: each faint makes enemies 10% weaker, down to 60%", () =>
        {
            var r = RunOn(0);
            Eq(1f, r.MercyScale, "scale before any faint");
            float[] want = { 0.9f, 0.8f, 0.7f, 0.6f, 0.6f };
            foreach (var w in want)
            {
                Weaken(r.S); r.Fight(); r.WakeAtNursingHome(); r.BeginDay();
                Check(Math.Abs(r.MercyScale - w) < 0.001f, $"after {r.S.faints} faints: scale {r.MercyScale}, want {w}");
            }
            Eq(40, r.MercyPercent, "percent shown at the floor");
        });
        Test("Amma's blessing weakens the enemies Bablu actually fights", () =>
        {
            int d = DayIndexOf(x => EnemyIds(x).Contains("ceo"));
            int Hp(int mercy) { var r = new Run(5); r.DebugSkipTo(d + 1); r.S.mercy = mercy; return r.Fight().units.First(u => u.id == "ceo").maxHp; }
            Check(Hp(2) < Hp(0), $"Lalaji HP with 2 faints {Hp(2)} vs none {Hp(0)}");
        });
        Test("Amma's blessing survives the rollback and ends once Bablu gets past the wall", () =>
        {
            var r = new Run(4);
            r.BeginDay(); r.EndDay();                  // Day 1
            r.BeginDay(); r.EndDay();                  // Day 2
            r.BeginDay(); Weaken(r.S); r.Fight();      // faint on Day 3 (the wall)
            r.WakeAtNursingHome();                     // back to Day 2
            Eq(1, r.S.mercy, "mercy after the faint");
            Eq(2, r.S.furthestDay, "wall (Day 3 index)");
            r.BeginDay(); r.EndDay();                  // replay Day 2: still before the wall
            Eq(1, r.S.mercy, "mercy on the way back to the wall");
            r.BeginDay(); r.EndDay();                  // get past Day 3
            Eq(0, r.S.mercy, "mercy after passing the wall");
            Eq(1f, r.MercyScale, "scale after passing the wall");
        });
        Test("the zombie retry is easier than the first try (Day 9, 300 seeds)", () =>
        {
            int d = DayIndexOf(x => EnemyIds(x).Contains("rwa_aunty"));
            int Wins(bool zombie)
            {
                int w = 0;
                for (int seed = 1; seed <= 300; seed++)
                {
                    var r = new Run(seed); r.DebugSkipTo(d + 1);
                    r.S.zombieFights = zombie ? 1 : 0;
                    if (r.Fight().won) w++;
                }
                return w;
            }
            int plain = Wins(false), zomb = Wins(true);
            Check(zomb > plain, $"zombie wins {zomb}/300, normal {plain}/300");
        });
    }

    // --- battle ------------------------------------------------------------------

    static void BattleTests()
    {
        var fightDays = Enumerable.Range(0, GameData.Days.Count).Where(i => GameData.Days[i].type == "fight" || GameData.Days[i].type == "boss").ToList();

        Test("same seed, same fight (replays are deterministic)", () =>
        {
            foreach (int d in fightDays)
            {
                var a = new Run(77); a.DebugSkipTo(d + 1);
                var b = new Run(77); b.DebugSkipTo(d + 1);
                var ra = a.Fight(); var rb = b.Fight();
                Eq(ra.events.Count, rb.events.Count, $"Day {d + 1} event count");
                Eq(ra.won, rb.won, $"Day {d + 1} result");
                for (int i = 0; i < ra.events.Count; i++)
                    Check(ra.events[i].type == rb.events[i].type && ra.events[i].amount == rb.events[i].amount, $"Day {d + 1} event {i} differs");
            }
        });
        Test("every fight ends by the hard cap with a clean result (all days x 80 seeds)", () =>
        {
            foreach (int d in fightDays)
                for (int seed = 1; seed <= 80; seed++)
                {
                    var r = new Run(seed); r.DebugSkipTo(d + 1);
                    var res = r.Fight();
                    string at = $"Day {d + 1} seed {seed}";
                    Check(res.rounds >= 1 && res.rounds <= AutoBattle.HardCap, $"{at}: {res.rounds} rounds");
                    Check(res.won ? res.loseReason == null : res.loseReason == "hp" || res.loseReason == "time", $"{at}: won={res.won} reason={res.loseReason}");
                    foreach (var e in res.events)
                    {
                        Check(e.actor < res.units.Count && e.target < res.units.Count, $"{at}: event points at unit {e.actor}/{e.target} of {res.units.Count}");
                        Check(e.hpAfter >= 0, $"{at}: negative hp in an event");
                    }
                    foreach (var c in res.inflicted) Check(Conditions.All.ContainsKey(c), $"{at}: unknown status {c}");
                }
        });
        Test("Lalaji summons the CFO and CTO below 50% (once)", () =>
        {
            int d = DayIndexOf(x => EnemyIds(x).Contains("ceo"));
            int summonFights = 0;
            for (int seed = 1; seed <= 150; seed++)
            {
                var r = new Run(seed); r.DebugSkipTo(d + 1);
                var res = r.Fight();
                var summons = res.events.Where(e => e.type == BattleEventType.Summon).ToList();
                if (summons.Count == 0) continue;
                summonFights++;
                Check(summons.Count <= 2, $"seed {seed}: {summons.Count} summons");
                foreach (var e in summons) Check(new[] { "cfo_bunty", "cto_pappu" }.Contains(res.units[e.target].id), $"seed {seed}: summoned {res.units[e.target].id}");
            }
            Check(summonFights > 0, "Lalaji never summoned in 150 fights");
        });
        Test("KHATRA makes the same enemy tougher", () =>
        {
            int Hp(int spice) => AutoBattle.Simulate(new RunState(), new List<CombatantData> { GameData.Combatants["broker"] }, new Random(1), spice)
                                           .units.First(u => u.id == "broker").maxHp;
            Check(Hp(8) > Hp(4) && Hp(4) > Hp(1), $"broker HP at KHATRA 1/4/8: {Hp(1)}/{Hp(4)}/{Hp(8)}");
        });
        Test("statuses bleed Bablu every round", () =>
        {
            var s = new RunState(); s.conditions.Add("shaved");
            var res = AutoBattle.Simulate(s, new List<CombatantData> { GameData.Combatants["broker"] }, new Random(3), 1);
            int h = HeroIndex(res);
            Check(res.events.Any(e => e.type == BattleEventType.Hit && e.target == h && (e.label ?? "").Contains(Conditions.Name("shaved"))), "no status bleed event");
        });
        Test("a KADAK opener wins more fights than a pheeka one (Day 9, 300 seeds)", () =>
        {
            int d = DayIndexOf(x => EnemyIds(x).Contains("rwa_aunty"));
            int Wins(int opener)
            {
                int w = 0;
                for (int seed = 1; seed <= 300; seed++) { var r = new Run(seed); r.DebugSkipTo(d + 1); if (r.Fight(opener).won) w++; }
                return w;
            }
            int pheeka = Wins(0), theek = Wins(1), kadak = Wins(2);
            Check(kadak > theek && theek >= pheeka, $"wins kadak {kadak}, theek {theek}, pheeka {pheeka}");
        });
        Test("the opener shouts its buff, and only when earned", () =>
        {
            int d = DayIndexOf(x => EnemyIds(x).Contains("broker"));
            bool Shouts(int opener, string text)
            {
                var r = new Run(3); r.DebugSkipTo(d + 1);
                return r.Fight(opener).events.Any(e => e.type == BattleEventType.Shout && (e.label ?? "").Contains(text));
            }
            Check(Shouts(2, "KADAK!"), "no KADAK shout");
            Check(Shouts(1, "Theek hai"), "no theek shout");
            Check(!Shouts(0, "KADAK!") && !Shouts(0, "Theek hai"), "pheeka still got a buff");
        });
        Test("a won fight pays rupees and EXP", () =>
        {
            var r = RunOn(0); Buff(r.S);
            int rs = r.S.rupees;
            var res = r.Fight();
            Check(res.won && res.rupeesGained > 0 && res.xpGained > 0, $"won={res.won} rs={res.rupeesGained} xp={res.xpGained}");
            Eq(rs + res.rupeesGained, r.S.rupees, "rupees after the win");
        });
    }

    // --- stickers ----------------------------------------------------------------

    static void StickerTests()
    {
        Test("unlocking: new once, then never again; unknown ids are ignored", () =>
        {
            Album.Reset();
            Check(Album.Unlock("broker") != null, "first unlock returned null");
            Check(Album.Unlock("broker") == null, "second unlock counted as new");
            Check(Album.Unlock("not_a_sticker") == null, "unknown sticker unlocked");
            Eq(1, Album.Count, "collected");
            Check(Album.NewThisRun.Contains("broker"), "not marked NEW");
            Album.StartRun();
            Check(Album.NewThisRun.Count == 0 && Album.Has("broker"), "a new run lost the sticker or kept NEW");
        });
        Test("the album survives a save and a load (collected across runs)", () =>
        {
            Album.Reset();
            string saved = null;
            Album.Save = x => saved = x;
            Album.Unlock("babu"); Album.Unlock("kadak");
            Album.Save = null;
            Album.Reset();
            Album.Load(saved + ",junk");
            Check(Album.Has("babu") && Album.Has("kadak") && Album.Count == 2, $"loaded '{Album.Serialize()}' from '{saved}'");
        });
        Test("beating Lalaji also earns his board", () =>
        {
            var ids = Album.ForDefeated(new[] { "ceo" }).ToList();
            Check(ids.Contains("ceo") && ids.Contains("cfo_bunty") && ids.Contains("cto_pappu"), string.Join(",", ids));
        });
        Test("the run state earns friends, the bald head and the Laathi", () =>
        {
            var st = new RunState { companion = "sheru" };
            st.conditions.Add("shaved");
            st.weapon = new OwnedItem { data = GameData.Items["laathi"], fightsLeft = 3 };
            var ids = Album.ForState(st).ToList();
            Check(ids.Contains("sheru") && ids.Contains("bald_bablu") && ids.Contains("laathi"), string.Join(",", ids));
        });
        Test("every enemy and companion in days.json has a sticker", () =>
        {
            var missing = GameData.Days.SelectMany(d => EnemyIds(d).Concat(string.IsNullOrEmpty(d.companion) ? new string[0] : new[] { d.companion }))
                                       .Concat(new[] { "pinky_aunty", "cfo_bunty", "cto_pappu" }).Distinct().Where(id => !Album.Exists(id)).ToList();
            Check(missing.Count == 0, "no sticker for: " + string.Join(", ", missing));
        });
        Test("every sticker has its art and its blank (Tools/make_stickers.py)", () =>
        {
            string dir = Path.Combine(root, "Assets/_Game/Resources/Art/Stickers");
            var missing = Album.All.SelectMany(st => new[] { st.id, st.id + "_blank" }).Where(n => !File.Exists(Path.Combine(dir, n + ".png"))).ToList();
            Check(missing.Count == 0, "missing: " + string.Join(", ", missing));
        });
        Album.Reset();
    }

    // --- balance -----------------------------------------------------------------

    static void BalanceTests()
    {
        var st = new SimStats();
        RunSimulator.Simulate(400, 1, true, st);
        Console.WriteLine($"        400 runs: finished {st.FinishedPct:0}%, clean win {st.CleanWinPct:0}%, {st.FaintsPerRun:0.0} faints/run; " +
                          "faint % per day: " + string.Join(" ", Enumerable.Range(0, st.deaths.Length).Select(i => $"D{i + 1}:{st.FaintPct(i):0}")));

        Test("every run reaches the stamp (the nursing home has no dead ends)", () => Eq(100f, st.FinishedPct, "finished %"));
        Test("clean win rate (no faints) stays in the 30-60% band (target ~40%)", () => Between(30, 60, st.CleanWinPct, "clean win %"));
        Test("at most 1.5 faints per run on average", () => Between(0, 1.5, st.FaintsPerRun, "faints per run"));
        Test("no wall: no day floors more than 35% of visits", () =>
        {
            for (int i = 0; i < st.deaths.Length; i++) Between(0, 35, st.FaintPct(i), $"Day {i + 1} faint %");
        });
        Test("judge-friendly opening: Days 1-4 each faint under 15% per visit", () =>
        {
            for (int i = 0; i < 4; i++) Between(0, 15, st.FaintPct(i), $"Day {i + 1} faint %");
        });
        Test("the Babu (Day 11) is a real final boss: 5% or more of visits faint", () =>
            Between(5, 100, st.FaintPct(st.deaths.Length - 1), "Day 11 faint %"));

        var lazy = new SimStats();
        RunSimulator.Simulate(200, 1, false, lazy);
        Console.WriteLine($"        200 never-shop runs: finished within 10 faints {lazy.FinishedPct:0}%, clean win {lazy.CleanWinPct:0}%, {lazy.FaintsPerRun:0.0} faints/run");
        Test("a player who never opens JhatPat finishes within 10 faints", () =>
            Check(lazy.FinishedPct >= 95, $"only {lazy.FinishedPct:0}% finish; faint % per day: " +
                  string.Join(" ", Enumerable.Range(0, lazy.deaths.Length).Where(i => lazy.FaintPct(i) >= 10).Select(i => $"D{i + 1}:{lazy.FaintPct(i):0}"))));
    }
}
