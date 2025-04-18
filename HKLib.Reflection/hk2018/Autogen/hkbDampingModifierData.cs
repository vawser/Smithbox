// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbDampingModifierData : HavokData<hkbDampingModifier> 
{
    public hkbDampingModifierData(HavokType type, hkbDampingModifier instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (instance.m_variableBindingSet is null)
                {
                    return true;
                }
                if (instance.m_variableBindingSet is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_userData":
            case "userData":
            {
                if (instance.m_userData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (instance.m_name is null)
                {
                    return true;
                }
                if (instance.m_name is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_enable":
            case "enable":
            {
                if (instance.m_enable is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_kP":
            case "kP":
            {
                if (instance.m_kP is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_kI":
            case "kI":
            {
                if (instance.m_kI is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_kD":
            case "kD":
            {
                if (instance.m_kD is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_enableScalarDamping":
            case "enableScalarDamping":
            {
                if (instance.m_enableScalarDamping is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_enableVectorDamping":
            case "enableVectorDamping":
            {
                if (instance.m_enableVectorDamping is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_rawValue":
            case "rawValue":
            {
                if (instance.m_rawValue is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_dampedValue":
            case "dampedValue":
            {
                if (instance.m_dampedValue is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_rawVector":
            case "rawVector":
            {
                if (instance.m_rawVector is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_dampedVector":
            case "dampedVector":
            {
                if (instance.m_dampedVector is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_vecErrorSum":
            case "vecErrorSum":
            {
                if (instance.m_vecErrorSum is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_vecPreviousError":
            case "vecPreviousError":
            {
                if (instance.m_vecPreviousError is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_errorSum":
            case "errorSum":
            {
                if (instance.m_errorSum is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_previousError":
            case "previousError":
            {
                if (instance.m_previousError is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
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
                if (value is hkbVariableBindingSet castValue)
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
