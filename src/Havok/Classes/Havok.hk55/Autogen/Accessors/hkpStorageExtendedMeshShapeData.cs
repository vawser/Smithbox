// Automatically Generated

#nullable enable

namespace Havok.hk55.Accessors;

internal sealed class hkpStorageExtendedMeshShapeData : Havok.Formats.HavokFieldAccess<Havok.hk55.hkpStorageExtendedMeshShape>
{
    public hkpStorageExtendedMeshShapeData(Havok.hk55.hkpStorageExtendedMeshShape instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_disableWelding":
            case "disableWelding":
            {
                value = instance.m_disableWelding;
                return true;
            }
            case "m_embeddedTrianglesSubpart":
            case "embeddedTrianglesSubpart":
            {
                value = instance.m_embeddedTrianglesSubpart;
                return true;
            }
            case "m_aabbHalfExtents":
            case "aabbHalfExtents":
            {
                value = instance.m_aabbHalfExtents;
                return true;
            }
            case "m_aabbCenter":
            case "aabbCenter":
            {
                value = instance.m_aabbCenter;
                return true;
            }
            case "m_numBitsForSubpartIndex":
            case "numBitsForSubpartIndex":
            {
                value = instance.m_numBitsForSubpartIndex;
                return true;
            }
            case "m_trianglesSubparts":
            case "trianglesSubparts":
            {
                value = instance.m_trianglesSubparts;
                return true;
            }
            case "m_shapesSubparts":
            case "shapesSubparts":
            {
                value = instance.m_shapesSubparts;
                return true;
            }
            case "m_weldingInfo":
            case "weldingInfo":
            {
                value = instance.m_weldingInfo;
                return true;
            }
            case "m_weldingType":
            case "weldingType":
            {
                value = instance.m_weldingType;
                return true;
            }
            case "m_defaultCollisionFilterInfo":
            case "defaultCollisionFilterInfo":
            {
                value = instance.m_defaultCollisionFilterInfo;
                return true;
            }
            case "m_cachedNumChildShapes":
            case "cachedNumChildShapes":
            {
                value = instance.m_cachedNumChildShapes;
                return true;
            }
            case "m_triangleRadius":
            case "triangleRadius":
            {
                value = instance.m_triangleRadius;
                return true;
            }
            case "m_meshstorage":
            case "meshstorage":
            {
                value = instance.m_meshstorage;
                return true;
            }
            case "m_shapestorage":
            case "shapestorage":
            {
                value = instance.m_shapestorage;
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
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_disableWelding":
            case "disableWelding":
            {
                if (value is not bool castValue) return false;
                instance.m_disableWelding = castValue;
                return true;
            }
            case "m_embeddedTrianglesSubpart":
            case "embeddedTrianglesSubpart":
            {
                if (value is not Havok.hk55.hkpExtendedMeshShapeTrianglesSubpart castValue) return false;
                instance.m_embeddedTrianglesSubpart = castValue;
                return true;
            }
            case "m_aabbHalfExtents":
            case "aabbHalfExtents":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_aabbHalfExtents = castValue;
                return true;
            }
            case "m_aabbCenter":
            case "aabbCenter":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_aabbCenter = castValue;
                return true;
            }
            case "m_numBitsForSubpartIndex":
            case "numBitsForSubpartIndex":
            {
                if (value is not int castValue) return false;
                instance.m_numBitsForSubpartIndex = castValue;
                return true;
            }
            case "m_trianglesSubparts":
            case "trianglesSubparts":
            {
                if (value is not List<Havok.hk55.hkpExtendedMeshShapeTrianglesSubpart> castValue) return false;
                instance.m_trianglesSubparts = castValue;
                return true;
            }
            case "m_shapesSubparts":
            case "shapesSubparts":
            {
                if (value is not List<Havok.hk55.hkpExtendedMeshShapeShapesSubpart> castValue) return false;
                instance.m_shapesSubparts = castValue;
                return true;
            }
            case "m_weldingInfo":
            case "weldingInfo":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_weldingInfo = castValue;
                return true;
            }
            case "m_weldingType":
            case "weldingType":
            {
                if (value is not byte castValue) return false;
                instance.m_weldingType = castValue;
                return true;
            }
            case "m_defaultCollisionFilterInfo":
            case "defaultCollisionFilterInfo":
            {
                if (value is not uint castValue) return false;
                instance.m_defaultCollisionFilterInfo = castValue;
                return true;
            }
            case "m_cachedNumChildShapes":
            case "cachedNumChildShapes":
            {
                if (value is not int castValue) return false;
                instance.m_cachedNumChildShapes = castValue;
                return true;
            }
            case "m_triangleRadius":
            case "triangleRadius":
            {
                if (value is not float castValue) return false;
                instance.m_triangleRadius = castValue;
                return true;
            }
            case "m_meshstorage":
            case "meshstorage":
            {
                if (value is not List<Havok.hk55.hkpStorageExtendedMeshShapeMeshSubpartStorage?> castValue) return false;
                instance.m_meshstorage = castValue;
                return true;
            }
            case "m_shapestorage":
            case "shapestorage":
            {
                if (value is not List<Havok.hk55.hkpStorageExtendedMeshShapeShapeSubpartStorage?> castValue) return false;
                instance.m_shapestorage = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
