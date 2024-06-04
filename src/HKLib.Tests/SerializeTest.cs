using System.Diagnostics;
using System.Runtime.Intrinsics;
using System.Text;
using HKLib.hk2018;
using HKLib.Reflection.hk2018;
using HKLib.Serialization.hk2018.Binary;

namespace HKLib.Tests;

public class SerializeTest
{
    private const string GameDirectory = @"D:\Elden Ring Unpacked";

    private const string FileConvertPath =
        @"C:\Users\Admin\Desktop\Modding Tools\ER\FileConvert\release\FileConvert.exe";

    private static HavokTypeRegistry? _registry;
    private static HavokBinarySerializer _serializer = new();

    private static HavokTypeRegistry GetRegistry()
    {
        _registry ??= HavokTypeRegistry.Instance;
        return _registry;
    }

    private static IEnumerable<HavokType> ImplementedHavokTypes() =>
        GetRegistry().Types.Where(x => x.Type is not null);

    // Certain record types which are non-serializable are not implemented
    private static IEnumerable<HavokType> ImplementedHavokRecordTypes() =>
        ImplementedHavokTypes().Where(x => x.Kind == HavokType.TypeKind.Record);

    private static IEnumerable<HavokType> InstantiatableHavokRecordTypes() =>
        ImplementedHavokRecordTypes().Where(x => !x.Type!.IsInterface && !x.Type.IsAbstract);

    private static IEnumerable<IHavokObject> RandomizedHavokRecordTypeInstances() => InstantiatableHavokRecordTypes()
        .Where(x => x.Hash is not null)
        .Select(type =>
        {
            IHavokObject havokObject = (IHavokObject)type.Instantiate();
            RandomizeValues(havokObject);
            return havokObject;
        });

    private static IEnumerable<object[]> EldenRingAnims() =>
        Directory.EnumerateDirectories(GameDirectory, "*anibnd*", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateDirectories(GameDirectory, "*cutscenebnd*", SearchOption.AllDirectories))
            .SelectMany(
                x =>
                {
                    string? compendium = Directory.EnumerateFiles(x, "*.compendium", SearchOption.AllDirectories)
                        .SingleOrDefault();
                    return Directory.EnumerateFiles(x, "*.hkx", SearchOption.AllDirectories)
                        .Select(y => new[] { y, compendium! });
                });

    private static IEnumerable<object[]> EldenRingCharacterPhysics() =>
        Directory.EnumerateDirectories(GameDirectory, "*chrbnd*", SearchOption.AllDirectories).SelectMany(x =>
        {
            string? compendium = Directory.EnumerateFiles(x, "*.compendium", SearchOption.AllDirectories)
                .SingleOrDefault();
            return Directory.EnumerateFiles(x, "*.hkx", SearchOption.AllDirectories)
                .Select(y => new[] { y, compendium! });
        });

    private static IEnumerable<object[]> EldenRingAegPhysics() =>
        Directory.EnumerateDirectories(GameDirectory, "*geomhkxbnd*", SearchOption.AllDirectories).SelectMany(x =>
        {
            string? compendium = Directory.EnumerateFiles(x, "*.compendium", SearchOption.AllDirectories)
                .SingleOrDefault();
            return Directory.EnumerateFiles(x, "*.hkx", SearchOption.AllDirectories)
                .Select(y => new[] { y, compendium! });
        });

    private static IEnumerable<object[]> EldenRingMapCollision() =>
        Directory.EnumerateDirectories(GameDirectory, "*hkxbhd", SearchOption.AllDirectories).SelectMany(x =>
        {
            string? compendium = Directory.EnumerateFiles(x, "*.compendium", SearchOption.AllDirectories)
                .SingleOrDefault();
            return Directory.EnumerateFiles(x, "*.hkx", SearchOption.AllDirectories)
                .Select(y => new[] { y, compendium! });
        });

