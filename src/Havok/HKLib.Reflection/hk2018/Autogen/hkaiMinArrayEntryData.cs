// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiMinArrayEntryData<T> : HavokData<hkaiMinArrayEntry<T>> 
{
    public hkaiMinArrayEntryData(HavokType type, hkaiMinArrayEntry<T> instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_item":
            case "item":
            {
                if (instance.m_item is null)
                {
                    return true;
                }
                if (instance.m_item is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_score":
            case "score":
            {
                if (instance.m_score is not TGet castValue) return false;
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
            case "m_item":
            case "item":
            {
                if (value is null)
                {
                    instance.m_item = default;
                    return true;
                }
                if (value is T castValue)
                {
                    instance.m_item = castValue;
                    return true;
                }
                return false;
            }
            case "m_score":
            case "score":
            {
                if (value is not float castValue) return false;
                instance.m_score = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
