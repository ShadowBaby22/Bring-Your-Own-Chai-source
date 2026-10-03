using System;
using System.Collections.Generic;
using System.Linq;

namespace KadakChai
{
    public enum BattleEventType { Round, Hit, Miss, Dodge, Immune, Heal, Stun, Shout, Death, Revive }

    /// <summary>One beat of a fight. The view replays these with animations; the sim just counts them.</summary>
    public class BattleEvent
    {
        public BattleEventType type;
        public int actor = -1, target = -1; // indices into BattleResult.units
        public int amount, hpAfter;
        public string label;                 // skill name or shout text
        public bool big;                     // skill hit: bigger popup
        public bool ranged;                  // thrown/fired: the view flies a projectile instead of a lunge
        public string projectile;            // Art/UI sprite for the projectile ("item_cork_ball", "shot_ceo")
    }

    public class UnitInfo
    {
        public string id, name, hpLabel, color;
        public bool isPlayerSide, isPet;
        public int maxHp, hp;
    }

    public class BattleResult
    {
        public bool won;
        public int rounds;
        public string loseReason;            // "hp" or "time"
        public int heroHpAfter;
        public int xpGained;
        public int rupeesGained;
        public bool revived;                    // lost, but Health Insurance brought Bablu back (fight again)
        public bool died;                       // lost with no insurance: the run is over
        public string killer;                   // name of whoever landed the last hit
        public List<string> inflicted = new();  // lasting statuses picked up in this fight (Conditions.cs)
        public List<ItemData> broken = new();   // gear that broke after this win
        public readonly List<UnitInfo> units = new();
        public readonly List<BattleEvent> events = new();
    }

    /// <summary>
    /// Fully automatic fight, simulated instantly (GDD §4). The player makes no inputs;
    /// all decisions happen between fights. Deterministic for a given Random.
    /// </summary>
    public class AutoBattle
    {
        public const int MaxRounds = 15;          // after this: AQI 999 sudden death until someone drops
        public const int HardCap = 40;
        public const float SuddenDeathPct = 0.12f; // of max HP, everyone, every sudden-death round
        // Khatra level (Balatro-style ante, 1-8): every level above 1 adds this much to enemy HP and ATK.
        public const float SpiceHp = 0.08f, SpiceAtk = 0.10f;
        public const float RangedPenalty = 0.8f;  // ranged attacks do 20% less damage
        public const float CloseBonus = 1.25f;    // close attacks do 25% more to ranged targets
        static readonly HashSet<string> Uncles = new() { "scooty_uncle", "auto_bhaiya", "guard", "broker", "babu", "sector_cabbie" };

        readonly RunState s;
        readonly Random rng;
        readonly BattleResult r = new();
        readonly List<Combatant> units = new();
        readonly List<Combatant> foes = new();
        readonly List<SkillData> active = new();
        Combatant hero, aunty;
        ItemData weapon;
        readonly HashSet<string> cond = new();  // Bablu's statuses: the ones he came in with plus any picked up here
        GrapStage stage;
        bool ignoreAqi;
        int round, shieldHits, spice;
        float dodge;

        AutoBattle(RunState s, Random rng, int spice) { this.s = s; this.rng = rng; this.spice = Math.Max(1, spice); }

        /// <param name="spice">Khatra level 1-8 of the day: scales enemy HP and ATK.</param>
        public static BattleResult Simulate(RunState s, List<CombatantData> enemies, Random rng, int spice = 1) =>
            new AutoBattle(s, rng, spice).Run(enemies);

