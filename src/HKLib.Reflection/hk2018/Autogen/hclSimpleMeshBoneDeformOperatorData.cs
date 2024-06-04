// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclSimpleMeshBoneDeformOperatorData : HavokData<hclSimpleMeshBoneDeformOperator> 
{
    public hclSimpleMeshBoneDeformOperatorData(HavokType type, hclSimpleMeshBoneDeformOperator instance) : base(type, instance) {}

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
            case "m_outputTransformSetIdx":
            case "outputTransformSetIdx":
            {
                if (instance.m_outputTransformSetIdx is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_triangleBonePairs":
            case "triangleBonePairs":
            {
                if (instance.m_triangleBonePairs is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_localBoneTransforms":
            case "localBoneTransforms":
            {
                if (instance.m_localBoneTransforms is not TGet castValue) return false;
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
                if (value is not List<hclSimpleMeshBoneDeformOperator.TriangleBonePair> castValue) return false;
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
