// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hknpBodyData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hknpBody>
{
    private static readonly System.Reflection.FieldInfo _motionToBodyRotationInfo = typeof(Havok.hk2014.hknpBody).GetField("m_motionToBodyRotation")!;
    public hknpBodyData(Havok.hk2014.hknpBody instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_transform":
            case "transform":
            {
                value = instance.m_transform;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                value = instance.m_flags;
                return true;
            }
            case "m_collisionFilterInfo":
            case "collisionFilterInfo":
            {
                value = instance.m_collisionFilterInfo;
                return true;
            }
            case "m_shape":
            case "shape":
            {
                value = instance.m_shape;
                return true;
            }
            case "m_aabb":
            case "aabb":
            {
                value = instance.m_aabb;
                return true;
            }
            case "m_id":
            case "id":
            {
                value = instance.m_id;
                return true;
            }
            case "m_nextAttachedBodyId":
            case "nextAttachedBodyId":
            {
                value = instance.m_nextAttachedBodyId;
                return true;
            }
            case "m_motionId":
            case "motionId":
            {
                value = instance.m_motionId;
                return true;
            }
            case "m_broadPhaseId":
            case "broadPhaseId":
            {
                value = instance.m_broadPhaseId;
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
            case "m_timAngle":
            case "timAngle":
            {
                value = instance.m_timAngle;
                return true;
            }
            case "m_maxTimDistance":
            case "maxTimDistance":
            {
                value = instance.m_maxTimDistance;
                return true;
            }
            case "m_maxContactDistance":
            case "maxContactDistance":
            {
                value = instance.m_maxContactDistance;
                return true;
            }
            case "m_indexIntoActiveListOrDeactivatedIslandId":
            case "indexIntoActiveListOrDeactivatedIslandId":
            {
                value = instance.m_indexIntoActiveListOrDeactivatedIslandId;
                return true;
            }
            case "m_radiusOfComCenteredBoundingSphere":
            case "radiusOfComCenteredBoundingSphere":
            {
                value = instance.m_radiusOfComCenteredBoundingSphere;
                return true;
            }
            case "m_spuFlags":
            case "spuFlags":
            {
                value = instance.m_spuFlags;
                return true;
            }
            case "m_shapeSizeDiv16":
            case "shapeSizeDiv16":
            {
                value = instance.m_shapeSizeDiv16;
                return true;
            }
            case "m_motionToBodyRotation":
            case "motionToBodyRotation":
            {
                value = instance.m_motionToBodyRotation;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
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
            case "m_transform":
            case "transform":
            {
                if (value is not Matrix4x4 castValue) return false;
                instance.m_transform = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (value is not int castValue) return false;
                instance.m_flags = castValue;
                return true;
            }
            case "m_collisionFilterInfo":
            case "collisionFilterInfo":
            {
                if (value is not uint castValue) return false;
                instance.m_collisionFilterInfo = castValue;
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
                if (value is Havok.hk2014.hknpShape castValue)
                {
                    instance.m_shape = castValue;
                    return true;
                }
                return false;
            }
            case "m_aabb":
            case "aabb":
            {
                if (value is not Havok.hk2014.hkAabb16 castValue) return false;
                instance.m_aabb = castValue;
                return true;
            }
            case "m_id":
            case "id":
            {
                if (value is not uint castValue) return false;
                instance.m_id = castValue;
                return true;
            }
            case "m_nextAttachedBodyId":
            case "nextAttachedBodyId":
            {
                if (value is not uint castValue) return false;
                instance.m_nextAttachedBodyId = castValue;
                return true;
            }
            case "m_motionId":
            case "motionId":
            {
                if (value is not uint castValue) return false;
                instance.m_motionId = castValue;
                return true;
            }
            case "m_broadPhaseId":
            case "broadPhaseId":
            {
                if (value is not uint castValue) return false;
                instance.m_broadPhaseId = castValue;
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
            case "m_timAngle":
            case "timAngle":
            {
                if (value is not byte castValue) return false;
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
            case "m_maxContactDistance":
            case "maxContactDistance":
            {
                if (value is not ushort castValue) return false;
                instance.m_maxContactDistance = castValue;
                return true;
            }
            case "m_indexIntoActiveListOrDeactivatedIslandId":
            case "indexIntoActiveListOrDeactivatedIslandId":
            {
                if (value is not uint castValue) return false;
                instance.m_indexIntoActiveListOrDeactivatedIslandId = castValue;
                return true;
            }
            case "m_radiusOfComCenteredBoundingSphere":
            case "radiusOfComCenteredBoundingSphere":
            {
                if (value is not float castValue) return false;
                instance.m_radiusOfComCenteredBoundingSphere = castValue;
                return true;
            }
            case "m_spuFlags":
            case "spuFlags":
            {
                if (value is Havok.hk2014.hknpBody.SpuFlagsEnum castValue)
                {
                    instance.m_spuFlags = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_spuFlags = (Havok.hk2014.hknpBody.SpuFlagsEnum)byteValue;
                    return true;
                }
                return false;
            }
            case "m_shapeSizeDiv16":
            case "shapeSizeDiv16":
            {
                if (value is not byte castValue) return false;
                instance.m_shapeSizeDiv16 = castValue;
                return true;
            }
            case "m_motionToBodyRotation":
            case "motionToBodyRotation":
            {
                if (value is not short[] castValue || castValue.Length != 4) return false;
                try
                {
                    _motionToBodyRotationInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
