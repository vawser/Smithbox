// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiDefaultAstarCostModifierData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiDefaultAstarCostModifier>
{
    private static readonly System.Reflection.FieldInfo _costMultiplierLookupTableInfo = typeof(Havok.hk2014.hkaiDefaultAstarCostModifier).GetField("m_costMultiplierLookupTable")!;
    public hkaiDefaultAstarCostModifierData(Havok.hk2014.hkaiDefaultAstarCostModifier instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_type":
            case "type":
            {
                value = instance.m_type;
                return true;
            }
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
            case "m_type":
            case "type":
            {
                if (value is Havok.hk2014.hkaiAstarCostModifier.CostModifierType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_type = (Havok.hk2014.hkaiAstarCostModifier.CostModifierType)byteValue;
                    return true;
                }
                return false;
            }
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
