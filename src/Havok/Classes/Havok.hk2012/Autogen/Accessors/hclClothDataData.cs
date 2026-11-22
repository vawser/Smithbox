// Automatically Generated

#nullable enable

namespace Havok.hk2012.Accessors;

internal sealed class hclClothDataData : Havok.Formats.HavokFieldAccess<Havok.hk2012.hclClothData>
{
    public hclClothDataData(Havok.hk2012.hclClothData instance) : base(instance) { }

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
            case "m_simClothDatas":
            case "simClothDatas":
            {
                value = instance.m_simClothDatas;
                return true;
            }
            case "m_bufferDefinitions":
            case "bufferDefinitions":
            {
                value = instance.m_bufferDefinitions;
                return true;
            }
            case "m_transformSetDefinitions":
            case "transformSetDefinitions":
            {
                value = instance.m_transformSetDefinitions;
                return true;
            }
            case "m_operators":
            case "operators":
            {
                value = instance.m_operators;
                return true;
            }
            case "m_clothStateDatas":
            case "clothStateDatas":
            {
                value = instance.m_clothStateDatas;
                return true;
            }
            case "m_actions":
            case "actions":
            {
                value = instance.m_actions;
                return true;
            }
            case "m_targetPlatform":
            case "targetPlatform":
            {
                value = instance.m_targetPlatform;
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
            case "m_simClothDatas":
            case "simClothDatas":
            {
                if (value is not List<Havok.hk2012.hclSimClothData?> castValue) return false;
                instance.m_simClothDatas = castValue;
                return true;
            }
            case "m_bufferDefinitions":
            case "bufferDefinitions":
            {
                if (value is not List<Havok.hk2012.hclBufferDefinition?> castValue) return false;
                instance.m_bufferDefinitions = castValue;
                return true;
            }
            case "m_transformSetDefinitions":
            case "transformSetDefinitions":
            {
                if (value is not List<Havok.hk2012.hclTransformSetDefinition?> castValue) return false;
                instance.m_transformSetDefinitions = castValue;
                return true;
            }
            case "m_operators":
            case "operators":
            {
                if (value is not List<Havok.hk2012.hclOperator?> castValue) return false;
                instance.m_operators = castValue;
                return true;
            }
            case "m_clothStateDatas":
            case "clothStateDatas":
            {
                if (value is not List<Havok.hk2012.hclClothState?> castValue) return false;
                instance.m_clothStateDatas = castValue;
                return true;
            }
            case "m_actions":
            case "actions":
            {
                if (value is not List<Havok.hk2012.hclAction?> castValue) return false;
                instance.m_actions = castValue;
                return true;
            }
            case "m_targetPlatform":
            case "targetPlatform":
            {
                if (value is Havok.hk2012.hclClothData.Platform castValue)
                {
                    instance.m_targetPlatform = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_targetPlatform = (Havok.hk2012.hclClothData.Platform)uintValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
