// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkContactPointData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkContactPoint>
{
    public hkContactPointData(Havok.hk2019.hkContactPoint instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_position":
            case "position":
            {
                value = instance.m_position;
                return true;
            }
            case "m_separatingNormal":
            case "separatingNormal":
            {
                value = instance.m_separatingNormal;
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
            case "m_position":
            case "position":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_position = castValue;
                return true;
            }
            case "m_separatingNormal":
            case "separatingNormal":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_separatingNormal = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
