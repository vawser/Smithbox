using SoulsFormats;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

if (args[0] == "models") { ModelsDump.Run(args[1]); return; }
if (args[0] == "diff") { GpDiff.Run(args[1]); return; }
if (args[0] == "param") { ParamDump.Run(args); return; }
if (args[0] == "msb")
{
    // gpdump msb <NR|AC6|ER> <msb.dcx>
    object msb = args[1] switch { "NR" => MSB_NR.Read(args[2]), "AC6" => MSB_AC6.Read(args[2]), _ => MSBE.Read(args[2]) };
    var parts = msb.GetType().GetProperty("Parts").GetValue(msb);
    var all = (IEnumerable)parts.GetType().GetMethod("GetEntries").Invoke(parts, null);
    var counts = new Dictionary<string, int>();
    foreach (var p in all)
    {
        foreach (var prop in p.GetType().GetProperties())
        {
            if (!prop.PropertyType.Name.Contains("Gparam")) continue;
            var g = prop.GetValue(p);
            if (g == null) continue;
            var vals = string.Join(",", g.GetType().GetProperties().Where(q => q.PropertyType.IsPrimitive).Select(q => $"{q.Name}={q.GetValue(g)}"));
            var key = $"{p.GetType().Name} {prop.PropertyType.Name}: {vals}";
            counts[key] = counts.GetValueOrDefault(key) + 1;
        }
    }
    foreach (var kv in counts.OrderBy(k => k.Key)) Console.WriteLine($"{kv.Value,5} {kv.Key}");
    return;
}

var gp = GPARAM.Read(args[0]);
string filter = args.Length > 1 ? args[1] : null;
foreach (var p in gp.Params)
{
    if (filter != null && !p.Key.Contains(filter, StringComparison.OrdinalIgnoreCase)) continue;
    Console.WriteLine($"== {p.Key} | {p.Name}");
    foreach (var f in p.Fields)
    {
        var vals = (IList)f.GetType().GetProperty("Values").GetValue(f);
        string vs = string.Join("; ", vals.Cast<object>().Select(v => {
            var t = v.GetType();
            return $"[{t.GetProperty("ID").GetValue(v)} t{t.GetProperty("TimeOfDay").GetValue(v)}]{t.GetProperty("Value").GetValue(v)}";
        }));
        Console.WriteLine($"   {f.Key} | n={vals.Count} | {vs}");
    }
}