        BattleResult Run(List<CombatantData> enemies)
        {
            Setup(enemies);

            bool heroSkipShown = false, foeSkipShown = false, suddenShown = false;
            for (round = 1; round <= HardCap; round++)
            {
                Emit(BattleEventType.Round, amount: round);
                if (round > MaxRounds)
                {
                    // AQI 999: no more stalling. Everyone chokes until someone drops (enemies first).
                    if (!suddenShown) { suddenShown = true; Shout(null, "AQI 999! Sudden death: everyone chokes on the smog."); }
                    foreach (var u in foes.Where(f => f.Alive).Append(hero).ToList())
                    {
                        int dmg = Math.Max(1, (int)Math.Ceiling(u.maxHp * SuddenDeathPct));
                        u.hp = Math.Max(0, u.hp - dmg);
                        Emit(BattleEventType.Hit, null, u, dmg, u.hp, "Smog");
                        if (u.hp == 0) { if (u == hero) r.killer = "the smog"; OnZeroHp(u); }
                        if (Decided()) break;
                    }
                    if (Decided()) break;
                }

                bool oddEven = stage == GrapStage.Grap4;
                bool heroTurn = !(oddEven && !ignoreAqi && round % 2 == 0);
                bool foeTurn = !(oddEven && round % 2 == 1);

                if (heroTurn) HeroAct();
                else if (!heroSkipShown) { heroSkipShown = true; Shout(hero, "Odd-Even! Your car stays home this round."); }
                if (Decided()) break;

                if (aunty != null && round % 3 == 0) AuntyAct();
                if (Decided()) break;

                if (foeTurn) foreach (var f in foes.ToList()) { if (f.Alive) EnemyAct(f); if (Decided()) break; }
                else if (!foeSkipShown) { foeSkipShown = true; Shout(null, "Odd-Even! Enemies wait their turn."); }
                if (Decided()) break;

                foreach (var u in units) u.TickStatuses();
                if (cond.Count > 0 && hero.Alive)
                {
                    // Lasting statuses bleed every round, like Pokémon poison (change log), until Pudin Hara.
                    int stacks = cond.Count + (cond.Contains("bitten") ? 1 : 0);
                    int dmg = Math.Max(1, (int)Math.Ceiling(hero.maxHp * Conditions.PoisonPerRound * stacks));
                    hero.hp = Math.Max(0, hero.hp - dmg);
                    Emit(BattleEventType.Hit, null, hero, dmg, hero.hp, string.Join(" + ", cond.Select(Conditions.Name)));
                    if (hero.hp == 0) { r.killer = Conditions.Name(cond.First()); OnZeroHp(hero); }
                    if (Decided()) break;
                }
            }

            r.rounds = Math.Min(round, HardCap);
            r.won = hero.Alive && foes.All(f => !f.Alive);
            r.loseReason = r.won ? null : hero.Alive ? "time" : "hp";
            r.heroHpAfter = hero.hp;
            return r;
        }

        void Setup(List<CombatantData> enemies)
        {
            stage = Aqi.StageFor(s.aqi);
            foreach (var o in s.skills) active.Add(o.data);
            ignoreAqi = active.Any(k => k.kind == "ignore_aqi");

            weapon = s.weapon?.data;
            float toll = s.tollDays > 0 ? 1f - RunState.TollPenalty : 1f;
            int heroMax = (int)Math.Round(s.maxHp * toll);
            hero = AddUnit(new Combatant(units.Count, "new_kid", GameData.HeroName, true, false, Math.Min(s.hp, heroMax), heroMax, s.AtkTotal * toll, s.DefTotal * toll));
            hero.ranged = s.Ranged;
            if (s.HasAunty && GameData.Combatants.TryGetValue("pinky_aunty", out var ad))
                aunty = AddUnit(new Combatant(units.Count, ad.id, ad.name, true, true, 1, 1, ad.atk, 0, ad));
            float hpMul = 1f + SpiceHp * (spice - 1), atkMul = 1f + SpiceAtk * (spice - 1);
            foreach (var e in enemies)
            {
                int hp = (int)Math.Round(e.hp * hpMul);
                foes.Add(AddUnit(new Combatant(units.Count, e.id, e.name, false, false, hp, hp, e.atk * atkMul, e.def, e) { ranged = e.Ranged }));
            }

            if (s.tollDays > 0) Shout(hero, $"Toll Tax! All stats -{RunState.TollPenalty * 100:0}% ({s.tollDays} day{(s.tollDays > 1 ? "s" : "")} left).");
            foreach (var c in s.conditions) { cond.Add(c); ApplyCondition(c); }
            if (cond.Contains("grilled"))
            {
                int g = (int)Math.Ceiling(hero.maxHp * Conditions.GrilledHp);
                hero.hp = Math.Max(1, hero.hp - g);
                Emit(BattleEventType.Hit, null, hero, g, hero.hp, "Interrogated");
            }

            // Pre-fight gimmicks
            if (foes.Any(f => f.Ai == "firecracker") && stage < GrapStage.Grap4)
            {
                stage++;
                Shout(null, $"Firecrackers! AQI spikes to {Aqi.StageName(stage)}.");
            }
            if (foes.Any(f => f.Ai == "monkey") && s.skills.Count > 0)
            {
                var stolen = s.skills.OrderBy(o => o.daysLeft).First().data;
                active.Remove(stolen);
                Shout(null, $"A monkey stole your {stolen.name}!");
            }
            foreach (var b in foes.Where(f => f.Ai == "babu"))
            {
                b.Add(new StatusEffect(StatusKind.Invulnerable, 2));
                Shout(b, "Lunch Break! Come back in 2 rounds.");
            }


            foreach (var k in active)
            {
                if (k.kind == "start_buff") { hero.Add(new StatusEffect(StatusKind.AtkUp, k.every, k.power)); Shout(hero, k.name + "!"); }
                if (k.kind == "half_damage") shieldHits = k.every;
                if (k.kind == "dodge") dodge += k.chance;
            }

            // Ranged units get one free shot before Round 1 (hero first).
            if (hero.ranged && PickTarget() is { } first)
            {
                bool yorker = weapon?.perk == "yorker";
                Attack(hero, first, yorker ? weapon.power : 1f, yorker ? "Yorker!" : "Opening shot!", yorker);
                if (yorker && first.Alive && Roll(weapon.chance))
                {
                    first.Add(new StatusEffect(StatusKind.Stunned, 1));
                    Emit(BattleEventType.Stun, hero, first, label: "Clean bowled! Stunned.");
                }
            }
            foreach (var f in foes.Where(f => f.ranged && f.Alive).ToList())
                if (hero.Alive) Attack(f, hero, 1f, f.data?.shot ?? "Opening shot!", false);
        }

