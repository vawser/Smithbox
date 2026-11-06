// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkSphereData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkSphere>
{
    public hkSphereData(Havok.hk2016.hkSphere instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_pos":
            case "pos":
            {
                value = instance.m_pos;
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
            case "m_pos":
            case "pos":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_pos = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
