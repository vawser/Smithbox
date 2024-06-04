// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbSetNodePropertyCommandData : HavokData<hkbSetNodePropertyCommand> 
{
    public hkbSetNodePropertyCommandData(HavokType type, hkbSetNodePropertyCommand instance) : base(type, instance) {}

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
            case "m_nodeName":
            case "nodeName":
            {
                if (instance.m_nodeName is null)
                {
                    return true;
                }
                if (instance.m_nodeName is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_propertyName":
            case "propertyName":
            {
                if (instance.m_propertyName is null)
                {
                    return true;
                }
                if (instance.m_propertyName is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_propertyValue":
            case "propertyValue":
            {
                if (instance.m_propertyValue is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_padding":
            case "padding":
            {
                if (instance.m_padding is not TGet castValue) return false;
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
            case "m_nodeName":
            case "nodeName":
            {
                if (value is null)
                {
                    instance.m_nodeName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_nodeName = castValue;
                    return true;
                }
                return false;
            }
            case "m_propertyName":
            case "propertyName":
            {
                if (value is null)
                {
                    instance.m_propertyName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_propertyName = castValue;
                    return true;
                }
                return false;
            }
            case "m_propertyValue":
            case "propertyValue":
            {
                if (value is not hkbVariableValue castValue) return false;
                instance.m_propertyValue = castValue;
                return true;
            }
            case "m_padding":
            case "padding":
            {
                if (value is not int castValue) return false;
                instance.m_padding = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
