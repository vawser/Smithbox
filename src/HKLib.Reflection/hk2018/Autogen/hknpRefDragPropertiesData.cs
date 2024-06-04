// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpRefDragPropertiesData : HavokData<hknpRefDragProperties> 
{
    public hknpRefDragPropertiesData(HavokType type, hknpRefDragProperties instance) : base(type, instance) {}

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
            case "m_dragProperties":
            case "dragProperties":
            {
                if (instance.m_dragProperties is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_dragProperties":
            case "dragProperties":
            {
                if (value is not hknpDragProperties castValue) return false;
                instance.m_dragProperties = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
