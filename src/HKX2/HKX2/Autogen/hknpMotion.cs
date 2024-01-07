using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpMotion : IHavokObject
    {
        public virtual uint Signature { get => 3131184858; }
        
        public Vector4 m_centerOfMassAndMassFactor;
        public Quaternion m_orientation;
        public short m_inverseInertia_0;
        public short m_inverseInertia_1;
        public short m_inverseInertia_2;
        public short m_inverseInertia_3;
        public uint m_firstAttachedBodyId;
        public short m_linearVelocityCage_0;
        public short m_linearVelocityCage_1;
        public short m_linearVelocityCage_2;
        public short m_integrationFactor;
        public ushort m_motionPropertiesId;
        public short m_maxLinearAccelerationDistancePerStep;
        public short m_maxRotationToPreventTunneling;
        public byte m_cellIndex;
        public byte m_spaceSplitterWeight;
        public Vector4 m_linearVelocity;
        public Vector4 m_angularVelocity;
        public Vector4 m_previousStepLinearVelocity;
        public Vector4 m_previousStepAngularVelocity;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_centerOfMassAndMassFactor = des.ReadVector4(br);
            m_orientation = des.ReadQuaternion(br);
            m_inverseInertia_0 = br.ReadInt16();
            m_inverseInertia_1 = br.ReadInt16();
            m_inverseInertia_2 = br.ReadInt16();
            m_inverseInertia_3 = br.ReadInt16();
            m_firstAttachedBodyId = br.ReadUInt32();
            br.ReadUInt32();
            m_linearVelocityCage_0 = br.ReadInt16();
            m_linearVelocityCage_1 = br.ReadInt16();
            m_linearVelocityCage_2 = br.ReadInt16();
            m_integrationFactor = br.ReadInt16();
            m_motionPropertiesId = br.ReadUInt16();
            m_maxLinearAccelerationDistancePerStep = br.ReadInt16();
            m_maxRotationToPreventTunneling = br.ReadInt16();
            m_cellIndex = br.ReadByte();
            m_spaceSplitterWeight = br.ReadByte();
            m_linearVelocity = des.ReadVector4(br);
            m_angularVelocity = des.ReadVector4(br);
            m_previousStepLinearVelocity = des.ReadVector4(br);
            m_previousStepAngularVelocity = des.ReadVector4(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteVector4(bw, m_centerOfMassAndMassFactor);
            s.WriteQuaternion(bw, m_orientation);
            bw.WriteInt16(m_inverseInertia_0);
            bw.WriteInt16(m_inverseInertia_1);
            bw.WriteInt16(m_inverseInertia_2);
            bw.WriteInt16(m_inverseInertia_3);
            bw.WriteUInt32(m_firstAttachedBodyId);
            bw.WriteUInt32(0);
            bw.WriteInt16(m_linearVelocityCage_0);
            bw.WriteInt16(m_linearVelocityCage_1);
            bw.WriteInt16(m_linearVelocityCage_2);
            bw.WriteInt16(m_integrationFactor);
            bw.WriteUInt16(m_motionPropertiesId);
            bw.WriteInt16(m_maxLinearAccelerationDistancePerStep);
            bw.WriteInt16(m_maxRotationToPreventTunneling);
            bw.WriteByte(m_cellIndex);
            bw.WriteByte(m_spaceSplitterWeight);
            s.WriteVector4(bw, m_linearVelocity);
            s.WriteVector4(bw, m_angularVelocity);
            s.WriteVector4(bw, m_previousStepLinearVelocity);
            s.WriteVector4(bw, m_previousStepAngularVelocity);
        }
    }
}
