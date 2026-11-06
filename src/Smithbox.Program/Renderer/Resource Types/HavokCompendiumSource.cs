using SoulsFormats;

namespace StudioCore.Renderer;

/// <summary>
/// The type compendium of a Havok binder, which the tagfiles in the binder refer to rather than holding their own
/// types. It is read the first time it is needed, with the profile of the files that use it, and then shared by
/// every file of the binder.
/// </summary>
public sealed class HavokCompendiumSource
{
    private readonly Memory<byte> _bytes;
    private Havok.HavokCompendium _unified;
    private readonly object _unifiedLock = new();

    public HavokCompendiumSource(Memory<byte> bytes)
    {
        _bytes = bytes;
    }

    /// <summary>
    /// The compendium for the unified Havok library (Havok.Files), read once, with the profile of the files that use it.
    /// </summary>
    public Havok.HavokCompendium Unified(Havok.HavokProfile profile)
    {
        lock (_unifiedLock)
        {
            if (_unified == null)
            {
                var data = DCX.Is(_bytes) ? DCX.Decompress(_bytes) : _bytes;
                _unified = Havok.Files.HavokFile.ReadCompendium(data.ToArray(), profile);
            }

            return _unified;
        }
    }

    /// <summary>
    /// Gets the compendium held in a binder, or null if it has none.
    /// </summary>
    public static HavokCompendiumSource Find(BinderReader binder)
    {
        var header = binder.Files.FirstOrDefault(e => e.Name?.Contains(".compendium", StringComparison.OrdinalIgnoreCase) ?? false);

        return header == null ? null : new HavokCompendiumSource(binder.ReadFile(header));
    }
}
