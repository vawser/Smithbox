// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclMeshMeshDeformSetupObjectData : HavokData<hclMeshMeshDeformSetupObject> 
{
    public hclMeshMeshDeformSetupObjectData(HavokType type, hclMeshMeshDeformSetupObject instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (instance.m_name is null)
                {
                    return true;
                }
                if (instance.m_name is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_inputBufferSetupObject":
            case "inputBufferSetupObject":
            {
                if (instance.m_inputBufferSetupObject is null)
                {
                    return true;
                }
                if (instance.m_inputBufferSetupObject is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_inputBufferPrevSetupObject":
            case "inputBufferPrevSetupObject":
            {
                if (instance.m_inputBufferPrevSetupObject is null)
                {
                    return true;
                }
                if (instance.m_inputBufferPrevSetupObject is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_inputTriangleSelection":
            case "inputTriangleSelection":
            {
                if (instance.m_inputTriangleSelection is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_outputBufferSetupObject":
            case "outputBufferSetupObject":
            {
                if (instance.m_outputBufferSetupObject is null)
                {
                    return true;
                }
                if (instance.m_outputBufferSetupObject is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_outputBufferPrevSetupObject":
            case "outputBufferPrevSetupObject":
            {
                if (instance.m_outputBufferPrevSetupObject is null)
                {
                    return true;
                }
                if (instance.m_outputBufferPrevSetupObject is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_outputVertexSelection":
            case "outputVertexSelection":
            {
                if (instance.m_outputVertexSelection is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_influenceRadiusPerVertex":
            case "influenceRadiusPerVertex":
            {
                if (instance.m_influenceRadiusPerVertex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_scaleNormalBehaviour":
            case "scaleNormalBehaviour":
            {
                if (instance.m_scaleNormalBehaviour is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((uint)instance.m_scaleNormalBehaviour is TGet uintValue)
                {
                    value = uintValue;
                    return true;
                }
                return false;
            }
            case "m_triangleWeightMode":
            case "triangleWeightMode":
            {
                if (instance.m_triangleWeightMode is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((uint)instance.m_triangleWeightMode is TGet uintValue)
                {
                    value = uintValue;
                    return true;
                }
                return false;
            }
            case "m_maxTrianglesPerVertex":
            case "maxTrianglesPerVertex":
            {
                if (instance.m_maxTrianglesPerVertex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_minimumTriangleWeight":
            case "minimumTriangleWeight":
            {
                if (instance.m_minimumTriangleWeight is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_deformNormals":
            case "deformNormals":
            {
                if (instance.m_deformNormals is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_deformTangents":
            case "deformTangents":
            {
                if (instance.m_deformTangents is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_deformBiTangents":
            case "deformBiTangents":
            {
                if (instance.m_deformBiTangents is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_useMeshTopology":
            case "useMeshTopology":
            {
                if (instance.m_useMeshTopology is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
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
                if (value is hclBufferSetupObject castValue)
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
                if (value is hclBufferSetupObject castValue)
                {
                    instance.m_inputBufferPrevSetupObject = castValue;
                    return true;
                }
                return false;
            }
            case "m_inputTriangleSelection":
            case "inputTriangleSelection":
            {
                if (value is not hclTriangleSelectionInput castValue) return false;
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
                if (value is hclBufferSetupObject castValue)
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
                if (value is hclBufferSetupObject castValue)
                {
                    instance.m_outputBufferPrevSetupObject = castValue;
                    return true;
                }
                return false;
            }
            case "m_outputVertexSelection":
            case "outputVertexSelection":
            {
                if (value is not hclVertexSelectionInput castValue) return false;
                instance.m_outputVertexSelection = castValue;
                return true;
            }
            case "m_influenceRadiusPerVertex":
            case "influenceRadiusPerVertex":
            {
                if (value is not hclVertexFloatInput castValue) return false;
                instance.m_influenceRadiusPerVertex = castValue;
                return true;
            }
            case "m_scaleNormalBehaviour":
            case "scaleNormalBehaviour":
            {
                if (value is hclMeshMeshDeformOperator.ScaleNormalBehaviour castValue)
                {
                    instance.m_scaleNormalBehaviour = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_scaleNormalBehaviour = (hclMeshMeshDeformOperator.ScaleNormalBehaviour)uintValue;
                    return true;
                }
                return false;
            }
            case "m_triangleWeightMode":
            case "triangleWeightMode":
            {
                if (value is hclMeshMeshDeformSetupObject.TriangleWeightMode castValue)
                {
                    instance.m_triangleWeightMode = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_triangleWeightMode = (hclMeshMeshDeformSetupObject.TriangleWeightMode)uintValue;
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
