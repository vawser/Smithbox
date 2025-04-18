using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkxCamera : hkReferencedObject
    {
        public override uint Signature { get => 1641286740; }
        
        public Vector4 m_from;
        public Vector4 m_focus;
        public Vector4 m_up;
        public float m_fov;
        public float m_far;
        public float m_near;
        public bool m_leftHanded;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_from = des.ReadVector4(br);
            m_focus = des.ReadVector4(br);
            m_up = des.ReadVector4(br);
            m_fov = br.ReadSingle();
            m_far = br.ReadSingle();
            m_near = br.ReadSingle();
            m_leftHanded = br.ReadBoolean();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteVector4(bw, m_from);
            s.WriteVector4(bw, m_focus);
            s.WriteVector4(bw, m_up);
            bw.WriteSingle(m_fov);
            bw.WriteSingle(m_far);
            bw.WriteSingle(m_near);
            bw.WriteBoolean(m_leftHanded);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
