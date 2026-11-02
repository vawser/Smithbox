// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkSignalData : HavokData<HKLib.hk2019.hkSignal> 
{
    public hkSignalData(HavokType type, HKLib.hk2019.hkSignal instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_slots":
            case "slots":
            {
                if (instance.m_slots is null)
                {
                    return true;
                }
                if (instance.m_slots is TGet castValue)
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
