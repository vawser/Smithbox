// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclMeshMeshDeformSetupObjectData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclMeshMeshDeformSetupObject>
{
    public hclMeshMeshDeformSetupObjectData(Havok.hk2014.hclMeshMeshDeformSetupObject instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_inputBufferSetupObject":
            case "inputBufferSetupObject":
            {
                value = instance.m_inputBufferSetupObject;
                return true;
            }
            case "m_inputTriangleSelection":
            case "inputTriangleSelection":
            {
                value = instance.m_inputTriangleSelection;
                return true;
            }
            case "m_outputBufferSetupObject":
            case "outputBufferSetupObject":
            {
                value = instance.m_outputBufferSetupObject;
                return true;
            }
            case "m_outputVertexSelection":
            case "outputVertexSelection":
            {
                value = instance.m_outputVertexSelection;
                return true;
            }
            case "m_influenceRadiusPerVertex":
            case "influenceRadiusPerVertex":
            {
                value = instance.m_influenceRadiusPerVertex;
                return true;
            }
            case "m_scaleNormalBehaviour":
            case "scaleNormalBehaviour":
            {
                value = instance.m_scaleNormalBehaviour;
                return true;
            }
            case "m_maxTrianglesPerVertex":
            case "maxTrianglesPerVertex":
            {
                value = instance.m_maxTrianglesPerVertex;
                return true;
            }
            case "m_minimumTriangleWeight":
            case "minimumTriangleWeight":
            {
                value = instance.m_minimumTriangleWeight;
                return true;
            }
            case "m_deformNormals":
            case "deformNormals":
            {
                value = instance.m_deformNormals;
                return true;
            }
            case "m_deformTangents":
            case "deformTangents":
            {
                value = instance.m_deformTangents;
                return true;
            }
            case "m_deformBiTangents":
            case "deformBiTangents":
            {
                value = instance.m_deformBiTangents;
                return true;
            }
            case "m_useMeshTopology":
            case "useMeshTopology":
            {
                value = instance.m_useMeshTopology;
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
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_inputBufferSetupObject":
            case "inputBufferSetupObject":
            {
                if (value is null)
                {
                    instance.m_inputBufferSetupObject = default;
                    return true;
                }
                if (value is Havok.hk2014.hclBufferSetupObject castValue)
                {
                    instance.m_inputBufferSetupObject = castValue;
                    return true;
                }
                return false;
            }
            case "m_inputTriangleSelection":
            case "inputTriangleSelection":
            {
                if (value is not Havok.hk2014.hclTriangleSelectionInput castValue) return false;
                instance.m_inputTriangleSelection = castValue;
                return true;
            }
            case "m_outputBufferSetupObject":
            case "outputBufferSetupObject":
            {
                if (value is null)
                {
                    instance.m_outputBufferSetupObject = default;
                    return true;
                }
                if (value is Havok.hk2014.hclBufferSetupObject castValue)
                {
                    instance.m_outputBufferSetupObject = castValue;
                    return true;
                }
                return false;
            }
            case "m_outputVertexSelection":
            case "outputVertexSelection":
            {
                if (value is not Havok.hk2014.hclVertexSelectionInput castValue) return false;
                instance.m_outputVertexSelection = castValue;
                return true;
            }
            case "m_influenceRadiusPerVertex":
            case "influenceRadiusPerVertex":
            {
                if (value is not Havok.hk2014.hclVertexFloatInput castValue) return false;
                instance.m_influenceRadiusPerVertex = castValue;
                return true;
            }
            case "m_scaleNormalBehaviour":
            case "scaleNormalBehaviour":
            {
                if (value is not uint castValue) return false;
                instance.m_scaleNormalBehaviour = castValue;
                return true;
            }
            case "m_maxTrianglesPerVertex":
            case "maxTrianglesPerVertex":
            {
                if (value is not uint castValue) return false;
                instance.m_maxTrianglesPerVertex = castValue;
                return true;
            }
            case "m_minimumTriangleWeight":
            case "minimumTriangleWeight":
            {
                if (value is not float castValue) return false;
                instance.m_minimumTriangleWeight = castValue;
                return true;
            }
            case "m_deformNormals":
            case "deformNormals":
            {
                if (value is not bool castValue) return false;
                instance.m_deformNormals = castValue;
                return true;
            }
            case "m_deformTangents":
            case "deformTangents":
            {
                if (value is not bool castValue) return false;
                instance.m_deformTangents = castValue;
                return true;
            }
            case "m_deformBiTangents":
            case "deformBiTangents":
            {
                if (value is not bool castValue) return false;
                instance.m_deformBiTangents = castValue;
                return true;
            }
            case "m_useMeshTopology":
            case "useMeshTopology":
            {
                if (value is not bool castValue) return false;
                instance.m_useMeshTopology = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