        Combatant AddUnit(Combatant c)
        {
            units.Add(c);
            r.units.Add(new UnitInfo
            {
                id = c.id, name = c.name, isPlayerSide = c.isPlayerSide, isPet = c.isPet,
                maxHp = c.maxHp, hp = c.hp,
                hpLabel = string.IsNullOrEmpty(c.data?.hpLabel) ? "HP" : c.data.hpLabel,
                color = c.data?.color ?? "#F2A65A",
            });
            return c;
        }

        bool Decided() => !hero.Alive || foes.All(f => !f.Alive);
        Combatant FrontFoe() => foes.FirstOrDefault(f => f.Alive);
        /// <summary>Close: the front enemy. Ranged: aims at the weakest enemy.</summary>
        Combatant PickTarget() => hero.ranged ? foes.Where(f => f.Alive).OrderBy(f => f.hp).FirstOrDefault() : FrontFoe();
        bool Roll(float chance) => rng.NextDouble() < chance;

        // --- Turns -------------------------------------------------------------

        void HeroAct()
        {
            if (hero.Has(StatusKind.Stunned)) { Emit(BattleEventType.Stun, hero, hero, label: "Stuck explaining where you're going"); return; }
            if (round == 1 && cond.Contains("limping")) { Emit(BattleEventType.Stun, hero, hero, label: "Tyre Marks! Still limping."); return; }
            if (cond.Contains("smoked") && Roll(Conditions.SmokedChance)) { Emit(BattleEventType.Stun, hero, hero, label: "*cough cough* Smoked out."); return; }
            if (cond.Contains("confused") && Roll(Conditions.ConfusedChance))
            {
                int self = Math.Max(1, (int)Math.Round(hero.atk * 0.3f));
                hero.hp = Math.Max(0, hero.hp - self);
                Emit(BattleEventType.Hit, hero, hero, self, hero.hp, "Confused! Hit yourself.", true);
                if (hero.hp == 0) { r.killer = "confusion"; OnZeroHp(hero); }
                return;
            }

            foreach (var k in active)
                if (k.kind == "heal_every" && k.every > 0 && round % k.every == 0)
                    Heal(hero, hero, (int)Math.Round(hero.maxHp * k.power), k.name);

            var target = PickTarget();
            if (target == null) return;
            WeaponAttack(target);
            if (Decided()) return;

            float chanceMult = stage == GrapStage.Grap3 && !ignoreAqi ? 0.5f : 1f;
            foreach (var k in active)
            {
                target = PickTarget();
                if (target == null) return;
                if (!Roll(k.chance * chanceMult)) continue;

                switch (k.kind)
                {
                    case "extra_hit":
                        Attack(hero, target, k.power, k.name, true, typed: false);
                        break;
                    case "aoe":
                        bool first = true;
                        foreach (var f in foes.Where(f => f.Alive).ToList()) { Attack(hero, f, k.power, first ? k.name : null, true, typed: false); first = false; }
                        break;
                    case "big_hit":
                        if (Roll(k.backfire))
                        {
                            int self = Math.Max(1, (int)Math.Round(hero.atk * 1.5f));
                            hero.hp = Math.Max(1, hero.hp - self); // backfire hurts but never kills
                            Emit(BattleEventType.Hit, hero, hero, self, hero.hp, "Backfired! Nobody knows your father.", true);
                        }
                        else Attack(hero, target, k.power, k.name, true, typed: false);
                        break;
                    case "confuse":
                        target.Add(new StatusEffect(StatusKind.Stunned, 1));
                        Emit(BattleEventType.Stun, hero, target, label: k.name + "! Confused.");
                        break;
                    case "steal_atk":
                        float st = target.atk * 0.1f;
                        target.atk -= st; hero.atk += st;
                        Shout(hero, k.name + "! Stole 10% ATK.");
                        break;
                }
            }
        }

