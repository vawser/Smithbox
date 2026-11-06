// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclMeshMeshDeformSetupObjectData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclMeshMeshDeformSetupObject>
{
    public hclMeshMeshDeformSetupObjectData(Havok.hk2018.hclMeshMeshDeformSetupObject instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
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
            case "m_inputBufferPrevSetupObject":
            case "inputBufferPrevSetupObject":
            {
                value = instance.m_inputBufferPrevSetupObject;
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
            case "m_outputBufferPrevSetupObject":
            case "outputBufferPrevSetupObject":
            {
                value = instance.m_outputBufferPrevSetupObject;
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
            case "m_triangleWeightMode":
            case "triangleWeightMode":
            {
                value = instance.m_triangleWeightMode;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
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
                if (value is Havok.hk2018.hclBufferSetupObject castValue)
                {
                    instance.m_inputBufferSetupObject = castValue;
                    return true;
                }
                return false;
            }
            case "m_inputBufferPrevSetupObject":
            case "inputBufferPrevSetupObject":
            {
                if (value is null)
                {
                    instance.m_inputBufferPrevSetupObject = default;
                    return true;
                }
                if (value is Havok.hk2018.hclBufferSetupObject castValue)
                {
                    instance.m_inputBufferPrevSetupObject = castValue;
                    return true;
                }
                return false;
            }
            case "m_inputTriangleSelection":
            case "inputTriangleSelection":
            {
                if (value is not Havok.hk2018.hclTriangleSelectionInput castValue) return false;
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
                if (value is Havok.hk2018.hclBufferSetupObject castValue)
                {
                    instance.m_outputBufferSetupObject = castValue;
                    return true;
                }
                return false;
            }
            case "m_outputBufferPrevSetupObject":
            case "outputBufferPrevSetupObject":
            {
                if (value is null)
                {
                    instance.m_outputBufferPrevSetupObject = default;
                    return true;
                }
                if (value is Havok.hk2018.hclBufferSetupObject castValue)
                {
                    instance.m_outputBufferPrevSetupObject = castValue;
                    return true;
                }
                return false;
            }
            case "m_outputVertexSelection":
            case "outputVertexSelection":
            {
                if (value is not Havok.hk2018.hclVertexSelectionInput castValue) return false;
                instance.m_outputVertexSelection = castValue;
                return true;
            }
            case "m_influenceRadiusPerVertex":
            case "influenceRadiusPerVertex":
            {
                if (value is not Havok.hk2018.hclVertexFloatInput castValue) return false;
                instance.m_influenceRadiusPerVertex = castValue;
                return true;
            }
            case "m_scaleNormalBehaviour":
            case "scaleNormalBehaviour":
            {
                if (value is Havok.hk2018.hclMeshMeshDeformOperator.ScaleNormalBehaviour castValue)
                {
                    instance.m_scaleNormalBehaviour = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_scaleNormalBehaviour = (Havok.hk2018.hclMeshMeshDeformOperator.ScaleNormalBehaviour)uintValue;
                    return true;
                }
                return false;
            }
            case "m_triangleWeightMode":
            case "triangleWeightMode":
            {
                if (value is Havok.hk2018.hclMeshMeshDeformSetupObject.TriangleWeightMode castValue)
                {
                    instance.m_triangleWeightMode = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_triangleWeightMode = (Havok.hk2018.hclMeshMeshDeformSetupObject.TriangleWeightMode)uintValue;
                    return true;
                }
                return false;
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
