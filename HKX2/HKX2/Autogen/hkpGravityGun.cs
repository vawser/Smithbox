using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpGravityGun : hkpFirstPersonGun
    {
        public override uint Signature { get => 1479190421; }
        
        public int m_maxNumObjectsPicked;
        public float m_maxMassOfObjectPicked;
        public float m_maxDistOfObjectPicked;
        public float m_impulseAppliedWhenObjectNotPicked;
        public float m_throwVelocity;
        public Vector4 m_capturedObjectPosition;
        public Vector4 m_capturedObjectsOffset;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadUInt64();
            br.ReadUInt64();
            m_maxNumObjectsPicked = br.ReadInt32();
            m_maxMassOfObjectPicked = br.ReadSingle();
            m_maxDistOfObjectPicked = br.ReadSingle();
            m_impulseAppliedWhenObjectNotPicked = br.ReadSingle();
            m_throwVelocity = br.ReadSingle();
            br.ReadUInt32();
            m_capturedObjectPosition = des.ReadVector4(br);
            m_capturedObjectsOffset = des.ReadVector4(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteInt32(m_maxNumObjectsPicked);
            bw.WriteSingle(m_maxMassOfObjectPicked);
            bw.WriteSingle(m_maxDistOfObjectPicked);
            bw.WriteSingle(m_impulseAppliedWhenObjectNotPicked);
            bw.WriteSingle(m_throwVelocity);
            bw.WriteUInt32(0);
            s.WriteVector4(bw, m_capturedObjectPosition);
            s.WriteVector4(bw, m_capturedObjectsOffset);
        }
    }
}
