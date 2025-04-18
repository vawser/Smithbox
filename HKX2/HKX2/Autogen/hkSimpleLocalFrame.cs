using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkSimpleLocalFrame : hkLocalFrame
    {
        public override uint Signature { get => 2039144338; }
        
        public Matrix4x4 m_transform;
        public List<hkLocalFrame> m_children;
        public hkLocalFrame m_parentFrame;
        public hkLocalFrameGroup m_group;
        public string m_name;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_transform = des.ReadTransform(br);
            m_children = des.ReadClassPointerArray<hkLocalFrame>(br);
            m_parentFrame = des.ReadClassPointer<hkLocalFrame>(br);
            m_group = des.ReadClassPointer<hkLocalFrameGroup>(br);
            m_name = des.ReadStringPointer(br);
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteTransform(bw, m_transform);
            s.WriteClassPointerArray<hkLocalFrame>(bw, m_children);
            s.WriteClassPointer<hkLocalFrame>(bw, m_parentFrame);
            s.WriteClassPointer<hkLocalFrameGroup>(bw, m_group);
            s.WriteStringPointer(bw, m_name);
            bw.WriteUInt64(0);
        }
    }
}
