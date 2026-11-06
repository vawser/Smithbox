// Automatically Generated

#nullable enable

namespace Havok.hk55.Accessors;

internal sealed class hkpLinkedCollidableData : Havok.Formats.HavokFieldAccess<Havok.hk55.hkpLinkedCollidable>
{
    public hkpLinkedCollidableData(Havok.hk55.hkpLinkedCollidable instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_shape":
            case "shape":
            {
                value = instance.m_shape;
                return true;
            }
            case "m_shapeKey":
            case "shapeKey":
            {
                value = instance.m_shapeKey;
                return true;
            }
            case "m_parent":
            case "parent":
            {
                value = instance.m_parent;
                return true;
            }
            case "m_forceCollideOntoPpu":
            case "forceCollideOntoPpu":
            {
                value = instance.m_forceCollideOntoPpu;
                return true;
            }
            case "m_broadPhaseHandle":
            case "broadPhaseHandle":
            {
                value = instance.m_broadPhaseHandle;
                return true;
            }
            case "m_boundingVolumeData":
            case "boundingVolumeData":
            {
                value = instance.m_boundingVolumeData;
                return true;
            }
            case "m_allowedPenetrationDepth":
            case "allowedPenetrationDepth":
            {
                value = instance.m_allowedPenetrationDepth;
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
            case "m_shape":
            case "shape":
            {
                if (value is null)
                {
                    instance.m_shape = default;
                    return true;
                }
                if (value is Havok.hk55.hkpShape castValue)
                {
                    instance.m_shape = castValue;
                    return true;
                }
                return false;
            }
            case "m_shapeKey":
            case "shapeKey":
            {
                if (value is not uint castValue) return false;
                instance.m_shapeKey = castValue;
                return true;
            }
            case "m_parent":
            case "parent":
            {
                if (value is null)
                {
                    instance.m_parent = default;
                    return true;
                }
                if (value is Havok.hk55.hkpCdBody castValue)
                {
                    instance.m_parent = castValue;
                    return true;
                }
                return false;
            }
            case "m_forceCollideOntoPpu":
            case "forceCollideOntoPpu":
            {
                if (value is not byte castValue) return false;
                instance.m_forceCollideOntoPpu = castValue;
                return true;
            }
            case "m_broadPhaseHandle":
            case "broadPhaseHandle":
            {
                if (value is not Havok.hk55.hkpTypedBroadPhaseHandle castValue) return false;
                instance.m_broadPhaseHandle = castValue;
                return true;
            }
            case "m_boundingVolumeData":
            case "boundingVolumeData":
            {
                if (value is not Havok.hk55.hkpCollidableBoundingVolumeData castValue) return false;
                instance.m_boundingVolumeData = castValue;
                return true;
            }
            case "m_allowedPenetrationDepth":
            case "allowedPenetrationDepth":
            {
                if (value is not float castValue) return false;
                instance.m_allowedPenetrationDepth = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
