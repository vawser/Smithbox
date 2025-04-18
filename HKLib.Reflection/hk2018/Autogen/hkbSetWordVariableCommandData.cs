// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbSetWordVariableCommandData : HavokData<hkbSetWordVariableCommand> 
{
    public hkbSetWordVariableCommandData(HavokType type, hkbSetWordVariableCommand instance) : base(type, instance) {}

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
            case "m_characterId":
            case "characterId":
            {
                if (instance.m_characterId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_variableName":
            case "variableName":
            {
                if (instance.m_variableName is null)
                {
                    return true;
                }
                if (instance.m_variableName is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_value":
            case "value":
            {
                if (instance.m_value is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_quadValue":
            case "quadValue":
            {
                if (instance.m_quadValue is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_type":
            case "type":
            {
                if (instance.m_type is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_type is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_global":
            case "global":
            {
                if (instance.m_global is not TGet castValue) return false;
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
                if (value is not hkbVariableValue castValue) return false;
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
                if (value is hkbVariableInfo.VariableType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_type = (hkbVariableInfo.VariableType)byteValue;
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
