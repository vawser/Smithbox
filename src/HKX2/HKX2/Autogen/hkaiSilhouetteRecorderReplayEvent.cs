using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum ReplayEventType
    {
        EVENT_CONNECT_TO_WORLD = 0,
        EVENT_INSTANCE_LOADED = 1,
        EVENT_INSTANCE_UNLOADED = 2,
        EVENT_STEP_SILHOUETTES = 3,
        EVENT_VOLUME_LOADED = 4,
        EVENT_VOLUME_UNLOADED = 5,
        EVENT_GRAPH_LOADED = 6,
        EVENT_GRAPH_UNLOADED = 7,
    }
    
    public partial class hkaiSilhouetteRecorderReplayEvent : hkReferencedObject
    {
        public override uint Signature { get => 2206032283; }
        
        public ReplayEventType m_eventType;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_eventType = (ReplayEventType)br.ReadByte();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteByte((byte)m_eventType);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
