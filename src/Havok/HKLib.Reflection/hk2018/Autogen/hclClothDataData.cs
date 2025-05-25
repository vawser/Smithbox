// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclClothDataData : HavokData<hclClothData> 
{
    public hclClothDataData(HavokType type, hclClothData instance) : base(type, instance) {}

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
            case "m_simClothDatas":
            case "simClothDatas":
            {
                if (instance.m_simClothDatas is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bufferDefinitions":
            case "bufferDefinitions":
            {
                if (instance.m_bufferDefinitions is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_transformSetDefinitions":
            case "transformSetDefinitions":
            {
                if (instance.m_transformSetDefinitions is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_operators":
            case "operators":
            {
                if (instance.m_operators is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_clothStateDatas":
            case "clothStateDatas":
            {
                if (instance.m_clothStateDatas is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_stateTransitions":
            case "stateTransitions":
            {
                if (instance.m_stateTransitions is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_actions":
            case "actions":
            {
                if (instance.m_actions is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_targetPlatform":
            case "targetPlatform":
            {
                if (instance.m_targetPlatform is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((uint)instance.m_targetPlatform is TGet uintValue)
                {
                    value = uintValue;
                    return true;
                }
                return false;
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
            case "m_simClothDatas":
            case "simClothDatas":
            {
                if (value is not List<hclSimClothData?> castValue) return false;
                instance.m_simClothDatas = castValue;
                return true;
            }
            case "m_bufferDefinitions":
            case "bufferDefinitions":
            {
                if (value is not List<hclBufferDefinition?> castValue) return false;
                instance.m_bufferDefinitions = castValue;
                return true;
            }
            case "m_transformSetDefinitions":
            case "transformSetDefinitions":
            {
                if (value is not List<hclTransformSetDefinition?> castValue) return false;
                instance.m_transformSetDefinitions = castValue;
                return true;
            }
            case "m_operators":
            case "operators":
            {
                if (value is not List<hclOperator?> castValue) return false;
                instance.m_operators = castValue;
                return true;
            }
            case "m_clothStateDatas":
            case "clothStateDatas":
            {
                if (value is not List<hclClothState?> castValue) return false;
                instance.m_clothStateDatas = castValue;
                return true;
            }
            case "m_stateTransitions":
            case "stateTransitions":
            {
                if (value is not List<hclStateTransition?> castValue) return false;
                instance.m_stateTransitions = castValue;
                return true;
            }
            case "m_actions":
            case "actions":
            {
                if (value is not List<hclAction?> castValue) return false;
                instance.m_actions = castValue;
                return true;
            }
            case "m_targetPlatform":
            case "targetPlatform":
            {
                if (value is hclClothData.Platform castValue)
                {
                    instance.m_targetPlatform = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_targetPlatform = (hclClothData.Platform)uintValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
