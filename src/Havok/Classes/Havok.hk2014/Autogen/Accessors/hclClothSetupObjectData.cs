// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclClothSetupObjectData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclClothSetupObject>
{
    public hclClothSetupObjectData(Havok.hk2014.hclClothSetupObject instance) : base(instance) { }

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
            case "m_bufferSetupObjects":
            case "bufferSetupObjects":
            {
                if (value is not List<Havok.hk2014.hclBufferSetupObject?> castValue) return false;
                instance.m_bufferSetupObjects = castValue;
                return true;
            }
            case "m_transformSetSetupObjects":
            case "transformSetSetupObjects":
            {
                if (value is not List<Havok.hk2014.hclTransformSetSetupObject?> castValue) return false;
                instance.m_transformSetSetupObjects = castValue;
                return true;
            }
            case "m_simClothSetupObjects":
            case "simClothSetupObjects":
            {
                if (value is not List<Havok.hk2014.hclSimClothSetupObject?> castValue) return false;
                instance.m_simClothSetupObjects = castValue;
                return true;
            }
            case "m_operatorSetupObjects":
            case "operatorSetupObjects":
            {
                if (value is not List<Havok.hk2014.hclOperatorSetupObject?> castValue) return false;
                instance.m_operatorSetupObjects = castValue;
                return true;
            }
            case "m_clothStateSetupObjects":
            case "clothStateSetupObjects":
            {
                if (value is not List<Havok.hk2014.hclClothStateSetupObject?> castValue) return false;
                instance.m_clothStateSetupObjects = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
