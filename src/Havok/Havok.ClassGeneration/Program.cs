using Havok.ClassGeneration;
using Havok.Reflection;

// Generates an SDK's class set for the unified Havok library: the classes, a field accessor per class, the identity
// type map, the SDK's IHavokObject marker and its class set entry point.
//
//   Havok.ClassGeneration <src/Havok directory> <registry xml> <namespace> <output project directory> [partial class...] [--overlay <xml>]...
//
// e.g. Havok.ClassGeneration src/Havok src/Havok/Havok.Core/Res/HavokTypeRegistry20160200.xml Havok.hk2016 src/Havok/Classes/Havok.hk2016
//
// Writes <output>/Autogen. Existing output is not overwritten: move Autogen to the .archive folder before regenerating.
// Types that also exist in the 2018 classes (Havok.hk2018) keep their naming and placement, so code reads the same on every SDK.

if (args.Length < 4)
{
    Console.WriteLine("Usage: Havok.ClassGeneration <src/Havok directory> <registry xml> <namespace> <output project directory> [partial class...] [--overlay <xml>]...");
    return 1;
}

string havokDirectory = args[0];
string registryPath = args[1];
Options.Namespace = args[2];
string outputDirectory = Path.Join(args[3], "Autogen");

// the rest: partial class names, and "--overlay <xml>" for each game variant whose fields the classes must also hold
List<string> overlays = new();
for (int i = 4; i < args.Length; i++)
{
    if (args[i] == "--overlay" && i + 1 < args.Length) overlays.Add(args[++i]);
    else Options.PartialClasses.Add(args[i]);
}

if (Directory.Exists(outputDirectory) && Directory.EnumerateFileSystemEntries(outputDirectory).Any())
{
    Console.WriteLine($"{outputDirectory} is not empty. Move it to the .archive folder before regenerating.");
    return 1;
}

// The 2018 registry with its C# types bound to Havok.hk2018, whose naming and placement the other SDKs follow
string resDirectory = Path.Join(havokDirectory, "Havok.Core", "Res");
HavokTypeRegistry registry2018 = HavokTypeRegistry.Load(Path.Join(resDirectory, "HavokTypeRegistry20180100.xml"),
    identity => Havok.hk2018.IdentityTypeMap.Map.GetValueOrDefault(identity));
RegistryLoader registry = RegistryLoader.Load(registryPath);
List<RegistryLoader> variants = overlays.Select(overlay => RegistryLoader.Load(registryPath, overlay)).ToList();

Generator generator = new(registry, registry2018, variants);
generator.Run();

Emitter.WriteClasses(generator.Decls, Path.Join(outputDirectory, "Classes"));
Emitter.WriteData(generator.Decls, Path.Join(outputDirectory, "Accessors"));
Emitter.WriteIdentityTypeMap(generator.IdentityTypeMap, Path.Join(outputDirectory, "IdentityTypeMap.cs"));
Emitter.WriteSupportFiles(outputDirectory, Path.GetFileName(registryPath));

foreach (string warning in generator.Warnings.Distinct()) Console.WriteLine($"Warning: {warning}");

Console.WriteLine($"Types: {registry.Types.Count} | Declarations: {generator.Decls.Count} | " +
                  $"Identity map entries: {generator.IdentityTypeMap.Count}");
return 0;
