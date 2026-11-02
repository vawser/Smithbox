// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiViewerNodesNavMeshInstanceData : HavokData<HKLib.hk2019.hkaiViewerNodes.NavMeshInstance> 
{
    public hkaiViewerNodesNavMeshInstanceData(HavokType type, HKLib.hk2019.hkaiViewerNodes.NavMeshInstance instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_uid":
            case "uid":
            {
                if (instance.m_uid is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_runtimeId":
            case "runtimeId":
            {
                if (instance.m_runtimeId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_referenceFrame":
            case "referenceFrame":
            {
                if (instance.m_referenceFrame is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numFaces":
            case "numFaces":
            {
                if (instance.m_numFaces is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numEdges":
            case "numEdges":
            {
                if (instance.m_numEdges is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numVertices":
            case "numVertices":
            {
                if (instance.m_numVertices is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
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
                if (value is not HKLib.hk2019.hkaiReferenceFrame castValue) return false;
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