    private static IEnumerable<object[]> EldenRingNavMesh() =>
        Directory.EnumerateDirectories(GameDirectory, "*nvmhktbnd*", SearchOption.AllDirectories).SelectMany(x =>
        {
            string? compendium = Directory.EnumerateFiles(x, "*.compendium", SearchOption.AllDirectories)
                .SingleOrDefault();
            return Directory.EnumerateFiles(x, "*.hkx", SearchOption.AllDirectories)
                .Select(y => new[] { y, compendium! });
        });

    [TestCaseSource(nameof(EldenRingAnims))]
    public void AnimReadWriteTest(string path, string? compendiumPath) => FileRoundtrip(path, compendiumPath);

    // some tests will fail due to boolean values not always being 1 or 0, this is not an issue
    [TestCaseSource(nameof(EldenRingCharacterPhysics))]
    public void CharPhysicsReadWriteTest(string path, string? compendiumPath) => FileRoundtrip(path, compendiumPath);

    // collisions contain custom classes
    // [TestCaseSource(nameof(EldenRingAegPhysics))]
    public void AegPhysicsReadWriteTest(string path, string? compendiumPath) => FileRoundtrip(path, compendiumPath);

    // collisions contain custom classes
    // [TestCaseSource(nameof(EldenRingMapCollision))]
    public void MapCollisionReadWriteTest(string path, string? compendiumPath) => FileRoundtrip(path, compendiumPath);

    // file convert does not have hkai
    // [TestCaseSource(nameof(EldenRingNavMesh))]
    public void NavMeshReadWriteTest(string path, string? compendiumPath) => FileRoundtrip(path, compendiumPath);

    // [TestCaseSource(nameof(RandomizedHavokRecordTypeInstances))]
    public void ShallowReadWriteTest(IHavokObject havokObject)
    {
        MemoryStream stream = new();
        Assert.That(() => _serializer.Write(havokObject, stream), Throws.Nothing,
            $"Failed to write type {havokObject.GetType()} to stream.");
        byte[] buffer = stream.GetBuffer();

        stream = new MemoryStream(buffer);
        IHavokObject copy = null!;
        Assert.That(() => copy = _serializer.Read(stream), Throws.Nothing,
            $"Failed to read type {havokObject.GetType()} from stream.");

        HavokData originalData = HavokData.Of(havokObject);
        HavokData deserializedData = HavokData.Of(copy);
        Assert.That(originalData.Type, Is.EqualTo(deserializedData.Type),
            "Deserialized object is of a different type than the original.");
        DeepCompareFields(originalData, deserializedData);
    }

    private static void DeepCompareFields(HavokData originalData, HavokData otherData)
    {
        foreach (HavokType.Member field in originalData.Type.Fields.Where(x => !x.NonSerializable))
        {
            Assume.That(originalData.TryGetField(field.Name, out object? originalVal),
                $"Could not get original field value for field {field.Name} of type {field.Type} in type {originalData.Type}.");
            Assert.That(otherData.TryGetField(field.Name, out object? deserializedVal),
                $"Could not get deserialized field value for field {field.Name} of type {field.Type} in type {originalData.Type}.");

            switch (field.Type.Kind)
            {
                case HavokType.TypeKind.Void:
                case HavokType.TypeKind.Opaque:
                case HavokType.TypeKind.Array:
                    continue;
                case HavokType.TypeKind.Record:
                    DeepCompareFields(HavokData.Of((IHavokObject)originalVal!),
                        HavokData.Of((IHavokObject)originalVal!));
                    break;
                case HavokType.TypeKind.Bool:
                case HavokType.TypeKind.String:
                case HavokType.TypeKind.Int:
                case HavokType.TypeKind.Float:
                case HavokType.TypeKind.Pointer:
                default:
                    Assert.That(originalVal, Is.EqualTo(deserializedVal),
                        $"Differing value after deserialization in field {field.Name} of type {field.Type} in type {originalData.Type}.");
                    break;
            }
        }
    }

    private static void RandomizeValues(IHavokObject havokObject)
    {
        HavokData data = HavokData.Of(havokObject);
        foreach (HavokType.Member field in data.Type.Fields.Where(x =>
                     x.Type.Kind is not HavokType.TypeKind.Pointer and not HavokType.TypeKind.Array))
        {
            object? randomValue = GetRandomValue(field.Type);
            data.TrySetField(field.Name, randomValue);
        }
    }

