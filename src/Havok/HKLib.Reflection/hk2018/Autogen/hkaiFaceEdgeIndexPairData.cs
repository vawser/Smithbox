// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiFaceEdgeIndexPairData : HavokData<hkaiFaceEdgeIndexPair> 
{
    public hkaiFaceEdgeIndexPairData(HavokType type, hkaiFaceEdgeIndexPair instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_faceIndex":
            case "faceIndex":
            {
                if (instance.m_faceIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edgeIndex":
            case "edgeIndex":
            {
                if (instance.m_edgeIndex is not TGet castValue) return false;
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
            case "m_faceIndex":
            case "faceIndex":
            {
                if (value is not int castValue) return false;
                instance.m_faceIndex = castValue;
                return true;
            }
            case "m_edgeIndex":
            case "edgeIndex":
            {
                if (value is not int castValue) return false;
                instance.m_edgeIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
