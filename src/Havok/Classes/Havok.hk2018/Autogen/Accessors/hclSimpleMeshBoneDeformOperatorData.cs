// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclSimpleMeshBoneDeformOperatorData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclSimpleMeshBoneDeformOperator>
{
    public hclSimpleMeshBoneDeformOperatorData(Havok.hk2018.hclSimpleMeshBoneDeformOperator instance) : base(instance) { }

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
            case "m_outputTransformSetIdx":
            case "outputTransformSetIdx":
            {
                value = instance.m_outputTransformSetIdx;
                return true;
            }
            case "m_triangleBonePairs":
            case "triangleBonePairs":
            {
                value = instance.m_triangleBonePairs;
                return true;
            }
            case "m_localBoneTransforms":
            case "localBoneTransforms":
            {
                value = instance.m_localBoneTransforms;
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
            case "m_outputTransformSetIdx":
            case "outputTransformSetIdx":
            {
                if (value is not uint castValue) return false;
                instance.m_outputTransformSetIdx = castValue;
                return true;
            }
            case "m_triangleBonePairs":
            case "triangleBonePairs":
            {
                if (value is not List<Havok.hk2018.hclSimpleMeshBoneDeformOperator.TriangleBonePair> castValue) return false;
                instance.m_triangleBonePairs = castValue;
                return true;
            }
            case "m_localBoneTransforms":
            case "localBoneTransforms":
            {
                if (value is not List<Matrix4x4> castValue) return false;
                instance.m_localBoneTransforms = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
