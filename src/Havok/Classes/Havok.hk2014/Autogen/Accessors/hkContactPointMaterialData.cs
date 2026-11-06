// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkContactPointMaterialData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkContactPointMaterial>
{
    public hkContactPointMaterialData(Havok.hk2014.hkContactPointMaterial instance) : base(instance) { }

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
            case "m_maxImpulse":
            case "maxImpulse":
            {
                value = instance.m_maxImpulse;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                value = instance.m_flags;
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
            case "m_friction":
            case "friction":
            {
                if (value is not Havok.hk2014.hkUFloat8 castValue) return false;
                instance.m_friction = castValue;
                return true;
            }
            case "m_restitution":
            case "restitution":
            {
                if (value is not byte castValue) return false;
                instance.m_restitution = castValue;
                return true;
            }
            case "m_maxImpulse":
            case "maxImpulse":
            {
                if (value is not Havok.hk2014.hkUFloat8 castValue) return false;
                instance.m_maxImpulse = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (value is not byte castValue) return false;
                instance.m_flags = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
