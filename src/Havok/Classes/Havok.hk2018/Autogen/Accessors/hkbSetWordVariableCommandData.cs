// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbSetWordVariableCommandData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbSetWordVariableCommand>
{
    public hkbSetWordVariableCommandData(Havok.hk2018.hkbSetWordVariableCommand instance) : base(instance) { }

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
            case "m_characterId":
            case "characterId":
            {
                value = instance.m_characterId;
                return true;
            }
            case "m_variableName":
            case "variableName":
            {
                value = instance.m_variableName;
                return true;
            }
            case "m_value":
            case "value":
            {
                value = instance.m_value;
                return true;
            }
            case "m_quadValue":
            case "quadValue":
            {
                value = instance.m_quadValue;
                return true;
            }
            case "m_type":
            case "type":
            {
                value = instance.m_type;
                return true;
            }
            case "m_global":
            case "global":
            {
                value = instance.m_global;
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
            case "m_characterId":
            case "characterId":
            {
                if (value is not ulong castValue) return false;
                instance.m_characterId = castValue;
                return true;
            }
            case "m_variableName":
            case "variableName":
            {
                if (value is null)
                {
                    instance.m_variableName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_variableName = castValue;
                    return true;
                }
                return false;
            }
            case "m_value":
            case "value":
            {
                if (value is not Havok.hk2018.hkbVariableValue castValue) return false;
                instance.m_value = castValue;
                return true;
            }
            case "m_quadValue":
            case "quadValue":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_quadValue = castValue;
                return true;
            }
            case "m_type":
            case "type":
            {
                if (value is Havok.hk2018.hkbVariableInfo.VariableType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_type = (Havok.hk2018.hkbVariableInfo.VariableType)byteValue;
                    return true;
                }
                return false;
            }
            case "m_global":
            case "global":
            {
                if (value is not bool castValue) return false;
                instance.m_global = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
