using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclSimClothDataTransferMotionData : IHavokObject
    {
        public virtual uint Signature { get => 222936661; }
        
        public uint m_transformSetIndex;
        public uint m_transformIndex;
        public bool m_transferTranslationMotion;
        public float m_minTranslationSpeed;
        public float m_maxTranslationSpeed;
        public float m_minTranslationBlend;
        public float m_maxTranslationBlend;
        public bool m_transferRotationMotion;
        public float m_minRotationSpeed;
        public float m_maxRotationSpeed;
        public float m_minRotationBlend;
        public float m_maxRotationBlend;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_transformSetIndex = br.ReadUInt32();
            m_transformIndex = br.ReadUInt32();
            m_transferTranslationMotion = br.ReadBoolean();
            br.ReadUInt16();
            br.ReadByte();
            m_minTranslationSpeed = br.ReadSingle();
            m_maxTranslationSpeed = br.ReadSingle();
            m_minTranslationBlend = br.ReadSingle();
            m_maxTranslationBlend = br.ReadSingle();
            m_transferRotationMotion = br.ReadBoolean();
            br.ReadUInt16();
            br.ReadByte();
            m_minRotationSpeed = br.ReadSingle();
            m_maxRotationSpeed = br.ReadSingle();
            m_minRotationBlend = br.ReadSingle();
            m_maxRotationBlend = br.ReadSingle();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteUInt32(m_transformSetIndex);
            bw.WriteUInt32(m_transformIndex);
            bw.WriteBoolean(m_transferTranslationMotion);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            bw.WriteSingle(m_minTranslationSpeed);
            bw.WriteSingle(m_maxTranslationSpeed);
            bw.WriteSingle(m_minTranslationBlend);
            bw.WriteSingle(m_maxTranslationBlend);
            bw.WriteBoolean(m_transferRotationMotion);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            bw.WriteSingle(m_minRotationSpeed);
            bw.WriteSingle(m_maxRotationSpeed);
            bw.WriteSingle(m_minRotationBlend);
            bw.WriteSingle(m_maxRotationBlend);
        }
    }
}
