// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkDefaultPropertyBagData : HavokData<hkDefaultPropertyBag> 
{
    public hkDefaultPropertyBagData(HavokType type, hkDefaultPropertyBag instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyMap":
            case "propertyMap":
            {
                if (instance.m_propertyMap is not TGet castValue) return false;
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
            case "m_propertyMap":
            case "propertyMap":
            {
                if (value is not hkHashMap<hkPropertyId, object> castValue) return false;
                instance.m_propertyMap = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
