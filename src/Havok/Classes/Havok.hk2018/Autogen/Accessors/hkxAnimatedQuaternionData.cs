// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkxAnimatedQuaternionData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkxAnimatedQuaternion>
{
    public hkxAnimatedQuaternionData(Havok.hk2018.hkxAnimatedQuaternion instance) : base(instance) { }

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
            case "m_quaternions":
            case "quaternions":
            {
                value = instance.m_quaternions;
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
            case "m_quaternions":
            case "quaternions":
            {
                if (value is not List<float> castValue) return false;
                instance.m_quaternions = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
