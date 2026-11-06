// Automatically Generated

namespace Havok.hk2014;

public class hkpTriggerVolume : hkReferencedObject
{
    public List<hkpRigidBody?> m_overlappingBodies = new();

    public List<hkpTriggerVolumeEventInfo> m_eventQueue = new();

    public hkpRigidBody? m_triggerBody;

    public uint m_sequenceNumber;


    public enum Operation : int
    {
        ADDED_OP = 0,
        REMOVED_OP = 1,
        CONTACT_OP = 2,
        TOI_OP = 3
    }

}
