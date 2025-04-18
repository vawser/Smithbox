// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbRigidBodySetupData : HavokData<hkbRigidBodySetup> 
{
    public hkbRigidBodySetupData(HavokType type, hkbRigidBodySetup instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_collisionFilterInfo":
            case "collisionFilterInfo":
            {
                if (instance.m_collisionFilterInfo is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_type":
            case "type":
            {
                if (instance.m_type is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_type is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_collisionShapeProfiles":
            case "collisionShapeProfiles":
            {
                if (instance.m_collisionShapeProfiles is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
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
                if (value is hkbRigidBodySetup.Type castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_type = (hkbRigidBodySetup.Type)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_collisionShapeProfiles":
            case "collisionShapeProfiles":
            {
                if (value is not List<hkbShapeSetup> castValue) return false;
                instance.m_collisionShapeProfiles = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
