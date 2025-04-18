// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbAiControlControlDataNonBlendableData : HavokData<hkbAiControlControlDataNonBlendable> 
{
    public hkbAiControlControlDataNonBlendableData(HavokType type, hkbAiControlControlDataNonBlendable instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_canControl":
            case "canControl":
            {
                if (instance.m_canControl is not TGet castValue) return false;
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
            case "m_canControl":
            case "canControl":
            {
                if (value is not bool castValue) return false;
                instance.m_canControl = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
