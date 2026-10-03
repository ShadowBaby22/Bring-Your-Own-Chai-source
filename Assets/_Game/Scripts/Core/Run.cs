using System;
using System.Collections.Generic;
using System.Linq;

namespace KadakChai
{
    /// <summary>A skill you hold, with its lease countdown (GDD §3.1).</summary>
    public class OwnedSkill
    {
        public SkillData data;
        public int daysLeft;
        public bool IsLastDay => daysLeft == 1;
    }

    /// <summary>A weapon or armour you hold, with its durability countdown in fights.</summary>
    public class OwnedItem
    {
        public ItemData data;
        public int fightsLeft;
        public bool IsLastFight => fightsLeft == 1;
    }

    /// <summary>Everything about the current run. Plain data + small helpers.</summary>
    public class RunState
    {
        public int dayIndex;                 // 0-based into GameData.Days
        public int level = 1, xp;
        public int maxHp = 120, hp = 120, atk = 16, def = 4;
        public int aqi = Aqi.Start;
        public readonly List<OwnedSkill> skills = new();
        public readonly HashSet<string> flags = new();
        public readonly HashSet<string> usedCards = new(); // event-pool cards already seen this run

        // JhatPat shop (GDD: Weapons and the JhatPat shop, Rupees, Map and toll tax)
        public int rupees = 200;            // pocket money from Mummy
        public int earned, spent;           // for the simulator's report
        public OwnedItem weapon, armour;
        public int fastags;                 // each one skips one Toll Tax
        public int tollDays;                // Toll Tax debuff: all stats -15% while > 0
        public readonly List<ItemData> shop = new();

        public const float TollPenalty = 0.15f;
        public const int TollDays = 2;
        public bool Ranged => weapon?.data.type == "ranged";
        public int AtkTotal => atk + (weapon?.data.atk ?? 0);
        public int DefTotal => def + (armour?.data.def ?? 0);

        public int XpToNext => 30 + 20 * (level - 1);
        public bool HasAunty => flags.Contains("aunty");

        public void AddSkill(SkillData s)
        {
            skills.RemoveAll(o => o.data.id == s.id); // re-picking renews the lease
            skills.Add(new OwnedSkill { data = s, daysLeft = s.lease });
        }

        /// <summary>Up to n random skills you don't currently hold.</summary>
        public List<SkillData> OfferSkills(int n, Random rng)
        {
            var pool = GameData.Skills.Values.Where(s => skills.All(o => o.data.id != s.id)).ToList();
            var offer = new List<SkillData>();
            while (offer.Count < n && pool.Count > 0)
            {
                int i = rng.Next(pool.Count);
                offer.Add(pool[i]);
                pool.RemoveAt(i);
            }
            return offer;
        }
    }

    public enum DayKind { Fight, Event, Chai, Boss }

    /// <summary>
    /// Drives the 11-day lease. Engine-free: the Unity GameController calls these in order,
    /// and the simulator calls them in a loop to balance the game.
    /// </summary>
    public class Run
    {
        public readonly RunState S = new();
        public readonly Random Rng;
        public int Attempts;     // fights lost on the current day

        public Run(int seed) { Rng = new Random(seed); }

        /// <summary>Set by EndDay when the next day is in a new zone: (from, to). Null otherwise.</summary>
        public (string from, string to)? Border { get; private set; }
        /// <summary>True if a FASTag was used at the last border instead of taking the Toll Tax.</summary>
        public bool UsedFastag { get; private set; }
        /// <summary>Rupees the landlord took this morning (0 if it isn't rent day).</summary>
        public int RentPaid { get; private set; }

        public bool Finished => S.dayIndex >= GameData.Days.Count;
        public DayData Day => GameData.Days[Math.Min(S.dayIndex, GameData.Days.Count - 1)];
        public int DayNumber => S.dayIndex + 1;
        public int TotalDays => GameData.Days.Count;
        public DayKind Kind => Day.type switch { "event" => DayKind.Event, "chai" => DayKind.Chai, "boss" => DayKind.Boss, _ => DayKind.Fight };
        public bool IsFightDay => Kind == DayKind.Fight || Kind == DayKind.Boss;
        public Card IntroCard { get; private set; }
        public string WinCardId => (IntroCard?.id ?? Day.card) + "_win";
        List<CombatantData> todayEnemies = new();
        public int DayInZone
        {
            get
            {
                int n = 0;
                for (int i = S.dayIndex - 1; i >= 0 && GameData.Days[i].zone == Day.zone; i--) n++;
                return n;
            }
        }

