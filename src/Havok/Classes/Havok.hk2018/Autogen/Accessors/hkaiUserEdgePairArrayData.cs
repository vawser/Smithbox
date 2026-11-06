// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiUserEdgePairArrayData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiUserEdgePairArray>
{
    public hkaiUserEdgePairArrayData(Havok.hk2018.hkaiUserEdgePairArray instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_edgePairs":
            case "edgePairs":
            {
                if (value is not List<Havok.hk2018.hkaiUserEdgeUtils.UserEdgePair> castValue) return false;
                instance.m_edgePairs = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
