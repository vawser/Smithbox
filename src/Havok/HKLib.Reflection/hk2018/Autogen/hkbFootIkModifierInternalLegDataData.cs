// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbFootIkModifierInternalLegDataData : HavokData<hkbFootIkModifier.InternalLegData> 
{
    public hkbFootIkModifierInternalLegDataData(HavokType type, hkbFootIkModifier.InternalLegData instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_groundPosition":
            case "groundPosition":
            {
                if (instance.m_groundPosition is not TGet castValue) return false;
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
            case "m_groundPosition":
            case "groundPosition":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_groundPosition = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
