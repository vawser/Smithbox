// Automatically Generated

#nullable enable

namespace Havok.hk2015.Accessors;

internal sealed class hkpExtendedMeshShapeShapesSubpartData : Havok.Formats.HavokFieldAccess<Havok.hk2015.hkpExtendedMeshShape.ShapesSubpart>
{
    public hkpExtendedMeshShapeShapesSubpartData(Havok.hk2015.hkpExtendedMeshShape.ShapesSubpart instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_typeAndFlags":
            case "typeAndFlags":
            {
                value = instance.m_typeAndFlags;
                return true;
            }
            case "m_shapeInfo":
            case "shapeInfo":
            {
                value = instance.m_shapeInfo;
                return true;
            }
            case "m_materialIndexStriding":
            case "materialIndexStriding":
            {
                value = instance.m_materialIndexStriding;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_childShapes":
            case "childShapes":
            {
                value = instance.m_childShapes;
                return true;
            }
            case "m_rotation":
            case "rotation":
            {
                value = instance.m_rotation;
                return true;
            }
            case "m_translation":
            case "translation":
            {
                value = instance.m_translation;
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
            case "m_typeAndFlags":
            case "typeAndFlags":
            {
                if (value is not ushort castValue) return false;
                instance.m_typeAndFlags = castValue;
                return true;
            }
            case "m_shapeInfo":
            case "shapeInfo":
            {
                if (value is not ushort castValue) return false;
                instance.m_shapeInfo = castValue;
                return true;
            }
            case "m_materialIndexStriding":
            case "materialIndexStriding":
            {
                if (value is not ushort castValue) return false;
                instance.m_materialIndexStriding = castValue;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_childShapes":
            case "childShapes":
            {
                if (value is not List<Havok.hk2015.hkpConvexShape?> castValue) return false;
                instance.m_childShapes = castValue;
                return true;
            }
            case "m_rotation":
            case "rotation":
            {
                if (value is not Quaternion castValue) return false;
                instance.m_rotation = castValue;
                return true;
            }
            case "m_translation":
            case "translation":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_translation = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
