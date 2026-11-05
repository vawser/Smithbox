// Lists the public static methods of Hexa.NET.ImNodes.ImNodes (names and parameters), and its enums,
// filtered by the first argument, for writing the behavior graph view against the real API.
using System.Reflection;

var filter = args.Length > 0 ? args[0] : "";
var asm = typeof(Hexa.NET.ImNodes.ImNodes).Assembly;
var type = typeof(Hexa.NET.ImNodes.ImNodes);

foreach (var m in type.GetMethods(BindingFlags.Public | BindingFlags.Static).OrderBy(m => m.Name))
{
    if (filter != "" && !m.Name.Contains(filter, StringComparison.OrdinalIgnoreCase)) continue;
    var ps = string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
    Console.WriteLine($"{m.ReturnType.Name} {m.Name}({ps})");
}

if (filter == "enums")
    foreach (var t in asm.GetTypes().Where(t => t.IsEnum && t.IsPublic))
        Console.WriteLine($"enum {t.Name}: {string.Join(", ", Enum.GetNames(t).Take(40))}");
