// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hctClothSetupClothData20141OptionsData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hctClothSetupClothData20141Options>
{
    public hctClothSetupClothData20141OptionsData(Havok.hk2018.hctClothSetupClothData20141Options instance) : base(instance) { }

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
            case "m_currentState":
            case "currentState":
            {
                value = instance.m_currentState;
                return true;
            }
            case "m_defaultState":
            case "defaultState":
            {
                value = instance.m_defaultState;
                return true;
            }
            case "m_setupMeshes":
            case "setupMeshes":
            {
                value = instance.m_setupMeshes;
                return true;
            }
            case "m_simSetupMeshes":
            case "simSetupMeshes":
            {
                value = instance.m_simSetupMeshes;
                return true;
            }
            case "m_buffers":
            case "buffers":
            {
                value = instance.m_buffers;
                return true;
            }
            case "m_simCloth":
            case "simCloth":
            {
                value = instance.m_simCloth;
                return true;
            }
            case "m_operators":
            case "operators":
            {
                value = instance.m_operators;
                return true;
            }
            case "m_transformSets":
            case "transformSets":
            {
                value = instance.m_transformSets;
                return true;
            }
            case "m_simClothConstraints":
            case "simClothConstraints":
            {
                value = instance.m_simClothConstraints;
                return true;
            }
            case "m_clothStates":
            case "clothStates":
            {
                value = instance.m_clothStates;
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
                if (value is not List<Havok.hk2018.hctClothSetupObjectData?> castValue) return false;
                instance.m_setupMeshes = castValue;
                return true;
            }
            case "m_simSetupMeshes":
            case "simSetupMeshes":
            {
                if (value is not List<Havok.hk2018.hctClothSetupObjectData?> castValue) return false;
                instance.m_simSetupMeshes = castValue;
                return true;
            }
            case "m_buffers":
            case "buffers":
            {
                if (value is not List<Havok.hk2018.hctClothSetupObjectData?> castValue) return false;
                instance.m_buffers = castValue;
                return true;
            }
            case "m_simCloth":
            case "simCloth":
            {
                if (value is not List<Havok.hk2018.hctClothSetupObjectData?> castValue) return false;
                instance.m_simCloth = castValue;
                return true;
            }
            case "m_operators":
            case "operators":
            {
                if (value is not List<Havok.hk2018.hctClothSetupObjectData?> castValue) return false;
                instance.m_operators = castValue;
                return true;
            }
            case "m_transformSets":
            case "transformSets":
            {
                if (value is not List<Havok.hk2018.hctClothSetupObjectData?> castValue) return false;
                instance.m_transformSets = castValue;
                return true;
            }
            case "m_simClothConstraints":
            case "simClothConstraints":
            {
                if (value is not List<Havok.hk2018.hctClothSetupObjectData?> castValue) return false;
                instance.m_simClothConstraints = castValue;
                return true;
            }
            case "m_clothStates":
            case "clothStates":
            {
                if (value is not List<Havok.hk2018.hctClothSetupObjectData?> castValue) return false;
                instance.m_clothStates = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
