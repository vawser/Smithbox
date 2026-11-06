// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclBlendSetupObjectData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclBlendSetupObject>
{
    public hclBlendSetupObjectData(Havok.hk2014.hclBlendSetupObject instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_blendWeights":
            case "blendWeights":
            {
                value = instance.m_blendWeights;
                return true;
            }
            case "m_mapToScurve":
            case "mapToScurve":
            {
                value = instance.m_mapToScurve;
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
            default:
            return false;
        }
    }

    public override bool TrySetField(string fieldName, object? value)
    {
        switch (fieldName)
        {
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
                if (value is Havok.hk2014.hclBufferSetupObject castValue)
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
                if (value is Havok.hk2014.hclBufferSetupObject castValue)
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
                if (value is Havok.hk2014.hclBufferSetupObject castValue)
                {
                    instance.m_C = castValue;
                    return true;
                }
                return false;
            }
            case "m_vertexSelection":
            case "vertexSelection":
            {
                if (value is not Havok.hk2014.hclVertexSelectionInput castValue) return false;
                instance.m_vertexSelection = castValue;
                return true;
            }
            case "m_blendWeights":
            case "blendWeights":
            {
                if (value is not Havok.hk2014.hclVertexFloatInput castValue) return false;
                instance.m_blendWeights = castValue;
                return true;
            }
            case "m_mapToScurve":
            case "mapToScurve":
            {
                if (value is not bool castValue) return false;
                instance.m_mapToScurve = castValue;
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
            default:
            return false;
        }
    }

}
