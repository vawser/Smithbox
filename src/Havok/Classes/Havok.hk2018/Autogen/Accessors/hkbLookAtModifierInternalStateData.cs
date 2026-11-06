// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbLookAtModifierInternalStateData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbLookAtModifierInternalState>
{
    public hkbLookAtModifierInternalStateData(Havok.hk2018.hkbLookAtModifierInternalState instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_lookAtLastTargetWS":
            case "lookAtLastTargetWS":
            {
                value = instance.m_lookAtLastTargetWS;
                return true;
            }
            case "m_lookAtWeight":
            case "lookAtWeight":
            {
                value = instance.m_lookAtWeight;
                return true;
            }
            case "m_isTargetInsideLimitCone":
            case "isTargetInsideLimitCone":
            {
                value = instance.m_isTargetInsideLimitCone;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_lookAtLastTargetWS":
            case "lookAtLastTargetWS":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_lookAtLastTargetWS = castValue;
                return true;
            }
            case "m_lookAtWeight":
            case "lookAtWeight":
            {
                if (value is not float castValue) return false;
                instance.m_lookAtWeight = castValue;
                return true;
            }
            case "m_isTargetInsideLimitCone":
            case "isTargetInsideLimitCone":
            {
                if (value is not bool castValue) return false;
                instance.m_isTargetInsideLimitCone = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
