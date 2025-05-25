using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpDefaultBroadPhaseConfig : hknpBroadPhaseConfig
    {
        public override uint Signature { get => 1329300569; }
        
        public hknpBroadPhaseConfigLayer m_layers_0;
        public hknpBroadPhaseConfigLayer m_layers_1;
        public hknpBroadPhaseConfigLayer m_layers_2;
        public hknpBroadPhaseConfigLayer m_layers_3;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_layers_0 = new hknpBroadPhaseConfigLayer();
            m_layers_0.Read(des, br);
            m_layers_1 = new hknpBroadPhaseConfigLayer();
            m_layers_1.Read(des, br);
            m_layers_2 = new hknpBroadPhaseConfigLayer();
            m_layers_2.Read(des, br);
            m_layers_3 = new hknpBroadPhaseConfigLayer();
            m_layers_3.Read(des, br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            m_layers_0.Write(s, bw);
            m_layers_1.Write(s, bw);
            m_layers_2.Write(s, bw);
            m_layers_3.Write(s, bw);
        }
    }
}
