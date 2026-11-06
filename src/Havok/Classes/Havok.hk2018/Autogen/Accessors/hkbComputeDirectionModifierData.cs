// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbComputeDirectionModifierData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbComputeDirectionModifier>
{
    public hkbComputeDirectionModifierData(Havok.hk2018.hkbComputeDirectionModifier instance) : base(instance) { }

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
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                value = instance.m_variableBindingSet;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_enable":
            case "enable":
            {
                value = instance.m_enable;
                return true;
            }
            case "m_pointIn":
            case "pointIn":
            {
                value = instance.m_pointIn;
                return true;
            }
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
            case "m_verticalOffset":
            case "verticalOffset":
            {
                value = instance.m_verticalOffset;
                return true;
            }
            case "m_reverseGroundAngle":
            case "reverseGroundAngle":
            {
                value = instance.m_reverseGroundAngle;
                return true;
            }
            case "m_reverseUpAngle":
            case "reverseUpAngle":
            {
                value = instance.m_reverseUpAngle;
                return true;
            }
            case "m_projectPoint":
            case "projectPoint":
            {
                value = instance.m_projectPoint;
                return true;
            }
            case "m_normalizePoint":
            case "normalizePoint":
            {
                value = instance.m_normalizePoint;
                return true;
            }
            case "m_computeOnlyOnce":
            case "computeOnlyOnce":
            {
                value = instance.m_computeOnlyOnce;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (value is null)
                {
                    instance.m_variableBindingSet = default;
                    return true;
                }
                if (value is Havok.hk2018.hkbVariableBindingSet castValue)
                {
                    instance.m_variableBindingSet = castValue;
                    return true;
                }
                return false;
            }
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_enable":
            case "enable":
            {
                if (value is not bool castValue) return false;
                instance.m_enable = castValue;
                return true;
            }
            case "m_pointIn":
            case "pointIn":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_pointIn = castValue;
                return true;
            }
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
            case "m_verticalOffset":
            case "verticalOffset":
            {
                if (value is not float castValue) return false;
                instance.m_verticalOffset = castValue;
                return true;
            }
            case "m_reverseGroundAngle":
            case "reverseGroundAngle":
            {
                if (value is not bool castValue) return false;
                instance.m_reverseGroundAngle = castValue;
                return true;
            }
            case "m_reverseUpAngle":
            case "reverseUpAngle":
            {
                if (value is not bool castValue) return false;
                instance.m_reverseUpAngle = castValue;
                return true;
            }
            case "m_projectPoint":
            case "projectPoint":
            {
                if (value is not bool castValue) return false;
                instance.m_projectPoint = castValue;
                return true;
            }
            case "m_normalizePoint":
            case "normalizePoint":
            {
                if (value is not bool castValue) return false;
                instance.m_normalizePoint = castValue;
                return true;
            }
            case "m_computeOnlyOnce":
            case "computeOnlyOnce":
            {
                if (value is not bool castValue) return false;
                instance.m_computeOnlyOnce = castValue;
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