        /// <summary>The hero's normal attack, shaped by the equipped weapon's perk.</summary>
        void WeaponAttack(Combatant target)
        {
            switch (weapon?.perk)
            {
                case "cover_drive" when Roll(weapon.chance):
                    Attack(hero, target, weapon.power, "Cover Drive!", true);
                    return;
                case "double_tap":
                    Attack(hero, target, weapon.power, null, false);
                    if (PickTarget() is { } t2) Attack(hero, t2, weapon.power, null, false);
                    return;
                case "lathicharge" when Roll(weapon.chance) && foes.Count(f => f.Alive) > 1:
                    bool first = true;
                    foreach (var f in foes.Where(f => f.Alive).ToList()) { Attack(hero, f, 1f, first ? "Lathicharge!" : null, true); first = false; }
                    return;
                case "stun":
                    Attack(hero, target, 1f, null, false);
                    if (target.Alive && Roll(weapon.chance))
                    {
                        target.Add(new StatusEffect(StatusKind.Stunned, 1));
                        Emit(BattleEventType.Stun, hero, target, label: weapon.name + "! Stunned.");
                    }
                    return;
                default:
                    Attack(hero, target, 1f, null, false);
                    return;
            }
        }

        void AuntyAct()
        {
            if (hero.hp < hero.maxHp * 0.5f)
                Heal(aunty, hero, (int)Math.Round(hero.maxHp * 0.25f), "Good Morning Flower Image!");
            else if (FrontFoe() is { } t)
                Attack(aunty, t, 2f, "Complaint to Society Secretary!", true, typed: false);
        }

        void EnemyAct(Combatant f)
        {
            if (f.Has(StatusKind.Stunned)) { Emit(BattleEventType.Stun, f, f, label: "Confused"); return; }
            f.actions++;

            switch (f.Ai)
            {
                case "broker":
                    if (f.actions % 3 == 0) { Heal(f, f, (int)Math.Round(f.maxHp * 0.08f), "Owner is very nice!"); return; }
                    int dealt = Attack(f, hero, 1f, null, false);
                    if (dealt > 0 && f.hp < f.maxHp) Heal(f, f, Math.Max(1, dealt / 4), "Brokerage");
                    return;

                case "stunner":
                case "guard":
                    Attack(f, hero, 1f, null, false);
                    if (hero.Alive && Roll(0.25f))
                    {
                        hero.Add(new StatusEffect(StatusKind.Stunned, 2)); // 2 = survives this round's tick
                        Emit(BattleEventType.Stun, f, hero, label: f.data.quip ?? "Kahan jaana hai?");
                    }
                    return;

                case "buffer":
                case "techbro":
                    if (f.actions % 2 == 0) { f.Add(new StatusEffect(StatusKind.AtkUp, 99, f.AtkBonus() + 0.15f)); Shout(f, (f.data.quip ?? "Synergy!") + " +15% ATK"); }
                    break;

                case "couple":
                    // PDA couple: patch each other up; the survivor of a breakup is furious (see OnZeroHp).
                    var partner = foes.FirstOrDefault(o => o != f && o.Ai == "couple" && o.Alive);
                    if (partner != null && partner.hp < partner.maxHp * 0.5f && f.actions % 2 == 0)
                    {
                        Heal(f, partner, (int)Math.Round(partner.maxHp * 0.2f), f.id == "jaanu" ? "Are you okay, baby?" : "Jaanu, don't die!");
                        return;
                    }
                    break;

                case "ceo":
                    if (f.actions % 2 == 0)
                    {
                        f.b2bMode = !f.b2bMode;
                        f.atk = f.data.atk * (f.b2bMode ? 0.6f : 1.5f);
                        f.def = f.data.def * (f.b2bMode ? 2.5f : 0.6f);
                        Shout(f, f.b2bMode ? "PIVOT! Now B2B (high DEF)." : "PIVOT! Now B2C (high ATK).");
                    }
                    break;
            }
            Attack(f, hero, 1f, null, false);
        }

        // --- Resolution ----------------------------------------------------------

