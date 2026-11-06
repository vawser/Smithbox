// Automatically Generated

#nullable enable

namespace Havok.hk2010.Accessors;

internal sealed class hkpEntitySpuCollisionCallbackData : Havok.Formats.HavokFieldAccess<Havok.hk2010.hkpEntitySpuCollisionCallback>
{
    public hkpEntitySpuCollisionCallbackData(Havok.hk2010.hkpEntitySpuCollisionCallback instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_eventFilter":
            case "eventFilter":
            {
                value = instance.m_eventFilter;
                return true;
            }
            case "m_userFilter":
            case "userFilter":
            {
                value = instance.m_userFilter;
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
            case "m_eventFilter":
            case "eventFilter":
            {
                if (value is not byte castValue) return false;
                instance.m_eventFilter = castValue;
                return true;
            }
            case "m_userFilter":
            case "userFilter":
            {
                if (value is not byte castValue) return false;
                instance.m_userFilter = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
