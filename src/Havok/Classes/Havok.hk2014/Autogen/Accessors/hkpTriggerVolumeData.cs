// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpTriggerVolumeData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpTriggerVolume>
{
    public hkpTriggerVolumeData(Havok.hk2014.hkpTriggerVolume instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_overlappingBodies":
            case "overlappingBodies":
            {
                value = instance.m_overlappingBodies;
                return true;
            }
            case "m_eventQueue":
            case "eventQueue":
            {
                value = instance.m_eventQueue;
                return true;
            }
            case "m_triggerBody":
            case "triggerBody":
            {
                value = instance.m_triggerBody;
                return true;
            }
            case "m_sequenceNumber":
            case "sequenceNumber":
            {
                value = instance.m_sequenceNumber;
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
            case "m_overlappingBodies":
            case "overlappingBodies":
            {
                if (value is not List<Havok.hk2014.hkpRigidBody?> castValue) return false;
                instance.m_overlappingBodies = castValue;
                return true;
            }
            case "m_eventQueue":
            case "eventQueue":
            {
                if (value is not List<Havok.hk2014.hkpTriggerVolumeEventInfo> castValue) return false;
                instance.m_eventQueue = castValue;
                return true;
            }
            case "m_triggerBody":
            case "triggerBody":
            {
                if (value is null)
                {
                    instance.m_triggerBody = default;
                    return true;
                }
                if (value is Havok.hk2014.hkpRigidBody castValue)
                {
                    instance.m_triggerBody = castValue;
                    return true;
                }
                return false;
            }
            case "m_sequenceNumber":
            case "sequenceNumber":
            {
                if (value is not uint castValue) return false;
                instance.m_sequenceNumber = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
