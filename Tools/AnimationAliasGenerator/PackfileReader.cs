using SoulsFormats;
using System.Collections;
using System.Reflection;
using System.Text;

/// <summary>
/// A view of a Havok packfile by its fixups, for files whose class layouts the HKX2 classes don't match, such as
/// Bloodborne's. HKX2 reads the sections and fixups before it builds any objects, so they are taken from it by
/// reflection even when building the objects fails. Fields are then read at known offsets.
/// </summary>
public sealed class PackfileReader
{
    public byte[] Data { get; }

    private readonly Dictionary<uint, uint> Local = new();
    private readonly Dictionary<uint, uint> Global = new();

    /// <summary>
    /// The position and class name of every object, in order
    /// </summary>
    public List<(uint Offset, string ClassName)> Objects { get; } = new();

    public PackfileReader(byte[] bytes)
    {
        var deserializer = new HKX2.PackFileDeserializer();

        try
        {
            deserializer.Deserialize(new BinaryReaderEx(false, bytes));
        }
        catch
        {
            // Expected for layouts HKX2 doesn't know; the fixups are already read by then
        }

        const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

        var section = typeof(HKX2.PackFileDeserializer).GetField("_dataSection", flags)!.GetValue(deserializer)
            ?? throw new InvalidDataException("The packfile sections could not be read");
        var classNames = typeof(HKX2.PackFileDeserializer).GetField("_classnames", flags)!.GetValue(deserializer)
            ?? throw new InvalidDataException("The packfile class names could not be read");

        var sectionType = section.GetType();

        Data = (byte[])sectionType.GetField("SectionData", flags)!.GetValue(section);

        foreach (var fixup in (IEnumerable)sectionType.GetField("LocalFixups", flags)!.GetValue(section))
        {
            Local[(uint)Get(fixup, "Src")] = (uint)Get(fixup, "Dst");
        }

        foreach (var fixup in (IEnumerable)sectionType.GetField("GlobalFixups", flags)!.GetValue(section))
        {
            Global[(uint)Get(fixup, "Src")] = (uint)Get(fixup, "Dst");
        }

        var nameMap = (IDictionary)classNames.GetType().GetField("OffsetClassNamesMap", flags)!.GetValue(classNames);

        foreach (var fixup in (IEnumerable)sectionType.GetField("VirtualFixups", flags)!.GetValue(section))
        {
            var nameOffset = (uint)Get(fixup, "NameOffset");
            var className = nameMap[nameOffset] is HKX2.HKXClassName c ? c.ClassName : "?";

            Objects.Add(((uint)Get(fixup, "Src"), className));
        }

        Objects.Sort((a, b) => a.Offset.CompareTo(b.Offset));
    }

    private static object Get(object obj, string field)
    {
        return obj.GetType().GetField(field, BindingFlags.Public | BindingFlags.Instance)!.GetValue(obj);
    }

    public string ReadString(uint position)
    {
        if (!Local.TryGetValue(position, out var dst))
            return null;

        var end = Array.IndexOf(Data, (byte)0, (int)dst);

        return Encoding.ASCII.GetString(Data, (int)dst, (end < 0 ? Data.Length : end) - (int)dst);
    }

    public int ReadInt32(uint position) => BitConverter.ToInt32(Data, (int)position);

    /// <summary>
    /// Reads an hkArray of object pointers, giving the position of each object
    /// </summary>
    public List<uint> ReadPointerArray(uint position)
    {
        var result = new List<uint>();

        var size = BitConverter.ToUInt32(Data, (int)position + 8);

        if (size == 0 || !Local.TryGetValue(position, out var dst))
            return result;

        for (uint i = 0; i < size; i++)
        {
            if (Global.TryGetValue(dst + i * 8, out var target))
                result.Add(target);
        }

        return result;
    }

    /// <summary>
    /// The strings an object points to, by their offset in the object, for working out a layout
    /// </summary>
    public List<(uint FieldOffset, string Value)> StringFields(uint offset, uint size)
    {
        var result = new List<(uint, string)>();

        foreach (var (src, _) in Local)
        {
            if (src >= offset && src < offset + size && ReadString(src) is { } value && IsPrintable(value))
                result.Add((src - offset, value));
        }

        result.Sort((a, b) => a.Item1.CompareTo(b.Item1));
        return result;
    }

    public uint SizeOf(int objectIndex)
    {
        var start = Objects[objectIndex].Offset;
        var end = objectIndex + 1 < Objects.Count ? Objects[objectIndex + 1].Offset : (uint)Data.Length;

        return end - start;
    }

    private static bool IsPrintable(string value) => value.Length > 0 && value.All(c => c >= 0x20 && c < 0x7F);
}
