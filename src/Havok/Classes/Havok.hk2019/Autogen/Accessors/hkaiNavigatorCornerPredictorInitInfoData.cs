// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavigatorCornerPredictorInitInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavigator.CornerPredictorInitInfo>
{
    public hkaiNavigatorCornerPredictorInitInfoData(Havok.hk2019.hkaiNavigator.CornerPredictorInitInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_forwardVectorLocal":
            case "forwardVectorLocal":
            {
                value = instance.m_forwardVectorLocal;
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
            default:
            return false;
        }
    }

    public override bool TrySetField(string fieldName, object? value)
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
