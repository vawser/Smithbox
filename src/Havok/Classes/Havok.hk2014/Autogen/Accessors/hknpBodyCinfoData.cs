// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hknpBodyCinfoData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hknpBodyCinfo>
{
    public hknpBodyCinfoData(Havok.hk2014.hknpBodyCinfo instance) : base(instance) { }

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
            case "m_reservedBodyId":
            case "reservedBodyId":
            {
                value = instance.m_reservedBodyId;
                return true;
            }
            case "m_motionId":
            case "motionId":
            {
                value = instance.m_motionId;
                return true;
            }
            case "m_qualityId":
            case "qualityId":
            {
                value = instance.m_qualityId;
                return true;
            }
            case "m_materialId":
            case "materialId":
            {
                value = instance.m_materialId;
                return true;
            }
            case "m_collisionFilterInfo":
            case "collisionFilterInfo":
            {
                value = instance.m_collisionFilterInfo;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                value = instance.m_flags;
                return true;
            }
            case "m_collisionLookAheadDistance":
            case "collisionLookAheadDistance":
            {
                value = instance.m_collisionLookAheadDistance;
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
            case "m_spuFlags":
            case "spuFlags":
            {
                value = instance.m_spuFlags;
                return true;
            }
            case "m_localFrame":
            case "localFrame":
            {
                value = instance.m_localFrame;
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
                if (value is Havok.hk2014.hknpShape castValue)
                {
                    instance.m_shape = castValue;
                    return true;
                }
                return false;
            }
            case "m_reservedBodyId":
            case "reservedBodyId":
            {
                if (value is not uint castValue) return false;
                instance.m_reservedBodyId = castValue;
                return true;
            }
            case "m_motionId":
            case "motionId":
            {
                if (value is not uint castValue) return false;
                instance.m_motionId = castValue;
                return true;
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
            case "m_flags":
            case "flags":
            {
                if (value is not int castValue) return false;
                instance.m_flags = castValue;
                return true;
            }
            case "m_collisionLookAheadDistance":
            case "collisionLookAheadDistance":
            {
                if (value is not float castValue) return false;
                instance.m_collisionLookAheadDistance = castValue;
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
            case "m_localFrame":
            case "localFrame":
            {
                if (value is null)
                {
                    instance.m_localFrame = default;
                    return true;
                }
                if (value is Havok.hk2014.hkLocalFrame castValue)
                {
                    instance.m_localFrame = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
