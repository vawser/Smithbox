// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclVirtualCollisionPointsDataBarycentricPairData : HavokData<hclVirtualCollisionPointsData.BarycentricPair> 
{
    public hclVirtualCollisionPointsDataBarycentricPairData(HavokType type, hclVirtualCollisionPointsData.BarycentricPair instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_u":
            case "u":
            {
                if (instance.m_u is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_v":
            case "v":
            {
                if (instance.m_v is not TGet castValue) return false;
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
            case "m_u":
            case "u":
            {
                if (value is not float castValue) return false;
                instance.m_u = castValue;
                return true;
            }
            case "m_v":
            case "v":
            {
                if (value is not float castValue) return false;
                instance.m_v = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
