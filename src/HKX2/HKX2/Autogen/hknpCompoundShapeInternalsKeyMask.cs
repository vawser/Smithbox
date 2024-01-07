using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpCompoundShapeInternalsKeyMask : hknpCompoundShapeKeyMask
    {
        public override uint Signature { get => 1403878776; }
        
        public hknpCompoundShape m_shape;
        public List<hknpShapeKeyMask> m_instanceMasks;
        public List<uint> m_filter;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_shape = des.ReadClassPointer<hknpCompoundShape>(br);
            m_instanceMasks = des.ReadClassPointerArray<hknpShapeKeyMask>(br);
            m_filter = des.ReadUInt32Array(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hknpCompoundShape>(bw, m_shape);
            s.WriteClassPointerArray<hknpShapeKeyMask>(bw, m_instanceMasks);
            s.WriteUInt32Array(bw, m_filter);
        }
    }
}
