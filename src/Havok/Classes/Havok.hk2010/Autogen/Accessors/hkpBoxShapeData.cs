// Automatically Generated

#nullable enable

namespace Havok.hk2010.Accessors;

internal sealed class hkpBoxShapeData : Havok.Formats.HavokFieldAccess<Havok.hk2010.hkpBoxShape>
{
    public hkpBoxShapeData(Havok.hk2010.hkpBoxShape instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_radius":
            case "radius":
            {
                value = instance.m_radius;
                return true;
            }
            case "m_halfExtents":
            case "halfExtents":
            {
                value = instance.m_halfExtents;
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
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_radius":
            case "radius":
            {
                if (value is not float castValue) return false;
                instance.m_radius = castValue;
                return true;
            }
            case "m_halfExtents":
            case "halfExtents":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_halfExtents = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
