// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkxAnimatedQuaternionData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkxAnimatedQuaternion>
{
    public hkxAnimatedQuaternionData(Havok.hk2014.hkxAnimatedQuaternion instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
