using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum NodeType
    {
        NODE_TYPE_UNKNOWN = 0,
        NODE_TYPE_STATE_MACHINE = 1,
        NODE_TYPE_CLIP = 2,
        NODE_TYPE_BLEND = 4,
        NODE_TYPE_MODIFIER = 8,
        NODE_TYPE_GENERATOR = 16,
        NODE_TYPE_MODIFIER_GENERATOR = 32,
        NODE_TYPE_TRANSITION_EFFECT = 64,
        NODE_TYPE_BEHAVIOR_FILE_REFERENCE = 128,
    }
    
    public partial class hkbToolNodeType : IHavokObject
    {
        public virtual uint Signature { get => 2908338143; }
        
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteByte(0);
        }
    }
}
