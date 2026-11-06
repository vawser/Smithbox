// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpHeightFieldShapeData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpHeightFieldShape>
{
    public hknpHeightFieldShapeData(Havok.hk2018.hknpHeightFieldShape instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                value = instance.m_flags;
                return true;
            }
            case "m_type":
            case "type":
            {
                value = instance.m_type;
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
            case "m_shapeTagCodecInfo":
            case "shapeTagCodecInfo":
            {
                value = instance.m_shapeTagCodecInfo;
                return true;
            }
            case "m_materialTable":
            case "materialTable":
            {
                value = instance.m_materialTable;
                return true;
            }
            case "m_geometry":
            case "geometry":
            {
                value = instance.m_geometry;
                return true;
            }
            case "m_boundingVolumeData":
            case "boundingVolumeData":
            {
                value = instance.m_boundingVolumeData;
                return true;
            }
            case "m_intSizeX":
            case "intSizeX":
            {
                value = instance.m_intSizeX;
                return true;
            }
            case "m_intSizeZ":
            case "intSizeZ":
            {
                value = instance.m_intSizeZ;
                return true;
            }
            case "m_numBitsX":
            case "numBitsX":
            {
                value = instance.m_numBitsX;
                return true;
            }
            case "m_numBitsZ":
            case "numBitsZ":
            {
                value = instance.m_numBitsZ;
                return true;
            }
            case "m_includeShapeKeyInSdfContacts":
            case "includeShapeKeyInSdfContacts":
            {
                value = instance.m_includeShapeKeyInSdfContacts;
                return true;
            }
            case "m_floatToIntScale":
            case "floatToIntScale":
            {
                value = instance.m_floatToIntScale;
                return true;
            }
            case "m_intToFloatScale":
            case "intToFloatScale":
            {
                value = instance.m_intToFloatScale;
                return true;
            }
            case "m_aabb":
            case "aabb":
            {
                value = instance.m_aabb;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (value is Havok.hk2018.hknpShape.FlagsEnum castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_flags = (Havok.hk2018.hknpShape.FlagsEnum)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_type":
            case "type":
            {
                if (value is Havok.hk2018.hknpShapeType.Enum castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_type = (Havok.hk2018.hknpShapeType.Enum)byteValue;
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
                if (value is Havok.hk2018.hknpCollisionDispatchType.Enum castValue)
                {
                    instance.m_dispatchType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_dispatchType = (Havok.hk2018.hknpCollisionDispatchType.Enum)byteValue;
                    return true;
                }
                return false;
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
                if (value is Havok.hk2018.hkRefCountedProperties castValue)
                {
                    instance.m_properties = castValue;
                    return true;
                }
                return false;
            }
            case "m_shapeTagCodecInfo":
            case "shapeTagCodecInfo":
            {
                if (value is not uint castValue) return false;
                instance.m_shapeTagCodecInfo = castValue;
                return true;
            }
            case "m_materialTable":
            case "materialTable":
            {
                if (value is null)
                {
                    instance.m_materialTable = default;
                    return true;
                }
                if (value is Havok.hk2018.hkReferencedObject castValue)
                {
                    instance.m_materialTable = castValue;
                    return true;
                }
                return false;
            }
            case "m_geometry":
            case "geometry":
            {
                if (value is null)
                {
                    instance.m_geometry = default;
                    return true;
                }
                if (value is Havok.hk2018.hknpHeightFieldGeometry castValue)
                {
                    instance.m_geometry = castValue;
                    return true;
                }
                return false;
            }
            case "m_boundingVolumeData":
            case "boundingVolumeData":
            {
                if (value is null)
                {
                    instance.m_boundingVolumeData = default;
                    return true;
                }
                if (value is Havok.hk2018.hknpHeightFieldBoundingVolume castValue)
                {
                    instance.m_boundingVolumeData = castValue;
                    return true;
                }
                return false;
            }
            case "m_intSizeX":
            case "intSizeX":
            {
                if (value is not int castValue) return false;
                instance.m_intSizeX = castValue;
                return true;
            }
            case "m_intSizeZ":
            case "intSizeZ":
            {
                if (value is not int castValue) return false;
                instance.m_intSizeZ = castValue;
                return true;
            }
            case "m_numBitsX":
            case "numBitsX":
            {
                if (value is not int castValue) return false;
                instance.m_numBitsX = castValue;
                return true;
            }
            case "m_numBitsZ":
            case "numBitsZ":
            {
                if (value is not int castValue) return false;
                instance.m_numBitsZ = castValue;
                return true;
            }
            case "m_includeShapeKeyInSdfContacts":
            case "includeShapeKeyInSdfContacts":
            {
                if (value is not bool castValue) return false;
                instance.m_includeShapeKeyInSdfContacts = castValue;
                return true;
            }
            case "m_floatToIntScale":
            case "floatToIntScale":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_floatToIntScale = castValue;
                return true;
            }
            case "m_intToFloatScale":
            case "intToFloatScale":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_intToFloatScale = castValue;
                return true;
            }
            case "m_aabb":
            case "aabb":
            {
                if (value is not Havok.hk2018.hkAabb castValue) return false;
                instance.m_aabb = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
