using System;
using System.Collections.Generic;

namespace KadakChai
{
    /// <summary>Runtime state for one unit inside a single simulated fight.</summary>
    public class Combatant
    {
        public readonly int index;          // position in BattleResult.units
        public readonly string id, name;
        public readonly bool isPlayerSide, isPet;
        public readonly CombatantData data; // null for the hero
        public readonly List<StatusEffect> statuses = new();

        public int maxHp, hp;
        public float atk, def;
        public int actions;                 // turns taken, for "every N rounds" gimmicks
        public bool revived;                // Babu's one-time "Come Tomorrow"
        public bool b2bMode;                // CEO pivot state
        public bool ranged;                 // close vs ranged (GDD): ranged opens with a free shot, picks targets, -20% dmg

        public string Ai => data?.ai ?? "";
        public bool Alive => hp > 0;

        public Combatant(int index, string id, string name, bool isPlayerSide, bool isPet,
                         int hp, int maxHp, float atk, float def, CombatantData data = null)
        {
            this.index = index; this.id = id; this.name = name;
            this.isPlayerSide = isPlayerSide; this.isPet = isPet;
            this.hp = hp; this.maxHp = maxHp; this.atk = atk; this.def = def;
            this.data = data;
        }

        public bool Has(StatusKind k) => statuses.Exists(s => s.kind == k);

        public float AtkBonus()
        {
            float t = 0;
            foreach (var s in statuses) if (s.kind == StatusKind.AtkUp) t += s.value;
            return t;
        }

        public void Add(StatusEffect e)
        {
            statuses.RemoveAll(s => s.kind == e.kind); // refresh, don't stack
            statuses.Add(e);
        }

        public void TickStatuses()
        {
            for (int i = statuses.Count - 1; i >= 0; i--)
                if (--statuses[i].roundsLeft <= 0) statuses.RemoveAt(i);
        }

        public void Heal(int amount) => hp = Math.Min(maxHp, hp + amount);
    }
}
