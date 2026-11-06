// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbDampingModifierData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbDampingModifier>
{
    public hkbDampingModifierData(Havok.hk2014.hkbDampingModifier instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_kP":
            case "kP":
            {
                value = instance.m_kP;
                return true;
            }
            case "m_kI":
            case "kI":
            {
                value = instance.m_kI;
                return true;
            }
            case "m_kD":
            case "kD":
            {
                value = instance.m_kD;
                return true;
            }
            case "m_enableScalarDamping":
            case "enableScalarDamping":
            {
                value = instance.m_enableScalarDamping;
                return true;
            }
            case "m_enableVectorDamping":
            case "enableVectorDamping":
            {
                value = instance.m_enableVectorDamping;
                return true;
            }
            case "m_rawValue":
            case "rawValue":
            {
                value = instance.m_rawValue;
                return true;
            }
            case "m_dampedValue":
            case "dampedValue":
            {
                value = instance.m_dampedValue;
                return true;
            }
            case "m_rawVector":
            case "rawVector":
            {
                value = instance.m_rawVector;
                return true;
            }
            case "m_dampedVector":
            case "dampedVector":
            {
                value = instance.m_dampedVector;
                return true;
            }
            case "m_vecErrorSum":
            case "vecErrorSum":
            {
                value = instance.m_vecErrorSum;
                return true;
            }
            case "m_vecPreviousError":
            case "vecPreviousError":
            {
                value = instance.m_vecPreviousError;
                return true;
            }
            case "m_errorSum":
            case "errorSum":
            {
                value = instance.m_errorSum;
                return true;
            }
            case "m_previousError":
            case "previousError":
            {
                value = instance.m_previousError;
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
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (value is null)
                {
                    instance.m_variableBindingSet = default;
                    return true;
                }
                if (value is Havok.hk2014.hkbVariableBindingSet castValue)
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
            case "m_kP":
            case "kP":
            {
                if (value is not float castValue) return false;
                instance.m_kP = castValue;
                return true;
            }
            case "m_kI":
            case "kI":
            {
                if (value is not float castValue) return false;
                instance.m_kI = castValue;
                return true;
            }
            case "m_kD":
            case "kD":
            {
                if (value is not float castValue) return false;
                instance.m_kD = castValue;
                return true;
            }
            case "m_enableScalarDamping":
            case "enableScalarDamping":
            {
                if (value is not bool castValue) return false;
                instance.m_enableScalarDamping = castValue;
                return true;
            }
            case "m_enableVectorDamping":
            case "enableVectorDamping":
            {
                if (value is not bool castValue) return false;
                instance.m_enableVectorDamping = castValue;
                return true;
            }
            case "m_rawValue":
            case "rawValue":
            {
                if (value is not float castValue) return false;
                instance.m_rawValue = castValue;
                return true;
            }
            case "m_dampedValue":
            case "dampedValue":
            {
                if (value is not float castValue) return false;
                instance.m_dampedValue = castValue;
                return true;
            }
            case "m_rawVector":
            case "rawVector":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_rawVector = castValue;
                return true;
            }
            case "m_dampedVector":
            case "dampedVector":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_dampedVector = castValue;
                return true;
            }
            case "m_vecErrorSum":
            case "vecErrorSum":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_vecErrorSum = castValue;
                return true;
            }
            case "m_vecPreviousError":
            case "vecPreviousError":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_vecPreviousError = castValue;
                return true;
            }
            case "m_errorSum":
            case "errorSum":
            {
                if (value is not float castValue) return false;
                instance.m_errorSum = castValue;
                return true;
            }
            case "m_previousError":
            case "previousError":
            {
                if (value is not float castValue) return false;
                instance.m_previousError = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
