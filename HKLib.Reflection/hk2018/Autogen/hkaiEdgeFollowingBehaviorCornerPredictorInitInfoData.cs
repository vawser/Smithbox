// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiEdgeFollowingBehaviorCornerPredictorInitInfoData : HavokData<hkaiEdgeFollowingBehavior.CornerPredictorInitInfo> 
{
    public hkaiEdgeFollowingBehaviorCornerPredictorInitInfoData(HavokType type, hkaiEdgeFollowingBehavior.CornerPredictorInitInfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_positionLocal":
            case "positionLocal":
            {
                if (instance.m_positionLocal is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_forwardVectorLocal":
            case "forwardVectorLocal":
            {
                if (instance.m_forwardVectorLocal is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_upLocal":
            case "upLocal":
            {
                if (instance.m_upLocal is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_positionSectionIndex":
            case "positionSectionIndex":
            {
                if (instance.m_positionSectionIndex is not TGet castValue) return false;
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
            case "m_hasInfo":
            case "hasInfo":
            {
                if (instance.m_hasInfo is not TGet castValue) return false;
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
            case "m_positionLocal":
            case "positionLocal":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_positionLocal = castValue;
                return true;
            }
            case "m_forwardVectorLocal":
            case "forwardVectorLocal":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_forwardVectorLocal = castValue;
                return true;
            }
            case "m_upLocal":
            case "upLocal":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_upLocal = castValue;
                return true;
            }
            case "m_positionSectionIndex":
            case "positionSectionIndex":
            {
                if (value is not int castValue) return false;
                instance.m_positionSectionIndex = castValue;
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
            case "m_hasInfo":
            case "hasInfo":
            {
                if (value is not bool castValue) return false;
                instance.m_hasInfo = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
