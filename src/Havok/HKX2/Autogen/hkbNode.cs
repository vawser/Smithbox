using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum GetChildrenFlagBits
    {
        FLAG_ACTIVE_ONLY = 1,
        FLAG_GENERATORS_ONLY = 4,
        FLAG_IGNORE_REFERENCED_BEHAVIORS = 8,
    }
    
    public enum CloneState
    {
        CLONE_STATE_DEFAULT = 0,
        CLONE_STATE_TEMPLATE = 1,
        CLONE_STATE_CLONE = 2,
    }
    
    public enum TemplateOrClone
    {
        NODE_IS_TEMPLATE = 0,
        NODE_IS_CLONE = 1,
    }
    
    public partial class hkbNode : hkbBindable
    {
        public override uint Signature { get => 146023711; }
        
        public ulong m_userData;
        public string m_name;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_userData = br.ReadUInt64();
            m_name = des.ReadStringPointer(br);
            br.ReadUInt64();
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(m_userData);
            s.WriteStringPointer(bw, m_name);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
        }
    }
}
