// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiCornerPredictorUserEdgeTraversalData : HavokData<hkaiCornerPredictor.UserEdgeTraversal> 
{
    public hkaiCornerPredictorUserEdgeTraversalData(HavokType type, hkaiCornerPredictor.UserEdgeTraversal instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_entrancePointLocal":
            case "entrancePointLocal":
            {
                if (instance.m_entrancePointLocal is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edgeDataPtr":
            case "edgeDataPtr":
            {
                if (instance.m_edgeDataPtr is null)
                {
                    return true;
                }
                if (instance.m_edgeDataPtr is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_entrancePointLocal":
            case "entrancePointLocal":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_entrancePointLocal = castValue;
                return true;
            }
            case "m_edgeDataPtr":
            case "edgeDataPtr":
            {
                if (value is null)
                {
                    instance.m_edgeDataPtr = default;
                    return true;
                }
                if (value is int castValue)
                {
                    instance.m_edgeDataPtr = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
