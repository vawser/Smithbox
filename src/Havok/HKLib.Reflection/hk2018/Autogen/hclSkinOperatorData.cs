// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclSkinOperatorData : HavokData<hclSkinOperator> 
{
    public hclSkinOperatorData(HavokType type, hclSkinOperator instance) : base(type, instance) {}

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
            case "m_boneInfluences":
            case "boneInfluences":
            {
                if (instance.m_boneInfluences is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_boneInfluenceStartPerVertex":
            case "boneInfluenceStartPerVertex":
            {
                if (instance.m_boneInfluenceStartPerVertex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_boneFromSkinMeshTransforms":
            case "boneFromSkinMeshTransforms":
            {
                if (instance.m_boneFromSkinMeshTransforms is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_usedBoneGroupIds":
            case "usedBoneGroupIds":
            {
                if (instance.m_usedBoneGroupIds is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_skinPositions":
            case "skinPositions":
            {
                if (instance.m_skinPositions is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_skinNormals":
            case "skinNormals":
            {
                if (instance.m_skinNormals is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_skinTangents":
            case "skinTangents":
            {
                if (instance.m_skinTangents is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_skinBiTangents":
            case "skinBiTangents":
            {
                if (instance.m_skinBiTangents is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_inputBufferIndex":
            case "inputBufferIndex":
            {
                if (instance.m_inputBufferIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_outputBufferIndex":
            case "outputBufferIndex":
            {
                if (instance.m_outputBufferIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_transformSetIndex":
            case "transformSetIndex":
            {
                if (instance.m_transformSetIndex is not TGet castValue) return false;
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
            case "m_partialSkinning":
            case "partialSkinning":
            {
                if (instance.m_partialSkinning is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_dualQuaternionSkinning":
            case "dualQuaternionSkinning":
            {
                if (instance.m_dualQuaternionSkinning is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_boneGroupSize":
            case "boneGroupSize":
            {
                if (instance.m_boneGroupSize is not TGet castValue) return false;
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
            case "m_boneInfluences":
            case "boneInfluences":
            {
                if (value is not List<hclSkinOperator.BoneInfluence> castValue) return false;
                instance.m_boneInfluences = castValue;
                return true;
            }
            case "m_boneInfluenceStartPerVertex":
            case "boneInfluenceStartPerVertex":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_boneInfluenceStartPerVertex = castValue;
                return true;
            }
            case "m_boneFromSkinMeshTransforms":
            case "boneFromSkinMeshTransforms":
            {
                if (value is not List<Matrix4x4> castValue) return false;
                instance.m_boneFromSkinMeshTransforms = castValue;
                return true;
            }
            case "m_usedBoneGroupIds":
            case "usedBoneGroupIds":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_usedBoneGroupIds = castValue;
                return true;
            }
            case "m_skinPositions":
            case "skinPositions":
            {
                if (value is not bool castValue) return false;
                instance.m_skinPositions = castValue;
                return true;
            }
            case "m_skinNormals":
            case "skinNormals":
            {
                if (value is not bool castValue) return false;
                instance.m_skinNormals = castValue;
                return true;
            }
            case "m_skinTangents":
            case "skinTangents":
            {
                if (value is not bool castValue) return false;
                instance.m_skinTangents = castValue;
                return true;
            }
            case "m_skinBiTangents":
            case "skinBiTangents":
            {
                if (value is not bool castValue) return false;
                instance.m_skinBiTangents = castValue;
                return true;
            }
            case "m_inputBufferIndex":
            case "inputBufferIndex":
            {
                if (value is not uint castValue) return false;
                instance.m_inputBufferIndex = castValue;
                return true;
            }
            case "m_outputBufferIndex":
            case "outputBufferIndex":
            {
                if (value is not uint castValue) return false;
                instance.m_outputBufferIndex = castValue;
                return true;
            }
            case "m_transformSetIndex":
            case "transformSetIndex":
            {
                if (value is not uint castValue) return false;
                instance.m_transformSetIndex = castValue;
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
            case "m_partialSkinning":
            case "partialSkinning":
            {
                if (value is not bool castValue) return false;
                instance.m_partialSkinning = castValue;
                return true;
            }
            case "m_dualQuaternionSkinning":
            case "dualQuaternionSkinning":
            {
                if (value is not bool castValue) return false;
                instance.m_dualQuaternionSkinning = castValue;
                return true;
            }
            case "m_boneGroupSize":
            case "boneGroupSize":
            {
                if (value is not byte castValue) return false;
                instance.m_boneGroupSize = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