        /// <summary>Rolls today's AQI. Returns true if the GRAP stage changed (show a headline).</summary>
        public bool BeginDay()
        {
            Attempts = 0;
            DayStarted = true;
            ResolveDay();
            RentPaid = Math.Min(S.rupees, Day.rent);
            S.rupees -= RentPaid;
            var old = Aqi.StageFor(S.aqi);
            S.aqi = Aqi.Roll(S.aqi, Day.aqiDelta, Rng);
            return Aqi.StageFor(S.aqi) != old;
        }

        public List<CombatantData> Enemies() => new(todayEnemies);

        /// <summary>True between BeginDay and EndDay: today has been played, so "next" means tomorrow.</summary>
        public bool DayStarted { get; private set; }

        /// <summary>Every enemy id that could show up in the next day to play (for the shop's hint).</summary>
        public List<string> NextEnemyIds()
        {
            var ids = new List<string>();
            int next = S.dayIndex + (DayStarted ? 1 : 0);
            if (next >= GameData.Days.Count) return ids;
            var d = GameData.Days[next];
            if (d.enemies != null) ids.AddRange(d.enemies);
            if (d.enemyGroups != null) foreach (var g in d.enemyGroups) foreach (var id in g.Split('+')) if (!ids.Contains(id.Trim())) ids.Add(id.Trim());
            return ids;
        }

        /// <summary>Picks today's random bits once (so retries face the same thing): enemy group and event card.</summary>
        void ResolveDay()
        {
            var day = Day;
            string[] ids = day.enemies ?? new string[0];
            IntroCard = GameData.CardOrNull(day.card);
            if (day.enemyGroups != null && day.enemyGroups.Length > 0)
            {
                ids = day.enemyGroups[Rng.Next(day.enemyGroups.Length)].Split('+');
                IntroCard = GameData.CardOrNull("fight_" + ids[0].Trim()) ?? IntroCard;
            }
            todayEnemies = new List<CombatantData>();
            foreach (var id in ids)
                if (GameData.Combatants.TryGetValue(id.Trim(), out var c)) todayEnemies.Add(c);

            if (!string.IsNullOrEmpty(day.pool))
            {
                var card = GameData.RandomCard(day.pool, Rng, S.usedCards);
                if (card != null) { IntroCard = card; S.usedCards.Add(card.id); }
            }
        }

        /// <summary>Simulates today's fight. On a win, applies HP and EXP. On a loss, nothing changes (retry).</summary>
        public BattleResult Fight()
        {
            var result = AutoBattle.Simulate(S, Enemies(), Rng, Attempts);
            if (result.won)
            {
                S.hp = Math.Max(1, Math.Min(S.maxHp, result.heroHpAfter));
                result.xpGained = Enemies().Sum(e => e.xp);
                S.xp += result.xpGained;
                result.rupeesGained = FightReward();
                Earn(result.rupeesGained);
                result.broken = WearGear();
            }
            else Attempts++;
            return result;
        }

        /// <summary>Boss: ₹300. Otherwise ₹60-150 depending on the enemies (2x their EXP).</summary>
        int FightReward()
        {
            if (Kind == DayKind.Boss) return 300;
            int xp = Enemies().Sum(e => e.xp);
            return Math.Clamp((int)Math.Round(xp * 2f / 5f) * 5, 60, 150);
        }

        void Earn(int rs) { S.rupees += rs; S.earned += rs; }

        /// <summary>Each won fight uses up one fight of every gear item. Returns the items that broke.</summary>
        List<ItemData> WearGear()
        {
            var broken = new List<ItemData>();
            if (S.weapon != null && --S.weapon.fightsLeft <= 0) { broken.Add(S.weapon.data); S.weapon = null; }
            if (S.armour != null && --S.armour.fightsLeft <= 0) { broken.Add(S.armour.data); S.armour = null; }
            return broken;
        }

        // --- JhatPat ------------------------------------------------------------

        /// <summary>New stock for a new shop visit: 3 random items plus Cutting Chai. The Mystery Box is always there.</summary>
        public void RestockShop()
        {
            S.shop.Clear();
            var pool = GameData.Items.Values.Where(i => !i.boxOnly && i.slot != "consumable").ToList();
            while (S.shop.Count < 3 && pool.Count > 0)
            {
                int i = Rng.Next(pool.Count);
                S.shop.Add(pool[i]);
                pool.RemoveAt(i);
            }
            if (GameData.Items.TryGetValue("cutting_chai", out var chai)) S.shop.Add(chai);
        }

        public bool CanAfford(int price) => S.rupees >= price;

        /// <summary>Buys from today's stock. Gear and FASTags leave the shelf; chai can be bought again.</summary>
        public List<string> Buy(ItemData it)
        {
            if (!CanAfford(it.price)) return null;
            S.rupees -= it.price;
            S.spent += it.price;
            if (it.slot != "consumable") S.shop.Remove(it);
            return Take(it);
        }

