// Automatically Generated

#nullable enable

namespace Havok.hk2015.Accessors;

internal sealed class hkpMaterialData : Havok.Formats.HavokFieldAccess<Havok.hk2015.hkpMaterial>
{
    public hkpMaterialData(Havok.hk2015.hkpMaterial instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_responseType":
            case "responseType":
            {
                value = instance.m_responseType;
                return true;
            }
            case "m_rollingFrictionMultiplier":
            case "rollingFrictionMultiplier":
            {
                value = instance.m_rollingFrictionMultiplier;
                return true;
            }
            case "m_friction":
            case "friction":
            {
                value = instance.m_friction;
                return true;
            }
            case "m_restitution":
            case "restitution":
            {
                value = instance.m_restitution;
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
            case "m_responseType":
            case "responseType":
            {
                if (value is not sbyte castValue) return false;
                instance.m_responseType = castValue;
                return true;
            }
            case "m_rollingFrictionMultiplier":
            case "rollingFrictionMultiplier":
            {
                if (value is not float castValue) return false;
                instance.m_rollingFrictionMultiplier = castValue;
                return true;
            }
            case "m_friction":
            case "friction":
            {
                if (value is not float castValue) return false;
                instance.m_friction = castValue;
                return true;
            }
            case "m_restitution":
            case "restitution":
            {
                if (value is not float castValue) return false;
                instance.m_restitution = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
