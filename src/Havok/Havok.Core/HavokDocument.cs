using Havok.Reflection;

namespace Havok;

/// <summary>
/// A type compendium (TCM0): the types that tagfiles without a TYPE section refer to by id.
/// </summary>
public sealed class HavokCompendium
{
    public HavokCompendium(IReadOnlyList<ulong> ids, IReadOnlyList<HavokType> types, HavokSdk sdk)
    {
        Ids = ids;
        Types = types;
        Sdk = sdk;
    }

    /// <summary>
    /// 64-bit ids by which tagfiles refer to this compendium (TCRF → TCID).
    /// </summary>
    public IReadOnlyList<ulong> Ids { get; }

    /// <summary>
    /// The types by their index in the compendium, starting with the null type (index 0 is null).
    /// </summary>
    public IReadOnlyList<HavokType> Types { get; }

    public HavokSdk Sdk { get; }

    /// <summary>
    /// The types whose hashes the compendium stores (THSH), written as its top level types.
    /// </summary>
    public IReadOnlyList<HavokType> TopLevelTypes { get; init; } = Array.Empty<HavokType>();

    /// <summary>
    /// The TYPE section as it was read, so writing the compendium back reproduces it exactly while its types are
    /// unchanged. Null for compendiums built in memory.
    /// </summary>
    public byte[]? TypeSectionBytes { get; init; }
}

/// <summary>
/// A Havok file held in memory: its root object, the format it was read from, and the profile and compendium used
/// to read it, so it can be written back in the same form. The unified readers and writers that produce and consume
/// documents arrive with the format engines (Phases 2 to 5); this phase defines the shape.
/// </summary>
public sealed class HavokDocument
{
    public HavokDocument(IHavokObject root, HavokProfile profile, HavokFileInfo source, HavokCompendium? compendium = null)
    {
        Root = root;
        Profile = profile;
        Source = source;
        Compendium = compendium;
    }

    /// <summary>
    /// The root level object, normally an hkRootLevelContainer.
    /// </summary>
    public IHavokObject Root { get; set; }

    public HavokProfile Profile { get; }

    /// <summary>
    /// The header of the file this document was read from. Writes keep its container, SDK and layout.
    /// </summary>
    public HavokFileInfo Source { get; }

    /// <summary>
    /// The compendium the file was read against, if its types live in one. Writes refer to the same compendium.
    /// </summary>
    public HavokCompendium? Compendium { get; }

    public HavokContainer Container => Source.Container;

    public HavokSdk Sdk => Source.Sdk;
}
