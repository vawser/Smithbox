// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiEdgeFollowingBehaviorCornerPredictorInitInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiEdgeFollowingBehavior.CornerPredictorInitInfo>
{
    public hkaiEdgeFollowingBehaviorCornerPredictorInitInfoData(Havok.hk2018.hkaiEdgeFollowingBehavior.CornerPredictorInitInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_positionLocal":
            case "positionLocal":
            {
                value = instance.m_positionLocal;
                return true;
            }
            case "m_forwardVectorLocal":
            case "forwardVectorLocal":
            {
                value = instance.m_forwardVectorLocal;
                return true;
            }
            case "m_upLocal":
            case "upLocal":
            {
                value = instance.m_upLocal;
                return true;
            }
            case "m_positionSectionIndex":
            case "positionSectionIndex":
            {
                value = instance.m_positionSectionIndex;
                return true;
            }
            case "m_nextEdgeIndex":
            case "nextEdgeIndex":
            {
                value = instance.m_nextEdgeIndex;
                return true;
            }
            case "m_nextIsLeft":
            case "nextIsLeft":
            {
                value = instance.m_nextIsLeft;
                return true;
            }
            case "m_hasInfo":
            case "hasInfo":
            {
                value = instance.m_hasInfo;
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
