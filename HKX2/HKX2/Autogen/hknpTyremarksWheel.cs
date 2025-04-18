using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpTyremarksWheel : hkReferencedObject
    {
        public override uint Signature { get => 2405991805; }
        
        public int m_currentPosition;
        public int m_numPoints;
        public List<hknpTyremarkPoint> m_tyremarkPoints;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_currentPosition = br.ReadInt32();
            m_numPoints = br.ReadInt32();
            m_tyremarkPoints = des.ReadClassArray<hknpTyremarkPoint>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteInt32(m_currentPosition);
            bw.WriteInt32(m_numPoints);
            s.WriteClassArray<hknpTyremarkPoint>(bw, m_tyremarkPoints);
        }
    }
}
