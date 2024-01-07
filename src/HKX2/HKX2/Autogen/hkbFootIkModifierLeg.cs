using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbFootIkModifierLeg : IHavokObject
    {
        public virtual uint Signature { get => 3921947741; }
        
        public Matrix4x4 m_originalAnkleTransformMS;
        public Vector4 m_kneeAxisLS;
        public Vector4 m_footEndLS;
        public hkbEventProperty m_ungroundedEvent;
        public float m_footPlantedAnkleHeightMS;
        public float m_footRaisedAnkleHeightMS;
        public float m_maxAnkleHeightMS;
        public float m_minAnkleHeightMS;
        public float m_maxKneeAngleDegrees;
        public float m_minKneeAngleDegrees;
        public float m_verticalError;
        public short m_hipIndex;
        public short m_kneeIndex;
        public short m_ankleIndex;
        public bool m_hitSomething;
        public bool m_isPlantedMS;
        public bool m_isOriginalAnkleTransformMSSet;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_originalAnkleTransformMS = des.ReadQSTransform(br);
            m_kneeAxisLS = des.ReadVector4(br);
            m_footEndLS = des.ReadVector4(br);
            m_ungroundedEvent = new hkbEventProperty();
            m_ungroundedEvent.Read(des, br);
            m_footPlantedAnkleHeightMS = br.ReadSingle();
            m_footRaisedAnkleHeightMS = br.ReadSingle();
            m_maxAnkleHeightMS = br.ReadSingle();
            m_minAnkleHeightMS = br.ReadSingle();
            m_maxKneeAngleDegrees = br.ReadSingle();
            m_minKneeAngleDegrees = br.ReadSingle();
            m_verticalError = br.ReadSingle();
            m_hipIndex = br.ReadInt16();
            m_kneeIndex = br.ReadInt16();
            m_ankleIndex = br.ReadInt16();
            m_hitSomething = br.ReadBoolean();
            m_isPlantedMS = br.ReadBoolean();
            m_isOriginalAnkleTransformMSSet = br.ReadBoolean();
            br.ReadUInt64();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteQSTransform(bw, m_originalAnkleTransformMS);
            s.WriteVector4(bw, m_kneeAxisLS);
            s.WriteVector4(bw, m_footEndLS);
            m_ungroundedEvent.Write(s, bw);
            bw.WriteSingle(m_footPlantedAnkleHeightMS);
            bw.WriteSingle(m_footRaisedAnkleHeightMS);
            bw.WriteSingle(m_maxAnkleHeightMS);
            bw.WriteSingle(m_minAnkleHeightMS);
            bw.WriteSingle(m_maxKneeAngleDegrees);
            bw.WriteSingle(m_minKneeAngleDegrees);
            bw.WriteSingle(m_verticalError);
            bw.WriteInt16(m_hipIndex);
            bw.WriteInt16(m_kneeIndex);
            bw.WriteInt16(m_ankleIndex);
            bw.WriteBoolean(m_hitSomething);
            bw.WriteBoolean(m_isPlantedMS);
            bw.WriteBoolean(m_isOriginalAnkleTransformMSSet);
            bw.WriteUInt64(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
