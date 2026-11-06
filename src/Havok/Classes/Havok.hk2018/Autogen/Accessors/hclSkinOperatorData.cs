// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclSkinOperatorData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclSkinOperator>
{
    public hclSkinOperatorData(Havok.hk2018.hclSkinOperator instance) : base(instance) { }

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
            case "m_boneInfluences":
            case "boneInfluences":
            {
                value = instance.m_boneInfluences;
                return true;
            }
            case "m_boneInfluenceStartPerVertex":
            case "boneInfluenceStartPerVertex":
            {
                value = instance.m_boneInfluenceStartPerVertex;
                return true;
            }
            case "m_boneFromSkinMeshTransforms":
            case "boneFromSkinMeshTransforms":
            {
                value = instance.m_boneFromSkinMeshTransforms;
                return true;
            }
            case "m_usedBoneGroupIds":
            case "usedBoneGroupIds":
            {
                value = instance.m_usedBoneGroupIds;
                return true;
            }
            case "m_skinPositions":
            case "skinPositions":
            {
                value = instance.m_skinPositions;
                return true;
            }
            case "m_skinNormals":
            case "skinNormals":
            {
                value = instance.m_skinNormals;
                return true;
            }
            case "m_skinTangents":
            case "skinTangents":
            {
                value = instance.m_skinTangents;
                return true;
            }
            case "m_skinBiTangents":
            case "skinBiTangents":
            {
                value = instance.m_skinBiTangents;
                return true;
            }
            case "m_inputBufferIndex":
            case "inputBufferIndex":
            {
                value = instance.m_inputBufferIndex;
                return true;
            }
            case "m_outputBufferIndex":
            case "outputBufferIndex":
            {
                value = instance.m_outputBufferIndex;
                return true;
            }
            case "m_transformSetIndex":
            case "transformSetIndex":
            {
                value = instance.m_transformSetIndex;
                return true;
            }
            case "m_startVertex":
            case "startVertex":
            {
                value = instance.m_startVertex;
                return true;
            }
            case "m_endVertex":
            case "endVertex":
            {
                value = instance.m_endVertex;
                return true;
            }
            case "m_partialSkinning":
            case "partialSkinning":
            {
                value = instance.m_partialSkinning;
                return true;
            }
            case "m_dualQuaternionSkinning":
            case "dualQuaternionSkinning":
            {
                value = instance.m_dualQuaternionSkinning;
                return true;
            }
            case "m_boneGroupSize":
            case "boneGroupSize":
            {
                value = instance.m_boneGroupSize;
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
            case "m_boneInfluences":
            case "boneInfluences":
            {
                if (value is not List<Havok.hk2018.hclSkinOperator.BoneInfluence> castValue) return false;
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
