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
        GameData.Load(f => File.Exists(Path.Combine(dir, f)) ? File.ReadAllText(Path.Combine(dir, f)) : null, (json, t) => JsonSerializer.Deserialize(json, t, opts));
        int n = args.Length > 0 ? int.Parse(args[0]) : 1000;
        Console.WriteLine(RunSimulator.Simulate(n, 1, true));
        Console.WriteLine(RunSimulator.Simulate(n, 1, false));
    }
}
