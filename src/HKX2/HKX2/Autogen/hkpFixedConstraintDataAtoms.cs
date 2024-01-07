using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpFixedConstraintDataAtoms : IHavokObject
    {
        public virtual uint Signature { get => 647952837; }
        
        public hkpSetLocalTransformsConstraintAtom m_transforms;
        public hkpSetupStabilizationAtom m_setupStabilization;
        public hkpBallSocketConstraintAtom m_ballSocket;
        public hkp3dAngConstraintAtom m_ang;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_transforms = new hkpSetLocalTransformsConstraintAtom();
            m_transforms.Read(des, br);
            m_setupStabilization = new hkpSetupStabilizationAtom();
            m_setupStabilization.Read(des, br);
            m_ballSocket = new hkpBallSocketConstraintAtom();
            m_ballSocket.Read(des, br);
            m_ang = new hkp3dAngConstraintAtom();
            m_ang.Read(des, br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            m_transforms.Write(s, bw);
            m_setupStabilization.Write(s, bw);
            m_ballSocket.Write(s, bw);
            m_ang.Write(s, bw);
        }
    }
}
