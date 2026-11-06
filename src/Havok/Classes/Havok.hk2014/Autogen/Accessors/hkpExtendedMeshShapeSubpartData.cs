// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpExtendedMeshShapeSubpartData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpExtendedMeshShapeSubpart>
{
    public hkpExtendedMeshShapeSubpartData(Havok.hk2014.hkpExtendedMeshShapeSubpart instance) : base(instance) { }

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
            default:
            return false;
        }
    }

}
