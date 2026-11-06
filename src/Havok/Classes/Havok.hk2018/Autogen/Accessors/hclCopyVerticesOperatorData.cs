// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclCopyVerticesOperatorData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclCopyVerticesOperator>
{
    public hclCopyVerticesOperatorData(Havok.hk2018.hclCopyVerticesOperator instance) : base(instance) { }

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
            case "m_numberOfVertices":
            case "numberOfVertices":
            {
                value = instance.m_numberOfVertices;
                return true;
            }
            case "m_startVertexIn":
            case "startVertexIn":
            {
                value = instance.m_startVertexIn;
                return true;
            }
            case "m_startVertexOut":
            case "startVertexOut":
            {
                value = instance.m_startVertexOut;
                return true;
            }
            case "m_copyNormals":
            case "copyNormals":
            {
                value = instance.m_copyNormals;
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
            case "m_numberOfVertices":
            case "numberOfVertices":
            {
                if (value is not uint castValue) return false;
                instance.m_numberOfVertices = castValue;
                return true;
            }
            case "m_startVertexIn":
            case "startVertexIn":
            {
                if (value is not uint castValue) return false;
                instance.m_startVertexIn = castValue;
                return true;
            }
            case "m_startVertexOut":
            case "startVertexOut":
            {
                if (value is not uint castValue) return false;
                instance.m_startVertexOut = castValue;
                return true;
            }
            case "m_copyNormals":
            case "copyNormals":
            {
                if (value is not bool castValue) return false;
                instance.m_copyNormals = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
