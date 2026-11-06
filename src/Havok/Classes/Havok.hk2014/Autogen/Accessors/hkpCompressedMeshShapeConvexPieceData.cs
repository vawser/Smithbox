// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpCompressedMeshShapeConvexPieceData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpCompressedMeshShapeConvexPiece>
{
    public hkpCompressedMeshShapeConvexPieceData(Havok.hk2014.hkpCompressedMeshShapeConvexPiece instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_offset":
            case "offset":
            {
                value = instance.m_offset;
                return true;
            }
            case "m_vertices":
            case "vertices":
            {
                value = instance.m_vertices;
                return true;
            }
            case "m_reference":
            case "reference":
            {
                value = instance.m_reference;
                return true;
            }
            case "m_transformIndex":
            case "transformIndex":
            {
                value = instance.m_transformIndex;
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
            case "m_offset":
            case "offset":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_offset = castValue;
                return true;
            }
            case "m_vertices":
            case "vertices":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_vertices = castValue;
                return true;
            }
            case "m_reference":
            case "reference":
            {
                if (value is not ushort castValue) return false;
                instance.m_reference = castValue;
                return true;
            }
            case "m_transformIndex":
            case "transformIndex":
            {
                if (value is not ushort castValue) return false;
                instance.m_transformIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
