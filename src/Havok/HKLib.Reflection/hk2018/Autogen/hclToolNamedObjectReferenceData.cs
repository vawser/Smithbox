// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclToolNamedObjectReferenceData : HavokData<hclToolNamedObjectReference> 
{
    public hclToolNamedObjectReferenceData(HavokType type, hclToolNamedObjectReference instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_pluginName":
            case "pluginName":
            {
                if (instance.m_pluginName is null)
                {
                    return true;
                }
                if (instance.m_pluginName is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_objectName":
            case "objectName":
            {
                if (instance.m_objectName is null)
                {
                    return true;
                }
                if (instance.m_objectName is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_hash":
            case "hash":
            {
                if (instance.m_hash is not TGet castValue) return false;
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
