// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiNavMeshCutterMeshInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiNavMeshCutterMeshInfo>
{
    public hkaiNavMeshCutterMeshInfoData(Havok.hk2014.hkaiNavMeshCutterMeshInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_originalNumFaces":
            case "originalNumFaces":
            {
                value = instance.m_originalNumFaces;
                return true;
            }
            case "m_originalNumEdges":
            case "originalNumEdges":
            {
                value = instance.m_originalNumEdges;
                return true;
            }
            case "m_originalNumVertices":
            case "originalNumVertices":
            {
                value = instance.m_originalNumVertices;
                return true;
            }
            case "m_magic":
            case "magic":
            {
                value = instance.m_magic;
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
            case "m_originalNumFaces":
            case "originalNumFaces":
            {
                if (value is not int castValue) return false;
                instance.m_originalNumFaces = castValue;
                return true;
            }
            case "m_originalNumEdges":
            case "originalNumEdges":
            {
                if (value is not int castValue) return false;
                instance.m_originalNumEdges = castValue;
                return true;
            }
            case "m_originalNumVertices":
            case "originalNumVertices":
            {
                if (value is not int castValue) return false;
                instance.m_originalNumVertices = castValue;
                return true;
            }
            case "m_magic":
            case "magic":
            {
                if (value is not List<int> castValue) return false;
                instance.m_magic = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
