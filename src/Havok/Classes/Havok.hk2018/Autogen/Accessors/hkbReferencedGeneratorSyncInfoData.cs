// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbReferencedGeneratorSyncInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbReferencedGeneratorSyncInfo>
{
    public hkbReferencedGeneratorSyncInfoData(Havok.hk2018.hkbReferencedGeneratorSyncInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_syncInfo":
            case "syncInfo":
            {
                value = instance.m_syncInfo;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_syncInfo":
            case "syncInfo":
            {
                if (value is not Havok.hk2018.hkbGeneratorSyncInfo castValue) return false;
                instance.m_syncInfo = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
