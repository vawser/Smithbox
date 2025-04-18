using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkAlignSceneToNodeOptions : hkReferencedObject
    {
        public override uint Signature { get => 2764685286; }
        
        public bool m_invert;
        public bool m_transformPositionX;
        public bool m_transformPositionY;
        public bool m_transformPositionZ;
        public bool m_transformRotation;
        public bool m_transformScale;
        public bool m_transformSkew;
        public int m_keyframe;
        public string m_nodeName;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_invert = br.ReadBoolean();
            m_transformPositionX = br.ReadBoolean();
            m_transformPositionY = br.ReadBoolean();
            m_transformPositionZ = br.ReadBoolean();
            m_transformRotation = br.ReadBoolean();
            m_transformScale = br.ReadBoolean();
            m_transformSkew = br.ReadBoolean();
            br.ReadByte();
            m_keyframe = br.ReadInt32();
            br.ReadUInt32();
            m_nodeName = des.ReadStringPointer(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteBoolean(m_invert);
            bw.WriteBoolean(m_transformPositionX);
            bw.WriteBoolean(m_transformPositionY);
            bw.WriteBoolean(m_transformPositionZ);
            bw.WriteBoolean(m_transformRotation);
            bw.WriteBoolean(m_transformScale);
            bw.WriteBoolean(m_transformSkew);
            bw.WriteByte(0);
            bw.WriteInt32(m_keyframe);
            bw.WriteUInt32(0);
            s.WriteStringPointer(bw, m_nodeName);
        }
    }
}
