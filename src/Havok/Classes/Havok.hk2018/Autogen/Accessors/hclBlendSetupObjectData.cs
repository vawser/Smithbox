// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclBlendSetupObjectData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclBlendSetupObject>
{
    public hclBlendSetupObjectData(Havok.hk2018.hclBlendSetupObject instance) : base(instance) { }

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
            case "m_A":
            case "A":
            {
                value = instance.m_A;
                return true;
            }
            case "m_B":
            case "B":
            {
                value = instance.m_B;
                return true;
            }
            case "m_C":
            case "C":
            {
                value = instance.m_C;
                return true;
            }
            case "m_vertexSelection":
            case "vertexSelection":
            {
                value = instance.m_vertexSelection;
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
            case "m_blendWeights":
            case "blendWeights":
            {
                value = instance.m_blendWeights;
                return true;
            }
            case "m_dynamicBlendDefaultWeight":
            case "dynamicBlendDefaultWeight":
            {
                value = instance.m_dynamicBlendDefaultWeight;
                return true;
            }
            case "m_dynamicBlendTransitionPeriod":
            case "dynamicBlendTransitionPeriod":
            {
                value = instance.m_dynamicBlendTransitionPeriod;
                return true;
            }
            case "m_mapToScurve":
            case "mapToScurve":
            {
                value = instance.m_mapToScurve;
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
            case "m_A":
            case "A":
            {
                if (value is null)
                {
                    instance.m_A = default;
                    return true;
                }
                if (value is Havok.hk2018.hclBufferSetupObject castValue)
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
                if (value is Havok.hk2018.hclBufferSetupObject castValue)
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
                if (value is Havok.hk2018.hclBufferSetupObject castValue)
                {
                    instance.m_C = castValue;
                    return true;
                }
                return false;
            }
            case "m_vertexSelection":
            case "vertexSelection":
            {
                if (value is not Havok.hk2018.hclVertexSelectionInput castValue) return false;
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
                if (value is not Havok.hk2018.hclVertexFloatInput castValue) return false;
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
