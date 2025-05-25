// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclBlendSomeVerticesOperatorData : HavokData<hclBlendSomeVerticesOperator> 
{
    public hclBlendSomeVerticesOperatorData(HavokType type, hclBlendSomeVerticesOperator instance) : base(type, instance) {}

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
            case "m_blendEntries":
            case "blendEntries":
            {
                if (instance.m_blendEntries is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_blendVertices":
            case "blendVertices":
            {
                if (instance.m_blendVertices is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_dynamicBlendData":
            case "dynamicBlendData":
            {
                if (instance.m_dynamicBlendData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bufferIdx_A":
            case "bufferIdx_A":
            {
                if (instance.m_bufferIdx_A is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bufferIdx_B":
            case "bufferIdx_B":
            {
                if (instance.m_bufferIdx_B is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bufferIdx_C":
            case "bufferIdx_C":
            {
                if (instance.m_bufferIdx_C is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_blendNormals":
            case "blendNormals":
            {
                if (instance.m_blendNormals is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_blendTangents":
            case "blendTangents":
            {
                if (instance.m_blendTangents is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_blendBitangents":
            case "blendBitangents":
            {
                if (instance.m_blendBitangents is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_dynamicBlend":
            case "dynamicBlend":
            {
                if (instance.m_dynamicBlend is not TGet castValue) return false;
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
            case "m_blendEntries":
            case "blendEntries":
            {
                if (value is not List<hclBlendSomeVerticesOperator.BlendEntry> castValue) return false;
                instance.m_blendEntries = castValue;
                return true;
            }
            case "m_blendVertices":
            case "blendVertices":
            {
                if (value is not hclBlendSomeVerticesOperator.BlendVertices castValue) return false;
                instance.m_blendVertices = castValue;
                return true;
            }
            case "m_dynamicBlendData":
            case "dynamicBlendData":
            {
                if (value is not hclBlendSomeVerticesOperator.DynamicBlendData castValue) return false;
                instance.m_dynamicBlendData = castValue;
                return true;
            }
            case "m_bufferIdx_A":
            case "bufferIdx_A":
            {
                if (value is not uint castValue) return false;
                instance.m_bufferIdx_A = castValue;
                return true;
            }
            case "m_bufferIdx_B":
            case "bufferIdx_B":
            {
                if (value is not uint castValue) return false;
                instance.m_bufferIdx_B = castValue;
                return true;
            }
            case "m_bufferIdx_C":
            case "bufferIdx_C":
            {
                if (value is not uint castValue) return false;
                instance.m_bufferIdx_C = castValue;
                return true;
            }
            case "m_blendNormals":
            case "blendNormals":
            {
                if (value is not bool castValue) return false;
                instance.m_blendNormals = castValue;
                return true;
            }
            case "m_blendTangents":
            case "blendTangents":
            {
                if (value is not bool castValue) return false;
                instance.m_blendTangents = castValue;
                return true;
            }
            case "m_blendBitangents":
            case "blendBitangents":
            {
                if (value is not bool castValue) return false;
                instance.m_blendBitangents = castValue;
                return true;
            }
            case "m_dynamicBlend":
            case "dynamicBlend":
            {
                if (value is not bool castValue) return false;
                instance.m_dynamicBlend = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
