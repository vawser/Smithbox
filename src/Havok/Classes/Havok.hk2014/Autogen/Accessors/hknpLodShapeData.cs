// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hknpLodShapeData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hknpLodShape>
{
    private static readonly System.Reflection.FieldInfo _infosInfo = typeof(Havok.hk2014.hknpLodShape).GetField("m_infos")!;
    private static readonly System.Reflection.FieldInfo _shapesInfo = typeof(Havok.hk2014.hknpLodShape).GetField("m_shapes")!;
    private static readonly System.Reflection.FieldInfo _shapesMemorySizesInfo = typeof(Havok.hk2014.hknpLodShape).GetField("m_shapesMemorySizes")!;
    public hknpLodShapeData(Havok.hk2014.hknpLodShape instance) : base(instance) { }

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
            case "m_numLevelsOfDetail":
            case "numLevelsOfDetail":
            {
                value = instance.m_numLevelsOfDetail;
                return true;
            }
            case "m_infos":
            case "infos":
            {
                value = instance.m_infos;
                return true;
            }
            case "m_shapes":
            case "shapes":
            {
                value = instance.m_shapes;
                return true;
            }
            case "m_shapesMemorySizes":
            case "shapesMemorySizes":
            {
                value = instance.m_shapesMemorySizes;
                return true;
            }
            case "m_indexCurrentShapeOnSpu":
            case "indexCurrentShapeOnSpu":
            {
                value = instance.m_indexCurrentShapeOnSpu;
                return true;
            }
            case "m_currentShapePpuAddress":
            case "currentShapePpuAddress":
            {
                value = instance.m_currentShapePpuAddress;
                return true;
            }
            case "m_maximumAabb":
            case "maximumAabb":
            {
                value = instance.m_maximumAabb;
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
            case "m_numLevelsOfDetail":
            case "numLevelsOfDetail":
            {
                if (value is not int castValue) return false;
                instance.m_numLevelsOfDetail = castValue;
                return true;
            }
            case "m_infos":
            case "infos":
            {
                if (value is not Havok.hk2014.hknpLodShapeLevelOfDetailInfo[] castValue || castValue.Length != 8) return false;
                try
                {
                    _infosInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_shapes":
            case "shapes":
            {
                if (value is not Havok.hk2014.hknpShape?[] castValue || castValue.Length != 8) return false;
                try
                {
                    _shapesInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_shapesMemorySizes":
            case "shapesMemorySizes":
            {
                if (value is not uint[] castValue || castValue.Length != 8) return false;
                try
                {
                    _shapesMemorySizesInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_indexCurrentShapeOnSpu":
            case "indexCurrentShapeOnSpu":
            {
                if (value is not int castValue) return false;
                instance.m_indexCurrentShapeOnSpu = castValue;
                return true;
            }
            case "m_currentShapePpuAddress":
            case "currentShapePpuAddress":
            {
                if (value is null)
                {
                    instance.m_currentShapePpuAddress = default;
                    return true;
                }
                if (value is Havok.hk2014.hknpShape castValue)
                {
                    instance.m_currentShapePpuAddress = castValue;
                    return true;
                }
                return false;
            }
            case "m_maximumAabb":
            case "maximumAabb":
            {
                if (value is not Havok.hk2014.hkAabb castValue) return false;
                instance.m_maximumAabb = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
