using HKLib.Reflection;

namespace HKLib.Serialization;

/// <summary>
/// Represents a Havok compendium file which stores type information for tagfiles.
/// </summary>
public abstract class HavokCompendium
{
    private readonly List<ulong> _ids;

    protected HavokCompendium(List<ulong> ids)
    {
        _ids = ids;
    }

    /// <summary>
    /// 64-bit hashes by which tagfiles refer to this compendium
    /// </summary>
    public IReadOnlyList<ulong> Ids => _ids;

    /// <summary>
    /// The types in this compendium
    /// </summary>
    public abstract IReadOnlyList<HavokType> Types { get; }
}