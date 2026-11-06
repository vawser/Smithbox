// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpBodyCinfoData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpBodyCinfo>
{
    public hknpBodyCinfoData(Havok.hk2019.hknpBodyCinfo instance) : base(instance) { }

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
            case "m_flags":
            case "flags":
            {
                value = instance.m_flags;
                return true;
            }
            case "m_collisionCntrl":
            case "collisionCntrl":
            {
                value = instance.m_collisionCntrl;
                return true;
            }
            case "m_collisionFilterInfo":
            case "collisionFilterInfo":
            {
                value = instance.m_collisionFilterInfo;
                return true;
            }
            case "m_materialId":
            case "materialId":
            {
                value = instance.m_materialId;
                return true;
            }
            case "m_qualityId":
            case "qualityId":
            {
                value = instance.m_qualityId;
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
            case "m_motionType":
            case "motionType":
            {
                value = instance.m_motionType;
                return true;
            }
            case "m_position":
            case "position":
            {
                value = instance.m_position;
                return true;
            }
            case "m_orientation":
            case "orientation":
            {
                value = instance.m_orientation;
                return true;
            }
            case "m_linearVelocity":
            case "linearVelocity":
            {
                value = instance.m_linearVelocity;
                return true;
            }
            case "m_angularVelocity":
            case "angularVelocity":
            {
                value = instance.m_angularVelocity;
                return true;
            }
            case "m_mass":
            case "mass":
            {
                value = instance.m_mass;
                return true;
            }
            case "m_massDistribution":
            case "massDistribution":
            {
                value = instance.m_massDistribution;
                return true;
            }
            case "m_dragProperties":
            case "dragProperties":
            {
                value = instance.m_dragProperties;
                return true;
            }
            case "m_motionPropertiesId":
            case "motionPropertiesId":
            {
                value = instance.m_motionPropertiesId;
                return true;
            }
            case "m_desiredBodyId":
            case "desiredBodyId":
            {
                value = instance.m_desiredBodyId;
                return true;
            }
            case "m_motionId":
            case "motionId":
            {
                value = instance.m_motionId;
                return true;
            }
            case "m_collisionLookAheadDistance":
            case "collisionLookAheadDistance":
            {
                value = instance.m_collisionLookAheadDistance;
                return true;
            }
            case "m_localFrame":
            case "localFrame":
            {
                value = instance.m_localFrame;
                return true;
            }
            case "m_activationPriority":
            case "activationPriority":
            {
                value = instance.m_activationPriority;
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
                if (value is Havok.hk2019.hknpShape castValue)
                {
                    instance.m_shape = castValue;
                    return true;
                }
                return false;
            }
            case "m_flags":
            case "flags":
            {
                if (value is Havok.hk2019.hknpCollisionFlags.Enum castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_flags = (Havok.hk2019.hknpCollisionFlags.Enum)intValue;
                    return true;
                }
                return false;
            }
            case "m_collisionCntrl":
            case "collisionCntrl":
            {
                if (value is Havok.hk2019.hknpCollisionControl.Enum castValue)
                {
                    instance.m_collisionCntrl = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_collisionCntrl = (Havok.hk2019.hknpCollisionControl.Enum)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_collisionFilterInfo":
            case "collisionFilterInfo":
            {
                if (value is not uint castValue) return false;
                instance.m_collisionFilterInfo = castValue;
                return true;
            }
            case "m_materialId":
            case "materialId":
            {
                if (value is not ushort castValue) return false;
                instance.m_materialId = castValue;
                return true;
            }
            case "m_qualityId":
            case "qualityId":
            {
                if (value is not byte castValue) return false;
                instance.m_qualityId = castValue;
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
            case "m_motionType":
            case "motionType":
            {
                if (value is not byte castValue) return false;
                instance.m_motionType = castValue;
                return true;
            }
            case "m_position":
            case "position":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_position = castValue;
                return true;
            }
            case "m_orientation":
            case "orientation":
            {
                if (value is not Quaternion castValue) return false;
                instance.m_orientation = castValue;
                return true;
            }
            case "m_linearVelocity":
            case "linearVelocity":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_linearVelocity = castValue;
                return true;
            }
            case "m_angularVelocity":
            case "angularVelocity":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_angularVelocity = castValue;
                return true;
            }
            case "m_mass":
            case "mass":
            {
                if (value is not float castValue) return false;
                instance.m_mass = castValue;
                return true;
            }
            case "m_massDistribution":
            case "massDistribution":
            {
                if (value is null)
                {
                    instance.m_massDistribution = default;
                    return true;
                }
                if (value is Havok.hk2019.hknpRefMassDistribution castValue)
                {
                    instance.m_massDistribution = castValue;
                    return true;
                }
                return false;
            }
            case "m_dragProperties":
            case "dragProperties":
            {
                if (value is null)
                {
                    instance.m_dragProperties = default;
                    return true;
                }
                if (value is Havok.hk2019.hknpRefDragProperties castValue)
                {
                    instance.m_dragProperties = castValue;
                    return true;
                }
                return false;
            }
            case "m_motionPropertiesId":
            case "motionPropertiesId":
            {
                if (value is not ushort castValue) return false;
                instance.m_motionPropertiesId = castValue;
                return true;
            }
            case "m_desiredBodyId":
            case "desiredBodyId":
            {
                if (value is not Havok.hk2019.hknpBodyId castValue) return false;
                instance.m_desiredBodyId = castValue;
                return true;
            }
            case "m_motionId":
            case "motionId":
            {
                if (value is not uint castValue) return false;
                instance.m_motionId = castValue;
                return true;
            }
            case "m_collisionLookAheadDistance":
            case "collisionLookAheadDistance":
            {
                if (value is not float castValue) return false;
                instance.m_collisionLookAheadDistance = castValue;
                return true;
            }
            case "m_localFrame":
            case "localFrame":
            {
                if (value is null)
                {
                    instance.m_localFrame = default;
                    return true;
                }
                if (value is Havok.hk2019.hkLocalFrame castValue)
                {
                    instance.m_localFrame = castValue;
                    return true;
                }
                return false;
            }
            case "m_activationPriority":
            case "activationPriority":
            {
                if (value is not sbyte castValue) return false;
                instance.m_activationPriority = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
