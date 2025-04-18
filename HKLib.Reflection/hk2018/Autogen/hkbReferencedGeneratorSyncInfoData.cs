// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbReferencedGeneratorSyncInfoData : HavokData<hkbReferencedGeneratorSyncInfo> 
{
    public hkbReferencedGeneratorSyncInfoData(HavokType type, hkbReferencedGeneratorSyncInfo instance) : base(type, instance) {}

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
            case "m_syncInfo":
            case "syncInfo":
            {
                if (instance.m_syncInfo is not TGet castValue) return false;
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
            case "m_syncInfo":
            case "syncInfo":
            {
                if (value is not hkbGeneratorSyncInfo castValue) return false;
                instance.m_syncInfo = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
