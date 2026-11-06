// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiViewerNodesNavMeshInstanceData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiViewerNodes.NavMeshInstance>
{
    public hkaiViewerNodesNavMeshInstanceData(Havok.hk2019.hkaiViewerNodes.NavMeshInstance instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_uid":
            case "uid":
            {
                value = instance.m_uid;
                return true;
            }
            case "m_runtimeId":
            case "runtimeId":
            {
                value = instance.m_runtimeId;
                return true;
            }
            case "m_referenceFrame":
            case "referenceFrame":
            {
                value = instance.m_referenceFrame;
                return true;
            }
            case "m_numFaces":
            case "numFaces":
            {
                value = instance.m_numFaces;
                return true;
            }
            case "m_numEdges":
            case "numEdges":
            {
                value = instance.m_numEdges;
                return true;
            }
            case "m_numVertices":
            case "numVertices":
            {
                value = instance.m_numVertices;
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
            case "m_uid":
            case "uid":
            {
                if (value is not uint castValue) return false;
                instance.m_uid = castValue;
                return true;
            }
            case "m_runtimeId":
            case "runtimeId":
            {
                if (value is not int castValue) return false;
                instance.m_runtimeId = castValue;
                return true;
            }
            case "m_referenceFrame":
            case "referenceFrame":
            {
                if (value is not Havok.hk2019.hkaiReferenceFrame castValue) return false;
                instance.m_referenceFrame = castValue;
                return true;
            }
            case "m_numFaces":
            case "numFaces":
            {
                if (value is not int castValue) return false;
                instance.m_numFaces = castValue;
                return true;
            }
            case "m_numEdges":
            case "numEdges":
            {
                if (value is not int castValue) return false;
                instance.m_numEdges = castValue;
                return true;
            }
            case "m_numVertices":
            case "numVertices":
            {
                if (value is not int castValue) return false;
                instance.m_numVertices = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
