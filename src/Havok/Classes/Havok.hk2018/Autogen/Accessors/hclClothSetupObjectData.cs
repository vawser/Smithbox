// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclClothSetupObjectData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclClothSetupObject>
{
    public hclClothSetupObjectData(Havok.hk2018.hclClothSetupObject instance) : base(instance) { }

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
            case "m_bufferSetupObjects":
            case "bufferSetupObjects":
            {
                value = instance.m_bufferSetupObjects;
                return true;
            }
            case "m_transformSetSetupObjects":
            case "transformSetSetupObjects":
            {
                value = instance.m_transformSetSetupObjects;
                return true;
            }
            case "m_simClothSetupObjects":
            case "simClothSetupObjects":
            {
                value = instance.m_simClothSetupObjects;
                return true;
            }
            case "m_operatorSetupObjects":
            case "operatorSetupObjects":
            {
                value = instance.m_operatorSetupObjects;
                return true;
            }
            case "m_clothStateSetupObjects":
            case "clothStateSetupObjects":
            {
                value = instance.m_clothStateSetupObjects;
                return true;
            }
            case "m_stateTransitionSetupObjects":
            case "stateTransitionSetupObjects":
            {
                value = instance.m_stateTransitionSetupObjects;
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
            case "m_bufferSetupObjects":
            case "bufferSetupObjects":
            {
                if (value is not List<Havok.hk2018.hclBufferSetupObject?> castValue) return false;
                instance.m_bufferSetupObjects = castValue;
                return true;
            }
            case "m_transformSetSetupObjects":
            case "transformSetSetupObjects":
            {
                if (value is not List<Havok.hk2018.hclTransformSetSetupObject?> castValue) return false;
                instance.m_transformSetSetupObjects = castValue;
                return true;
            }
            case "m_simClothSetupObjects":
            case "simClothSetupObjects":
            {
                if (value is not List<Havok.hk2018.hclSimClothSetupObject?> castValue) return false;
                instance.m_simClothSetupObjects = castValue;
                return true;
            }
            case "m_operatorSetupObjects":
            case "operatorSetupObjects":
            {
                if (value is not List<Havok.hk2018.hclOperatorSetupObject?> castValue) return false;
                instance.m_operatorSetupObjects = castValue;
                return true;
            }
            case "m_clothStateSetupObjects":
            case "clothStateSetupObjects":
            {
                if (value is not List<Havok.hk2018.hclClothStateSetupObject?> castValue) return false;
                instance.m_clothStateSetupObjects = castValue;
                return true;
            }
            case "m_stateTransitionSetupObjects":
            case "stateTransitionSetupObjects":
            {
                if (value is not List<Havok.hk2018.hclStateTransitionSetupObject?> castValue) return false;
                instance.m_stateTransitionSetupObjects = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
