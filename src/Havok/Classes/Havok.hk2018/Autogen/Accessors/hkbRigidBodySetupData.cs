// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbRigidBodySetupData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbRigidBodySetup>
{
    public hkbRigidBodySetupData(Havok.hk2018.hkbRigidBodySetup instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_collisionFilterInfo":
            case "collisionFilterInfo":
            {
                value = instance.m_collisionFilterInfo;
                return true;
            }
            case "m_type":
            case "type":
            {
                value = instance.m_type;
                return true;
            }
            case "m_collisionShapeProfiles":
            case "collisionShapeProfiles":
            {
                value = instance.m_collisionShapeProfiles;
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
            case "m_collisionFilterInfo":
            case "collisionFilterInfo":
            {
                if (value is not uint castValue) return false;
                instance.m_collisionFilterInfo = castValue;
                return true;
            }
            case "m_type":
            case "type":
            {
                if (value is Havok.hk2018.hkbRigidBodySetup.Type castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_type = (Havok.hk2018.hkbRigidBodySetup.Type)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_collisionShapeProfiles":
            case "collisionShapeProfiles":
            {
                if (value is not List<Havok.hk2018.hkbShapeSetup> castValue) return false;
                instance.m_collisionShapeProfiles = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
