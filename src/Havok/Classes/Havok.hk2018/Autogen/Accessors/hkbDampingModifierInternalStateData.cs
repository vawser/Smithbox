// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbDampingModifierInternalStateData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbDampingModifierInternalState>
{
    public hkbDampingModifierInternalStateData(Havok.hk2018.hkbDampingModifierInternalState instance) : base(instance) { }

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
            case "m_dampedValue":
            case "dampedValue":
            {
                value = instance.m_dampedValue;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
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
