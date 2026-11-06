using Havok.ClassGeneration;
using HKLib.Reflection.hk2018;

// Generates an SDK's class set for the unified Havok library: the classes, a field accessor per class, the identity
// type map, the SDK's IHavokObject marker and its class set entry point.
//
//   Havok.ClassGeneration <src/Havok directory> <registry xml> <namespace> <output project directory> [partial class...]
//
// e.g. Havok.ClassGeneration src/Havok src/Havok/Havok.Core/Res/HavokTypeRegistry20160200.xml Havok.hk2016 src/Havok/Classes/Havok.hk2016
//
// Writes <output>/Autogen. Existing output is not overwritten: move Autogen to the .archive folder before regenerating.
// Types that also exist in HKLib's 2018 classes keep their naming and placement, so code reads the same on every SDK.

if (args.Length < 4)
{
    Console.WriteLine("Usage: Havok.ClassGeneration <src/Havok directory> <registry xml> <namespace> <output project directory> [partial class...]");
    return 1;
}

string havokDirectory = args[0];
string registryPath = args[1];
Options.Namespace = args[2];
string outputDirectory = Path.Join(args[3], "Autogen");
foreach (string partialClass in args.Skip(4)) Options.PartialClasses.Add(partialClass);

if (Directory.Exists(outputDirectory) && Directory.EnumerateFileSystemEntries(outputDirectory).Any())
{
    Console.WriteLine($"{outputDirectory} is not empty. Move it to the .archive folder before regenerating.");
    return 1;
}

string resDirectory = Path.Join(havokDirectory, "HKLib.Reflection", "Res");
HavokTypeRegistry registry2018 = HavokTypeRegistry.Load(Path.Join(resDirectory, "HavokTypeRegistry20180100.xml"));
RegistryLoader registry = RegistryLoader.Load(registryPath);

Generator generator = new(registry, registry2018);
generator.Run();

Emitter.WriteClasses(generator.Decls, Path.Join(outputDirectory, "Classes"));
Emitter.WriteData(generator.Decls, Path.Join(outputDirectory, "Accessors"));
Emitter.WriteIdentityTypeMap(generator.IdentityTypeMap, Path.Join(outputDirectory, "IdentityTypeMap.cs"));
Emitter.WriteSupportFiles(outputDirectory, Path.GetFileName(registryPath));

foreach (string warning in generator.Warnings.Distinct()) Console.WriteLine($"Warning: {warning}");

Console.WriteLine($"Types: {registry.Types.Count} | Declarations: {generator.Decls.Count} | " +
                  $"Identity map entries: {generator.IdentityTypeMap.Count}");
return 0;
