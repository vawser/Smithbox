// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclClothSetupObjectData : HavokData<hclClothSetupObject> 
{
    public hclClothSetupObjectData(HavokType type, hclClothSetupObject instance) : base(type, instance) {}

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
            case "m_bufferSetupObjects":
            case "bufferSetupObjects":
            {
                if (instance.m_bufferSetupObjects is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_transformSetSetupObjects":
            case "transformSetSetupObjects":
            {
                if (instance.m_transformSetSetupObjects is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_simClothSetupObjects":
            case "simClothSetupObjects":
            {
                if (instance.m_simClothSetupObjects is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_operatorSetupObjects":
            case "operatorSetupObjects":
            {
                if (instance.m_operatorSetupObjects is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_clothStateSetupObjects":
            case "clothStateSetupObjects":
            {
                if (instance.m_clothStateSetupObjects is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_stateTransitionSetupObjects":
            case "stateTransitionSetupObjects":
            {
                if (instance.m_stateTransitionSetupObjects is not TGet castValue) return false;
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
            case "m_bufferSetupObjects":
            case "bufferSetupObjects":
            {
                if (value is not List<hclBufferSetupObject?> castValue) return false;
                instance.m_bufferSetupObjects = castValue;
                return true;
            }
            case "m_transformSetSetupObjects":
            case "transformSetSetupObjects":
            {
                if (value is not List<hclTransformSetSetupObject?> castValue) return false;
                instance.m_transformSetSetupObjects = castValue;
                return true;
            }
            case "m_simClothSetupObjects":
            case "simClothSetupObjects":
            {
                if (value is not List<hclSimClothSetupObject?> castValue) return false;
                instance.m_simClothSetupObjects = castValue;
                return true;
            }
            case "m_operatorSetupObjects":
            case "operatorSetupObjects":
            {
                if (value is not List<hclOperatorSetupObject?> castValue) return false;
                instance.m_operatorSetupObjects = castValue;
                return true;
            }
            case "m_clothStateSetupObjects":
            case "clothStateSetupObjects":
            {
                if (value is not List<hclClothStateSetupObject?> castValue) return false;
                instance.m_clothStateSetupObjects = castValue;
                return true;
            }
            case "m_stateTransitionSetupObjects":
            case "stateTransitionSetupObjects":
            {
                if (value is not List<hclStateTransitionSetupObject?> castValue) return false;
                instance.m_stateTransitionSetupObjects = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
