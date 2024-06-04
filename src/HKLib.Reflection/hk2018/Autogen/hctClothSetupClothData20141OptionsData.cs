// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hctClothSetupClothData20141OptionsData : HavokData<hctClothSetupClothData20141Options> 
{
    public hctClothSetupClothData20141OptionsData(HavokType type, hctClothSetupClothData20141Options instance) : base(type, instance) {}

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
            case "m_currentState":
            case "currentState":
            {
                if (instance.m_currentState is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_defaultState":
            case "defaultState":
            {
                if (instance.m_defaultState is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_setupMeshes":
            case "setupMeshes":
            {
                if (instance.m_setupMeshes is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_simSetupMeshes":
            case "simSetupMeshes":
            {
                if (instance.m_simSetupMeshes is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_buffers":
            case "buffers":
            {
                if (instance.m_buffers is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_simCloth":
            case "simCloth":
            {
                if (instance.m_simCloth is not TGet castValue) return false;
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
            case "m_transformSets":
            case "transformSets":
            {
                if (instance.m_transformSets is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_simClothConstraints":
            case "simClothConstraints":
            {
                if (instance.m_simClothConstraints is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_clothStates":
            case "clothStates":
            {
                if (instance.m_clothStates is not TGet castValue) return false;
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
            case "m_currentState":
            case "currentState":
            {
                if (value is not int castValue) return false;
                instance.m_currentState = castValue;
                return true;
            }
            case "m_defaultState":
            case "defaultState":
            {
                if (value is not int castValue) return false;
                instance.m_defaultState = castValue;
                return true;
            }
            case "m_setupMeshes":
            case "setupMeshes":
            {
                if (value is not List<hctClothSetupObjectData?> castValue) return false;
                instance.m_setupMeshes = castValue;
                return true;
            }
            case "m_simSetupMeshes":
            case "simSetupMeshes":
            {
                if (value is not List<hctClothSetupObjectData?> castValue) return false;
                instance.m_simSetupMeshes = castValue;
                return true;
            }
            case "m_buffers":
            case "buffers":
            {
                if (value is not List<hctClothSetupObjectData?> castValue) return false;
                instance.m_buffers = castValue;
                return true;
            }
            case "m_simCloth":
            case "simCloth":
            {
                if (value is not List<hctClothSetupObjectData?> castValue) return false;
                instance.m_simCloth = castValue;
                return true;
            }
            case "m_operators":
            case "operators":
            {
                if (value is not List<hctClothSetupObjectData?> castValue) return false;
                instance.m_operators = castValue;
                return true;
            }
            case "m_transformSets":
            case "transformSets":
            {
                if (value is not List<hctClothSetupObjectData?> castValue) return false;
                instance.m_transformSets = castValue;
                return true;
            }
            case "m_simClothConstraints":
            case "simClothConstraints":
            {
                if (value is not List<hctClothSetupObjectData?> castValue) return false;
                instance.m_simClothConstraints = castValue;
                return true;
            }
            case "m_clothStates":
            case "clothStates":
            {
                if (value is not List<hctClothSetupObjectData?> castValue) return false;
                instance.m_clothStates = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
