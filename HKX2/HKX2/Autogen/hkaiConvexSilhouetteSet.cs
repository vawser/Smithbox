using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiConvexSilhouetteSet : hkReferencedObject
    {
        public override uint Signature { get => 3424370516; }
        
        public List<Vector4> m_vertexPool;
        public List<int> m_silhouetteOffsets;
        public hkQTransform m_cachedTransform;
        public Vector4 m_cachedUp;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_vertexPool = des.ReadVector4Array(br);
            m_silhouetteOffsets = des.ReadInt32Array(br);
            m_cachedTransform = new hkQTransform();
            m_cachedTransform.Read(des, br);
            m_cachedUp = des.ReadVector4(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteVector4Array(bw, m_vertexPool);
            s.WriteInt32Array(bw, m_silhouetteOffsets);
            m_cachedTransform.Write(s, bw);
            s.WriteVector4(bw, m_cachedUp);
        }
    }
}
