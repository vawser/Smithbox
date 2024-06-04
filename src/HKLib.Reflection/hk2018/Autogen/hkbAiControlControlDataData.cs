// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbAiControlControlDataData : HavokData<hkbAiControlControlData> 
{
    public hkbAiControlControlDataData(HavokType type, hkbAiControlControlData instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_blendable":
            case "blendable":
            {
                if (instance.m_blendable is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nonBlendable":
            case "nonBlendable":
            {
                if (instance.m_nonBlendable is not TGet castValue) return false;
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
            case "m_blendable":
            case "blendable":
            {
                if (value is not hkbAiControlControlDataBlendable castValue) return false;
                instance.m_blendable = castValue;
                return true;
            }
            case "m_nonBlendable":
            case "nonBlendable":
            {
                if (value is not hkbAiControlControlDataNonBlendable castValue) return false;
                instance.m_nonBlendable = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
