// Automatically Generated

#nullable enable

namespace Havok.hk2015.Accessors;

internal sealed class hkpConvexVerticesConnectivityData : Havok.Formats.HavokFieldAccess<Havok.hk2015.hkpConvexVerticesConnectivity>
{
    public hkpConvexVerticesConnectivityData(Havok.hk2015.hkpConvexVerticesConnectivity instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_vertexIndices":
            case "vertexIndices":
            {
                value = instance.m_vertexIndices;
                return true;
            }
            case "m_numVerticesPerFace":
            case "numVerticesPerFace":
            {
                value = instance.m_numVerticesPerFace;
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
            case "m_vertexIndices":
            case "vertexIndices":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_vertexIndices = castValue;
                return true;
            }
            case "m_numVerticesPerFace":
            case "numVerticesPerFace":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_numVerticesPerFace = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
