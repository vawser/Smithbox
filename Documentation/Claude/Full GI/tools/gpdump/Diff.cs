using SoulsFormats;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
// gpdump diff <file>: per field, which value IDs differ from ID 0 (compared at the same TimeOfDay)
public static class GpDiff
{
    public static void Run(string path)
    {
        var g = GPARAM.Read(path);
        var idsAll = new SortedSet<int>();
        foreach (var p in g.Params)
            foreach (var f in p.Fields)
            {
                var vals = ((IList)f.GetType().GetProperty("Values").GetValue(f)).Cast<object>()
                    .Select(v => (Id: (int)v.GetType().GetProperty("ID").GetValue(v), T: (float)v.GetType().GetProperty("TimeOfDay").GetValue(v), V: v.GetType().GetProperty("Value").GetValue(v)?.ToString())).ToList();
                foreach (var v in vals) idsAll.Add(v.Id);
                var baseVals = vals.Where(v => v.Id == 0).ToDictionary(v => v.T, v => v.V);
                var diffs = vals.Where(v => v.Id != 0 && (!baseVals.TryGetValue(v.T, out var b) || b != v.V)).Select(v => $"{v.Id}@t{v.T}={v.V}").ToList();
                if (diffs.Count > 0) Console.WriteLine($"{p.Name} | {f.Key} | base={string.Join(",", baseVals.Take(2).Select(k => $"t{k.Key}={k.Value}"))} | {string.Join("; ", diffs.Take(6))}");
            }
        Console.WriteLine($"IDs: {string.Join(",", idsAll)}");
    }
}
