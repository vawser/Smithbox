using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum EventType
    {
        ENTERED_EVENT = 1,
        LEFT_EVENT = 2,
        ENTERED_AND_LEFT_EVENT = 3,
        TRIGGER_BODY_LEFT_EVENT = 6,
    }
    
    public enum Operation
    {
        ADDED_OP = 0,
        REMOVED_OP = 1,
        CONTACT_OP = 2,
        TOI_OP = 3,
    }
    
    public partial class hkpTriggerVolume : hkReferencedObject
    {
        public override uint Signature { get => 3752019127; }
        
        public List<hkpRigidBody> m_overlappingBodies;
        public List<hkpTriggerVolumeEventInfo> m_eventQueue;
        public hkpRigidBody m_triggerBody;
        public uint m_sequenceNumber;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            m_overlappingBodies = des.ReadClassPointerArray<hkpRigidBody>(br);
            m_eventQueue = des.ReadClassArray<hkpTriggerVolumeEventInfo>(br);
            m_triggerBody = des.ReadClassPointer<hkpRigidBody>(br);
            m_sequenceNumber = br.ReadUInt32();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            s.WriteClassPointerArray<hkpRigidBody>(bw, m_overlappingBodies);
            s.WriteClassArray<hkpTriggerVolumeEventInfo>(bw, m_eventQueue);
            s.WriteClassPointer<hkpRigidBody>(bw, m_triggerBody);
            bw.WriteUInt32(m_sequenceNumber);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
        }
    }
}
