// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbSetNodePropertyCommandData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbSetNodePropertyCommand>
{
    public hkbSetNodePropertyCommandData(Havok.hk2014.hkbSetNodePropertyCommand instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_characterId":
            case "characterId":
            {
                value = instance.m_characterId;
                return true;
            }
            case "m_nodeName":
            case "nodeName":
            {
                value = instance.m_nodeName;
                return true;
            }
            case "m_propertyName":
            case "propertyName":
            {
                value = instance.m_propertyName;
                return true;
            }
            case "m_propertyValue":
            case "propertyValue":
            {
                value = instance.m_propertyValue;
                return true;
            }
            case "m_padding":
            case "padding":
            {
                value = instance.m_padding;
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
                if (value is not Havok.hk2014.hkbVariableValue castValue) return false;
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
