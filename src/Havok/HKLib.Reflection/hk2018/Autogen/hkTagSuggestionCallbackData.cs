// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hk;

namespace HKLib.Reflection.hk2018;

internal class hkTagSuggestionCallbackData : HavokData<TagSuggestionCallback> 
{
    public hkTagSuggestionCallbackData(HavokType type, TagSuggestionCallback instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_cb":
            case "cb":
            {
                if (instance.m_cb is null)
                {
                    return true;
                }
                if (instance.m_cb is TGet castValue)
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
            case "m_cb":
            case "cb":
            {
                if (value is null)
                {
                    instance.m_cb = default;
                    return true;
                }
                if (value is object castValue)
                {
                    instance.m_cb = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
