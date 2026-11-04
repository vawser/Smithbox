using SoulsFormats;
using System;
using System.Linq;
using System.Collections;
// gpdump models <AC6 msb.dcx>: part type and model name counts, plus the model list
public static class ModelsDump
{
    public static void Run(string path)
    {
        var msb = MSB_AC6.Read(path);
        foreach (var g in msb.Parts.GetEntries().GroupBy(p => (p.GetType().Name, p.ModelName)).OrderBy(g => g.Key.Item1).ThenBy(g => g.Key.Item2))
            if (g.Key.Item1 != "MapPiece" && g.Key.Item1 != "Asset" && g.Key.Item1 != "Collision") Console.WriteLine($"{g.Count(),4} {g.Key.Item1} {g.Key.Item2}");
        foreach (var m in msb.Models.GetEntries().Where(m => !m.Name.StartsWith("m") && !m.Name.StartsWith("AEG") && !m.Name.StartsWith("h"))) Console.WriteLine($"model {m.GetType().Name} {m.Name}");
    }
}
