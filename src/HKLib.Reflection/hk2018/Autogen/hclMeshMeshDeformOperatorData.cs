// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclMeshMeshDeformOperatorData : HavokData<hclMeshMeshDeformOperator> 
{
    public hclMeshMeshDeformOperatorData(HavokType type, hclMeshMeshDeformOperator instance) : base(type, instance) {}

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
            case "m_operatorID":
            case "operatorID":
            {
                if (instance.m_operatorID is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_usedBuffers":
            case "usedBuffers":
            {
                if (instance.m_usedBuffers is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_usedTransformSets":
            case "usedTransformSets":
            {
                if (instance.m_usedTransformSets is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_inputTrianglesSubset":
            case "inputTrianglesSubset":
            {
                if (instance.m_inputTrianglesSubset is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_triangleVertexPairs":
            case "triangleVertexPairs":
            {
                if (instance.m_triangleVertexPairs is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_triangleVertexStartForVertex":
            case "triangleVertexStartForVertex":
            {
                if (instance.m_triangleVertexStartForVertex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_inputBufferIdx":
            case "inputBufferIdx":
            {
                if (instance.m_inputBufferIdx is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_outputBufferIdx":
            case "outputBufferIdx":
            {
                if (instance.m_outputBufferIdx is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_startVertex":
            case "startVertex":
            {
                if (instance.m_startVertex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_endVertex":
            case "endVertex":
            {
                if (instance.m_endVertex is not TGet castValue) return false;
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
            case "m_deformNormals":
            case "deformNormals":
            {
                if (instance.m_deformNormals is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_partialDeform":
            case "partialDeform":
            {
                if (instance.m_partialDeform is not TGet castValue) return false;
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
            case "m_operatorID":
            case "operatorID":
            {
                if (value is not uint castValue) return false;
                instance.m_operatorID = castValue;
                return true;
            }
            case "m_usedBuffers":
            case "usedBuffers":
            {
                if (value is not List<hclClothState.BufferAccess> castValue) return false;
                instance.m_usedBuffers = castValue;
                return true;
            }
            case "m_usedTransformSets":
            case "usedTransformSets":
            {
                if (value is not List<hclClothState.TransformSetAccess> castValue) return false;
                instance.m_usedTransformSets = castValue;
                return true;
            }
            case "m_inputTrianglesSubset":
            case "inputTrianglesSubset":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_inputTrianglesSubset = castValue;
                return true;
            }
            case "m_triangleVertexPairs":
            case "triangleVertexPairs":
            {
                if (value is not List<hclMeshMeshDeformOperator.TriangleVertexPair> castValue) return false;
                instance.m_triangleVertexPairs = castValue;
                return true;
            }
            case "m_triangleVertexStartForVertex":
            case "triangleVertexStartForVertex":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_triangleVertexStartForVertex = castValue;
                return true;
            }
            case "m_inputBufferIdx":
            case "inputBufferIdx":
            {
                if (value is not uint castValue) return false;
                instance.m_inputBufferIdx = castValue;
                return true;
            }
            case "m_outputBufferIdx":
            case "outputBufferIdx":
            {
                if (value is not uint castValue) return false;
                instance.m_outputBufferIdx = castValue;
                return true;
            }
            case "m_startVertex":
            case "startVertex":
            {
                if (value is not ushort castValue) return false;
                instance.m_startVertex = castValue;
                return true;
            }
            case "m_endVertex":
            case "endVertex":
            {
                if (value is not ushort castValue) return false;
                instance.m_endVertex = castValue;
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
            case "m_deformNormals":
            case "deformNormals":
            {
                if (value is not bool castValue) return false;
                instance.m_deformNormals = castValue;
                return true;
            }
            case "m_partialDeform":
            case "partialDeform":
            {
                if (value is not bool castValue) return false;
                instance.m_partialDeform = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
