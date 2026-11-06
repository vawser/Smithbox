// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class fsnpCustomParamCompressedMeshShapeData : Havok.Formats.HavokFieldAccess<Havok.hk2019.fsnpCustomParamCompressedMeshShape>
{
    public fsnpCustomParamCompressedMeshShapeData(Havok.hk2019.fsnpCustomParamCompressedMeshShape instance) : base(instance) { }

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
            case "m_data":
            case "data":
            {
                value = instance.m_data;
                return true;
            }
            case "m_triangleIsInterior":
            case "triangleIsInterior":
            {
                value = instance.m_triangleIsInterior;
                return true;
            }
            case "m_externShapes":
            case "externShapes":
            {
                value = instance.m_externShapes;
                return true;
            }
            case "m_pParam":
            case "pParam":
            {
                value = instance.m_pParam;
                return true;
            }
            case "m_triangleIndexToShapeKey":
            case "triangleIndexToShapeKey":
            {
                value = instance.m_triangleIndexToShapeKey;
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
                if (value is Havok.hk2019.hknpShape.FlagsEnum castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_flags = (Havok.hk2019.hknpShape.FlagsEnum)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_type":
            case "type":
            {
                if (value is Havok.hk2019.hknpShapeType.Enum castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_type = (Havok.hk2019.hknpShapeType.Enum)byteValue;
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
                if (value is Havok.hk2019.hknpCollisionDispatchType.Enum castValue)
                {
                    instance.m_dispatchType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_dispatchType = (Havok.hk2019.hknpCollisionDispatchType.Enum)byteValue;
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
                if (value is Havok.hk2019.hkRefCountedProperties castValue)
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
            case "m_data":
            case "data":
            {
                if (value is null)
                {
                    instance.m_data = default;
                    return true;
                }
                if (value is Havok.hk2019.hkReferencedObject castValue)
                {
                    instance.m_data = castValue;
                    return true;
                }
                return false;
            }
            case "m_triangleIsInterior":
            case "triangleIsInterior":
            {
                if (value is not Havok.hk2019.hkBitField castValue) return false;
                instance.m_triangleIsInterior = castValue;
                return true;
            }
            case "m_externShapes":
            case "externShapes":
            {
                if (value is not List<Havok.hk2019.hknpShapeInstance> castValue) return false;
                instance.m_externShapes = castValue;
                return true;
            }
            case "m_pParam":
            case "pParam":
            {
                if (value is null)
                {
                    instance.m_pParam = default;
                    return true;
                }
                if (value is Havok.hk2019.fsnpCustomMeshParameter castValue)
                {
                    instance.m_pParam = castValue;
                    return true;
                }
                return false;
            }
            case "m_triangleIndexToShapeKey":
            case "triangleIndexToShapeKey":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_triangleIndexToShapeKey = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
