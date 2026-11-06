// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hclRuntimeConversionInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hclRuntimeConversionInfo>
{
    private static readonly System.Reflection.FieldInfo _slotConversionsInfo = typeof(Havok.hk2016.hclRuntimeConversionInfo).GetField("m_slotConversions")!;
    private static readonly System.Reflection.FieldInfo _elementConversionsInfo = typeof(Havok.hk2016.hclRuntimeConversionInfo).GetField("m_elementConversions")!;
    public hclRuntimeConversionInfoData(Havok.hk2016.hclRuntimeConversionInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_slotConversions":
            case "slotConversions":
            {
                value = instance.m_slotConversions;
                return true;
            }
            case "m_elementConversions":
            case "elementConversions":
            {
                value = instance.m_elementConversions;
                return true;
            }
            case "m_numSlotsConverted":
            case "numSlotsConverted":
            {
                value = instance.m_numSlotsConverted;
                return true;
            }
            case "m_numElementsConverted":
            case "numElementsConverted":
            {
                value = instance.m_numElementsConverted;
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
            case "m_slotConversions":
            case "slotConversions":
            {
                if (value is not Havok.hk2016.hclRuntimeConversionInfo.SlotConversion[] castValue || castValue.Length != 4) return false;
                try
                {
                    _slotConversionsInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_elementConversions":
            case "elementConversions":
            {
                if (value is not Havok.hk2016.hclRuntimeConversionInfo.ElementConversion[] castValue || castValue.Length != 4) return false;
                try
                {
                    _elementConversionsInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_numSlotsConverted":
            case "numSlotsConverted":
            {
                if (value is not byte castValue) return false;
                instance.m_numSlotsConverted = castValue;
                return true;
            }
            case "m_numElementsConverted":
            case "numElementsConverted":
            {
                if (value is not byte castValue) return false;
                instance.m_numElementsConverted = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
