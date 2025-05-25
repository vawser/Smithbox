// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclBlendSetupObjectData : HavokData<hclBlendSetupObject> 
{
    public hclBlendSetupObjectData(HavokType type, hclBlendSetupObject instance) : base(type, instance) {}

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
            case "m_A":
            case "A":
            {
                if (instance.m_A is null)
                {
                    return true;
                }
                if (instance.m_A is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_B":
            case "B":
            {
                if (instance.m_B is null)
                {
                    return true;
                }
                if (instance.m_B is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_C":
            case "C":
            {
                if (instance.m_C is null)
                {
                    return true;
                }
                if (instance.m_C is TGet castValue)
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
            case "m_blendWeights":
            case "blendWeights":
            {
                if (instance.m_blendWeights is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_dynamicBlendDefaultWeight":
            case "dynamicBlendDefaultWeight":
            {
                if (instance.m_dynamicBlendDefaultWeight is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_dynamicBlendTransitionPeriod":
            case "dynamicBlendTransitionPeriod":
            {
                if (instance.m_dynamicBlendTransitionPeriod is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_mapToScurve":
            case "mapToScurve":
            {
                if (instance.m_mapToScurve is not TGet castValue) return false;
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
            case "m_A":
            case "A":
            {
                if (value is null)
                {
                    instance.m_A = default;
                    return true;
                }
                if (value is hclBufferSetupObject castValue)
                {
                    instance.m_A = castValue;
                    return true;
                }
                return false;
            }
            case "m_B":
            case "B":
            {
                if (value is null)
                {
                    instance.m_B = default;
                    return true;
                }
                if (value is hclBufferSetupObject castValue)
                {
                    instance.m_B = castValue;
                    return true;
                }
                return false;
            }
            case "m_C":
            case "C":
            {
                if (value is null)
                {
                    instance.m_C = default;
                    return true;
                }
                if (value is hclBufferSetupObject castValue)
                {
                    instance.m_C = castValue;
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
            case "m_blendWeights":
            case "blendWeights":
            {
                if (value is not hclVertexFloatInput castValue) return false;
                instance.m_blendWeights = castValue;
                return true;
            }
            case "m_dynamicBlendDefaultWeight":
            case "dynamicBlendDefaultWeight":
            {
                if (value is not float castValue) return false;
                instance.m_dynamicBlendDefaultWeight = castValue;
                return true;
            }
            case "m_dynamicBlendTransitionPeriod":
            case "dynamicBlendTransitionPeriod":
            {
                if (value is not float castValue) return false;
                instance.m_dynamicBlendTransitionPeriod = castValue;
                return true;
            }
            case "m_mapToScurve":
            case "mapToScurve":
            {
                if (value is not bool castValue) return false;
                instance.m_mapToScurve = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