        /// <summary>Puts an item to use: equips gear (replacing the old one), drinks chai, files a FASTag.</summary>
        public List<string> Take(ItemData it)
        {
            var notes = new List<string>();
            switch (it.slot)
            {
                case "weapon":
                    S.weapon = new OwnedItem { data = it, fightsLeft = it.fights };
                    notes.Add($"Equipped {it.name} ({(it.type == "ranged" ? "Ranged" : "Close")}, +{it.atk} ATK, {it.fights} fights)");
                    break;
                case "armour":
                    S.armour = new OwnedItem { data = it, fightsLeft = it.fights };
                    notes.Add($"Equipped {it.name} (+{it.def} DEF, {it.fights} fights)");
                    break;
                case "pass":
                    S.fastags++;
                    notes.Add("FASTag ready for the next toll");
                    break;
                case "consumable":
                    int before = S.hp;
                    S.hp = Math.Min(S.maxHp, S.hp + (int)Math.Round(S.maxHp * it.power));
                    notes.Add($"+{S.hp - before} HP");
                    break;
            }
            return notes;
        }

        /// <summary>Mystery Box: pay, then draw one item by boxWeight. The item is NOT applied yet: the player decides.</summary>
        public ItemData OpenBox()
        {
            if (!CanAfford(GameData.BoxPrice)) return null;
            S.rupees -= GameData.BoxPrice;
            S.spent += GameData.BoxPrice;
            return DrawBox(Rng);
        }

        public static ItemData DrawBox(Random rng)
        {
            var pool = GameData.Items.Values.Where(i => i.boxWeight > 0).ToList();
            int roll = rng.Next(pool.Sum(i => i.boxWeight));
            foreach (var it in pool) { if (roll < it.boxWeight) return it; roll -= it.boxWeight; }
            return pool.LastOrDefault();
        }

        /// <summary>Percent chance of an item in the Mystery Box.</summary>
        public static float BoxOdds(ItemData it)
        {
            int total = GameData.Items.Values.Sum(i => i.boxWeight);
            return total == 0 ? 0 : 100f * it.boxWeight / total;
        }

        /// <summary>Sell an unwanted box item back to JhatPat for half its value (min ₹50, junk ₹0).</summary>
        public int SellValue(ItemData it) => it.slot == "junk" ? 0 : Math.Max(50, (it.price > 0 ? it.price : 500) / 2);
        public void Sell(ItemData it) => Earn(SellValue(it));

        /// <summary>Debug: jump to a later day with roughly the stats a normal run would have by then.</summary>
        public void DebugSkipTo(int day)
        {
            S.dayIndex = Math.Clamp(day - 1, 0, GameData.Days.Count - 1);
            ResolveDay();
            S.xp = 30 * S.dayIndex + 20;
            ConsumeLevelUps();
            if (day > 2) S.flags.Add("aunty");
            S.rupees = 200 + 120 * S.dayIndex;
            foreach (var s in S.OfferSkills(3, Rng)) S.AddSkill(s);
            S.hp = S.maxHp;
        }

        /// <summary>"Come Tomorrow": retry the same day at full HP.</summary>
        public void RetryDay() => S.hp = S.maxHp;

        /// <summary>Applies an event choice (0 = A, 1 = B). Returns (result text, change notes).</summary>
        public (string text, List<string> notes) Choose(int option)
        {
            var card = IntroCard;
            if (card == null) return ("", new List<string>());
            string effect = option == 0 ? card.effectA : card.effectB;
            string text = option == 0 ? card.resultA : card.resultB;
            return (text, Effects.Apply(effect, S, Rng));
        }

        /// <summary>Spends EXP on level-ups. Each one should be followed by a skill pick.</summary>
        public int ConsumeLevelUps()
        {
            int n = 0;
            while (S.xp >= S.XpToNext)
            {
                S.xp -= S.XpToNext;
                S.level++;
                S.maxHp += 12; S.hp += 12;
                S.atk += 3; S.def += 1;
                n++;
            }
            return n;
        }

        /// <summary>End of day: tick every skill's lease. Returns the skills whose lease ended.</summary>
        public List<SkillData> EndDay()
        {
            var expired = new List<SkillData>();
            foreach (var o in S.skills) o.daysLeft--;
            foreach (var o in S.skills) if (o.daysLeft <= 0) expired.Add(o.data);
            S.skills.RemoveAll(o => o.daysLeft <= 0);
            if (S.tollDays > 0) S.tollDays--;

            string from = Day.zone;
            S.dayIndex++;
            DayStarted = false;
            Border = null; UsedFastag = false;
            if (!Finished && Day.zone != from)
            {
                // Toll Tax at every border: all stats down for a couple of days, unless you have a FASTag.
                Border = (from, Day.zone);
                if (S.fastags > 0) { S.fastags--; UsedFastag = true; }
                else S.tollDays = RunState.TollDays;
            }
            return expired;
        }
    }
}
