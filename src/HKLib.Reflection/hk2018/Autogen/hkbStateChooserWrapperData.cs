// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbStateChooserWrapperData : HavokData<hkbStateChooserWrapper> 
{
    public hkbStateChooserWrapperData(HavokType type, hkbStateChooserWrapper instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_wrappedChooser":
            case "wrappedChooser":
            {
                if (instance.m_wrappedChooser is null)
                {
                    return true;
                }
                if (instance.m_wrappedChooser is TGet castValue)
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_wrappedChooser":
            case "wrappedChooser":
            {
                if (value is null)
                {
                    instance.m_wrappedChooser = default;
                    return true;
                }
                if (value is hkbStateChooser castValue)
                {
                    instance.m_wrappedChooser = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
