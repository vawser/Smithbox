// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclVertexCopySetupObjectData : HavokData<hclVertexCopySetupObject> 
{
    public hclVertexCopySetupObjectData(HavokType type, hclVertexCopySetupObject instance) : base(type, instance) {}

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
            case "m_inputBufferSetupObject":
            case "inputBufferSetupObject":
            {
                if (instance.m_inputBufferSetupObject is null)
                {
                    return true;
                }
                if (instance.m_inputBufferSetupObject is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_outputBufferSetupObject":
            case "outputBufferSetupObject":
            {
                if (instance.m_outputBufferSetupObject is null)
                {
                    return true;
                }
                if (instance.m_outputBufferSetupObject is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
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
            case "m_inputBufferSetupObject":
            case "inputBufferSetupObject":
            {
                if (value is null)
                {
                    instance.m_inputBufferSetupObject = default;
                    return true;
                }
                if (value is hclBufferSetupObject castValue)
                {
                    instance.m_inputBufferSetupObject = castValue;
                    return true;
                }
                return false;
            }
            case "m_outputBufferSetupObject":
            case "outputBufferSetupObject":
            {
                if (value is null)
                {
                    instance.m_outputBufferSetupObject = default;
                    return true;
                }
                if (value is hclBufferSetupObject castValue)
                {
                    instance.m_outputBufferSetupObject = castValue;
                    return true;
                }
                return false;
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
