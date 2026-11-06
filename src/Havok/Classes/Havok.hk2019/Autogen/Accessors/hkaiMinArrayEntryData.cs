// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiMinArrayEntryData<T> : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiMinArrayEntry<T>>
{
    public hkaiMinArrayEntryData(Havok.hk2019.hkaiMinArrayEntry<T> instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_item":
            case "item":
            {
                value = instance.m_item;
                return true;
            }
            case "m_score":
            case "score":
            {
                value = instance.m_score;
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
