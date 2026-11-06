// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclToolNamedObjectReferenceData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclToolNamedObjectReference>
{
    public hclToolNamedObjectReferenceData(Havok.hk2018.hclToolNamedObjectReference instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_pluginName":
            case "pluginName":
            {
                value = instance.m_pluginName;
                return true;
            }
            case "m_objectName":
            case "objectName":
            {
                value = instance.m_objectName;
                return true;
            }
            case "m_hash":
            case "hash":
            {
                value = instance.m_hash;
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
            case "m_pluginName":
            case "pluginName":
            {
                if (value is null)
                {
                    instance.m_pluginName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_pluginName = castValue;
                    return true;
                }
                return false;
            }
            case "m_objectName":
            case "objectName":
            {
                if (value is null)
                {
                    instance.m_objectName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_objectName = castValue;
                    return true;
                }
                return false;
            }
            case "m_hash":
            case "hash":
            {
                if (value is not uint castValue) return false;
                instance.m_hash = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
