using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpVehicleFrictionDescriptionAxisDescription : IHavokObject
    {
        public virtual uint Signature { get => 1506678079; }
        
        public float m_frictionCircleYtab_0;
        public float m_frictionCircleYtab_1;
        public float m_frictionCircleYtab_2;
        public float m_frictionCircleYtab_3;
        public float m_frictionCircleYtab_4;
        public float m_frictionCircleYtab_5;
        public float m_frictionCircleYtab_6;
        public float m_frictionCircleYtab_7;
        public float m_frictionCircleYtab_8;
        public float m_frictionCircleYtab_9;
        public float m_frictionCircleYtab_10;
        public float m_frictionCircleYtab_11;
        public float m_frictionCircleYtab_12;
        public float m_frictionCircleYtab_13;
        public float m_frictionCircleYtab_14;
        public float m_frictionCircleYtab_15;
        public float m_xStep;
        public float m_xStart;
        public float m_wheelSurfaceInertia;
        public float m_wheelSurfaceInertiaInv;
        public float m_wheelChassisMassRatio;
        public float m_wheelRadius;
        public float m_wheelRadiusInv;
        public float m_wheelDownForceFactor;
        public float m_wheelDownForceSumFactor;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_frictionCircleYtab_0 = br.ReadSingle();
            m_frictionCircleYtab_1 = br.ReadSingle();
            m_frictionCircleYtab_2 = br.ReadSingle();
            m_frictionCircleYtab_3 = br.ReadSingle();
            m_frictionCircleYtab_4 = br.ReadSingle();
            m_frictionCircleYtab_5 = br.ReadSingle();
            m_frictionCircleYtab_6 = br.ReadSingle();
            m_frictionCircleYtab_7 = br.ReadSingle();
            m_frictionCircleYtab_8 = br.ReadSingle();
            m_frictionCircleYtab_9 = br.ReadSingle();
            m_frictionCircleYtab_10 = br.ReadSingle();
            m_frictionCircleYtab_11 = br.ReadSingle();
            m_frictionCircleYtab_12 = br.ReadSingle();
            m_frictionCircleYtab_13 = br.ReadSingle();
            m_frictionCircleYtab_14 = br.ReadSingle();
            m_frictionCircleYtab_15 = br.ReadSingle();
            m_xStep = br.ReadSingle();
            m_xStart = br.ReadSingle();
            m_wheelSurfaceInertia = br.ReadSingle();
            m_wheelSurfaceInertiaInv = br.ReadSingle();
            m_wheelChassisMassRatio = br.ReadSingle();
            m_wheelRadius = br.ReadSingle();
            m_wheelRadiusInv = br.ReadSingle();
            m_wheelDownForceFactor = br.ReadSingle();
            m_wheelDownForceSumFactor = br.ReadSingle();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteSingle(m_frictionCircleYtab_0);
            bw.WriteSingle(m_frictionCircleYtab_1);
            bw.WriteSingle(m_frictionCircleYtab_2);
            bw.WriteSingle(m_frictionCircleYtab_3);
            bw.WriteSingle(m_frictionCircleYtab_4);
            bw.WriteSingle(m_frictionCircleYtab_5);
            bw.WriteSingle(m_frictionCircleYtab_6);
            bw.WriteSingle(m_frictionCircleYtab_7);
            bw.WriteSingle(m_frictionCircleYtab_8);
            bw.WriteSingle(m_frictionCircleYtab_9);
            bw.WriteSingle(m_frictionCircleYtab_10);
            bw.WriteSingle(m_frictionCircleYtab_11);
            bw.WriteSingle(m_frictionCircleYtab_12);
            bw.WriteSingle(m_frictionCircleYtab_13);
            bw.WriteSingle(m_frictionCircleYtab_14);
            bw.WriteSingle(m_frictionCircleYtab_15);
            bw.WriteSingle(m_xStep);
            bw.WriteSingle(m_xStart);
            bw.WriteSingle(m_wheelSurfaceInertia);
            bw.WriteSingle(m_wheelSurfaceInertiaInv);
            bw.WriteSingle(m_wheelChassisMassRatio);
            bw.WriteSingle(m_wheelRadius);
            bw.WriteSingle(m_wheelRadiusInv);
            bw.WriteSingle(m_wheelDownForceFactor);
            bw.WriteSingle(m_wheelDownForceSumFactor);
        }
    }
}
