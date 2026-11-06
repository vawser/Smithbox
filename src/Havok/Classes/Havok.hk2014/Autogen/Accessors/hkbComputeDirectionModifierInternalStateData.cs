// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbComputeDirectionModifierInternalStateData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbComputeDirectionModifierInternalState>
{
    public hkbComputeDirectionModifierInternalStateData(Havok.hk2014.hkbComputeDirectionModifierInternalState instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_pointOut":
            case "pointOut":
            {
                value = instance.m_pointOut;
                return true;
            }
            case "m_groundAngleOut":
            case "groundAngleOut":
            {
                value = instance.m_groundAngleOut;
                return true;
            }
            case "m_upAngleOut":
            case "upAngleOut":
            {
                value = instance.m_upAngleOut;
                return true;
            }
            case "m_computedOutput":
            case "computedOutput":
            {
                value = instance.m_computedOutput;
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
            case "m_pointOut":
            case "pointOut":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_pointOut = castValue;
                return true;
            }
            case "m_groundAngleOut":
            case "groundAngleOut":
            {
                if (value is not float castValue) return false;
                instance.m_groundAngleOut = castValue;
                return true;
            }
            case "m_upAngleOut":
            case "upAngleOut":
            {
                if (value is not float castValue) return false;
                instance.m_upAngleOut = castValue;
                return true;
            }
            case "m_computedOutput":
            case "computedOutput":
            {
                if (value is not bool castValue) return false;
                instance.m_computedOutput = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
