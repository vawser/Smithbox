using Andre.Core;
using Andre.IO.VFS;
using SoulsFormats;
using System.Reflection;

// Checks the Map Editor Create tool's types against the save path, for one game.
// For each Part/Region/Event subclass the Create tool lists (same reflection as CreateAction.PopulateClassNames),
// it builds the MSB the way MapUniverse saves it (fresh MSB + carried-over Layers/Routes + every existing entry +
// one default-constructed new entry + models), writes it, reads it back and checks the new entry is there.
//   MsbCreateCheck <outFile> [GAME] [mapCount]
// Run with the working directory at src/Smithbox.Data, so the UXM dictionaries are found.

var outFile = Path.GetFullPath(args[0]);
var gameName = args.Length > 1 ? args[1] : "NR";
var mapCount = args.Length > 2 ? int.Parse(args[2]) : 2;

var gamePaths = File.ReadAllLines(Path.Combine("..", "..", "game_data.txt"))
    .Select(l => l.Split(':', 2))
    .Where(p => p.Length == 2)
    .ToDictionary(p => p[0].Trim(), p => p[1].Trim().Trim('"').Trim());

var root = gamePaths[gameName];
var game = Enum.Parse<Game>(gameName);
var fs = new CompundVirtualFileSystem([ArchiveBinderVirtualFileSystem.FromGameFolder(root, game), new RealVirtualFileSystem(root, false)]);

Type msbClass = gameName switch
{
    "NR" => typeof(MSB_NR),
    "ER" => typeof(MSBE),
    "AC6" => typeof(MSB_AC6),
    "DS3" => typeof(MSB3),
    "SDT" => typeof(MSBS),
    _ => throw new ArgumentException(gameName)
};

var paths = fs.GetFileNamesMatchingRecursive("/map/mapstudio", @"(?i)\.msb(\.dcx)?$").Select(p => p.Replace('\\', '/'))
    .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToList();

var output = new List<string>();
void Log(string s) { Console.WriteLine(s); output.Add(s); }

Log($"{gameName}: {paths.Count} MSBs found, checking {mapCount}");

// Every MSB must at least read, or the Create tool cannot be used on it
var readFailures = 0;
var loadable = new List<(string path, IMsb msb)>();
foreach (var path in paths)
{
    try
    {
        var msb = Read(fs.ReadFile(path).Value.ToArray());
        if (loadable.Count < mapCount && msb.Parts.GetEntries().Count > 0)
            loadable.Add((path, msb));
    }
    catch (Exception e)
    {
        readFailures++;
        if (readFailures <= 5) Log($"READ FAIL {path}: {e.GetType().Name} {e.Message}");
    }
}
Log($"Read failures: {readFailures}/{paths.Count}");

List<Type> Subclasses(string nested)
{
    var baseType = msbClass.GetNestedType(nested);
    var list = msbClass.Assembly.GetTypes().Where(t => t.IsSubclassOf(baseType) && !t.IsAbstract).ToList();
    if (list.Count == 0 && nested == "Region") list.Add(baseType);
    return list;
}

var kinds = new (string kind, List<Type> types)[]
{
    ("Part", Subclasses("Part")),
    ("Region", Subclasses("Region")),
    ("Event", Subclasses("Event")),
};

foreach (var (kind, types) in kinds)
    Log($"{kind} types offered: {types.Count}");

foreach (var (path, original) in loadable)
{
    Log($"");
    Log($"== {path}");
    var mapId = Path.GetFileName(path)[..12];

    foreach (var (kind, types) in kinds)
    {
        foreach (var type in types)
        {
            var label = $"{kind}.{type.Name}";
            string stage = "construct";
            try
            {
                var created = (IMsbEntry)type.GetConstructor(Type.EmptyTypes).Invoke([]);
                created.Name = $"CreateCheck_{type.Name}";

                stage = "build";
                var msb = CreateForWrite(original);
                foreach (var p in original.Parts.GetEntries()) msb.Parts.Add(p);
                foreach (var r in original.Regions.GetEntries()) msb.Regions.Add(r);
                foreach (var e in original.Events.GetEntries()) msb.Events.Add(e);

                stage = "add";
                switch (created)
                {
                    case IMsbPart p: msb.Parts.Add(p); break;
                    case IMsbRegion r: msb.Regions.Add(r); break;
                    case IMsbEvent e: msb.Events.Add(e); break;
                }

                foreach (var m in original.Models.GetEntries()) msb.Models.Add(m);

                stage = "write";
                var bytes = msb.Write(DCX.Type.None);

                stage = "reread";
                var back = Read(bytes);
                IEnumerable<IMsbEntry> entries = kind switch
                {
                    "Part" => back.Parts.GetEntries(),
                    "Region" => back.Regions.GetEntries(),
                    _ => back.Events.GetEntries(),
                };
                var found = entries.FirstOrDefault(x => x.Name == created.Name);

                if (found == null)
                    Log($"LOST    {label}: written, but not in the file read back");
                else if (found.GetType() != type)
                    Log($"RETYPED {label}: read back as {found.GetType().Name}");
                else
                {
                    stage = "rewrite";
                    var again = back.Write(DCX.Type.None);
                    if (!again.AsSpan().SequenceEqual(bytes))
                        Log($"UNSTABLE {label}: second write differs");
                    else
                        Log($"ok      {label}");
                }
            }
            catch (Exception e)
            {
                var inner = e is TargetInvocationException tie ? tie.InnerException : e;
                Log($"FAIL    {label} at {stage}: {inner.GetType().Name} {inner.Message}");
                if (inner is NullReferenceException)
                    Log("        " + string.Join(" <- ", inner.StackTrace.Split('\n').Take(3).Select(l => l.Trim())));
            }
        }
    }

}

File.WriteAllLines(outFile, output);
Log($"Wrote {outFile}");

IMsb Read(byte[] data) => gameName switch
{
    "NR" => MSB_NR.Read(data),
    "ER" => MSBE.Read(data),
    "AC6" => MSB_AC6.Read(data),
    "DS3" => MSB3.Read(data),
    "SDT" => MSBS.Read(data),
    _ => throw new ArgumentException(gameName)
};

// Same as MsbGameAdapter: a fresh MSB carrying over the sections the editor doesn't manage
IMsb CreateForWrite(IMsb prev)
{
    var msb = (IMsb)Activator.CreateInstance(msbClass);
    foreach (var name in new[] { "Layers", "Routes", "PartsPoses" })
    {
        var prop = msbClass.GetProperty(name);
        if (prop != null && prop.CanWrite) prop.SetValue(msb, prop.GetValue(prev));
    }
    return msb;
}
