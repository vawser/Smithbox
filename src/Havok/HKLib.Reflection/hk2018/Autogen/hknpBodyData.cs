// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hknpLodBodyFlags;
using Enum = HKLib.hk2018.hknpLodBodyFlags.Enum;

namespace HKLib.Reflection.hk2018;

internal class hknpBodyData : HavokData<hknpBody> 
{
    public hknpBodyData(HavokType type, hknpBody instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_transform":
            case "transform":
            {
                if (instance.m_transform is not TGet castValue) return false;
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
            case "m_flags":
            case "flags":
            {
                if (instance.m_flags is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_collisionControl":
            case "collisionControl":
            {
                if (instance.m_collisionControl is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_timAngle":
            case "timAngle":
            {
                if (instance.m_timAngle is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxTimDistance":
            case "maxTimDistance":
            {
                if (instance.m_maxTimDistance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxTimDistanceFromRotation":
            case "maxTimDistanceFromRotation":
            {
                if (instance.m_maxTimDistanceFromRotation is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_aabb":
            case "aabb":
            {
                if (instance.m_aabb is not TGet castValue) return false;
                value = castValue;
                return true;
            }
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
            case "m_lodFlags":
            case "lodFlags":
            {
                if (instance.m_lodFlags is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_lodFlags is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_qualityId":
            case "qualityId":
            {
                if (instance.m_qualityId is not TGet castValue) return false;
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
            case "m_collisionFilterInfo":
            case "collisionFilterInfo":
            {
                if (instance.m_collisionFilterInfo is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_id":
            case "id":
            {
                if (instance.m_id is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nextAttachedBodyId":
            case "nextAttachedBodyId":
            {
                if (instance.m_nextAttachedBodyId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_broadPhaseId":
            case "broadPhaseId":
            {
                if (instance.m_broadPhaseId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxContactDistance":
            case "maxContactDistance":
            {
                if (instance.m_maxContactDistance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxContactDistanceFromRotation":
            case "maxContactDistanceFromRotation":
            {
                if (instance.m_maxContactDistanceFromRotation is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_motionToBodyRotation":
            case "motionToBodyRotation":
            {
                if (instance.m_motionToBodyRotation is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                if (instance.m_userData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_activationPriority":
            case "activationPriority":
            {
                if (instance.m_activationPriority is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_radiusOfComCenteredBoundingSphere":
            case "radiusOfComCenteredBoundingSphere":
            {
                if (instance.m_radiusOfComCenteredBoundingSphere is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_avgSurfaceVelocity":
            case "avgSurfaceVelocity":
            {
                if (instance.m_avgSurfaceVelocity is not TGet castValue) return false;
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
            case "m_transform":
            case "transform":
            {
                if (value is not Matrix4x4 castValue) return false;
                instance.m_transform = castValue;
                return true;
            }
            case "m_motionId":
            case "motionId":
            {
                if (value is not uint castValue) return false;
                instance.m_motionId = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (value is not int castValue) return false;
                instance.m_flags = castValue;
                return true;
            }
            case "m_collisionControl":
            case "collisionControl":
            {
                if (value is not short castValue) return false;
                instance.m_collisionControl = castValue;
                return true;
            }
            case "m_timAngle":
            case "timAngle":
            {
                if (value is not ushort castValue) return false;
                instance.m_timAngle = castValue;
                return true;
            }
            case "m_maxTimDistance":
            case "maxTimDistance":
            {
                if (value is not ushort castValue) return false;
                instance.m_maxTimDistance = castValue;
                return true;
            }
            case "m_maxTimDistanceFromRotation":
            case "maxTimDistanceFromRotation":
            {
                if (value is not ushort castValue) return false;
                instance.m_maxTimDistanceFromRotation = castValue;
                return true;
            }
            case "m_aabb":
            case "aabb":
            {
                if (value is not hkAabb24_16_24 castValue) return false;
                instance.m_aabb = castValue;
                return true;
            }
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
            case "m_lodFlags":
            case "lodFlags":
            {
                if (value is Enum castValue)
                {
                    instance.m_lodFlags = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_lodFlags = (Enum)byteValue;
                    return true;
                }
                return false;
            }
            case "m_qualityId":
            case "qualityId":
            {
                if (value is not byte castValue) return false;
                instance.m_qualityId = castValue;
                return true;
            }
            case "m_materialId":
            case "materialId":
            {
                if (value is not ushort castValue) return false;
                instance.m_materialId = castValue;
                return true;
            }
            case "m_collisionFilterInfo":
            case "collisionFilterInfo":
            {
                if (value is not uint castValue) return false;
                instance.m_collisionFilterInfo = castValue;
                return true;
            }
            case "m_id":
            case "id":
            {
                if (value is not hknpBodyId castValue) return false;
                instance.m_id = castValue;
                return true;
            }
            case "m_nextAttachedBodyId":
            case "nextAttachedBodyId":
            {
                if (value is not hknpBodyId castValue) return false;
                instance.m_nextAttachedBodyId = castValue;
                return true;
            }
            case "m_broadPhaseId":
            case "broadPhaseId":
            {
                if (value is not uint castValue) return false;
                instance.m_broadPhaseId = castValue;
                return true;
            }
            case "m_maxContactDistance":
            case "maxContactDistance":
            {
                if (value is not ushort castValue) return false;
                instance.m_maxContactDistance = castValue;
                return true;
            }
            case "m_maxContactDistanceFromRotation":
            case "maxContactDistanceFromRotation":
            {
                if (value is not ushort castValue) return false;
                instance.m_maxContactDistanceFromRotation = castValue;
                return true;
            }
            case "m_motionToBodyRotation":
            case "motionToBodyRotation":
            {
                if (value is not Quaternion castValue) return false;
                instance.m_motionToBodyRotation = castValue;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_activationPriority":
            case "activationPriority":
            {
                if (value is not sbyte castValue) return false;
                instance.m_activationPriority = castValue;
                return true;
            }
            case "m_radiusOfComCenteredBoundingSphere":
            case "radiusOfComCenteredBoundingSphere":
            {
                if (value is not float castValue) return false;
                instance.m_radiusOfComCenteredBoundingSphere = castValue;
                return true;
            }
            case "m_avgSurfaceVelocity":
            case "avgSurfaceVelocity":
            {
                if (value is not float castValue) return false;
                instance.m_avgSurfaceVelocity = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
