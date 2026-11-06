// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclBlendSomeVerticesOperatorData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclBlendSomeVerticesOperator>
{
    public hclBlendSomeVerticesOperatorData(Havok.hk2018.hclBlendSomeVerticesOperator instance) : base(instance) { }

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
            case "m_blendEntries":
            case "blendEntries":
            {
                value = instance.m_blendEntries;
                return true;
            }
            case "m_blendVertices":
            case "blendVertices":
            {
                value = instance.m_blendVertices;
                return true;
            }
            case "m_dynamicBlendData":
            case "dynamicBlendData":
            {
                value = instance.m_dynamicBlendData;
                return true;
            }
            case "m_bufferIdx_A":
            case "bufferIdx_A":
            {
                value = instance.m_bufferIdx_A;
                return true;
            }
            case "m_bufferIdx_B":
            case "bufferIdx_B":
            {
                value = instance.m_bufferIdx_B;
                return true;
            }
            case "m_bufferIdx_C":
            case "bufferIdx_C":
            {
                value = instance.m_bufferIdx_C;
                return true;
            }
            case "m_blendNormals":
            case "blendNormals":
            {
                value = instance.m_blendNormals;
                return true;
            }
            case "m_blendTangents":
            case "blendTangents":
            {
                value = instance.m_blendTangents;
                return true;
            }
            case "m_blendBitangents":
            case "blendBitangents":
            {
                value = instance.m_blendBitangents;
                return true;
            }
            case "m_dynamicBlend":
            case "dynamicBlend":
            {
                value = instance.m_dynamicBlend;
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
            case "m_blendEntries":
            case "blendEntries":
            {
                if (value is not List<Havok.hk2018.hclBlendSomeVerticesOperator.BlendEntry> castValue) return false;
                instance.m_blendEntries = castValue;
                return true;
            }
            case "m_blendVertices":
            case "blendVertices":
            {
                if (value is not Havok.hk2018.hclBlendSomeVerticesOperator.BlendVertices castValue) return false;
                instance.m_blendVertices = castValue;
                return true;
            }
            case "m_dynamicBlendData":
            case "dynamicBlendData":
            {
                if (value is not Havok.hk2018.hclBlendSomeVerticesOperator.DynamicBlendData castValue) return false;
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
