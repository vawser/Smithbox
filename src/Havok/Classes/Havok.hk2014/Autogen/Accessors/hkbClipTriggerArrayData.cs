// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbClipTriggerArrayData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbClipTriggerArray>
{
    public hkbClipTriggerArrayData(Havok.hk2014.hkbClipTriggerArray instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_triggers":
            case "triggers":
            {
                value = instance.m_triggers;
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
            case "m_triggers":
            case "triggers":
            {
                if (value is not List<Havok.hk2014.hkbClipTrigger> castValue) return false;
                instance.m_triggers = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
