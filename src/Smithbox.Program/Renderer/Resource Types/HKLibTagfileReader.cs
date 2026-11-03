using SoulsFormats;

namespace StudioCore.Renderer;

/// <summary>
/// Reads the Havok 2018 (ER, NR) and 2019 (AC6) tagfiles held by HKLib. Safe to call from several threads at once.
/// </summary>
public static class HKLibTagfileReader
{
    /// <param name="bytes">The file, compressed or not</param>
    /// <param name="compendium">The compendium of the binder the file is in, for files which refer to one for their types</param>
    public static HKLib.hk2018.hkRootLevelContainer Read2018(Memory<byte> bytes, HavokCompendiumSource compendium = null)
    {
        var serializer = new HKLib.Serialization.hk2018.Binary.HavokBinarySerializer();

        if (compendium != null)
            serializer.LoadCompendium(compendium.Hk2018);

        using var stream = Open(bytes);

        return (HKLib.hk2018.hkRootLevelContainer)serializer.Read(stream);
    }

    /// <inheritdoc cref="Read2018" />
    public static HKLib.hk2019.hkRootLevelContainer Read2019(Memory<byte> bytes, HavokCompendiumSource compendium = null)
    {
        var serializer = new HKLib.Serialization.hk2019.Binary.HavokBinarySerializer();

        if (compendium != null)
            serializer.LoadCompendium(compendium.Hk2019);

        using var stream = Open(bytes);

        return (HKLib.hk2019.hkRootLevelContainer)serializer.Read(stream);
    }

    internal static MemoryStream Open(Memory<byte> bytes)
    {
        var data = DCX.Is(bytes) ? DCX.Decompress(bytes) : bytes;

        return new MemoryStream(data.ToArray());
    }
}

/// <summary>
/// The type compendium of a Havok binder, which the tagfiles in the binder refer to rather than holding their own
/// types. It is read the first time it is needed, with the HKLib version of the file that needs it, and then shared
/// by every file of the binder.
/// </summary>
public sealed class HavokCompendiumSource
{
    private readonly Lazy<HKLib.Serialization.hk2018.HavokCompendium> _hk2018;
    private readonly Lazy<HKLib.Serialization.hk2019.HavokCompendium> _hk2019;

    public HavokCompendiumSource(Memory<byte> bytes)
    {
        _hk2018 = new(() =>
        {
            using var stream = HKLibTagfileReader.Open(bytes);
            return new HKLib.Serialization.hk2018.Binary.HavokBinarySerializer().ReadCompendium(stream);
        });

        _hk2019 = new(() =>
        {
            using var stream = HKLibTagfileReader.Open(bytes);
            return new HKLib.Serialization.hk2019.Binary.HavokBinarySerializer().ReadCompendium(stream);
        });
    }

    public HKLib.Serialization.hk2018.HavokCompendium Hk2018 => _hk2018.Value;

    public HKLib.Serialization.hk2019.HavokCompendium Hk2019 => _hk2019.Value;

    /// <summary>
    /// Gets the compendium held in a binder, or null if it has none.
    /// </summary>
    public static HavokCompendiumSource Find(BinderReader binder)
    {
        var header = binder.Files.FirstOrDefault(e => e.Name?.Contains(".compendium", StringComparison.OrdinalIgnoreCase) ?? false);

        return header == null ? null : new HavokCompendiumSource(binder.ReadFile(header));
    }
}
