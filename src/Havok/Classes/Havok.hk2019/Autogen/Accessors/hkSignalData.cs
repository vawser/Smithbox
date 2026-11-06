// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkSignalData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkSignal>
{
    public hkSignalData(Havok.hk2019.hkSignal instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_slots":
            case "slots":
            {
                value = instance.m_slots;
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
            case "m_slots":
            case "slots":
            {
                if (value is null)
                {
                    instance.m_slots = default;
                    return true;
                }
                if (value is object castValue)
                {
                    instance.m_slots = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
