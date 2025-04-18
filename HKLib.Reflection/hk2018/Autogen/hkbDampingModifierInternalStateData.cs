// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbDampingModifierInternalStateData : HavokData<hkbDampingModifierInternalState> 
{
    public hkbDampingModifierInternalStateData(HavokType type, hkbDampingModifierInternalState instance) : base(type, instance) {}

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
            case "m_dampedValue":
            case "dampedValue":
            {
                if (instance.m_dampedValue is not TGet castValue) return false;
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
            case "m_dampedValue":
            case "dampedValue":
            {
                if (value is not float castValue) return false;
                instance.m_dampedValue = castValue;
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
