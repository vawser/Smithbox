using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbHandle : hkReferencedObject
    {
        public override uint Signature { get => 1137004340; }
        
        public hkLocalFrame m_frame;
        public hkReferencedObject m_rigidBody;
        public hkbCharacter m_character;
        public short m_animationBoneIndex;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_frame = des.ReadClassPointer<hkLocalFrame>(br);
            m_rigidBody = des.ReadClassPointer<hkReferencedObject>(br);
            m_character = des.ReadClassPointer<hkbCharacter>(br);
            m_animationBoneIndex = br.ReadInt16();
            br.ReadUInt32();
            br.ReadUInt16();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkLocalFrame>(bw, m_frame);
            s.WriteClassPointer<hkReferencedObject>(bw, m_rigidBody);
            s.WriteClassPointer<hkbCharacter>(bw, m_character);
            bw.WriteInt16(m_animationBoneIndex);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
        }
    }
}
