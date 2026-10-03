using System;
using System.IO;
using System.Text.Json;
using KadakChai;

class Program
{
    static void Main(string[] args)
    {
        string dir = "/Users/siddharthgupta/Kadak Chai/Assets/_Game/Resources/Data";
        var opts = new JsonSerializerOptions { IncludeFields = true, PropertyNameCaseInsensitive = true };
        GameData.Load(f => File.ReadAllText(Path.Combine(dir, f)), (json, t) => JsonSerializer.Deserialize(json, t, opts));
        Console.WriteLine($"Loaded {GameData.Combatants.Count} combatants, {GameData.Skills.Count} skills, {GameData.Days.Count} days, {GameData.Cards.Count} cards");
        foreach (var d in GameData.Days)
        {
            if (GameData.CardOrNull(d.card) == null) Console.WriteLine($"MISSING card {d.card}");
            if (d.type == "fight" || d.type == "boss") { if (GameData.CardOrNull(d.card + "_win") == null) Console.WriteLine($"no win card for {d.card}"); }
        }
        Console.WriteLine(GameData.Cards["credits"].text);
        Probe2.Stuck(); Console.WriteLine(RunSimulator.Simulate(500, 1, true)); Console.WriteLine(RunSimulator.Simulate(500, 1, false));
    }
}
