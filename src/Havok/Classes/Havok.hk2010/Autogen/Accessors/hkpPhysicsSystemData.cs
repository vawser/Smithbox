// Automatically Generated

#nullable enable

namespace Havok.hk2010.Accessors;

internal sealed class hkpPhysicsSystemData : Havok.Formats.HavokFieldAccess<Havok.hk2010.hkpPhysicsSystem>
{
    public hkpPhysicsSystemData(Havok.hk2010.hkpPhysicsSystem instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_rigidBodies":
            case "rigidBodies":
            {
                value = instance.m_rigidBodies;
                return true;
            }
            case "m_constraints":
            case "constraints":
            {
                value = instance.m_constraints;
                return true;
            }
            case "m_actions":
            case "actions":
            {
                value = instance.m_actions;
                return true;
            }
            case "m_phantoms":
            case "phantoms":
            {
                value = instance.m_phantoms;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_active":
            case "active":
            {
                value = instance.m_active;
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
            case "m_rigidBodies":
            case "rigidBodies":
            {
                if (value is not List<Havok.hk2010.hkpRigidBody?> castValue) return false;
                instance.m_rigidBodies = castValue;
                return true;
            }
            case "m_constraints":
            case "constraints":
            {
                if (value is not List<Havok.hk2010.hkpConstraintInstance?> castValue) return false;
                instance.m_constraints = castValue;
                return true;
            }
            case "m_actions":
            case "actions":
            {
                if (value is not List<Havok.hk2010.hkpAction?> castValue) return false;
                instance.m_actions = castValue;
                return true;
            }
            case "m_phantoms":
            case "phantoms":
            {
                if (value is not List<Havok.hk2010.hkpPhantom?> castValue) return false;
                instance.m_phantoms = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_active":
            case "active":
            {
                if (value is not bool castValue) return false;
                instance.m_active = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
