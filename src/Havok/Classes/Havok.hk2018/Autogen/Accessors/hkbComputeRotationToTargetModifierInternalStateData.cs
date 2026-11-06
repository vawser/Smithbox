// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbComputeRotationToTargetModifierInternalStateData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbComputeRotationToTargetModifierInternalState>
{
    public hkbComputeRotationToTargetModifierInternalStateData(Havok.hk2018.hkbComputeRotationToTargetModifierInternalState instance) : base(instance) { }

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
            case "m_rotationOut":
            case "rotationOut":
            {
                value = instance.m_rotationOut;
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
            case "m_rotationOut":
            case "rotationOut":
            {
                if (value is not Quaternion castValue) return false;
                instance.m_rotationOut = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
