using System.Reflection;

// apidump <type name filter> : the public API of the DotRecast assemblies, to write the generator against
static class ApiDump
{
    public static void Run(string filter)
    {
        foreach (var asm in new[] { typeof(DotRecast.Recast.Geom.IRcInputGeomProvider).Assembly, typeof(DotRecast.Core.Numerics.RcVec3f).Assembly })
            foreach (var t in asm.GetExportedTypes().Where(t => t.FullName.Contains(filter)).OrderBy(t => t.FullName))
            {
                Console.WriteLine($"== {t.FullName}");
                foreach (var c in t.GetConstructors()) Console.WriteLine($"  ctor({string.Join(", ", c.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"))})");
                foreach (var f in t.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)) Console.WriteLine($"  field {f.FieldType.Name} {f.Name}");
                foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)) Console.WriteLine($"  prop {p.PropertyType.Name} {p.Name}");
                foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName))
                    Console.WriteLine($"  {(m.IsStatic ? "static " : "")}{m.ReturnType.Name} {m.Name}({string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"))})");
            }
    }
}
