// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavigatorCornerPredictorInitInfoData : HavokData<HKLib.hk2019.hkaiNavigator.CornerPredictorInitInfo> 
{
    public hkaiNavigatorCornerPredictorInitInfoData(HavokType type, HKLib.hk2019.hkaiNavigator.CornerPredictorInitInfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_forwardVectorLocal":
            case "forwardVectorLocal":
            {
                if (instance.m_forwardVectorLocal is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nextEdgeIndex":
            case "nextEdgeIndex":
            {
                if (instance.m_nextEdgeIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nextIsLeft":
            case "nextIsLeft":
            {
                if (instance.m_nextIsLeft is not TGet castValue) return false;
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
            case "m_forwardVectorLocal":
            case "forwardVectorLocal":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_forwardVectorLocal = castValue;
                return true;
            }
            case "m_nextEdgeIndex":
            case "nextEdgeIndex":
            {
                if (value is not int castValue) return false;
                instance.m_nextEdgeIndex = castValue;
                return true;
            }
            case "m_nextIsLeft":
            case "nextIsLeft":
            {
                if (value is not bool castValue) return false;
                instance.m_nextIsLeft = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
