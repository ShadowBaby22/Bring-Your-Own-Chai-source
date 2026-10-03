namespace KadakChai
{
    public enum StatusKind
    {
        AtkUp,        // value = fraction, e.g. 0.25 = +25%
        Stunned,      // skips actions while it lasts
        Invulnerable, // Babu's "Lunch Break"
    }

    /// <summary>
    /// A battle buff/debuff. roundsLeft ticks down at the end of each round, so everything
    /// in a fight is temporary too.
    /// </summary>
    public class StatusEffect
    {
        public StatusKind kind;
        public float value;
        public int roundsLeft;

        public StatusEffect(StatusKind kind, int rounds, float value = 0f)
        {
            this.kind = kind;
            roundsLeft = rounds;
            this.value = value;
        }
    }
}
