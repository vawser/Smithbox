// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbRigidBodySetupData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbRigidBodySetup>
{
    public hkbRigidBodySetupData(Havok.hk2014.hkbRigidBodySetup instance) : base(instance) { }

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
            case "m_shapeSetup":
            case "shapeSetup":
            {
                value = instance.m_shapeSetup;
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
                if (value is Havok.hk2014.hkbRigidBodySetup.Type castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_type = (Havok.hk2014.hkbRigidBodySetup.Type)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_shapeSetup":
            case "shapeSetup":
            {
                if (value is not Havok.hk2014.hkbShapeSetup castValue) return false;
                instance.m_shapeSetup = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