        /// <param name="typed">False for skills and Pinky Aunty: no close/ranged modifiers (GDD: skills have no type).</param>
        int Attack(Combatant a, Combatant t, float power, string label, bool big, bool typed = true)
        {
            bool ranged = typed && a.ranged;
            string proj = !ranged ? null : a == hero ? "item_" + weapon?.id : "shot_" + a.id;
            bool sureShot = a == hero && weapon?.perk == "sure_shot";
            bool aqiApplies = !(a.isPlayerSide && ignoreAqi) && !sureShot;
            if (stage == GrapStage.Grap2 && aqiApplies && Roll(0.15f)) { Emit(BattleEventType.Miss, a, t, label: label, ranged: ranged, proj: proj); return 0; }
            if (a == hero && cond.Contains("lost") && Roll(Conditions.LostMiss)) { Emit(BattleEventType.Miss, a, t, label: "Lost!", ranged: ranged, proj: proj); return 0; }
            if (t == hero && dodge > 0 && Roll(dodge)) { Emit(BattleEventType.Dodge, a, t, ranged: ranged, proj: proj); return 0; }
            if (t.Has(StatusKind.Invulnerable)) { Emit(BattleEventType.Immune, a, t, label: "LUNCH BREAK", ranged: ranged, proj: proj); return 0; }

            float atk = a.atk * (1f + a.AtkBonus());
            if (typed)
            {
                if (a.ranged) atk *= RangedPenalty;
                else if (t.ranged) atk *= CloseBonus;
                if (sureShot && Uncles.Contains(t.id)) atk *= weapon.power;
            }
            if (a == hero && stage == GrapStage.RareGoodDay) atk *= 1.2f;
            if (!a.isPlayerSide && stage == GrapStage.Grap1 && !ignoreAqi) atk *= 1.1f;

            float roll = 0.9f + (float)rng.NextDouble() * 0.2f;
            int dmg = Math.Max(1, (int)Math.Round(atk * power * roll - t.def * 0.5f));
            if (t == hero && shieldHits > 0) { dmg = Math.Max(1, dmg / 2); shieldHits--; }

            t.hp = Math.Max(0, t.hp - dmg);
            Emit(BattleEventType.Hit, a, t, dmg, t.hp, label, big, ranged, proj);
            if (t.hp == 0) { if (t == hero) r.killer = a.name; OnZeroHp(t); }
            else if (t == hero && a.data != null && !string.IsNullOrEmpty(a.data.inflicts) && !cond.Contains(a.data.inflicts)
                     && Roll(a.data.inflictChance > 0 ? a.data.inflictChance : 0.25f))
            {
                string c = a.data.inflicts;
                cond.Add(c); r.inflicted.Add(c); ApplyCondition(c);
                Emit(BattleEventType.Shout, a, hero, label: Conditions.Name(c).ToUpper() + "!", big: true); // DISHOOM-style status popup
            }
            return dmg;
        }

        void OnZeroHp(Combatant t)
        {
            if (t.Ai == "babu" && !t.revived)
            {
                t.revived = true;
                t.hp = t.maxHp;
                t.atk *= 1.2f;
                Emit(BattleEventType.Revive, t, t, t.hp, t.hp, "Come Tomorrow! Form rejected: wrong ink colour.");
                return;
            }
            Emit(BattleEventType.Death, t, t);
            if (t.Ai == "couple" && foes.FirstOrDefault(o => o != t && o.Ai == "couple" && o.Alive) is { } ex)
            {
                ex.Add(new StatusEffect(StatusKind.AtkUp, 99, 0.5f));
                Shout(ex, "BREAKUP! Now it's personal. (+50% ATK)");
            }
        }

        void Heal(Combatant a, Combatant t, int amount, string label)
        {
            if (t == hero && cond.Contains("cringe")) amount = Math.Max(1, (int)(amount * Conditions.CringeHeal));
            t.Heal(amount);
            Emit(BattleEventType.Heal, a, t, amount, t.hp, label);
        }

        /// <summary>Stat effects of a lasting status, applied when the fight starts or the status lands.</summary>
        void ApplyCondition(string c)
        {
            if (c == "shaved") hero.def *= Conditions.ShavedDef;
            if (c == "cancelled") hero.atk *= Conditions.CancelledAtk;
        }

        void Shout(Combatant a, string text) => Emit(BattleEventType.Shout, a, null, label: text);

        void Emit(BattleEventType type, Combatant a = null, Combatant t = null, int amount = 0, int hpAfter = 0, string label = null, bool big = false, bool ranged = false, string proj = null) =>
            r.events.Add(new BattleEvent { type = type, actor = a?.index ?? -1, target = t?.index ?? -1, amount = amount, hpAfter = hpAfter, label = label, big = big, ranged = ranged, projectile = proj });
    }
}
