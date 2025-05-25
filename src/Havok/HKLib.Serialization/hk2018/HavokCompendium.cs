using HKLib.Reflection.hk2018;

namespace HKLib.Serialization.hk2018;

public class HavokCompendium : Serialization.HavokCompendium
{
    private readonly List<HavokType> _types;

    public HavokCompendium(List<ulong> ids, List<HavokType> types) : base(ids)
    {
        _types = types;
    }

    public override IReadOnlyList<HavokType> Types => _types;

    public List<HavokType> TopLevelTypes { get; } = new();
}