    private static object? GetRandomValue(HavokType havokType)
    {
        int format = havokType.Format;
        if (havokType.Kind == HavokType.TypeKind.Pointer) return null;
        switch (havokType.Kind)
        {
            case HavokType.TypeKind.Void:
            case HavokType.TypeKind.Opaque:
            case HavokType.TypeKind.Pointer:
                return null;
            case HavokType.TypeKind.Bool:
                return Random.Shared.Next(0, 2) != 0;
            case HavokType.TypeKind.String:
                byte[] bytes = new byte[Random.Shared.Next(1, 11)];
                Random.Shared.NextBytes(bytes);
                return Encoding.ASCII.GetString(bytes);
            case HavokType.TypeKind.Int:
                long randomLong = Random.Shared.NextInt64();
                unchecked
                {
                    return ((format >> 10) / 8, (format & 0x200) != 0) switch
                    {
                        (1, true) => (sbyte)randomLong,
                        (1, false) => (byte)randomLong,
                        (2, true) => (short)randomLong,
                        (2, false) => (ushort)randomLong,
                        (4, true) => (int)randomLong,
                        (4, false) => (uint)randomLong,
                        (8, true) => randomLong,
                        (8, false) => (ulong)randomLong,
                        _ => throw new InvalidDataException("Unexpected int format")
                    };
                }
            case HavokType.TypeKind.Float:
                return (havokType.Size, format >> 16) switch
                {
                    (2, _) => BitConverter.ToSingle(new byte[]
                        { 0, 0, (byte)Random.Shared.Next(), (byte)Random.Shared.Next() }),
                    (4, _) => Random.Shared.NextSingle(),
                    (8, _) => Random.Shared.NextDouble(),
                    (16, 23) => Vector128.Create(Random.Shared.NextSingle(), Random.Shared.NextSingle(),
                        Random.Shared.NextSingle(), Random.Shared.NextSingle()),
                    (16, 52) => Vector128.Create(Random.Shared.NextDouble(), Random.Shared.NextDouble()),
                    _ => throw new InvalidDataException("Unexpected float format")
                };
            case HavokType.TypeKind.Record:
                IHavokObject havokObject = (IHavokObject)havokType.Instantiate();
                RandomizeValues(havokObject);
                return havokObject;
            case HavokType.TypeKind.Array:
            default:
                throw new NotImplementedException();
        }
    }

    private static void FileRoundtrip(string path, string? compendiumPath)
    {
        HavokBinarySerializer serializer = new();
        if (compendiumPath is not null)
        {
            serializer.LoadCompendium(compendiumPath);
        }

        IHavokObject obj = serializer.Read(path);
        string outputPath = path + "-out";
        serializer.Write(obj, outputPath);
        Assert.That(FileConvert(outputPath, outputPath, null), Is.EqualTo(0),
            "FileConvert failed to read output file.");
        byte[] output = File.ReadAllBytes(outputPath);

        string copyPath = path + "-copy";
        Assert.That(FileConvert(path, copyPath, compendiumPath), Is.EqualTo(0),
            "FileConvert failed to read input file.");
        byte[] copy = File.ReadAllBytes(copyPath);

        Assert.That(copy.SequenceEqual(output), $"Output of file \"{path}\" is not byte-identical after write.");
    }

    private static int FileConvert(string input, string output, string? compendium)
    {
        string arguments = "-r=64m ";
        if (compendium is not null)
        {
            arguments += $"--compendium \"{compendium}\" ";
        }

        arguments += $"\"{input}\" \"{output}\"";

        ProcessStartInfo startInfo = new()
        {
            Arguments = arguments,
            CreateNoWindow = true,
            FileName = FileConvertPath,
        };
        Process fileConvert = Process.Start(startInfo)!;
        fileConvert.WaitForExit();
        return fileConvert.ExitCode;
    }
}