// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiDefaultAstarCostModifierData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiDefaultAstarCostModifier>
{
    private static readonly System.Reflection.FieldInfo _costMultiplierLookupTableInfo = typeof(Havok.hk2019.hkaiDefaultAstarCostModifier).GetField("m_costMultiplierLookupTable")!;
    public hkaiDefaultAstarCostModifierData(Havok.hk2019.hkaiDefaultAstarCostModifier instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_maxCostPenalty":
            case "maxCostPenalty":
            {
                value = instance.m_maxCostPenalty;
                return true;
            }
            case "m_costMultiplierLookupTable":
            case "costMultiplierLookupTable":
            {
                value = instance.m_costMultiplierLookupTable;
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
