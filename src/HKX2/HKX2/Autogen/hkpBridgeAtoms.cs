using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpBridgeAtoms : IHavokObject
    {
        public virtual uint Signature { get => 2182408700; }
        
        public hkpBridgeConstraintAtom m_bridgeAtom;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_bridgeAtom = new hkpBridgeConstraintAtom();
            m_bridgeAtom.Read(des, br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            m_bridgeAtom.Write(s, bw);
        }
    }
}
