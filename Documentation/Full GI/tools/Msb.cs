using SoulsFormats;
using System;
using System.Linq;
using System.Reflection;
public static class Msb
{
    public static void Run(string game, string path, string filter)
    {
        IMsb msb = game switch { "ER" => MSBE.Read(path), "NR" => MSB_NR.Read(path), _ => MSB_AC6.Read(path) };
        foreach (var r in msb.Regions.GetEntries().Where(r => r.GetType().Name.Contains(filter)))
        {
            var props = r.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetIndexParameters().Length == 0 && p.Name is not ("Shape" or "Position" or "Rotation" or "Name"))
                .Select(p => { var v = p.GetValue(r); return $"{p.Name}={(v is System.Collections.IEnumerable a && v is not string ? "[" + string.Join(",", a.Cast<object>()) + "]" : v)}"; });
            var shape = r.Shape is MSB.Shape.Box b ? $"box {b.Width}x{b.Height}x{b.Depth}" : r.Shape?.GetType().Name;
            Console.WriteLine($"{r.GetType().Name} {r.Name} pos={r.Position} rot={r.Rotation} {shape} | {string.Join(" ", props)}");
        }
    }
}
