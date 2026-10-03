using System; using System.Linq; using KadakChai;
static class Probe {
  public static void Babu() {
    Console.WriteLine("scenes: " + string.Join(", ", GameData.Scenes.Select(k => k.Key + "(" + k.Value.Count + ")")));
    foreach (var d in GameData.Days) {
      if (!string.IsNullOrEmpty(d.scene) && GameData.Scene(d.scene) == null) Console.WriteLine("MISSING scene " + d.scene);
      if (!string.IsNullOrEmpty(d.outro) && GameData.Scene(d.outro) == null) Console.WriteLine("MISSING outro " + d.outro);
      if (!string.IsNullOrEmpty(d.pool)) Console.WriteLine($"pool {d.pool}: {GameData.Cards.Keys.Count(k => k.StartsWith(d.pool))} cards");
      if (d.enemyGroups != null) foreach (var g in d.enemyGroups) { var id = g.Split('+')[0]; if (GameData.CardOrNull("fight_" + id) == null || GameData.CardOrNull("fight_" + id + "_win") == null) Console.WriteLine("missing fight card " + id); foreach (var e in g.Split('+')) if (!GameData.Combatants.ContainsKey(e)) Console.WriteLine("missing enemy " + e); }
    }
    var run = new Run(3);
    for (int i = 0; i < 11; i++) { run.BeginDay(); Console.WriteLine($"day {run.DayNumber}: {run.IntroCard?.id} vs [{string.Join(",", run.Enemies().Select(e => e.id))}]"); run.EndDay(); }
    var c = GameData.CardOrNull("ev_dilli_chole"); Console.WriteLine($"chole B='{c.choiceB}' eff='{c.effectB}'");
  }
}
