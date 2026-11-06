// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkAxialRotationData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkAxialRotation>
{
    public hkAxialRotationData(Havok.hk2018.hkAxialRotation instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_tag":
            case "tag":
            {
                value = instance.m_tag;
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
            case "m_tag":
            case "tag":
            {
                if (value is not byte castValue) return false;
                instance.m_tag = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
