// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclSkinSetupObjectData : HavokData<hclSkinSetupObject> 
{
    public hclSkinSetupObjectData(HavokType type, hclSkinSetupObject instance) : base(type, instance) {}

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
            case "m_transformSetSetup":
            case "transformSetSetup":
            {
                if (instance.m_transformSetSetup is null)
                {
                    return true;
                }
                if (instance.m_transformSetSetup is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_referenceBufferSetup":
            case "referenceBufferSetup":
            {
                if (instance.m_referenceBufferSetup is null)
                {
                    return true;
                }
                if (instance.m_referenceBufferSetup is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_outputBufferSetup":
            case "outputBufferSetup":
            {
                if (instance.m_outputBufferSetup is null)
                {
                    return true;
                }
                if (instance.m_outputBufferSetup is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_vertexSelection":
            case "vertexSelection":
            {
                if (instance.m_vertexSelection is not TGet castValue) return false;
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
            case "m_useDualQuaternionMethod":
            case "useDualQuaternionMethod":
            {
                if (instance.m_useDualQuaternionMethod is not TGet castValue) return false;
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
            case "m_transformSetSetup":
            case "transformSetSetup":
            {
                if (value is null)
                {
                    instance.m_transformSetSetup = default;
                    return true;
                }
                if (value is hclTransformSetSetupObject castValue)
                {
                    instance.m_transformSetSetup = castValue;
                    return true;
                }
                return false;
            }
            case "m_referenceBufferSetup":
            case "referenceBufferSetup":
            {
                if (value is null)
                {
                    instance.m_referenceBufferSetup = default;
                    return true;
                }
                if (value is hclBufferSetupObject castValue)
                {
                    instance.m_referenceBufferSetup = castValue;
                    return true;
                }
                return false;
            }
            case "m_outputBufferSetup":
            case "outputBufferSetup":
            {
                if (value is null)
                {
                    instance.m_outputBufferSetup = default;
                    return true;
                }
                if (value is hclBufferSetupObject castValue)
                {
                    instance.m_outputBufferSetup = castValue;
                    return true;
                }
                return false;
            }
            case "m_vertexSelection":
            case "vertexSelection":
            {
                if (value is not hclVertexSelectionInput castValue) return false;
                instance.m_vertexSelection = castValue;
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
            case "m_useDualQuaternionMethod":
            case "useDualQuaternionMethod":
            {
                if (value is not bool castValue) return false;
                instance.m_useDualQuaternionMethod = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
