// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiDefaultAstarCostModifierData : HavokData<HKLib.hk2019.hkaiDefaultAstarCostModifier> 
{
    private static readonly System.Reflection.FieldInfo _costMultiplierLookupTableInfo = typeof(HKLib.hk2019.hkaiDefaultAstarCostModifier).GetField("m_costMultiplierLookupTable")!;
    public hkaiDefaultAstarCostModifierData(HavokType type, HKLib.hk2019.hkaiDefaultAstarCostModifier instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_maxCostPenalty":
            case "maxCostPenalty":
            {
                if (instance.m_maxCostPenalty is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_costMultiplierLookupTable":
            case "costMultiplierLookupTable":
            {
                if (instance.m_costMultiplierLookupTable is not TGet castValue) return false;
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
            case "m_maxCostPenalty":
            case "maxCostPenalty":
            {
                if (value is not float castValue) return false;
                instance.m_maxCostPenalty = castValue;
                return true;
            }
            case "m_costMultiplierLookupTable":
            case "costMultiplierLookupTable":
            {
                if (value is not float[] castValue || castValue.Length != 32) return false;
                try
                {
                    _costMultiplierLookupTableInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            default:
            return false;
        }
    }

}
