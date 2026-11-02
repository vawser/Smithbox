// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiUserEdgePairArrayData : HavokData<HKLib.hk2019.hkaiUserEdgePairArray> 
{
    public hkaiUserEdgePairArrayData(HavokType type, HKLib.hk2019.hkaiUserEdgePairArray instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_edgePairs":
            case "edgePairs":
            {
                if (instance.m_edgePairs is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_edgePairs":
            case "edgePairs":
            {
                if (value is not List<HKLib.hk2019.hkaiUserEdgeUtils.UserEdgePair> castValue) return false;
                instance.m_edgePairs = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
