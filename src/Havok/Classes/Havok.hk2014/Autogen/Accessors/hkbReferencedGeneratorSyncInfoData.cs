// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbReferencedGeneratorSyncInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbReferencedGeneratorSyncInfo>
{
    public hkbReferencedGeneratorSyncInfoData(Havok.hk2014.hkbReferencedGeneratorSyncInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_syncInfo":
            case "syncInfo":
            {
                if (value is not Havok.hk2014.hkbGeneratorSyncInfo castValue) return false;
                instance.m_syncInfo = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
