// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiUserEdgePairArrayData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiUserEdgePairArray>
{
    public hkaiUserEdgePairArrayData(Havok.hk2019.hkaiUserEdgePairArray instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_edgePairs":
            case "edgePairs":
            {
                value = instance.m_edgePairs;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField(string fieldName, object? value)
    {
        switch (fieldName)
        {
            case "m_edgePairs":
            case "edgePairs":
            {
                if (value is not List<Havok.hk2019.hkaiUserEdgeUtils.UserEdgePair> castValue) return false;
                instance.m_edgePairs = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
