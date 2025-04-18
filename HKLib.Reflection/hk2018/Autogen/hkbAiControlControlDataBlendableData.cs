// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbAiControlControlDataBlendableData : HavokData<hkbAiControlControlDataBlendable> 
{
    public hkbAiControlControlDataBlendableData(HavokType type, hkbAiControlControlDataBlendable instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_desiredSpeed":
            case "desiredSpeed":
            {
                if (instance.m_desiredSpeed is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maximumSpeed":
            case "maximumSpeed":
            {
                if (instance.m_maximumSpeed is not TGet castValue) return false;
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
            case "m_desiredSpeed":
            case "desiredSpeed":
            {
                if (value is not float castValue) return false;
                instance.m_desiredSpeed = castValue;
                return true;
            }
            case "m_maximumSpeed":
            case "maximumSpeed":
            {
                if (value is not float castValue) return false;
                instance.m_maximumSpeed = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
