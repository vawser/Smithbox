// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclCopyVerticesOperatorData : HavokData<hclCopyVerticesOperator> 
{
    public hclCopyVerticesOperatorData(HavokType type, hclCopyVerticesOperator instance) : base(type, instance) {}

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
            case "m_numberOfVertices":
            case "numberOfVertices":
            {
                if (instance.m_numberOfVertices is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_startVertexIn":
            case "startVertexIn":
            {
                if (instance.m_startVertexIn is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_startVertexOut":
            case "startVertexOut":
            {
                if (instance.m_startVertexOut is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_copyNormals":
            case "copyNormals":
            {
                if (instance.m_copyNormals is not TGet castValue) return false;
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
