// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkRefCountedPropertiesEntryData : HavokData<hkRefCountedProperties.Entry> 
{
    public hkRefCountedPropertiesEntryData(HavokType type, hkRefCountedProperties.Entry instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_object":
            case "object":
            {
                if (instance.m_object is null)
                {
                    return true;
                }
                if (instance.m_object is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_key":
            case "key":
            {
                if (instance.m_key is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (instance.m_flags is not TGet castValue) return false;
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
            case "m_object":
            case "object":
            {
                if (value is null)
                {
                    instance.m_object = default;
                    return true;
                }
                if (value is hkReferencedObject castValue)
                {
                    instance.m_object = castValue;
                    return true;
                }
                return false;
            }
            case "m_key":
            case "key":
            {
                if (value is not ushort castValue) return false;
                instance.m_key = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (value is not ushort castValue) return false;
                instance.m_flags = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
