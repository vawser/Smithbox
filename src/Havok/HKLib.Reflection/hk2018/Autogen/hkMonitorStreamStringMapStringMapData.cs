// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkMonitorStreamStringMapStringMapData : HavokData<hkMonitorStreamStringMap.StringMap> 
{
    public hkMonitorStreamStringMapStringMapData(HavokType type, hkMonitorStreamStringMap.StringMap instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_id":
            case "id":
            {
                if (instance.m_id is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_string":
            case "string":
            {
                if (instance.m_string is null)
                {
                    return true;
                }
                if (instance.m_string is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_id":
            case "id":
            {
                if (value is not ulong castValue) return false;
                instance.m_id = castValue;
                return true;
            }
            case "m_string":
            case "string":
            {
                if (value is null)
                {
                    instance.m_string = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_string = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
