// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpTriggerVolumeEventInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpTriggerVolumeEventInfo>
{
    public hkpTriggerVolumeEventInfoData(Havok.hk2014.hkpTriggerVolumeEventInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_sortValue":
            case "sortValue":
            {
                value = instance.m_sortValue;
                return true;
            }
            case "m_body":
            case "body":
            {
                value = instance.m_body;
                return true;
            }
            case "m_operation":
            case "operation":
            {
                value = instance.m_operation;
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
            case "m_sortValue":
            case "sortValue":
            {
                if (value is not ulong castValue) return false;
                instance.m_sortValue = castValue;
                return true;
            }
            case "m_body":
            case "body":
            {
                if (value is null)
                {
                    instance.m_body = default;
                    return true;
                }
                if (value is Havok.hk2014.hkpRigidBody castValue)
                {
                    instance.m_body = castValue;
                    return true;
                }
                return false;
            }
            case "m_operation":
            case "operation":
            {
                if (value is Havok.hk2014.hkpTriggerVolume.Operation castValue)
                {
                    instance.m_operation = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_operation = (Havok.hk2014.hkpTriggerVolume.Operation)intValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
