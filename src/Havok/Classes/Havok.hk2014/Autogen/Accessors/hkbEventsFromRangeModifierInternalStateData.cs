// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbEventsFromRangeModifierInternalStateData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbEventsFromRangeModifierInternalState>
{
    public hkbEventsFromRangeModifierInternalStateData(Havok.hk2014.hkbEventsFromRangeModifierInternalState instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_wasActiveInPreviousFrame":
            case "wasActiveInPreviousFrame":
            {
                value = instance.m_wasActiveInPreviousFrame;
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
            case "m_wasActiveInPreviousFrame":
            case "wasActiveInPreviousFrame":
            {
                if (value is not List<bool> castValue) return false;
                instance.m_wasActiveInPreviousFrame = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
