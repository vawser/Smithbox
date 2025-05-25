using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbSetWorldFromModelModifier : hkbModifier
    {
        public override uint Signature { get => 3025917183; }
        
        public Vector4 m_translation;
        public Quaternion m_rotation;
        public bool m_setTranslation;
        public bool m_setRotation;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadUInt64();
            m_translation = des.ReadVector4(br);
            m_rotation = des.ReadQuaternion(br);
            m_setTranslation = br.ReadBoolean();
            m_setRotation = br.ReadBoolean();
            br.ReadUInt64();
            br.ReadUInt32();
            br.ReadUInt16();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(0);
            s.WriteVector4(bw, m_translation);
            s.WriteQuaternion(bw, m_rotation);
            bw.WriteBoolean(m_setTranslation);
            bw.WriteBoolean(m_setRotation);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
        }
    }
}
