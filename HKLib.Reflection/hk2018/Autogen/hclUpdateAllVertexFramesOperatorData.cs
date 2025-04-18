// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclUpdateAllVertexFramesOperatorData : HavokData<hclUpdateAllVertexFramesOperator> 
{
    public hclUpdateAllVertexFramesOperatorData(HavokType type, hclUpdateAllVertexFramesOperator instance) : base(type, instance) {}

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
            case "m_vertToNormalID":
            case "vertToNormalID":
            {
                if (instance.m_vertToNormalID is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_triangleFlips":
            case "triangleFlips":
            {
                if (instance.m_triangleFlips is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_referenceVertices":
            case "referenceVertices":
            {
                if (instance.m_referenceVertices is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_tangentEdgeCosAngle":
            case "tangentEdgeCosAngle":
            {
                if (instance.m_tangentEdgeCosAngle is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_tangentEdgeSinAngle":
            case "tangentEdgeSinAngle":
            {
                if (instance.m_tangentEdgeSinAngle is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_biTangentFlip":
            case "biTangentFlip":
            {
                if (instance.m_biTangentFlip is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bufferIdx":
            case "bufferIdx":
            {
                if (instance.m_bufferIdx is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numUniqueNormalIDs":
            case "numUniqueNormalIDs":
            {
                if (instance.m_numUniqueNormalIDs is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_updateNormals":
            case "updateNormals":
            {
                if (instance.m_updateNormals is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_updateTangents":
            case "updateTangents":
            {
                if (instance.m_updateTangents is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_updateBiTangents":
            case "updateBiTangents":
            {
                if (instance.m_updateBiTangents is not TGet castValue) return false;
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
            case "m_vertToNormalID":
            case "vertToNormalID":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_vertToNormalID = castValue;
                return true;
            }
            case "m_triangleFlips":
            case "triangleFlips":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_triangleFlips = castValue;
                return true;
            }
            case "m_referenceVertices":
            case "referenceVertices":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_referenceVertices = castValue;
                return true;
            }
            case "m_tangentEdgeCosAngle":
            case "tangentEdgeCosAngle":
            {
                if (value is not List<float> castValue) return false;
                instance.m_tangentEdgeCosAngle = castValue;
                return true;
            }
            case "m_tangentEdgeSinAngle":
            case "tangentEdgeSinAngle":
            {
                if (value is not List<float> castValue) return false;
                instance.m_tangentEdgeSinAngle = castValue;
                return true;
            }
            case "m_biTangentFlip":
            case "biTangentFlip":
            {
                if (value is not List<float> castValue) return false;
                instance.m_biTangentFlip = castValue;
                return true;
            }
            case "m_bufferIdx":
            case "bufferIdx":
            {
                if (value is not uint castValue) return false;
                instance.m_bufferIdx = castValue;
                return true;
            }
            case "m_numUniqueNormalIDs":
            case "numUniqueNormalIDs":
            {
                if (value is not uint castValue) return false;
                instance.m_numUniqueNormalIDs = castValue;
                return true;
            }
            case "m_updateNormals":
            case "updateNormals":
            {
                if (value is not bool castValue) return false;
                instance.m_updateNormals = castValue;
                return true;
            }
            case "m_updateTangents":
            case "updateTangents":
            {
                if (value is not bool castValue) return false;
                instance.m_updateTangents = castValue;
                return true;
            }
            case "m_updateBiTangents":
            case "updateBiTangents":
            {
                if (value is not bool castValue) return false;
                instance.m_updateBiTangents = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
