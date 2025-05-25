// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclRuntimeConversionInfoData : HavokData<hclRuntimeConversionInfo> 
{
    private static readonly System.Reflection.FieldInfo _slotConversionsInfo = typeof(hclRuntimeConversionInfo).GetField("m_slotConversions")!;
    private static readonly System.Reflection.FieldInfo _elementConversionsInfo = typeof(hclRuntimeConversionInfo).GetField("m_elementConversions")!;
    public hclRuntimeConversionInfoData(HavokType type, hclRuntimeConversionInfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_slotConversions":
            case "slotConversions":
            {
                if (instance.m_slotConversions is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_elementConversions":
            case "elementConversions":
            {
                if (instance.m_elementConversions is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numSlotsConverted":
            case "numSlotsConverted":
            {
                if (instance.m_numSlotsConverted is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numElementsConverted":
            case "numElementsConverted":
            {
                if (instance.m_numElementsConverted is not TGet castValue) return false;
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
            case "m_slotConversions":
            case "slotConversions":
            {
                if (value is not hclRuntimeConversionInfo.SlotConversion[] castValue || castValue.Length != 4) return false;
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
                if (value is not hclRuntimeConversionInfo.ElementConversion[] castValue || castValue.Length != 4) return false;
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
