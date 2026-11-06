// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkRadiansData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkRadians>
{
    public hkRadiansData(Havok.hk2018.hkRadians instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_radians":
            case "radians":
            {
                value = instance.m_radians;
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
            case "m_radians":
            case "radians":
            {
                if (value is not float castValue) return false;
                instance.m_radians = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
