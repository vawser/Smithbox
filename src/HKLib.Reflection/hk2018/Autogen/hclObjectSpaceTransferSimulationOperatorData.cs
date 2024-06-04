// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclObjectSpaceTransferSimulationOperatorData : HavokData<hclObjectSpaceTransferSimulationOperator> 
{
    public hclObjectSpaceTransferSimulationOperatorData(HavokType type, hclObjectSpaceTransferSimulationOperator instance) : base(type, instance) {}

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
            case "m_inputTrianglesSubset":
            case "inputTrianglesSubset":
            {
                if (instance.m_inputTrianglesSubset is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_triangleFromMeshTransforms":
            case "triangleFromMeshTransforms":
            {
                if (instance.m_triangleFromMeshTransforms is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_objectSpaceDeformer":
            case "objectSpaceDeformer":
            {
                if (instance.m_objectSpaceDeformer is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_customSkinDeform":
            case "customSkinDeform":
            {
                if (instance.m_customSkinDeform is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_localPs":
            case "localPs":
            {
                if (instance.m_localPs is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_localUnpackedPs":
            case "localUnpackedPs":
            {
                if (instance.m_localUnpackedPs is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_inputBufferPrevIdx":
            case "inputBufferPrevIdx":
            {
                if (instance.m_inputBufferPrevIdx is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_outputBufferPrevIdx":
            case "outputBufferPrevIdx":
            {
                if (instance.m_outputBufferPrevIdx is not TGet castValue) return false;
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
            case "m_scaleNormalBehaviour":
            case "scaleNormalBehaviour":
            {
                if (value is hclObjectSpaceMeshMeshDeformOperator.ScaleNormalBehaviour castValue)
                {
                    instance.m_scaleNormalBehaviour = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_scaleNormalBehaviour = (hclObjectSpaceMeshMeshDeformOperator.ScaleNormalBehaviour)uintValue;
                    return true;
                }
                return false;
            }
            case "m_inputTrianglesSubset":
            case "inputTrianglesSubset":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_inputTrianglesSubset = castValue;
                return true;
            }
            case "m_triangleFromMeshTransforms":
            case "triangleFromMeshTransforms":
            {
                if (value is not List<Matrix4x4> castValue) return false;
                instance.m_triangleFromMeshTransforms = castValue;
                return true;
            }
            case "m_objectSpaceDeformer":
            case "objectSpaceDeformer":
            {
                if (value is not hclObjectSpaceDeformer castValue) return false;
                instance.m_objectSpaceDeformer = castValue;
                return true;
            }
            case "m_customSkinDeform":
            case "customSkinDeform":
            {
                if (value is not bool castValue) return false;
                instance.m_customSkinDeform = castValue;
                return true;
            }
            case "m_localPs":
            case "localPs":
            {
                if (value is not List<hclObjectSpaceDeformer.LocalBlockP> castValue) return false;
                instance.m_localPs = castValue;
                return true;
            }
            case "m_localUnpackedPs":
            case "localUnpackedPs":
            {
                if (value is not List<hclObjectSpaceDeformer.LocalBlockUnpackedP> castValue) return false;
                instance.m_localUnpackedPs = castValue;
                return true;
            }
            case "m_inputBufferPrevIdx":
            case "inputBufferPrevIdx":
            {
                if (value is not uint castValue) return false;
                instance.m_inputBufferPrevIdx = castValue;
                return true;
            }
            case "m_outputBufferPrevIdx":
            case "outputBufferPrevIdx":
            {
                if (value is not uint castValue) return false;
                instance.m_outputBufferPrevIdx = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
