using HKLib.ClassGeneration;
using HKLib.Reflection.hk2018;

// Usage: Havok.HKLib.ClassGeneration <src/Havok directory> [output directory]
// Writes HKLib/hk2019/Autogen and HKLib.Reflection/hk2019/Autogen under the output directory, which defaults to the
// src/Havok directory. Existing output is not overwritten, move it to the .archive folder before regenerating.

if (args.Length < 1)
{
    Console.WriteLine("Usage: Havok.HKLib.ClassGeneration <src/Havok directory> [output directory]");
    return 1;
}

string havokDirectory = args[0];
string outputDirectory = args.Length > 1 ? args[1] : havokDirectory;
string resDirectory = Path.Join(havokDirectory, "HKLib.Reflection", "Res");

string classDirectory = Path.Join(outputDirectory, "HKLib", "hk2019", "Autogen");
string dataDirectory = Path.Join(outputDirectory, "HKLib.Reflection", "hk2019", "Autogen");

foreach (string directory in new[] { classDirectory, dataDirectory })
{
    if (Directory.Exists(directory) && Directory.EnumerateFileSystemEntries(directory).Any())
    {
        Console.WriteLine($"{directory} is not empty. Move it to the .archive folder before regenerating.");
        return 1;
    }
}

HavokTypeRegistry registry2018 = HavokTypeRegistry.Load(Path.Join(resDirectory, "HavokTypeRegistry20180100.xml"));
RegistryLoader registry2019 = RegistryLoader.Load(Path.Join(resDirectory, "HavokTypeRegistry20190100.xml"));

Generator generator = new(registry2019, registry2018);
generator.Run();

Emitter.WriteClasses(generator.Decls, classDirectory);
Emitter.WriteData(generator.Decls, dataDirectory);
Emitter.WriteIdentityTypeMap(generator.IdentityTypeMap, Path.Join(dataDirectory, "IdentityTypeMap.cs"));

foreach (string warning in generator.Warnings.Distinct()) Console.WriteLine($"Warning: {warning}");

Console.WriteLine($"Types: {registry2019.Types.Count} | Declarations: {generator.Decls.Count} | " +
                  $"Identity map entries: {generator.IdentityTypeMap.Count}");
return 0;
