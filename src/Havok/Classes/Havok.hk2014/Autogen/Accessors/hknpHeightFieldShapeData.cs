// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hknpHeightFieldShapeData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hknpHeightFieldShape>
{
    public hknpHeightFieldShapeData(Havok.hk2014.hknpHeightFieldShape instance) : base(instance) { }

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
            case "m_aabb":
            case "aabb":
            {
                value = instance.m_aabb;
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
            case "m_minMaxTree":
            case "minMaxTree":
            {
                value = instance.m_minMaxTree;
                return true;
            }
            case "m_minMaxTreeCoarseness":
            case "minMaxTreeCoarseness":
            {
                value = instance.m_minMaxTreeCoarseness;
                return true;
            }
            case "m_includeShapeKeyInSdfContacts":
            case "includeShapeKeyInSdfContacts":
            {
                value = instance.m_includeShapeKeyInSdfContacts;
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
            case "m_aabb":
            case "aabb":
            {
                if (value is not Havok.hk2014.hkAabb castValue) return false;
                instance.m_aabb = castValue;
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
            case "m_minMaxTree":
            case "minMaxTree":
            {
                if (value is not Havok.hk2014.hknpMinMaxQuadTree castValue) return false;
                instance.m_minMaxTree = castValue;
                return true;
            }
            case "m_minMaxTreeCoarseness":
            case "minMaxTreeCoarseness":
            {
                if (value is not int castValue) return false;
                instance.m_minMaxTreeCoarseness = castValue;
                return true;
            }
            case "m_includeShapeKeyInSdfContacts":
            case "includeShapeKeyInSdfContacts":
            {
                if (value is not bool castValue) return false;
                instance.m_includeShapeKeyInSdfContacts = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
