using HKLib.Reflection.hk2019;

namespace HKLib.Serialization.hk2019.Binary;

/// <summary>
/// A TYPE section as it was read. 2019 files hold a fixed set of types, with bodies for only some of them, which
/// can't be reproduced from the type registry, so the section is written back as it was when it still holds every
/// type that is needed.
/// </summary>
internal class TypeSection
{
    public TypeSection(byte[] bytes, IReadOnlyList<HavokType> types)
    {
        Bytes = bytes;
        Types = types;
    }

    public byte[] Bytes { get; }

    /// <summary>
    /// The types by their index in the section, starting with the null type.
    /// </summary>
    public IReadOnlyList<HavokType> Types { get; }

    public bool TryGetIndices(IEnumerable<HavokType> types, out IReadOnlyDictionary<HavokType, int> typeIndices)
    {
        Dictionary<HavokType, int> indices = new();
        typeIndices = indices;

        for (int i = 1; i < Types.Count; i++)
        {
            if (Types[i] is { } type) indices.TryAdd(type, i);
        }

        return types.All(indices.ContainsKey);
    }
}
