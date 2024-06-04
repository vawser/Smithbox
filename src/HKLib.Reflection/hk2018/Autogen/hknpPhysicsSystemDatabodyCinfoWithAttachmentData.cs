// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpPhysicsSystemDatabodyCinfoWithAttachmentData : HavokData<HKLib.hk2018.hknpPhysicsSystemData.bodyCinfoWithAttachment> 
{
    public hknpPhysicsSystemDatabodyCinfoWithAttachmentData(HavokType type, HKLib.hk2018.hknpPhysicsSystemData.bodyCinfoWithAttachment instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_shape":
            case "shape":
            {
                if (instance.m_shape is null)
                {
                    return true;
                }
                if (instance.m_shape is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_flags":
            case "flags":
            {
                if (instance.m_flags is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_collisionCntrl":
            case "collisionCntrl":
            {
                if (instance.m_collisionCntrl is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_collisionFilterInfo":
            case "collisionFilterInfo":
            {
                if (instance.m_collisionFilterInfo is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_materialId":
            case "materialId":
            {
                if (instance.m_materialId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_qualityId":
            case "qualityId":
            {
                if (instance.m_qualityId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (instance.m_name is null)
                {
                    return true;
                }
                if (instance.m_name is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_userData":
            case "userData":
            {
                if (instance.m_userData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_motionType":
            case "motionType":
            {
                if (instance.m_motionType is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_position":
            case "position":
            {
                if (instance.m_position is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_orientation":
            case "orientation":
            {
                if (instance.m_orientation is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_linearVelocity":
            case "linearVelocity":
            {
                if (instance.m_linearVelocity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_angularVelocity":
            case "angularVelocity":
            {
                if (instance.m_angularVelocity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_mass":
            case "mass":
            {
                if (instance.m_mass is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_massDistribution":
            case "massDistribution":
            {
                if (instance.m_massDistribution is null)
                {
                    return true;
                }
                if (instance.m_massDistribution is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_dragProperties":
            case "dragProperties":
            {
                if (instance.m_dragProperties is null)
                {
                    return true;
                }
                if (instance.m_dragProperties is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_motionPropertiesId":
            case "motionPropertiesId":
            {
                if (instance.m_motionPropertiesId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_desiredBodyId":
            case "desiredBodyId":
            {
                if (instance.m_desiredBodyId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_motionId":
            case "motionId":
            {
                if (instance.m_motionId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_collisionLookAheadDistance":
            case "collisionLookAheadDistance":
            {
                if (instance.m_collisionLookAheadDistance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_localFrame":
            case "localFrame":
            {
                if (instance.m_localFrame is null)
                {
                    return true;
                }
                if (instance.m_localFrame is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_activationPriority":
            case "activationPriority":
            {
                if (instance.m_activationPriority is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_attachedBody":
            case "attachedBody":
            {
                if (instance.m_attachedBody is not TGet castValue) return false;
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
            case "m_shape":
            case "shape":
            {
                if (value is null)
                {
                    instance.m_shape = default;
                    return true;
                }
                if (value is hknpShape castValue)
                {
                    instance.m_shape = castValue;
                    return true;
                }
                return false;
            }
            case "m_flags":
            case "flags":
            {
                if (value is not int castValue) return false;
                instance.m_flags = castValue;
                return true;
            }
            case "m_collisionCntrl":
            case "collisionCntrl":
            {
                if (value is not short castValue) return false;
                instance.m_collisionCntrl = castValue;
                return true;
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
                if (value is hknpRefMassDistribution castValue)
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
                if (value is hknpRefDragProperties castValue)
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
                if (value is not hknpBodyId castValue) return false;
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
                if (value is hkLocalFrame castValue)
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
            case "m_attachedBody":
            case "attachedBody":
            {
                if (value is not int castValue) return false;
                instance.m_attachedBody = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
