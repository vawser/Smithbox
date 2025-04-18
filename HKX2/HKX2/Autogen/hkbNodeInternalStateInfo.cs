using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbNodeInternalStateInfo : hkReferencedObject
    {
        public override uint Signature { get => 2174392097; }
        
        public hkbReferencedGeneratorSyncInfo m_syncInfo;
        public string m_name;
        public hkReferencedObject m_internalState;
        public ushort m_nodeId;
        public bool m_hasActivateBeenCalled;
        public bool m_isModifierEnabled;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_syncInfo = des.ReadClassPointer<hkbReferencedGeneratorSyncInfo>(br);
            m_name = des.ReadStringPointer(br);
            m_internalState = des.ReadClassPointer<hkReferencedObject>(br);
            m_nodeId = br.ReadUInt16();
            m_hasActivateBeenCalled = br.ReadBoolean();
            m_isModifierEnabled = br.ReadBoolean();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkbReferencedGeneratorSyncInfo>(bw, m_syncInfo);
            s.WriteStringPointer(bw, m_name);
            s.WriteClassPointer<hkReferencedObject>(bw, m_internalState);
            bw.WriteUInt16(m_nodeId);
            bw.WriteBoolean(m_hasActivateBeenCalled);
            bw.WriteBoolean(m_isModifierEnabled);
            bw.WriteUInt32(0);
        }
    }
}
