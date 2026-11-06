// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hknpStaticCompoundShapeInternalsData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hknpStaticCompoundShapeInternals>
{
    public hknpStaticCompoundShapeInternalsData(Havok.hk2014.hknpStaticCompoundShapeInternals instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_flags":
            case "flags":
            {
                value = instance.m_flags;
                return true;
            }
            case "m_numShapeKeyBits":
            case "numShapeKeyBits":
            {
                value = instance.m_numShapeKeyBits;
                return true;
            }
            case "m_dispatchType":
            case "dispatchType":
            {
                value = instance.m_dispatchType;
                return true;
            }
            case "m_convexRadius":
            case "convexRadius":
            {
                value = instance.m_convexRadius;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_properties":
            case "properties":
            {
                value = instance.m_properties;
                return true;
            }
            case "m_edgeWeldingMap":
            case "edgeWeldingMap":
            {
                value = instance.m_edgeWeldingMap;
                return true;
            }
            case "m_shapeTagCodecInfo":
            case "shapeTagCodecInfo":
            {
                value = instance.m_shapeTagCodecInfo;
                return true;
            }
            case "m_instances":
            case "instances":
            {
                value = instance.m_instances;
                return true;
            }
            case "m_aabb":
            case "aabb":
            {
                value = instance.m_aabb;
                return true;
            }
            case "m_isMutable":
            case "isMutable":
            {
                value = instance.m_isMutable;
                return true;
            }
            case "m_boundingVolumeData":
            case "boundingVolumeData":
            {
                value = instance.m_boundingVolumeData;
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
            case "m_flags":
            case "flags":
            {
                if (value is Havok.hk2014.hknpShape.FlagsEnum castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_flags = (Havok.hk2014.hknpShape.FlagsEnum)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_numShapeKeyBits":
            case "numShapeKeyBits":
            {
                if (value is not byte castValue) return false;
                instance.m_numShapeKeyBits = castValue;
                return true;
            }
            case "m_dispatchType":
            case "dispatchType":
            {
                if (value is not byte castValue) return false;
                instance.m_dispatchType = castValue;
                return true;
            }
            case "m_convexRadius":
            case "convexRadius":
            {
                if (value is not float castValue) return false;
                instance.m_convexRadius = castValue;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_properties":
            case "properties":
            {
                if (value is null)
                {
                    instance.m_properties = default;
                    return true;
                }
                if (value is Havok.hk2014.hkRefCountedProperties castValue)
                {
                    instance.m_properties = castValue;
                    return true;
                }
                return false;
            }
            case "m_edgeWeldingMap":
            case "edgeWeldingMap":
            {
                if (value is not Havok.hk2014.hknpSparseCompactMapunsignedshort castValue) return false;
                instance.m_edgeWeldingMap = castValue;
                return true;
            }
            case "m_shapeTagCodecInfo":
            case "shapeTagCodecInfo":
            {
                if (value is not uint castValue) return false;
                instance.m_shapeTagCodecInfo = castValue;
                return true;
            }
            case "m_instances":
            case "instances":
            {
                if (value is not Havok.hk2014.hkFreeListArrayhknpShapeInstancehkHandleshort32767hknpShapeInstanceIdDiscriminant8hknpShapeInstance castValue) return false;
                instance.m_instances = castValue;
                return true;
            }
            case "m_aabb":
            case "aabb":
            {
                if (value is not Havok.hk2014.hkAabb castValue) return false;
                instance.m_aabb = castValue;
                return true;
            }
            case "m_isMutable":
            case "isMutable":
            {
                if (value is not bool castValue) return false;
                instance.m_isMutable = castValue;
                return true;
            }
            case "m_boundingVolumeData":
            case "boundingVolumeData":
            {
                if (value is null)
                {
                    instance.m_boundingVolumeData = default;
                    return true;
                }
                if (value is Havok.hk2014.hknpStaticCompoundShapeData castValue)
                {
                    instance.m_boundingVolumeData = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
