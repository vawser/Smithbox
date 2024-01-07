using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaBoneAttachment : hkReferencedObject
    {
        public override uint Signature { get => 3043424933; }
        
        public string m_originalSkeletonName;
        public Matrix4x4 m_boneFromAttachment;
        public hkReferencedObject m_attachment;
        public string m_name;
        public short m_boneIndex;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_originalSkeletonName = des.ReadStringPointer(br);
            br.ReadUInt64();
            m_boneFromAttachment = des.ReadMatrix4(br);
            m_attachment = des.ReadClassPointer<hkReferencedObject>(br);
            m_name = des.ReadStringPointer(br);
            m_boneIndex = br.ReadInt16();
            br.ReadUInt64();
            br.ReadUInt32();
            br.ReadUInt16();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteStringPointer(bw, m_originalSkeletonName);
            bw.WriteUInt64(0);
            s.WriteMatrix4(bw, m_boneFromAttachment);
            s.WriteClassPointer<hkReferencedObject>(bw, m_attachment);
            s.WriteStringPointer(bw, m_name);
            bw.WriteInt16(m_boneIndex);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
        }
    }
}
