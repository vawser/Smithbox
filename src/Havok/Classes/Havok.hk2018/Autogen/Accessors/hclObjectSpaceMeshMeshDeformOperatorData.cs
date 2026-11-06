// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclObjectSpaceMeshMeshDeformOperatorData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclObjectSpaceMeshMeshDeformOperator>
{
    public hclObjectSpaceMeshMeshDeformOperatorData(Havok.hk2018.hclObjectSpaceMeshMeshDeformOperator instance) : base(instance) { }

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
            case "m_operatorID":
            case "operatorID":
            {
                value = instance.m_operatorID;
                return true;
            }
            case "m_usedBuffers":
            case "usedBuffers":
            {
                value = instance.m_usedBuffers;
                return true;
            }
            case "m_usedTransformSets":
            case "usedTransformSets":
            {
                value = instance.m_usedTransformSets;
                return true;
            }
            case "m_inputBufferIdx":
            case "inputBufferIdx":
            {
                value = instance.m_inputBufferIdx;
                return true;
            }
            case "m_outputBufferIdx":
            case "outputBufferIdx":
            {
                value = instance.m_outputBufferIdx;
                return true;
            }
            case "m_scaleNormalBehaviour":
            case "scaleNormalBehaviour":
            {
                value = instance.m_scaleNormalBehaviour;
                return true;
            }
            case "m_inputTrianglesSubset":
            case "inputTrianglesSubset":
            {
                value = instance.m_inputTrianglesSubset;
                return true;
            }
            case "m_triangleFromMeshTransforms":
            case "triangleFromMeshTransforms":
            {
                value = instance.m_triangleFromMeshTransforms;
                return true;
            }
            case "m_objectSpaceDeformer":
            case "objectSpaceDeformer":
            {
                value = instance.m_objectSpaceDeformer;
                return true;
            }
            case "m_customSkinDeform":
            case "customSkinDeform":
            {
                value = instance.m_customSkinDeform;
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
                if (value is not List<Havok.hk2018.hclClothState.BufferAccess> castValue) return false;
                instance.m_usedBuffers = castValue;
                return true;
            }
            case "m_usedTransformSets":
            case "usedTransformSets":
            {
                if (value is not List<Havok.hk2018.hclClothState.TransformSetAccess> castValue) return false;
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
                if (value is Havok.hk2018.hclObjectSpaceMeshMeshDeformOperator.ScaleNormalBehaviour castValue)
                {
                    instance.m_scaleNormalBehaviour = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_scaleNormalBehaviour = (Havok.hk2018.hclObjectSpaceMeshMeshDeformOperator.ScaleNormalBehaviour)uintValue;
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
                if (value is not Havok.hk2018.hclObjectSpaceDeformer castValue) return false;
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
            default:
            return false;
        }
    }

}
