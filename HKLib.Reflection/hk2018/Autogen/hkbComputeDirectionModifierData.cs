// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbComputeDirectionModifierData : HavokData<hkbComputeDirectionModifier> 
{
    public hkbComputeDirectionModifierData(HavokType type, hkbComputeDirectionModifier instance) : base(type, instance) {}

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
            case "m_pointIn":
            case "pointIn":
            {
                if (instance.m_pointIn is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_pointOut":
            case "pointOut":
            {
                if (instance.m_pointOut is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_groundAngleOut":
            case "groundAngleOut":
            {
                if (instance.m_groundAngleOut is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_upAngleOut":
            case "upAngleOut":
            {
                if (instance.m_upAngleOut is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_verticalOffset":
            case "verticalOffset":
            {
                if (instance.m_verticalOffset is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_reverseGroundAngle":
            case "reverseGroundAngle":
            {
                if (instance.m_reverseGroundAngle is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_reverseUpAngle":
            case "reverseUpAngle":
            {
                if (instance.m_reverseUpAngle is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_projectPoint":
            case "projectPoint":
            {
                if (instance.m_projectPoint is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_normalizePoint":
            case "normalizePoint":
            {
                if (instance.m_normalizePoint is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_computeOnlyOnce":
            case "computeOnlyOnce":
            {
                if (instance.m_computeOnlyOnce is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_computedOutput":
            case "computedOutput":
            {
                if (instance.m_computedOutput is not TGet castValue) return false;
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
