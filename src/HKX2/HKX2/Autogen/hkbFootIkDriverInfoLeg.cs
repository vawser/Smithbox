using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbFootIkDriverInfoLeg : IHavokObject
    {
        public virtual uint Signature { get => 4239716005; }
        
        public Vector4 m_kneeAxisLS;
        public Vector4 m_footEndLS;
        public float m_footPlantedAnkleHeightMS;
        public float m_footRaisedAnkleHeightMS;
        public float m_maxAnkleHeightMS;
        public float m_minAnkleHeightMS;
        public float m_maxKneeAngleDegrees;
        public float m_minKneeAngleDegrees;
        public short m_hipIndex;
        public short m_hipSiblingIndex;
        public short m_kneeIndex;
        public short m_kneeSiblingIndex;
        public short m_ankleIndex;
        public bool m_ForceFootGround;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_kneeAxisLS = des.ReadVector4(br);
            m_footEndLS = des.ReadVector4(br);
            m_footPlantedAnkleHeightMS = br.ReadSingle();
            m_footRaisedAnkleHeightMS = br.ReadSingle();
            m_maxAnkleHeightMS = br.ReadSingle();
            m_minAnkleHeightMS = br.ReadSingle();
            m_maxKneeAngleDegrees = br.ReadSingle();
            m_minKneeAngleDegrees = br.ReadSingle();
            m_hipIndex = br.ReadInt16();
            m_hipSiblingIndex = br.ReadInt16();
            m_kneeIndex = br.ReadInt16();
            m_kneeSiblingIndex = br.ReadInt16();
            m_ankleIndex = br.ReadInt16();
            m_ForceFootGround = br.ReadBoolean();
            br.ReadUInt64();
            br.ReadUInt32();
            br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteVector4(bw, m_kneeAxisLS);
            s.WriteVector4(bw, m_footEndLS);
            bw.WriteSingle(m_footPlantedAnkleHeightMS);
            bw.WriteSingle(m_footRaisedAnkleHeightMS);
            bw.WriteSingle(m_maxAnkleHeightMS);
            bw.WriteSingle(m_minAnkleHeightMS);
            bw.WriteSingle(m_maxKneeAngleDegrees);
            bw.WriteSingle(m_minKneeAngleDegrees);
            bw.WriteInt16(m_hipIndex);
            bw.WriteInt16(m_hipSiblingIndex);
            bw.WriteInt16(m_kneeIndex);
            bw.WriteInt16(m_kneeSiblingIndex);
            bw.WriteInt16(m_ankleIndex);
            bw.WriteBoolean(m_ForceFootGround);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteByte(0);
        }
    }
}
