// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiFaceEdgePairData : HavokData<hkaiFaceEdgePair> 
{
    public hkaiFaceEdgePairData(HavokType type, hkaiFaceEdgePair instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_faceKey":
            case "faceKey":
            {
                if (instance.m_faceKey is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edgeKey":
            case "edgeKey":
            {
                if (instance.m_edgeKey is not TGet castValue) return false;
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
            case "m_faceKey":
            case "faceKey":
            {
                if (value is not uint castValue) return false;
                instance.m_faceKey = castValue;
                return true;
            }
            case "m_edgeKey":
            case "edgeKey":
            {
                if (value is not uint castValue) return false;
                instance.m_edgeKey = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
